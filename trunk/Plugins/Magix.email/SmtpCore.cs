/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Net;
using System.Web;
using System.Configuration;
using System.Collections.ObjectModel;
using sys = System.Security.Cryptography.X509Certificates;
using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Cryptography;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Magix.Core;
using Org.BouncyCastle.Security;

namespace Magix.email
{
	/*
	 * email smtp core
	 */
	internal class SmtpCore : ActiveController
	{
        /*
         * sends email over smtp
         */
        [ActiveEvent(Name = "magix.smtp.send-message")]
        public static void magix_smtp_send_message(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.smtp.send-message-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.smtp.send-message-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            MimeMessage msg = BuildMessageHeader(ip, dp);
            BodyBuilder builder = BuildMessageBody(ip, dp);
            MimeEntity body = builder.ToMessageBody();
            if (ip.Contains("signed") && ip["signed"].Get<bool>())
            {
                sys.X509Certificate2 origCert = MailCommon.FindCertificate(((MailboxAddress)msg.From[0]).Address);
                X509Certificate cert = DotNetUtilities.FromX509Certificate(origCert);
                AsymmetricKeyParameter akp = DotNetUtilities.GetKeyPair(origCert.PrivateKey).Private;
                CmsSigner signer = new CmsSigner(cert, akp);
                msg.Body = MultipartSigned.Create(signer, body);
            }
            else
                msg.Body = body;
            SendMessage(ip, dp, msg);
        }

        /*
         * builds header fields of message
         */
        private static MimeMessage BuildMessageHeader(Node ip, Node dp)
        {
            MimeMessage msg = new MimeMessage();

            msg.Subject = Expressions.GetExpressionValue<string>(ip.GetValue("subject", "(no subject)"), dp, ip, false);

            msg.From.Add(new MailboxAddress(ip["from"].GetValue("display-name", ""), ip.GetValue("from", "")));

            if (ip.Contains("reply-to"))
                msg.ReplyTo.Add(new MailboxAddress(ip["reply-to"].GetValue("display-name", ""), ip.GetValue("reply-to", "")));

            foreach (Node idxTo in ip["to"])
                msg.To.Add(new MailboxAddress(idxTo.GetValue("display-name", ""), idxTo.Get<string>()));

            foreach (Node idxCc in ip["cc"])
                msg.Cc.Add(new MailboxAddress(idxCc.GetValue("display-name", ""), idxCc.Get<string>()));

            foreach (Node idxBcc in ip["bcc"])
                msg.Bcc.Add(new MailboxAddress(idxBcc.GetValue("display-name", ""), idxBcc.Get<string>()));

            return msg;
        }

        /*
         * builds body parts of message
         */
        private static BodyBuilder BuildMessageBody(Node ip, Node dp)
        {
            BodyBuilder builder = new BodyBuilder();

            if (ip.Contains("body"))
            {
                if (ip["body"].Contains("html"))
                    builder.HtmlBody = ip["body"]["html"].Get<string>();
                if (ip["body"].Contains("plain"))
                    builder.TextBody = ip["body"]["plain"].Get<string>();
            }

            Node getRoot = new Node();
            RaiseActiveEvent(
                "magix.file.get-base-path",
                getRoot);

            string rootDirectory = getRoot["path"].Get<string>();

            if (ip.Contains("attachments"))
            {
                foreach (Node idx in ip["attachments"])
                {
                    builder.Attachments.Add(rootDirectory + idx.Get<string>());
                }
            }

            if (ip.Contains("linked-attachments"))
            {
                foreach (Node idx in ip["linked-attachments"])
                {
                    builder.LinkedResources.Add(rootDirectory + idx.Get<string>());
                }
            }
            return builder;
        }

        /*
         * sends message
         */
        private static void SendMessage(Node ip, Node dp, MimeMessage msg)
        {
            string host = Expressions.GetExpressionValue<string>(ip.GetValue("host", ""), dp, ip, false);
            int port = Expressions.GetExpressionValue<int>(ip.GetValue("port", "-1"), dp, ip, false);
            bool implicitSsl = Expressions.GetExpressionValue<bool>(ip.GetValue("implicit-ssl", "false"), dp, ip, false);
            string username = Expressions.GetExpressionValue<string>(ip.GetValue("username", ""), dp, ip, false);
            string password = Expressions.GetExpressionValue<string>(ip.GetValue("password", ""), dp, ip, false);

            using (SmtpClient client = new SmtpClient())
            {
                client.Connect(host, port, implicitSsl);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                if (!string.IsNullOrEmpty(username))
                    client.Authenticate(new NetworkCredential(username, password));

                client.Send(msg);

                client.Disconnect(true);
            }
        }
    }
}


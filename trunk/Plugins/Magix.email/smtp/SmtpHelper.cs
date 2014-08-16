/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Net;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using sys = System.Security.Cryptography.X509Certificates;
using Magix.Core;
using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Cryptography;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Magix.email
{
	/*
	 * email smtp helper
	 */
	internal class SmtpHelper
	{
        /*
         * builds header fields of message
         */
        internal static MimeMessage BuildMessageHeader(Node ip, Node dp)
        {
            MimeMessage msg = new MimeMessage();
            msg.Subject = Expressions.GetExpressionValue<string>(ip.GetValue("subject", "(no subject)"), dp, ip, false);
            msg.From.Add(new MailboxAddress(ip["from"].GetValue("display-name", ""), ip["from"].Get<string>()));

            if (ip.ContainsValue("reply-to"))
                msg.ReplyTo.Add(new MailboxAddress(ip["reply-to"].GetValue("display-name", ""), ip.GetValue("reply-to", "")));

            if (ip.Contains("to"))
                foreach (Node idxTo in ip["to"])
                    msg.To.Add(new MailboxAddress(idxTo.GetValue("display-name", ""), idxTo.Get<string>()));

            if (ip.Contains("cc"))
                foreach (Node idxCc in ip["cc"])
                    msg.Cc.Add(new MailboxAddress(idxCc.GetValue("display-name", ""), idxCc.Get<string>()));

            if (ip.Contains("bcc"))
                foreach (Node idxBcc in ip["bcc"])
                    msg.Bcc.Add(new MailboxAddress(idxBcc.GetValue("display-name", ""), idxBcc.Get<string>()));

            return msg;
        }

        /*
         * builds body parts of message
         */
        internal static MimeEntity BuildMessageBody(Node ip, Node dp)
        {
            BodyBuilder builder = new BodyBuilder();
            BuildBody(ip, builder);
            BuildAttachments(ip, builder);
            return builder.ToMessageBody();
        }

        /*
         * signs and encrypts message
         */
        internal static MimeEntity SignAndEncrypt(Node ip, MimeMessage msg, MimeEntity body)
        {
            if (ip.GetValue("signed", false))
            {
                // email is signed
                Node signNode = new Node();
                signNode["mime-entity"].Value = body;
                signNode["email-lookup"].Value = ((MailboxAddress)msg.From[0]).Address;
                string activeEvent = "magix.cryptography._sign-mime-entity";

                if (ip.GetValue("encrypt", false))
                {
                    // email is signed and encrypted
                    GetRecipients(msg, signNode);
                    activeEvent = "magix.cryptography._sign_and_encrypt-mime-entity";
                }
                ActiveEvents.Instance.RaiseActiveEvent(
                    typeof(SmtpHelper),
                    activeEvent,
                    signNode);
                body = signNode["mime-entity"].Get<MimeEntity>();
            }
            else if (ip.GetValue("encrypt", false))
            {
                // email is encrypted
                Node encryptNode = new Node();
                encryptNode["mime-entity"].Value = body;

                GetRecipients(msg, encryptNode);
                ActiveEvents.Instance.RaiseActiveEvent(
                    typeof(SmtpHelper),
                    "magix.cryptography._encrypt-mime-entity",
                    encryptNode);
                body = encryptNode["mime-entity"].Get<MimeEntity>();
            }
            return body;
        }

        /*
         * sends message
         */
        internal static void SendMessage(Node ip, Node dp, MimeMessage msg)
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

        /*
         * builds body of message
         */
        private static void BuildBody(Node ip, BodyBuilder builder)
        {
            if (ip.Contains("body"))
            {
                if (ip["body"].Contains("html"))
                    builder.HtmlBody = ip["body"]["html"].Get<string>();
                if (ip["body"].Contains("plain"))
                    builder.TextBody = ip["body"]["plain"].Get<string>();
            }
        }

        /*
         * builds attachments of message, if any
         */
        private static void BuildAttachments(Node ip, BodyBuilder builder)
        {
            if (!ip.Contains("attachments") && !ip.Contains("linked-attachments"))
                return;

            Node getRoot = new Node();
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(SmtpHelper),
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
        }

        /*
         * puts all recipients into node as email-lookups
         */
        private static void GetRecipients(MimeMessage msg, Node signNode)
        {
            foreach (InternetAddressList idxList in new InternetAddressList[] { msg.To, msg.Cc, msg.Bcc })
            {
                foreach (MailboxAddress idx in idxList)
                {
                    signNode["email-lookups"].Add(new Node("", idx.Address));
                }
            }
        }
    }
}


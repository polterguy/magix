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

            string fromEmail = Expressions.GetExpressionValue<string>(ip["from"].Get<string>(), dp, ip, false);
            string fromName = Expressions.GetExpressionValue<string>(ip["from"].GetValue("display-name", ""), dp, ip, false);
            msg.From.Add(new MailboxAddress(fromName, fromEmail));

            if (ip.ContainsValue("reply-to"))
            {
                string replyToEmail = Expressions.GetExpressionValue<string>(ip["reply-to"].Get<string>(), dp, ip, false);
                string replyToName = Expressions.GetExpressionValue<string>(ip["reply-to"].GetValue("display-name", ""), dp, ip, false);
                msg.ReplyTo.Add(new MailboxAddress(replyToName, replyToEmail));
            }

            if (ip.Contains("to"))
            {
                foreach (Node idxTo in ip["to"])
                {
                    string toEmail = Expressions.GetExpressionValue<string>(idxTo.Get<string>(), dp, ip, false);
                    string toName = Expressions.GetExpressionValue<string>(idxTo.GetValue("display-name", ""), dp, ip, false);
                    msg.To.Add(new MailboxAddress(toName, toEmail));
                }
            }

            if (ip.Contains("cc"))
            {
                foreach (Node idxTo in ip["cc"])
                {
                    string toEmail = Expressions.GetExpressionValue<string>(idxTo.Get<string>(), dp, ip, false);
                    string toName = Expressions.GetExpressionValue<string>(idxTo.GetValue("display-name", ""), dp, ip, false);
                    msg.To.Add(new MailboxAddress(toName, toEmail));
                }
            }

            if (ip.Contains("bcc"))
            {
                foreach (Node idxTo in ip["bcc"])
                {
                    string toEmail = Expressions.GetExpressionValue<string>(idxTo.Get<string>(), dp, ip, false);
                    string toName = Expressions.GetExpressionValue<string>(idxTo.GetValue("display-name", ""), dp, ip, false);
                    msg.To.Add(new MailboxAddress(toName, toEmail));
                }
            }

            return msg;
        }

        /*
         * builds body parts of message
         */
        internal static MimeEntity BuildMessageBody(Node ip, Node dp)
        {
            BodyBuilder builder = new BodyBuilder();
            BuildBody(ip, dp, builder);
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
                    client.Authenticate(username, password);
                client.Send(msg);
                client.Disconnect(true);
            }
        }

        /*
         * builds body of message
         */
        private static void BuildBody(Node ip, Node dp, BodyBuilder builder)
        {
            if (ip.Contains("body"))
            {
                if (ip["body"].Contains("html"))
                {
                    string body = Expressions.GetExpressionValue<string>(ip["body"]["html"].Get<string>(), dp, ip, false);
                    builder.HtmlBody = body;
                }
                if (ip["body"].Contains("plain"))
                {
                    string body = Expressions.GetExpressionValue<string>(ip["body"]["plain"].Get<string>(), dp, ip, false);
                    builder.TextBody = body;
                }
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


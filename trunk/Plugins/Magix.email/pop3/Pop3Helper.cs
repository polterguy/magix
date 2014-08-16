/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using MimeKit;
using MailKit.Net.Pop3;
using Magix.Core;

namespace Magix.email
{
	/*
	 * email pop3 helper
	 */
	internal class Pop3Helper
	{
        /*
         * returns message count from pop3 server
         */
        internal static int GetMessageCount(Core.Node ip, Core.Node dp)
        {
            string host = Expressions.GetExpressionValue<string>(ip.GetValue("host", ""), dp, ip, false);
            int port = Expressions.GetExpressionValue<int>(ip.GetValue("port", "-1"), dp, ip, false);
            bool implicitSsl = Expressions.GetExpressionValue<bool>(ip.GetValue("ssl", "false"), dp, ip, false);
            string username = Expressions.GetExpressionValue<string>(ip.GetValue("username", ""), dp, ip, false);
            string password = Expressions.GetExpressionValue<string>(ip.GetValue("password", ""), dp, ip, false);

            using (Pop3Client client = new Pop3Client())
            {
                client.Connect(host, port, implicitSsl);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                if (!string.IsNullOrEmpty(username))
                    client.Authenticate(username, password);
                return client.GetMessageCount();
            }
        }

        /*
         * returns messages from pop3 server
         */
        internal static void GetMessages(Node ip, Node dp)
        {
            string host = Expressions.GetExpressionValue<string>(ip.GetValue("host", ""), dp, ip, false);
            int port = Expressions.GetExpressionValue<int>(ip.GetValue("port", "-1"), dp, ip, false);
            bool implicitSsl = Expressions.GetExpressionValue<bool>(ip.GetValue("ssl", "false"), dp, ip, false);
            string username = Expressions.GetExpressionValue<string>(ip.GetValue("username", ""), dp, ip, false);
            string password = Expressions.GetExpressionValue<string>(ip.GetValue("password", ""), dp, ip, false);

            using (Pop3Client client = new Pop3Client())
            {
                client.Connect(host, port, implicitSsl);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                if (!string.IsNullOrEmpty(username))
                    client.Authenticate(username, password);

                int count = client.GetMessageCount();
                for (int idxMsg = 0; idxMsg < count; idxMsg++)
                {
                    MimeMessage msg = client.GetMessage(idxMsg);
                    BuildMessage(msg, ip["values"]["msg_" + idxMsg]);
                }
            }
        }

        /*
         * puts a message into a node for returning back to client
         */
        private static void BuildMessage(MimeMessage msg, Node node)
        {
            ExtractHeaders(msg, node);
            ExtractBody(msg, node);
            foreach (MimePart idxAtt in msg.Attachments)
            {
                ;// idxAtt.
            }
        }

        /*
         * extracts body parts from message and put into node
         */
        private static void ExtractBody(MimeMessage msg, Node node)
        {
            foreach (MimePart idxBody in msg.BodyParts)
            {
                if (idxBody is TextPart)
                {
                    TextPart tp = idxBody as TextPart;
                    if (idxBody.ContentType.MediaType == "text")
                    {
                        if (idxBody.ContentType.MediaSubtype == "plain")
                        {
                            node["body"]["plain"].Value = tp.Text;
                        }
                        else if (idxBody.ContentType.MediaSubtype == "html")
                        {
                            node["body"]["html"].Value = tp.Text;
                        }
                    }
                }
            }
        }

        /*
         * extract headers from email and put into node
         */
        private static void ExtractHeaders(MimeMessage msg, Node node)
        {
            GetAddress(msg.Sender, "sender", node);

            GetAddress(msg.From[0], "from", node);

            if (msg.ReplyTo.Count > 0)
                GetAddress(msg.ReplyTo[0], "reply-to", node);

            foreach (InternetAddress idxAdr in msg.To)
                GetAddress(idxAdr, "", node["to"]);

            foreach (InternetAddress idxAdr in msg.Cc)
                GetAddress(idxAdr, "", node["cc"]);

            node["date"].Value = msg.Date.DateTime;

            if (!string.IsNullOrEmpty(msg.InReplyTo))
                node["in-reply-to"].Value = msg.InReplyTo;

            node["message-id"].Value = msg.MessageId;

            node["subject"].Value = msg.Subject;
        }

        /*
         * puts in an email address into the given node
         */
        private static void GetAddress(InternetAddress adr, string field, Node node)
        {
            if (adr == null || !(adr is MailboxAddress))
                return;

            MailboxAddress mailboxAdr = adr as MailboxAddress;
            Node nNode = new Node(field, mailboxAdr.Address);
            if (!string.IsNullOrEmpty(mailboxAdr.Name))
                nNode.Add("display-name", mailboxAdr.Name);
            node.Add(nNode);
        }
    }
}


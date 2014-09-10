/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using MimeKit;
using MimeKit.Cryptography;
using MailKit;
using MailKit.Net.Pop3;
using HtmlAgilityPack;
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
                int retVal = client.GetMessageCount();
                client.Disconnect(true);
                return retVal;
            }
        }

        /*
         * returns messages from pop3 server
         */
        internal static void GetMessages(
            Node pars,
            Node ip, 
            Node dp, 
            string basePath, 
            string attachmentDirectory, 
            string linkedAttachmentDirectory)
        {
            string host = Expressions.GetExpressionValue<string>(ip.GetValue("host", ""), dp, ip, false);
            int port = Expressions.GetExpressionValue<int>(ip.GetValue("port", "-1"), dp, ip, false);
            bool implicitSsl = Expressions.GetExpressionValue<bool>(ip.GetValue("ssl", "false"), dp, ip, false);
            string username = Expressions.GetExpressionValue<string>(ip.GetValue("username", ""), dp, ip, false);
            string password = Expressions.GetExpressionValue<string>(ip.GetValue("password", ""), dp, ip, false);

            Node exeNode = null;
            if (ip.Contains("code"))
                exeNode = ip["code"];

            using (Pop3Client client = new Pop3Client())
            {
                client.Connect(host, port, implicitSsl);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                if (!string.IsNullOrEmpty(username))
                    client.Authenticate(username, password);

                try
                {
                    RetrieveMessagesFromServer(
                        pars,
                        dp,
                        ip,
                        basePath,
                        attachmentDirectory,
                        linkedAttachmentDirectory,
                        exeNode,
                        client);
                }
                finally
                {
                    if (client.IsConnected)
                        client.Disconnect(true);
                }
            }
        }

        /*
         * retrieves messages from pop3 server
         */
        private static void RetrieveMessagesFromServer(
            Node pars, 
            Node dp,
            Node ip, 
            string basePath, 
            string attachmentDirectory, 
            string linkedAttachmentDirectory, 
            Node exeNode, 
            Pop3Client client)
        {
            int serverCount = client.GetMessageCount();
            int count = Expressions.GetExpressionValue<int>(ip.GetValue("count", serverCount.ToString()), dp, ip, false);
            count = Math.Min(serverCount, count);

            bool delete = Expressions.GetExpressionValue<bool>(ip.GetValue("delete", "false"), dp, ip, false);

            for (int idxMsg = 0; idxMsg < count; idxMsg++)
            {
                MimeMessage msg = client.GetMessage(idxMsg);
                HandleMessage(
                    pars,
                    ip,
                    basePath,
                    attachmentDirectory,
                    linkedAttachmentDirectory,
                    exeNode,
                    idxMsg,
                    msg);

                if (delete)
                    client.DeleteMessage(idxMsg);
            }
        }

        /*
         * handles one message from server
         */
        private static void HandleMessage(
            Node pars, 
            Node ip, 
            string basePath, 
            string attachmentDirectory, 
            string linkedAttachmentDirectory, 
            Node exeNode, 
            int idxMsg, 
            MimeMessage msg)
        {
            if (exeNode == null)
            {
                // returning messages back to caller as a list of emails
                BuildMessage(msg, ip["values"]["msg_" + idxMsg], basePath, attachmentDirectory, linkedAttachmentDirectory);
            }
            else
            {
                HandleMessageInCallback(
                    pars,
                    basePath,
                    attachmentDirectory,
                    linkedAttachmentDirectory,
                    exeNode,
                    msg);
            }
        }

        /*
         * handles message by callback
         */
        private static void HandleMessageInCallback(
            Node pars, 
            string basePath, 
            string attachmentDirectory, 
            string linkedAttachmentDirectory, 
            Node exeNode, 
            MimeMessage msg)
        {
            // we have a code callback
            Node callbackNode = exeNode.Clone();
            BuildMessage(msg, exeNode["_message"], basePath, attachmentDirectory, linkedAttachmentDirectory);
            pars["_ip"].Value = exeNode;

            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Pop3Helper),
                "magix.execute",
                pars);
            exeNode.Clear();
            exeNode.AddRange(callbackNode);
        }

        /*
         * puts a message into a node for returning back to client
         */
        private static void BuildMessage(
            MimeMessage msg, 
            Node node, 
            string basePath, 
            string attachmentDirectory, 
            string linkedAttachmentDirectory)
        {
            ExtractHeaders(msg, node);
            ExtractBody(new MimeEntity[] { msg.Body }, node, false, msg, basePath, attachmentDirectory, linkedAttachmentDirectory);
            CleanUpInlineAttachments(node, attachmentDirectory, linkedAttachmentDirectory);
        }

        /*
         * tries to show inline attachments as they're supposed to be shown
         */
        private static void CleanUpInlineAttachments(Node node, string attachmentDirectory, string linkedAttachmentDirectory)
        {
            if (node.Contains("body") && node["body"].ContainsValue("html"))
            {
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(node["body"]["html"].Get<string>());
                List<HtmlNode> images = doc.DocumentNode.Descendants()
                    .Where(n => n.Name == "img")
                    .ToList();
                foreach (HtmlNode idxImage in images)
                {
                    if (idxImage.Attributes["src"] != null)
                    {
                        HtmlAttribute src = idxImage.Attributes["src"];
                        if (src.Value.StartsWith("cid:"))
                        {
                            string imageSrc = src.Value.Substring(4);
                            if (node.Contains("attachments"))
                            {
                                foreach (Node idxAtt in node["attachments"])
                                {
                                    if (idxAtt.Get<string>() == imageSrc)
                                    {
                                        Node moveImage = new Node();
                                        moveImage["from"].Value = attachmentDirectory + "/" + imageSrc;
                                        moveImage["to"].Value = linkedAttachmentDirectory + "/" + imageSrc;
                                        ActiveEvents.Instance.RaiseActiveEvent(
                                            typeof(Pop3Helper),
                                            "magix.file.move-file",
                                            moveImage);
                                        src.Value = linkedAttachmentDirectory + "/" + imageSrc;
                                    }
                                    idxAtt.UnTie();
                                }
                            }
                        }
                    }
                }
                node["body"]["html"].Value = doc.DocumentNode.InnerHtml;
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

            node["subject"].Value = msg.Subject ?? "";

            node["download-date"].Value = DateTime.Now;
        }

        /*
         * extracts body parts from message and put into node
         */
        private static void ExtractBody(
            IEnumerable<MimeEntity> entities, 
            Node node, 
            bool skipSignature,
            MimeMessage msg, 
            string basePath,
            string attachmentDirectory,
            string linkedAttachmentDirectory)
        {
            foreach (MimeEntity idxBody in entities)
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
                            node["body"]["html"].Value = CleanHtml(tp.Text);
                        }
                    }
                }
                else if (idxBody is ApplicationPkcs7Mime)
                {
                    ApplicationPkcs7Mime pkcs7 = idxBody as ApplicationPkcs7Mime;
                    if (pkcs7.SecureMimeType == SecureMimeType.EnvelopedData)
                    {
                        try
                        {
                            MimeEntity entity = pkcs7.Decrypt();
                            ExtractBody(new MimeEntity[] { entity }, node, false, msg, basePath, attachmentDirectory, linkedAttachmentDirectory);
                            node["encrypted"].Value = true;
                        }
                        catch (Exception err)
                        {
                            // couldn't decrypt
                            node["body"]["plain"].Value = "couldn't decrypt message, exceptions was; '" + err.Message + "'";
                            node["encrypted"].Value = true;
                        }
                    }
                }
                else if (!skipSignature && idxBody is MultipartSigned)
                {
                    MultipartSigned signed = idxBody as MultipartSigned;
                    bool valid = false;
                    foreach (IDigitalSignature signature in signed.Verify())
                    {
                        valid = signature.Verify();
                        if (!valid)
                            break;
                    }
                    node["signed"].Value = valid;
                    ExtractBody(new MimeEntity[] { signed }, node, true, msg, basePath, attachmentDirectory, linkedAttachmentDirectory);
                }
                else if (idxBody is Multipart)
                {
                    Multipart mp = idxBody as Multipart;
                    ExtractBody(mp, node, false, msg, basePath, attachmentDirectory, linkedAttachmentDirectory);
                }
                else if (idxBody is MimePart)
                {
                    // this is an attachment
                    ExtractAttachments(idxBody as MimePart, msg, node, basePath, attachmentDirectory, linkedAttachmentDirectory);
                }
            }
        }

        /*
         * cleans up html from email
         */
        private static string CleanHtml(string html)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            doc.DocumentNode.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "#comment")
                .ToList()
                .ForEach(n => n.Remove());
            HtmlNode el = doc.DocumentNode.SelectSingleNode("//body");
            if (el != null)
                return el.InnerHtml;
            else
                return doc.DocumentNode.InnerHtml;
        }

        /*
         * extracts attachments from message
         */
        private static void ExtractAttachments(
            MimePart entity,
            MimeMessage msg,
            Node node, 
            string basePath, 
            string attachmentDirectory, 
            string linkedAttachmentDirectory)
        {
            if (entity is ApplicationPkcs7Mime)
                return; // don't bother about p7m attachments

            string fileName;
            if (entity.Headers.Contains("Content-Location"))
            {
                // this is a linked resource, replacing references inside html with local filename
                fileName = linkedAttachmentDirectory + msg.MessageId + "_" + entity.FileName;
                if (node.Contains("body") && node["body"].ContainsValue("html"))
                    node["body"]["html"].Value =
                        node["body"]["html"].Get<string>().Replace(entity.Headers["Content-Location"], fileName);
            }
            else
            {
                fileName = attachmentDirectory + msg.MessageId + "_" + entity.FileName;
                Node attNode = new Node("", entity.FileName ?? "");
                attNode["local-file-name"].Value = fileName;
                node["attachments"].Add(attNode);
            }

            using (Stream stream = File.Create(basePath + fileName))
            {
                entity.ContentObject.DecodeTo(stream);
            }
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


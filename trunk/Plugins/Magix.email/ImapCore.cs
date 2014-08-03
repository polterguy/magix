/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using System.Collections.Generic;
using OpaqueMail.Net;
using Magix.Core;

namespace Magix.email
{
	/*
	 * email imap core
	 */
	public class ImapCore : ActiveController
	{
        /*
         * retrieves messages from imap server
         */
        [ActiveEvent(Name = "magix.imap.get-messages")]
        public void magix_imap_get_messages(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.get-messages-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.get-messages-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            Node imapSettings = new Node("magix.data.load");
            imapSettings["id"].Value = "magix.imap.settings";
            BypassExecuteActiveEvent(imapSettings, e.Params);

            string host = null;
            int port = -1;
            bool ssl = false;
            string username = null;
            string password = null;
            if (!imapSettings.Contains("value"))
            {
                host = imapSettings["value"]["host"].Get<string>();
                port = imapSettings["value"]["port"].Get<int>();
                ssl = imapSettings["value"]["ssl"].Get<bool>();
            }
            if (ip.ContainsValue("host"))
                host = ip["host"].Get<string>();
            if (ip.ContainsValue("port"))
                port = ip["port"].Get<int>();
            if (ip.ContainsValue("ssl"))
                ssl = ip["ssl"].Get<bool>();
            if (ip.ContainsValue("username"))
                username = ip["username"].Get<string>();
            if (ip.ContainsValue("password"))
                password = ip["password"].Get<string>();

            if (string.IsNullOrEmpty(host))
                throw new Exception("no host found in imap settings");
            if (port == -1)
                throw new Exception("no port found in imap settings");
            if (string.IsNullOrEmpty(username))
                throw new Exception("no username given to [magix.imap.get-messages]");
            if (string.IsNullOrEmpty(password))
                throw new Exception("no password given to [magix.imap.get-messages]");

            int count = 50;
            if (ip.ContainsValue("count"))
                count = ip["count"].Get<int>();

            int startIndex = 50;
            if (ip.ContainsValue("start"))
                startIndex = ip["start"].Get<int>();

            string mailBoxName = null;
            if (ip.ContainsValue("mailbox-name"))
                mailBoxName = ip["mailbox-name"].Get<string>();

            bool reverse = false;
            if (ip.ContainsValue("reverse"))
                reverse = ip["reverse"].Get<bool>();

            bool setSeenFlag = false;
            if (ip.ContainsValue("set-seen-flag"))
                setSeenFlag = ip["set-seen-flag"].Get<bool>();

            bool headersOnly = false;
            if (ip.ContainsValue("headers-only"))
                headersOnly = ip["headers-only"].Get<bool>();

            ImapClient imap = new ImapClient(host, port, username, password, ssl);
            imap.Connect();
            imap.Authenticate();

            List<ReadOnlyMailMessage> list = null;

            if (ip.Contains("search"))
            {
                string bcc = null;
                if (ip["search"].ContainsValue("bcc"))
                    bcc = ip["search"]["bcc"].Get<string>();

                string before = null;
                if (ip["search"].ContainsValue("before"))
                    before = ip["search"]["before"].Get<string>();

                string body = null;
                if (ip["search"].ContainsValue("body"))
                    body = ip["search"]["body"].Get<string>();

                string cc = null;
                if (ip["search"].ContainsValue("cc"))
                    cc = ip["search"]["cc"].Get<string>();

                bool deleted = false;
                if (ip["search"].ContainsValue("deleted"))
                    deleted = ip["search"]["deleted"].Get<bool>();

                bool draft = false;
                if (ip["search"].ContainsValue("draft"))
                    draft = ip["search"]["draft"].Get<bool>();

                bool flagged = false;
                if (ip["search"].ContainsValue("flagged"))
                    flagged = ip["search"]["flagged"].Get<bool>();

                string from = null;
                if (ip["search"].ContainsValue("from"))
                    from = ip["search"]["from"].Get<string>();

                string header = null;
                if (ip["search"].ContainsValue("header"))
                    header = ip["search"]["header"].Get<string>();

                bool newMsg = false;
                if (ip["search"].ContainsValue("new"))
                    newMsg = ip["search"]["new"].Get<bool>();

                string search = "";

                if (!string.IsNullOrEmpty(bcc))
                    search += "BCC \"" + bcc + "\" ";

                if (!string.IsNullOrEmpty(before))
                    search += "BEFORE \"" + before + "\" ";

                if (!string.IsNullOrEmpty(body))
                    search += "BODY \"" + body + "\" ";

                if (!string.IsNullOrEmpty(cc))
                    search += "CC \"" + cc + "\" ";

                if (deleted)
                    search += "DELETED ";

                if (draft)
                    search += "DRAFT ";

                if (flagged)
                    search += "FLAGGED ";

                if (!string.IsNullOrEmpty(from))
                    search += "FROM \"" + from + "\" ";

                if (!string.IsNullOrEmpty(header))
                    search += "HEADER \"" + header + "\" ";

                if (newMsg)
                    search += "NEW ";

                imap.SelectMailbox(mailBoxName);
                search = search.Trim();
                list = imap.Search(search);
            }
            else
                list = imap.GetMessages(mailBoxName, count, startIndex, reverse, headersOnly, setSeenFlag);

            foreach (ReadOnlyMailMessage idxEmail in list)
            {
                ip["result"]["uid-" + idxEmail.ImapUid]["subject"].Value = idxEmail.Subject;
                ip["result"]["uid-" + idxEmail.ImapUid]["to"].Value = idxEmail.DeliveredTo;
                foreach (MailAddress idxCc in idxEmail.CC)
                {
                    ip["result"]["uid-" + idxEmail.ImapUid]["cc"][idxCc.DisplayName].Value = idxCc.Address;
                }
                foreach (MailAddress idxBcc in idxEmail.Bcc)
                {
                    ip["result"]["uid-" + idxEmail.ImapUid]["bcc"][idxBcc.DisplayName].Value = idxBcc.Address;
                }
                ip["result"]["uid-" + idxEmail.ImapUid]["date"].Value = idxEmail.Date;
                ip["result"]["uid-" + idxEmail.ImapUid]["return-path"].Value = idxEmail.ReturnPath;
                ip["result"]["uid-" + idxEmail.ImapUid]["uid"].Value = idxEmail.ImapUid;
                ip["result"]["uid-" + idxEmail.ImapUid]["mailbox-name"].Value = idxEmail.Mailbox;
                ip["result"]["uid-" + idxEmail.ImapUid]["message-id"].Value = idxEmail.MessageId;
                ip["result"]["uid-" + idxEmail.ImapUid]["signed"].Value = idxEmail.SmimeSigned;
                ip["result"]["uid-" + idxEmail.ImapUid]["encrypted"].Value = idxEmail.SmimeEncryptedEnvelope;
                ip["result"]["uid-" + idxEmail.ImapUid]["triple-wrapped"].Value = idxEmail.SmimeTripleWrapped;
                if (!headersOnly)
                    ip["result"]["uid-" + idxEmail.ImapUid]["body"].Value = Functions.RemoveScriptTags(idxEmail.Body);
            }
        }

        /*
         * retrieves one message from imap server
         */
        [ActiveEvent(Name = "magix.imap.get-message")]
        public void magix_imap_get_message(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.get-message-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.get-message-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            Node imapSettings = new Node("magix.data.load");
            imapSettings["id"].Value = "magix.imap.settings";
            BypassExecuteActiveEvent(imapSettings, e.Params);

            string host = null;
            int port = -1;
            bool ssl = false;
            string username = null;
            string password = null;
            if (!imapSettings.Contains("value"))
            {
                host = imapSettings["value"]["host"].Get<string>();
                port = imapSettings["value"]["port"].Get<int>();
                ssl = imapSettings["value"]["ssl"].Get<bool>();
            }
            if (ip.ContainsValue("host"))
                host = ip["host"].Get<string>();
            if (ip.ContainsValue("port"))
                port = ip["port"].Get<int>();
            if (ip.ContainsValue("ssl"))
                ssl = ip["ssl"].Get<bool>();
            if (ip.ContainsValue("username"))
                username = ip["username"].Get<string>();
            if (ip.ContainsValue("password"))
                password = ip["password"].Get<string>();

            if (string.IsNullOrEmpty(host))
                throw new Exception("no host found in imap settings");
            if (port == -1)
                throw new Exception("no port found in imap settings");
            if (string.IsNullOrEmpty(username))
                throw new Exception("no username given to [magix.imap.get-messages]");
            if (string.IsNullOrEmpty(password))
                throw new Exception("no password given to [magix.imap.get-messages]");

            string mailBoxName = null;
            if (ip.ContainsValue("mailbox-name"))
                mailBoxName = ip["mailbox-name"].Get<string>();

            bool setSeenFlag = false;
            if (ip.ContainsValue("set-seen-flag"))
                setSeenFlag = ip["set-seen-flag"].Get<bool>();

            bool headersOnly = true;
            if (ip.ContainsValue("headers-only"))
                headersOnly = ip["headers-only"].Get<bool>();

            if (!ip.ContainsValue("uid"))
                throw new ArgumentException("no [uid] given to [magix.imap.get-message]");
            int uid = ip["uid"].Get<int>();

            ImapClient imap = new ImapClient(host, port, username, password, ssl);
            imap.Connect();
            imap.Authenticate();

            ReadOnlyMailMessage msg = imap.GetMessageUid(mailBoxName, uid, headersOnly, setSeenFlag);

            ip["value"]["subject"].Value = msg.Subject;
            ip["value"]["to"].Value = msg.DeliveredTo;
            foreach (MailAddress idxCc in msg.CC)
            {
                ip["value"]["cc"][idxCc.DisplayName].Value = idxCc.Address;
            }
            foreach (MailAddress idxBcc in msg.Bcc)
            {
                ip["value"]["bcc"][idxBcc.DisplayName].Value = idxBcc.Address;
            }
            ip["value"]["date"].Value = msg.Date;
            ip["value"]["return-path"].Value = msg.ReturnPath;
            ip["value"]["uid"].Value = msg.ImapUid;
            ip["value"]["mailbox-name"].Value = msg.Mailbox;
            ip["value"]["message-id"].Value = msg.MessageId;
            ip["value"]["priority"].Value = msg.Priority;
            ip["value"]["signed"].Value = msg.SmimeSigned;
            ip["value"]["encrypted"].Value = msg.SmimeEncryptedEnvelope;
            ip["value"]["triple-wrapped"].Value = msg.SmimeTripleWrapped;
            if (!headersOnly)
                ip["value"]["body"].Value = Functions.RemoveScriptTags(msg.Body);
        }

        /*
         * deletes an email message on imap server
         */
        [ActiveEvent(Name = "magix.imap.delete-messages")]
        public void magix_imap_delete_messages(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.delete-message-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.delete-message-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            Node imapSettings = new Node("magix.data.load");
            imapSettings["id"].Value = "magix.imap.settings";
            BypassExecuteActiveEvent(imapSettings, e.Params);

            string host = null;
            int port = -1;
            bool ssl = false;
            string username = null;
            string password = null;
            if (!imapSettings.Contains("value"))
            {
                host = imapSettings["value"]["host"].Get<string>();
                port = imapSettings["value"]["port"].Get<int>();
                ssl = imapSettings["value"]["ssl"].Get<bool>();
            }
            if (ip.ContainsValue("host"))
                host = ip["host"].Get<string>();
            if (ip.ContainsValue("port"))
                port = ip["port"].Get<int>();
            if (ip.ContainsValue("ssl"))
                ssl = ip["ssl"].Get<bool>();
            if (ip.ContainsValue("username"))
                username = ip["username"].Get<string>();
            if (ip.ContainsValue("password"))
                password = ip["password"].Get<string>();

            if (string.IsNullOrEmpty(host))
                throw new Exception("no host found in imap settings");
            if (port == -1)
                throw new Exception("no port found in imap settings");
            if (string.IsNullOrEmpty(username))
                throw new Exception("no username given to [magix.imap.get-messages]");
            if (string.IsNullOrEmpty(password))
                throw new Exception("no password given to [magix.imap.get-messages]");

            string mailBoxName = null;
            if (ip.ContainsValue("mailbox-name"))
                mailBoxName = ip["mailbox-name"].Get<string>();

            List<int> uids = new List<int>();
            if (ip.ContainsValue("uid"))
            {
                uids.Add(ip["uid"].Get<int>());
            }
            else if (ip.Contains("uid"))
            {
                foreach (Node idx in ip["uid"])
                {
                    uids.Add(idx.Get<int>());
                }
            }
            else
                throw new ArgumentException("no [uid] given to [magix.imap.delete-message]");

            ImapClient imap = new ImapClient(host, port, username, password, ssl);
            imap.Connect();
            imap.Authenticate();

            ip["success"].Value = imap.DeleteMessages(mailBoxName, uids.ToArray());
        }

        /*
         * returns imap server's capabilities
         */
        [ActiveEvent(Name = "magix.imap.get-capabilities")]
        public void magix_imap_get_capabilities(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.get-capabilities-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.get-capabilities-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            Node imapSettings = new Node("magix.data.load");
            imapSettings["id"].Value = "magix.imap.settings";
            BypassExecuteActiveEvent(imapSettings, e.Params);

            string host = null;
            int port = -1;
            bool ssl = false;
            string username = null;
            string password = null;
            if (!imapSettings.Contains("value"))
            {
                host = imapSettings["value"]["host"].Get<string>();
                port = imapSettings["value"]["port"].Get<int>();
                ssl = imapSettings["value"]["ssl"].Get<bool>();
            }
            if (ip.ContainsValue("host"))
                host = ip["host"].Get<string>();
            if (ip.ContainsValue("port"))
                port = ip["port"].Get<int>();
            if (ip.ContainsValue("ssl"))
                ssl = ip["ssl"].Get<bool>();
            if (ip.ContainsValue("username"))
                username = ip["username"].Get<string>();
            if (ip.ContainsValue("password"))
                password = ip["password"].Get<string>();

            if (string.IsNullOrEmpty(host))
                throw new Exception("no host found in imap settings");
            if (port == -1)
                throw new Exception("no port found in imap settings");
            if (string.IsNullOrEmpty(username))
                throw new Exception("no username given to [magix.imap.get-messages]");
            if (string.IsNullOrEmpty(password))
                throw new Exception("no password given to [magix.imap.get-messages]");

            ImapClient imap = new ImapClient(host, port, username, password, ssl);
            imap.Connect();
            imap.Authenticate();

            string version;
            string[] capabilities = imap.GetCapabilities(out version);

            ip["result"]["version"].Value = version;
            foreach (string idxFeature in capabilities)
            {
                ip["result"]["feature"].Add(new Node("", idxFeature));
            }
        }
    }
}


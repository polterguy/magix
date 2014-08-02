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
        [ActiveEvent(Name = "magix.imap.retrieve-messages")]
        public void magix_imap_retrieve_messages(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.retrieve-messages-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.retrieve-messages-sample]");
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
                throw new Exception("no username given to [magix.imap.retrieve-messages]");
            if (string.IsNullOrEmpty(password))
                throw new Exception("no password given to [magix.imap.retrieve-messages]");

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

            bool headersOnly = true;
            if (ip.ContainsValue("headers-only"))
                headersOnly = ip["headers-only"].Get<bool>();

            ImapClient imap = new ImapClient(host, port, username, password, ssl);
            imap.Connect();
            imap.Authenticate();

            List<ReadOnlyMailMessage> list = imap.GetMessages(mailBoxName, count, startIndex, reverse, headersOnly, setSeenFlag);

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
                ip["result"]["uid-" + idxEmail.ImapUid]["importance"].Value = idxEmail.Importance;
                ip["result"]["uid-" + idxEmail.ImapUid]["index"].Value = idxEmail.Index;
                ip["result"]["uid-" + idxEmail.ImapUid]["in-reply-to"].Value = idxEmail.InReplyTo;
                ip["result"]["uid-" + idxEmail.ImapUid]["mailbox-name"].Value = idxEmail.Mailbox;
                ip["result"]["uid-" + idxEmail.ImapUid]["message-id"].Value = idxEmail.MessageId;
                ip["result"]["uid-" + idxEmail.ImapUid]["priority"].Value = idxEmail.Priority;
                ip["result"]["uid-" + idxEmail.ImapUid]["signed"].Value = idxEmail.SmimeSigned;
                ip["result"]["uid-" + idxEmail.ImapUid]["encrypted"].Value = idxEmail.SmimeEncryptedEnvelope;
                ip["result"]["uid-" + idxEmail.ImapUid]["triple-wrapped"].Value = idxEmail.SmimeTripleWrapped;
                if (!headersOnly)
                    ip["result"]["uid-" + idxEmail.ImapUid]["body"].Value = Functions.RemoveScriptTags(idxEmail.Body);
            }
        }
    }
}


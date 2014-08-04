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
using System.Security.Cryptography.X509Certificates;
using OpaqueMail.Net;
using Magix.Core;
using System.IO;

namespace Magix.email
{
	/*
	 * email imap core
	 */
    internal class ImapCore : CommonHelper
	{
        /*
         * helper to retrieve connection settings
         */
        private Node GetConnectionSettings(Node ip, Node pars)
        {
            string host = null;
            int port = -1;
            bool ssl = false;
            string username = null;
            string password = null;

            if (!ip.ContainsValue("host"))
            {
                Node imapSettings = new Node("magix.data.load");
                imapSettings["id"].Value = "magix.imap.settings";
                BypassExecuteActiveEvent(imapSettings, pars);

                if (imapSettings.Contains("value"))
                {
                    host = imapSettings["value"]["host"].Get<string>();
                    port = imapSettings["value"]["port"].Get<int>();
                    ssl = imapSettings["value"]["ssl"].Get<bool>();
                }
            }
            else
            {
                if (ip.ContainsValue("host"))
                    host = ip["host"].Get<string>();
                if (ip.ContainsValue("port"))
                    port = ip["port"].Get<int>();
                if (ip.ContainsValue("ssl"))
                    ssl = ip["ssl"].Get<bool>();
            }
            if (!ip.ContainsValue("username"))
            {
                string logInUser = (Page.Session["magix.core.user"] as Node)["username"].Get<string>();
                
                Node imapSettings = new Node("magix.data.load");
                imapSettings["id"].Value = "magix.imap.settings-" + logInUser;
                BypassExecuteActiveEvent(imapSettings, pars);

                if (imapSettings.Contains("value"))
                {
                    username = imapSettings["username"].Get<string>();
                    password = imapSettings["password"].Get<string>();
                }
            }
            else
            {
                if (ip.ContainsValue("username"))
                    username = ip["username"].Get<string>();
                if (ip.ContainsValue("password"))
                    password = ip["password"].Get<string>();
            }

            if (string.IsNullOrEmpty(host))
                throw new Exception("no host found in imap settings");
            if (port == -1)
                throw new Exception("no port found in imap settings");
            if (string.IsNullOrEmpty(username))
                throw new Exception("no username given");
            if (string.IsNullOrEmpty(password))
                throw new Exception("no password given");

            Node retVal = new Node();
            retVal["host"].Value = host;
            retVal["port"].Value = port;
            retVal["ssl"].Value = ssl;
            retVal["username"].Value = username;
            retVal["password"].Value = password;
            return retVal;
        }

        /*
         * returns an ImapClient instance according to settings and opens it
         */
        private ImapClient OpenImapConnection(Node settings)
        {
            ImapClient imap = new ImapClient(
                settings["host"].Get<string>(),
                settings["port"].Get<int>(),
                settings["username"].Get<string>(),
                settings["password"].Get<string>(),
                settings["ssl"].Get<bool>());
            imap.Connect();
            imap.Authenticate();
            return imap;
        }

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

            int count = ip.GetValue("count", 50);
            int startIndex = ip.GetValue("start", 0);
            string mailBoxName = ip.GetValue<string>("mailbox-name", null);
            if (string.IsNullOrEmpty(mailBoxName))
                throw new ArgumentException("no [mailbox-name] given");
            bool reverse = ip.GetValue("reverse", false);
            bool setSeenFlag = ip.GetValue("set-seen-flag", false);
            bool headersOnly = ip.GetValue("headers-only", false);

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));

            List<ReadOnlyMailMessage> list = null;
            if (ip.Contains("search"))
            {
                string bcc = ip["search"].GetValue<string>("bcc", null);
                string before = ip["search"].GetValue<string>("before", null);
                string body = ip["search"].GetValue<string>("body", null);
                string cc = ip["search"].GetValue<string>("cc", null);
                bool deleted = ip["search"].GetValue("deleted", false);
                bool draft = ip["search"].GetValue("draft", false);
                bool flagged = ip["search"].GetValue("flagged", false);
                string from = ip["search"].GetValue<string>("from", null);
                string header = ip["search"].GetValue<string>("header", null);
                bool newMsg = ip["search"].GetValue("new", false);

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
                ip["result"]["uid-" + idxEmail.ImapUid]["from"].Value = idxEmail.From;
                ip["result"]["uid-" + idxEmail.ImapUid]["uid"].Value = idxEmail.ImapUid;
                ip["result"]["uid-" + idxEmail.ImapUid]["mailbox-name"].Value = idxEmail.Mailbox;
                ip["result"]["uid-" + idxEmail.ImapUid]["message-id"].Value = idxEmail.MessageId;
                ip["result"]["uid-" + idxEmail.ImapUid]["signed"].Value = idxEmail.SmimeSigned;
                ip["result"]["uid-" + idxEmail.ImapUid]["encrypted"].Value = idxEmail.SmimeEncryptedEnvelope;
                ip["result"]["uid-" + idxEmail.ImapUid]["triple-wrapped"].Value = idxEmail.SmimeTripleWrapped;
                if (!headersOnly)
                {
                    ip["result"]["uid-" + idxEmail.ImapUid]["body"].Value = Functions.RemoveScriptTags(idxEmail.Body);
                    SaveAttachmentsLocally(e.Params, idxEmail);
                }

                if (idxEmail.SmimeSigningCertificateChain.Count > 0)
                {
                    Node smtpSettings = new Node("magix.data.load");
                    smtpSettings["id"].Value = "magix.smtp.settings";
                    BypassExecuteActiveEvent(smtpSettings, e.Params);
                    bool localMachine = smtpSettings["value"]["smime-local-machine"].Get<bool>();

                    foreach (X509Certificate2 idxCert in idxEmail.SmimeSigningCertificateChain)
                    {
                        CertHelper.InstallWindowsCertificate(idxCert, localMachine ? StoreLocation.LocalMachine : StoreLocation.CurrentUser);
                    }
                }
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

            string mailBoxName = ip.GetValue<string>("mailbox-name", null);
            bool setSeenFlag = ip.GetValue("set-seen-flag", false);
            bool headersOnly = ip.GetValue("headers-only", false);
            if (!ip.ContainsValue("uid"))
                throw new ArgumentException("no [uid] given");
            int uid = ip["uid"].Get<int>();

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
            ReadOnlyMailMessage msg = imap.GetMessageUid(mailBoxName, uid, headersOnly, setSeenFlag);

            if (msg == null)
                throw new ArgumentException("message didn't exist on server");

            // decorating result
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
            ip["value"]["from"].Value = msg.From;
            ip["value"]["uid"].Value = msg.ImapUid;
            ip["value"]["mailbox-name"].Value = msg.Mailbox;
            ip["value"]["message-id"].Value = msg.MessageId;
            ip["value"]["priority"].Value = msg.Priority;
            ip["value"]["signed"].Value = msg.SmimeSigned;
            ip["value"]["encrypted"].Value = msg.SmimeEncryptedEnvelope;
            ip["value"]["triple-wrapped"].Value = msg.SmimeTripleWrapped;
            if (!headersOnly)
                ip["value"]["body"].Value = Functions.RemoveScriptTags(msg.Body);

            if (msg.SmimeSigningCertificateChain.Count > 0)
            {
                Node smtpSettings = new Node("magix.data.load");
                smtpSettings["id"].Value = "magix.smtp.settings";
                BypassExecuteActiveEvent(smtpSettings, e.Params);
                bool localMachine = smtpSettings["value"]["smime-local-machine"].Get<bool>();

                foreach (X509Certificate2 idxCert in msg.SmimeSigningCertificateChain)
                {
                    CertHelper.InstallWindowsCertificate(idxCert, localMachine ? StoreLocation.LocalMachine : StoreLocation.CurrentUser);
                }
            }
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
                    "[magix.imap.delete-messages-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.delete-messages-sample]");
                return;
            }

            Node dp = Dp(e.Params);

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
                throw new ArgumentException("no [uid] given");

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
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

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
            string version;
            string[] capabilities = imap.GetCapabilities(out version);

            ip["result"]["version"].Value = version;
            foreach (string idxFeature in capabilities)
            {
                ip["result"]["feature"].Add(new Node("", idxFeature));
            }
        }

        /*
         * returns imap server's mailbox names
         */
        [ActiveEvent(Name = "magix.imap.list-mailbox-names")]
        public void magix_imap_list_mailbox_names(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.list-mailbox-names-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.list-mailbox-names-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
            Mailbox[] boxes = imap.ListMailboxes(true);

            foreach (Mailbox idxBox in boxes)
            {
                ip["result"].Add(new Node("", idxBox.Name));
            }
        }

        /*
         * examines imap server's mailbox
         */
        [ActiveEvent(Name = "magix.imap.examine-mailbox")]
        public void magix_imap_examine_mailbox(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.examine-mailbox-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.examine-mailbox-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string mailBoxName = ip.GetValue<string>("mailbox-name", null);
            if (string.IsNullOrEmpty(mailBoxName))
                throw new ArgumentException("no [mailbox-name] given");

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
            Mailbox box = imap.ExamineMailbox(mailBoxName);

            ip["result"]["message-count"].Value = box.Count;
            ip["result"]["recent-count"].Value = box.Recent;
        }

        /*
         * creates an imap mailbox
         */
        [ActiveEvent(Name = "magix.imap.create-mailbox")]
        public void magix_imap_create_mailbox(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.create-mailbox-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.create-mailbox-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string mailBoxName = ip.GetValue<string>("mailbox-name", null);
            if (string.IsNullOrEmpty(mailBoxName))
                throw new ArgumentException("no [mailbox-name] given");

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
            ip["success"].Value = imap.CreateMailbox(mailBoxName);
        }

        /*
         * deletes an imap mailbox
         */
        [ActiveEvent(Name = "magix.imap.delete-mailbox")]
        public void magix_imap_delete_mailbox(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.delete-mailbox-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.delete-mailbox-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string mailBoxName = ip.GetValue<string>("mailbox-name", null);
            if (string.IsNullOrEmpty(mailBoxName))
                throw new ArgumentException("no [mailbox-name] given");

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
            ip["success"].Value = imap.DeleteMailbox(mailBoxName);
        }

        /*
         * returns quota for mailbox
         */
        [ActiveEvent(Name = "magix.imap.get-quota")]
        public void magix_imap_get_quota(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.get-quota-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.get-quota-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string mailBoxName = ip.GetValue<string>("mailbox-name", null);
            if (string.IsNullOrEmpty(mailBoxName))
                throw new ArgumentException("no [mailbox-name] given");

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
            QuotaUsage quota = imap.GetQuota(mailBoxName);

            ip["result"]["maximum"].Value = quota.QuotaMaximum;
            ip["result"]["usage"].Value = quota.Usage;
        }

        /*
         * sets quota for mailbox
         */
        [ActiveEvent(Name = "magix.imap.set-quota")]
        public void magix_imap_set_quota(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.set-quota-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.imap.set-quota-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string mailBoxName = ip.GetValue<string>("mailbox-name", null);
            if (string.IsNullOrEmpty(mailBoxName))
                throw new ArgumentException("no [mailbox-name] given");

            int quotaSize = ip.GetValue("size", -1);
            if (quotaSize == -1)
                throw new ArgumentException("no [size] given");

            ImapClient imap = OpenImapConnection(GetConnectionSettings(ip, e.Params));
            ip["success"].Value = imap.SetQuota(mailBoxName, quotaSize);
        }
    }
}


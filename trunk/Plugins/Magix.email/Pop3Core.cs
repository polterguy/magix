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

namespace Magix.email
{
	/*
	 * email pop3 core
	 */
    internal class Pop3Core : CommonHelper
	{
        /*
         * helper to retrieve connection settings
         */
        private static Node GetConnectionSettings(Node ip, Node pars, string userUsername)
        {
            string host = null;
            int port = -1;
            bool ssl = false;
            string username = null;
            string password = null;

            if (!ip.ContainsValue("host"))
            {
                Node pop3Settings = GetSettings(pars, userUsername);

                if (pop3Settings.Contains("value"))
                {
                    host = pop3Settings["value"]["host"].Get<string>();
                    port = pop3Settings["value"]["port"].Get<int>();
                    ssl = pop3Settings["value"]["ssl"].Get<bool>();
                    username = pop3Settings["value"]["username"].Get<string>();
                    password = pop3Settings["value"]["password"].Get<string>();
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
            if (ip.ContainsValue("username"))
                username = ip["username"].Get<string>();
            if (ip.ContainsValue("password"))
                password = ip["password"].Get<string>();

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
         * helper for above
         */
        private static Node GetSettings(Node pars, string username)
        {
            Node pop3Settings = new Node("magix.data.load");
            pop3Settings["id"].Value = "magix.pop3.settings-" + username;
            BypassExecuteActiveEvent(pop3Settings, pars);

            if (pop3Settings.Contains("value"))
                return pop3Settings;

            // trying global settings
            pop3Settings = new Node("magix.data.load");
            pop3Settings["id"].Value = "magix.pop3.settings";
            BypassExecuteActiveEvent(pop3Settings, pars);

            return pop3Settings;
        }

        /*
         * returns an ImapClient instance according to settings and opens it
         */
        private static Pop3Client OpenPop3Connection(Node settings)
        {
            Pop3Client pop3 = new Pop3Client(
                settings["host"].Get<string>(),
                settings["port"].Get<int>(),
                settings["username"].Get<string>(),
                settings["password"].Get<string>(),
                settings["ssl"].Get<bool>());
            pop3.Connect();
            pop3.Authenticate();
            return pop3;
        }

        /*
         * retrieves messages from imap server
         */
        [ActiveEvent(Name = "magix.pop3.get-messages")]
        public static void magix_pop3_get_messages(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.get-messages-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.get-messages-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            int count = ip.GetValue("count", 50);
            int startIndex = ip.GetValue("start", 0);
            bool reverse = ip.GetValue("reverse", false);
            bool headersOnly = ip.GetValue("headers-only", false);
            string user = ip.GetValue("user", "");

            if (string.IsNullOrEmpty(user) && 
                !ip.ContainsValue("username") && 
                HttpContext.Current != null && 
                HttpContext.Current.Session["magix.core.user"] != null)
                user = (HttpContext.Current.Session["magix.core.user"] as Node)["username"].Get<string>();

            Pop3Client pop3 = OpenPop3Connection(GetConnectionSettings(ip, e.Params, user));

            int messageCount = pop3.GetMessageCount();
            if (startIndex > messageCount)
                return;
            if (count + startIndex > messageCount)
                count = messageCount - startIndex;

            List<ReadOnlyMailMessage> list = pop3.GetMessages(count, startIndex, reverse, headersOnly);

            foreach (ReadOnlyMailMessage idxEmail in list)
            {
                ip["result"]["uid-" + idxEmail.MessageId]["index"].Value = idxEmail.Index;
                ip["result"]["uid-" + idxEmail.MessageId]["subject"].Value = idxEmail.Subject;
                ip["result"]["uid-" + idxEmail.MessageId]["to"].Value = idxEmail.DeliveredTo;
                foreach (MailAddress idxCc in idxEmail.CC)
                {
                    ip["result"]["uid-" + idxEmail.MessageId]["cc"][idxCc.DisplayName].Value = idxCc.Address;
                }
                foreach (MailAddress idxBcc in idxEmail.Bcc)
                {
                    ip["result"]["uid-" + idxEmail.MessageId]["bcc"][idxBcc.DisplayName].Value = idxBcc.Address;
                }
                ip["result"]["uid-" + idxEmail.MessageId]["date"].Value = idxEmail.Date;
                ip["result"]["uid-" + idxEmail.MessageId]["return-path"].Value = idxEmail.ReturnPath;
                ip["result"]["uid-" + idxEmail.MessageId]["from"].Value = idxEmail.From.Address;
                ip["result"]["uid-" + idxEmail.MessageId]["from"]["display-name"].Value = idxEmail.From.DisplayName;
                ip["result"]["uid-" + idxEmail.MessageId]["message-id"].Value = idxEmail.MessageId;
                ip["result"]["uid-" + idxEmail.MessageId]["message-id"].Value = idxEmail.MessageId;
                ip["result"]["uid-" + idxEmail.MessageId]["signed"].Value = idxEmail.SmimeSigned;
                ip["result"]["uid-" + idxEmail.MessageId]["encrypted"].Value = idxEmail.SmimeEncryptedEnvelope;
                ip["result"]["uid-" + idxEmail.MessageId]["triple-wrapped"].Value = idxEmail.SmimeTripleWrapped;
                if (!headersOnly)
                {
                    ip["result"]["uid-" + idxEmail.MessageId]["body"].Value = Functions.RemoveScriptTags(idxEmail.Body);
                    SaveAttachmentsLocally(e.Params, idxEmail, user);
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
         * retrieves one message from pop3 server
         */
        [ActiveEvent(Name = "magix.pop3.get-message")]
        public void magix_pop3_get_message(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.get-message-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.get-message-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            bool headersOnly = ip.GetValue("headers-only", false);
            if (!ip.ContainsValue("index"))
                throw new ArgumentException("no [index] given");
            int index = ip["index"].Get<int>();
            string user = ip.GetValue("user", "");

            Pop3Client imap = OpenPop3Connection(GetConnectionSettings(ip, e.Params, user));
            ReadOnlyMailMessage msg = imap.GetMessage(index, headersOnly);

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
            ip["value"]["from"].Value = msg.From.Address;
            ip["value"]["from"]["display-name"].Value = msg.From.DisplayName;
            ip["value"]["message-id"].Value = msg.MessageId;
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
        [ActiveEvent(Name = "magix.pop3.delete-messages")]
        public void magix_pop3_delete_messages(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.delete-messages-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.delete-messages-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            List<int> index = new List<int>();
            if (ip.ContainsValue("index"))
            {
                index.Add(ip["index"].Get<int>());
            }
            else if (ip.Contains("index"))
            {
                foreach (Node idx in ip["index"])
                {
                    index.Add(idx.Get<int>());
                }
            }
            else
                throw new ArgumentException("no [uid] given");

            string user = ip.GetValue("user", "");

            Pop3Client imap = OpenPop3Connection(GetConnectionSettings(ip, e.Params, user));
            ip["success"].Value = imap.DeleteMessages(index.ToArray());
        }

        /*
         * returns imap server's capabilities
         */
        [ActiveEvent(Name = "magix.pop3.get-capabilities")]
        public void magix_pop3_get_capabilities(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.get-capabilities-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.get-capabilities-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string user = ip.GetValue("user", "");

            Pop3Client imap = OpenPop3Connection(GetConnectionSettings(ip, e.Params, user));
            string[] capabilities = imap.GetCapabilities();

            foreach (string idxFeature in capabilities)
            {
                ip["result"]["feature"].Add(new Node("", idxFeature));
            }
        }
    }
}


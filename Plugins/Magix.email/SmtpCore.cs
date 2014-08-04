/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Configuration;
using OpaqueMail.Net;
using Magix.Core;

namespace Magix.email
{
	/*
	 * email smtp core
	 */
	public class SmtpCore : ActiveController
	{
        /*
         * sends an email over smtp
         */
        [ActiveEvent(Name = "magix.smtp.send-email")]
        public void magix_smtp_send_email(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.smtp.send-email-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.smtp.send-email-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string host = null;
            int port = -1;
            bool ssl = false;
            string username = null;
            string password = null;
            string fromEmail = null;
            bool useSmime = false;
            bool smimeSignTime = false;
            bool smimeEncryptEnvelope = false;
            bool smimeTripleWrap = false;
            bool smimeLocalMachine = false;
            string smimeSubjectName = "";

            if (!ip.ContainsValue("host"))
            {
                Node smtpSettings = new Node("magix.data.load");
                smtpSettings["id"].Value = "magix.smtp.settings";
                BypassExecuteActiveEvent(smtpSettings, e.Params);

                if (smtpSettings.Contains("value"))
                {
                    host = smtpSettings["value"]["host"].Get<string>();
                    port = smtpSettings["value"]["port"].Get<int>();
                    ssl = smtpSettings["value"]["ssl"].Get<bool>();
                    fromEmail = smtpSettings["value"]["admin-email"].Get<string>();
                    useSmime = smtpSettings["value"]["use-smime"].Get<bool>();
                    smimeSubjectName = smtpSettings["value"]["smime-subject-name"].Get<string>();
                    smimeSignTime = smtpSettings["value"]["smime-sign-time"].Get<bool>();
                    smimeEncryptEnvelope = smtpSettings["value"]["smime-encrypt-envelope"].Get<bool>();
                    smimeTripleWrap = smtpSettings["value"]["smime-triple-wrap"].Get<bool>();
                    smimeLocalMachine = smtpSettings["value"]["smime-local-machine"].Get<bool>();
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
                if (ip.ContainsValue("use-smime"))
                    useSmime = ip["use-smime"].Get<bool>();
                if (ip.ContainsValue("smime-subject-name"))
                    smimeSubjectName = ip["smime-subject-name"].Get<string>();
                if (ip.ContainsValue("smime-sign-time"))
                    smimeSignTime = ip["smime-sign-time"].Get<bool>();
                if (ip.ContainsValue("smime-encrypt-envelope"))
                    smimeEncryptEnvelope = ip["smime-encrypt-envelope"].Get<bool>();
                if (ip.ContainsValue("smime-triple-wrap"))
                    smimeTripleWrap = ip["smime-triple-wrap"].Get<bool>();
                if (ip.ContainsValue("smime-local-machine"))
                    smimeLocalMachine = ip["smime-local-machine"].Get<bool>();
            }
            if (!ip.ContainsValue("username"))
            {
                string logInUser = (Page.Session["magix.core.user"] as Node)["username"].Get<string>();

                Node smtpSettings = new Node("magix.data.load");
                smtpSettings["id"].Value = "magix.smtp.settings-" + logInUser;
                BypassExecuteActiveEvent(smtpSettings, e.Params);

                if (smtpSettings.Contains("value"))
                {
                    username = smtpSettings["username"].Get<string>();
                    password = smtpSettings["password"].Get<string>();
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
                throw new Exception("no host found in smtp settings");
            if (port == -1)
                throw new Exception("no port found in smtp settings");
            if (string.IsNullOrEmpty(username))
                throw new Exception("no username given");
            if (string.IsNullOrEmpty(password))
                throw new Exception("no password given");

            string subject = Expressions.GetExpressionValue<string>(ip["subject"].Get<string>(), dp, ip, false);
            if (ip["subject"].Count > 0)
                subject = Expressions.FormatString(dp, ip, ip["subject"], subject);

            string body = Expressions.GetExpressionValue<string>(ip["body"].Get<string>(), dp, ip, false);
            if (ip["body"].Count > 0)
                body = Expressions.FormatString(dp, ip, ip["body"], body);

            if (ip.ContainsValue("from"))
                fromEmail = Expressions.GetExpressionValue<string>(ip["from"].Get<string>(), dp, ip, false);

            SendSmtpEmail(
                ip, 
                dp, 
                host, 
                port, 
                ssl, 
                username, 
                password, 
                fromEmail, 
                useSmime, 
                smimeSignTime, 
                smimeEncryptEnvelope, 
                smimeTripleWrap, 
                smimeLocalMachine, 
                smimeSubjectName, 
                subject, 
                body);
        }

        /*
         * helper for above
         */
        private static void SendSmtpEmail(
            Node ip, 
            Node dp, 
            string host, 
            int port, 
            bool ssl, 
            string username, 
            string password, 
            string fromEmail, 
            bool useSmime, 
            bool smimeSignTime, 
            bool smimeEncryptEnvelope, 
            bool smimeTripleWrap, 
            bool smimeLocalMachine, 
            string smimeSubjectName, 
            string subject, 
            string body)
        {
            SmtpClient smtp = new SmtpClient(host, port);
            smtp.EnableSsl = ssl;

            smtp.Credentials = new System.Net.NetworkCredential(username, password);

            MailMessage msg = new MailMessage();
            msg.From = new System.Net.Mail.MailAddress(fromEmail);
            msg.Subject = subject;
            msg.Body = body;
            if (ip["to"].Value != null)
                msg.To.Add(Expressions.GetExpressionValue<string>(ip["to"].Get<string>(), dp, ip, false));
            foreach (Node idx in ip["to"])
            {
                msg.To.Add(new System.Net.Mail.MailAddress(idx.Get<string>(), idx.Name));
            }

            bool canEncrypt = false;
            bool validSmimeSettings = false;

            if (useSmime)
            {
                msg.SmimeSigned = true;
                msg.SmimeSigningOptionFlags =
                    smimeSignTime ?
                        SmimeSigningOptionFlags.SignTime :
                        SmimeSigningOptionFlags.None;
                msg.SmimeEncryptedEnvelope = smimeEncryptEnvelope;
                if (msg.SmimeEncryptedEnvelope && smimeTripleWrap)
                    msg.SmimeTripleWrapped = true;
                msg.SmimeSigningCertificate = CertHelper.GetCertificateBySubjectName(
                    smimeLocalMachine ?
                        System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine :
                        System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser,
                        smimeSubjectName);
                ip["encryption-successful"].Value = smtp.SmimeVerifyAllRecipientsHavePublicKeys(msg);
                canEncrypt = ip["encryption-successful"].Get<bool>();
            }

            if (!canEncrypt && ip.ContainsValue("force-encryption") && ip["force-encryption"].Get<bool>())
            {
                if (!validSmimeSettings)
                    throw new Exception("could not encrypt message, hence no message was sent. settings for s/mime not setup");
                else
                    throw new Exception("could not encrypt message, hence no email was sent. not all recipients had public keys");
            }

            if (ip.ContainsValue("async") && ip["async"].Get<bool>())
                smtp.SendAsync(msg);
            else
                smtp.Send(msg);
        }

        /*
         * verifies email can be sent over smtp encrypted
         */
        [ActiveEvent(Name = "magix.smtp.can-encrypt")]
        public void magix_smtp_can_encrypt(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.smtp.can-encrypt-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.smtp.can-encrypt-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            SmtpClient smtp = new SmtpClient();

            MailMessage msg = new MailMessage();
            if (ip["to"].Value != null)
                msg.To.Add(Expressions.GetExpressionValue<string>(ip["to"].Get<string>(), dp, ip, false));
            foreach (Node idx in ip["to"])
            {
                msg.To.Add(new System.Net.Mail.MailAddress(idx.Get<string>(), idx.Name));
            }

            ip["can-encrypt"].Value = smtp.SmimeVerifyAllRecipientsHavePublicKeys(msg);
        }
    }
}


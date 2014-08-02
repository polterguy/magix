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

            Node smtpSettings = new Node("magix.data.load");
            smtpSettings["id"].Value = "magix.smtp.settings";
            BypassExecuteActiveEvent(smtpSettings, e.Params);

            if (!smtpSettings.Contains("value"))
                throw new ApplicationException("no magix.smtp.settings in database, edit your settings before sending emails");

            SmtpClient smtp = new SmtpClient(
                smtpSettings["value"]["host"].Get<string>(),
                smtpSettings["value"]["port"].Get<int>());
            smtp.EnableSsl = smtpSettings["value"]["ssl"].Get<bool>();

            if (smtpSettings["value"].ContainsValue("username"))
            {
                smtp.Credentials = new System.Net.NetworkCredential(
                    smtpSettings["value"]["username"].Get<string>(),
                    smtpSettings["value"]["password"].Get<string>());
            }

            string subject = Expressions.GetExpressionValue<string>(ip["subject"].Get<string>(), dp, ip, false);
            if (ip["subject"].Count > 0)
                subject = Expressions.FormatString(dp, ip, ip["subject"], subject);

            string body = Expressions.GetExpressionValue<string>(ip["body"].Get<string>(), dp, ip, false);
            if (ip["body"].Count > 0)
                body = Expressions.FormatString(dp, ip, ip["body"], body);

            string fromEmail = smtpSettings["value"]["admin-email"].Get<string>();
            if (ip.ContainsValue("from"))
                fromEmail = Expressions.GetExpressionValue<string>(ip["from"].Get<string>(), dp, ip, false);

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

            if (smtpSettings["value"]["use-smime"].Get<bool>())
            {
                string smimeSubjectName = smtpSettings["value"]["smime-subject-name"].Get<string>();
                if (ip["smime-subject-name"].Value != null)
                    smimeSubjectName = ip["smime-subject-name"].Get<string>();
                msg.SmimeSigned = true;
                msg.SmimeSigningOptionFlags =
                    smtpSettings["value"]["smime-sign-time"].Get<bool>() ?
                        SmimeSigningOptionFlags.SignTime :
                        SmimeSigningOptionFlags.None;
                msg.SmimeEncryptedEnvelope = smtpSettings["value"]["smime-encrypt-envelope"].Get<bool>();
                if (msg.SmimeEncryptedEnvelope && smtpSettings["value"]["smime-triple-wrap"].Get<bool>())
                    msg.SmimeTripleWrapped = true;
                msg.SmimeSigningCertificate = CertHelper.GetCertificateBySubjectName(
                    smtpSettings["value"]["smime-local-machine"].Get<bool>() ?
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

            if (ip.Contains("async") && ip["async"].Get<bool>())
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


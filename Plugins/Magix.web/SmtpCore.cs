/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Net.Mail;
using System.Configuration;
using Magix.Core;
using Magix.UX.Builder;

namespace Magix.web
{
	/*
	 * email core
	 */
	public class SmtpCore : ActiveController
	{
		/*
		 * sends an email over smtp
		 */
		[ActiveEvent(Name = "magix.web.send-email")]
		public void magix_web_send_email(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.send-email-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.send-email-sample]");
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

            MailMessage msg = new MailMessage(
                smtpSettings["value"]["admin-email"].Get<string>(),
                Expressions.GetExpressionValue<string>(ip["to"].Get<string>(), dp, ip, false),
                subject,
                body);

            smtp.Send(msg);
		}
    }
}


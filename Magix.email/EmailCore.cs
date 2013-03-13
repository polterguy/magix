/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Net.Mail;
using Magix.Core;
using Magix.UX.Builder;

namespace Magix.execute
{
	/**
	 * Contains logic to help send emails
	 */
	public class EmailCore : ActiveController
	{
		/**
		 * Sends an email, parameters are "to", "from", "subject" and "message". Will
		 * use the configuration found in web.config to determine which SMTP server
		 * to use
		 */
		[ActiveEvent(Name = "magix.email.send-email")]
		public static void magix_email_email(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will send the given email to
the given [to] from [from].&nbsp;&nbsp;the [subject] will
become subject, and [body] the main message.&nbsp;&nbsp;thread safe";
				e.Params["magix.email.send-email"].Value = null;
				e.Params["magix.email.send-email"]["to"].Value = "to@somewhere.com";
				e.Params["magix.email.send-email"]["from"].Value = "from@somewhere-else.com";
				e.Params["magix.email.send-email"]["subject"].Value = "hi dude";
				e.Params["magix.email.send-email"]["body"].Value = "message i wrote, message you read";
				return;
			}

			if (!e.Params.Contains("to"))
				throw new ArgumentException("No recipient found. You need to add up 'to' child node");
			if (!e.Params.Contains("from"))
				throw new ArgumentException("No sender found. You need to add up 'from' child node");
			if (!e.Params.Contains("subject"))
				throw new ArgumentException("No subject found. You need to add up 'subject' child node");
			if (!e.Params.Contains("body"))
				throw new ArgumentException("No actual message found. You need to add up 'body' child node");

			string toEmail = e.Params["to"].Get<string>();
			string fromEmail = e.Params["from"].Get<string>();
			string subject = e.Params["subject"].Get<string>();
			string body = e.Params["body"].Get<string>();

			MailMessage msg = new MailMessage();
			msg.From = new MailAddress(fromEmail);
			msg.To.Add(new MailAddress(toEmail));
			msg.Subject = subject;
			msg.Body = body;

			SmtpClient client = new SmtpClient();
			client.Send(msg);
		}
	}
}


/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Net;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using sys = System.Security.Cryptography.X509Certificates;
using Magix.Core;
using MimeKit;
using MailKit.Net.Smtp;
using MimeKit.Cryptography;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace Magix.email
{
	/*
	 * email smtp core
	 */
	internal class SmtpCore : ActiveController
	{
        /*
         * sends email over smtp
         */
        [ActiveEvent(Name = "magix.smtp.send-message")]
        public static void magix_smtp_send_message(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.smtp.send-message-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.smtp.send-message-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            MimeMessage msg = SmtpHelper.BuildMessageHeader(ip, dp);
            msg.Body = SmtpHelper.SignAndEncrypt(ip, msg, SmtpHelper.BuildMessageBody(ip, dp));
            SmtpHelper.SendMessage(ip, dp, msg);
        }
    }
}


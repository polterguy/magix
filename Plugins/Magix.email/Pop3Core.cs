/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.email
{
	/*
	 * email pop3 core
	 */
    internal sealed class Pop3Core : ActiveController
	{
        /*
         * retrieves message count from pop3 server
         */
        [ActiveEvent(Name = "magix.pop3.get-message-count")]
        public static void magix_pop3_get_message_count(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.get-message-count-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.email",
                    "Magix.email.hyperlisp.inspect.hl",
                    "[magix.pop3.get-message-count-sample]");
                return;
            }

            Node dp = Dp(e.Params);
            ip["count"].Value = Pop3Helper.GetMessageCount(ip, dp);
        }

        /*
         * retrieves email from pop3 server
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
            Pop3Helper.GetMessages(ip, dp);
        }
    }
}


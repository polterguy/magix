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
            string linkedAttachmentDirectory = GetAttachmentDirectory(e.Params, ip, dp, "magix.email.attachment-directory");
            string attachmentDirectory = GetAttachmentDirectory(e.Params, ip, dp, "magix.email.attachment-directory-private");
            
            Node getBase = new Node();
            RaiseActiveEvent(
                "magix.file.get-base-path",
                getBase);
            string basePath = getBase["path"].Get<string>();

            Pop3Helper.GetMessages(ip, dp, basePath, attachmentDirectory, linkedAttachmentDirectory);
        }

        /*
         * helper to retrieve attachment directory
         */
        private static string GetAttachmentDirectory(Node pars, Node ip, Node dp, string idOfAttachmentDirectory)
        {
            string user = Expressions.GetExpressionValue<string>(ip.GetValue("user", ""), dp, ip, false);
            if (string.IsNullOrEmpty(user))
                throw new Exception("no [user] given");

            // getting base attachment directory
            Node loadAttachmentDirectory = new Node("magix.data.load");
            loadAttachmentDirectory["id"].Value = idOfAttachmentDirectory;
            BypassExecuteActiveEvent(loadAttachmentDirectory, pars);
            string attachmentDirectory = loadAttachmentDirectory["value"]["directory"].Get<string>() + "/" + user;

            // checking to see if directory exist
            Node verifyDirectoryExist = new Node("magix.file.directory-exist");
            verifyDirectoryExist["directory"].Value = attachmentDirectory;
            BypassExecuteActiveEvent(verifyDirectoryExist, pars);

            if (!verifyDirectoryExist["value"].Get<bool>())
            {
                // need to create attachment directory for user
                Node createAttachmentDirectory = new Node("magix.file.create-directory", null);
                createAttachmentDirectory["directory"].Value = attachmentDirectory;
                BypassExecuteActiveEvent(createAttachmentDirectory, pars);
            }

            return attachmentDirectory + "/";
        }
    }
}


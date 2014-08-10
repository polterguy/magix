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
    internal class CommonHelper : ActiveController
	{
        /*
         * saves attachments locally and updates html of email to update links
         */
        protected static string SaveAttachmentsLocally(Node pars, ReadOnlyMailMessage idxEmail, string username, string bodyHtml)
        {
            if (idxEmail.Attachments.Count > 0)
            {
                Node attachmentDirectory = new Node("magix.data.load");
                attachmentDirectory["id"].Value = "magix.email.attachment-directory";
                BypassExecuteActiveEvent(attachmentDirectory, pars);
                string directory = attachmentDirectory["value"]["directory"].Get<string>();

                string directoryPath = directory.Trim('/') + "/" + username + "/";

                Node checkForDirectory = new Node("magix.file.directory-exist");
                checkForDirectory["directory"].Value = directoryPath;
                BypassExecuteActiveEvent(checkForDirectory, pars);

                if (!checkForDirectory["value"].Get<bool>())
                {
                    Node createDirectory = new Node("magix.file.create-directory");
                    createDirectory["directory"].Value = directoryPath;
                    BypassExecuteActiveEvent(createDirectory, pars);
                }

                Node ip = Ip(pars);

                foreach (Attachment idxAtt in idxEmail.Attachments)
                {
                    Node getBase = new Node();
                    RaiseActiveEvent(
                        "magix.file.get-base-path",
                        getBase);
                    string absolutePath = getBase["path"].Get<string>() + "/" + directoryPath + idxAtt.ContentId + "_" + idxAtt.Name;
                    string relativePath = directoryPath + idxAtt.ContentId + "_" + idxAtt.Name;

                    using (FileStream stream = File.Create(absolutePath))
                    {
                        idxAtt.ContentStream.Seek(0, SeekOrigin.Begin);
                        idxAtt.ContentStream.CopyTo(stream);
                        stream.Close();
                    }
                    bodyHtml = bodyHtml.Replace("cid:" + idxAtt.ContentId, relativePath);
                    ip["attachments"].Add("", relativePath);
                    ip["attachments"][ip["attachments"].Count - 1]["original-name"].Value = idxAtt.Name;
                }
            }

            return bodyHtml;
        }

        /*
         * removes everything not inside of <body></body>
         */
        protected static string RemoveHtmlBody(string html)
        {
            int indexOfBody = html.IndexOf("<body", StringComparison.OrdinalIgnoreCase);
            if (indexOfBody != -1)
            {
                int indexOfBodyStartTagEnd = html.IndexOf(">", indexOfBody);
                html = html.Substring(indexOfBodyStartTagEnd);
                int indexOfBodyEnd = html.IndexOf("</body>", StringComparison.OrdinalIgnoreCase);
                if (indexOfBodyEnd != -1)
                {
                    html = html.Substring(0, indexOfBodyEnd);
                }
            }
            return html;
        }
    }
}


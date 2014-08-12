/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using System.Text;

namespace BlackMail
{
    /*
     * wraps an email message
     */
    public class MailMessage
    {
        public MailMessage()
        {
            To = new List<string>();
            Cc = new List<string>();
            Bcc = new List<string>();
        }

        public string From { get; set; }

        public List<string> To { get; private set; }

        public List<string> Cc { get; private set; }

        public List<string> Bcc { get; private set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}

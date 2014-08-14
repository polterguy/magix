/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Text;
using System.Net.Mime;
using sys = System.Net.Mail;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;
using System.Net;

namespace BlackMail.smtp
{
    /*
     * class wrapping SmtpClient from .net
     */
    public class SmtpClient
    {
        private sys.SmtpClient _client;

        #region [ -- ctors -- ]

        public SmtpClient()
        {
            _client = new sys.SmtpClient();
        }

        public SmtpClient(string host)
        {
            _client = new sys.SmtpClient(host);
        }

        public SmtpClient(string host, int port)
        {
            _client = new sys.SmtpClient(host, port);
        }

        #endregion

        /*
         * enables SSL
         */
        public bool EnableSsl
        {
            get { return _client.EnableSsl; }
            set { _client.EnableSsl = value; }
        }

        /*
         * host credentials
         */
        public ICredentialsByHost Credentials
        {
            get { return _client.Credentials; }
            set { _client.Credentials = value; }
        }

        /*
         * sends a BlackMail email
         */
        public void Send(MailMessage msg)
        {
            sys.MailMessage msg2 = msg.ToMailMessage();
            try
            {
                _client.Send(msg2);
            }
            finally
            {
                msg2.Dispose();
            }
        }
    }
}

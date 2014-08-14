/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Text;
using sys = System.Net.Mail;
using System.Security.Cryptography.X509Certificates;

namespace BlackMail.smtp
{
    /*
     * wraps an email address
     */
    public class MailAddress
    {
        private sys.MailAddress _mailAddress;

        #region [ -- ctors -- ]

        public MailAddress(string address)
        {
            _mailAddress = new sys.MailAddress(address);
        }

        public MailAddress(string address, string displayName)
        {
            _mailAddress = new sys.MailAddress(address, displayName);
        }

        public MailAddress(string address, string displayName, Encoding displayNameEncoding)
        {
            _mailAddress = new sys.MailAddress(address, displayName, displayNameEncoding);
        }

        public MailAddress(
            string address, 
            string displayName, 
            Encoding displayNameEncoding, 
            X509Certificate2 signingCertificate)
            : this(address, displayName, displayNameEncoding)
        {
            if (signingCertificate != null && !signingCertificate.HasPrivateKey)
                throw new Exception("your signing certificate doesn't have a private key");

            SigningCertificate = signingCertificate;
        }

        public MailAddress(
            string address, 
            string displayName, 
            Encoding displayNameEncoding, 
            X509Certificate2 signingCertificate, 
            X509Certificate2 encryptionCertificate)
            : this(address, displayName, displayNameEncoding, signingCertificate)
        {
            EncryptionCertificate = encryptionCertificate;
        }

        #endregion

        /*
         * address of email
         */
        public string Address
        {
            get { return _mailAddress.Address; }
        }

        /*
         * display name of email
         */
        public string DisplayName
        {
            get { return _mailAddress.DisplayName; }
        }

        /* 
         * signing certificate of mail address
         */
        public X509Certificate2 SigningCertificate { get; set; }

        /* 
         * encryption certificate
         */
        public X509Certificate2 EncryptionCertificate { get; set; }

        /*
         * returns internal mailaddress
         */
        internal sys.MailAddress GetMailAddress()
        {
            return _mailAddress;
        }
    }
}

/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Text;
using System.Net.Mime;
using sys = System.Net.Mail;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Security.Cryptography.X509Certificates;

namespace BlackMail.smtp
{
    /*
     * class wrapping a secure mail message
     */
    public class MailMessage : IDisposable
    {
        private sys.MailMessage _message;

        #region [ -- ctors -- ]

        public MailMessage()
        {
            _message = new sys.MailMessage();
            Attachments = new Collection<Attachment>();
            To = new Collection<MailAddress>();
            Cc = new Collection<MailAddress>();
            Bcc = new Collection<MailAddress>();
        }

        public MailMessage(string from, string to)
            : this()
        {
            From = new MailAddress(from);
            To.Add(new MailAddress(to));
        }

        public MailMessage(MailAddress from, MailAddress to)
            : this()
        {
            From = from;
            To.Add(to);
        }

        public MailMessage(string from, string to, string subject, string body)
            : this()
        {
            From = new MailAddress(from);
            To.Add(new MailAddress(to));
            _message.Subject = subject;
            _message.Body = body;
        }

        #endregion

        /*
         * from
         */
        public MailAddress From { get; set; }

        /*
         * recipients collection
         */
        public Collection<MailAddress> To { get; private set; }

        /*
         * cc recipients collection
         */
        public Collection<MailAddress> Cc { get; private set; }

        /*
         * bcc recipients collection
         */
        public Collection<MailAddress> Bcc { get; private set; }

        /*
         * attachments
         */
        public Collection<Attachment> Attachments { get; private set; }

        /*
         * body of message
         */
        public string Body
        {
            get { return _message.Body; }
            set { _message.Body = value; }
        }

        /*
         * encoding
         */
        public Encoding BodyEncoding
        {
            get { return _message.BodyEncoding; }
            set { _message.BodyEncoding = value; }
        }

        /*
         * notification of delivery
         */
        public sys.DeliveryNotificationOptions DeliveryNotificationOptions
        {
            get { return _message.DeliveryNotificationOptions; }
            set { _message.DeliveryNotificationOptions = value; }
        }

        /*
         * headers of meil message
         */
        public NameValueCollection Headers
        {
            get { return _message.Headers; }
        }

        /*
         * true if body is html
         */
        public bool IsBodyHtml
        {
            get { return _message.IsBodyHtml; }
            set { _message.IsBodyHtml = value; }
        }

        /*
         * priority of message
         */
        public sys.MailPriority Priority
        {
            get { return _message.Priority; }
            set { _message.Priority = value; }
        }

        /*
         * reply-to address
         */
        public MailAddress ReplyTo { get; set; }

        /*
         * subject of email
         */
        public string Subject
        {
            get { return _message.Subject; }
            set { _message.Subject = value; }
        }

        /*
         * encoding of subject
         */
        public Encoding SubjectEncoding
        {
            get { return _message.SubjectEncoding; }
            set { _message.SubjectEncoding = value; }
        }

        /*
         * returns true if message is multipart
         */
        internal bool IsMultipart
        {
            get { return !IsEncrypted && (Attachments.Count > 0 || From.SigningCertificate != null); }
        }

        /*
         * true if message should be encrypted
         */
        public bool IsEncrypted { get; set; }

        /*
         * returns content signed and encrypted, if we should
         */
        private MessageContent GetCompleteContent()
        {
            MessageContent returnValue = GetUnsignedContent();

            if (From.SigningCertificate != null)
                returnValue = SignContent(returnValue);

            if (IsEncrypted)
                returnValue = EncryptContent(returnValue);

            return returnValue;
        }

        /*
         * returns message as system MailMessage
         */
        public sys.MailMessage ToMailMessage()
        {
            sys.MailMessage returnValue = new sys.MailMessage();
            if (From != null)
                returnValue.From = From.GetMailAddress();

            if (ReplyTo != null)
                returnValue.ReplyTo = ReplyTo.GetMailAddress();

            foreach (MailAddress toAddress in To)
            {
                returnValue.To.Add(toAddress.GetMailAddress());
            }

            foreach (MailAddress ccAddress in Cc)
            {
                returnValue.CC.Add(ccAddress.GetMailAddress());
            }

            foreach (MailAddress bccAddress in Bcc)
            {
                returnValue.Bcc.Add(bccAddress.GetMailAddress());
            }

            returnValue.DeliveryNotificationOptions = DeliveryNotificationOptions;

            foreach (string header in Headers)
            {
                returnValue.Headers.Add(header, Headers[header]);
            }

            returnValue.Priority = Priority;
            returnValue.Subject = Subject;
            returnValue.SubjectEncoding = SubjectEncoding;

            MessageContent content = GetCompleteContent();

            MemoryStream contentStream = new MemoryStream();
            if (this.IsMultipart)
            {
                byte[] mimeMessage = Encoding.ASCII.GetBytes("This is a multi-part message in MIME format.\r\n\r\n");
                contentStream.Write(mimeMessage, 0, mimeMessage.Length);
            }

            byte[] encodedBody;

            switch (content.TransferEncoding)
            {
                case TransferEncoding.SevenBit:
                    encodedBody = Encoding.ASCII.GetBytes(Regex.Replace(Encoding.ASCII.GetString(content.Body), "^\\.", "..", RegexOptions.Multiline));
                    break;
                default:
                    encodedBody = content.Body;
                    break;
            }

            contentStream.Write(encodedBody, 0, encodedBody.Length);

            contentStream.Position = 0;

            System.Net.Mail.AlternateView contentView = new System.Net.Mail.AlternateView(contentStream, content.ContentType);
            contentView.TransferEncoding = content.TransferEncoding;

            returnValue.AlternateViews.Add(contentView);

            return returnValue;
        }

        /*
         * returns unsigned content
         */
        private MessageContent GetUnsignedContent()
        {
            ContentType bodyContentType = new ContentType();
            bodyContentType.MediaType = this.IsBodyHtml ? "text/html" : "text/plain";
            bodyContentType.CharSet = BodyEncoding.BodyName;

            TransferEncoding bodyTransferEncoding;
            Encoding bodyEncoding = BodyEncoding ?? Encoding.ASCII;
            if (bodyEncoding == Encoding.ASCII || bodyEncoding == Encoding.UTF8)
                bodyTransferEncoding = TransferEncoding.QuotedPrintable;
            else
                bodyTransferEncoding = TransferEncoding.Base64;

            MessageContent bodyContent = new MessageContent(
                bodyEncoding.GetBytes(Body),
                bodyContentType,
                bodyTransferEncoding,
                IsMultipart || IsEncrypted);

            if (this.Attachments.Count == 0)
                return bodyContent;

            ContentType bodyWithAttachmentsContentType = new ContentType("multipart/mixed");
            bodyWithAttachmentsContentType.Boundary = "--BlackMail=" + Guid.NewGuid().ToString().Replace("-", "");

            StringBuilder message = new StringBuilder();
            message.Append("\r\n");
            message.Append("--");
            message.Append(bodyWithAttachmentsContentType.Boundary);
            message.Append("\r\n");
            message.Append("Content-Type: ");
            message.Append(bodyContent.ContentType.ToString());
            message.Append("\r\n");
            message.Append("Content-Transfer-Encoding: ");
            message.Append(MessageContent.GetTransferEncodingName(bodyContent.TransferEncoding));
            message.Append("\r\n\r\n");
            message.Append(Encoding.ASCII.GetString(bodyContent.Body));
            message.Append("\r\n");

            foreach (Attachment attachment in Attachments)
            {
                message.Append("--");
                message.Append(bodyWithAttachmentsContentType.Boundary);
                message.Append("\r\n");
                message.Append("Content-Type: ");
                message.Append(attachment.ContentType.ToString());
                message.Append("\r\n");
                message.Append("Content-Transfer-Encoding: base64\r\n\r\n");
                message.Append(Convert.ToBase64String(attachment.RawBytes, Base64FormattingOptions.InsertLineBreaks));
                message.Append("\r\n\r\n");
            }

            message.Append("--");
            message.Append(bodyWithAttachmentsContentType.Boundary);
            message.Append("--\r\n");

            return new MessageContent(
                Encoding.ASCII.GetBytes(message.ToString()), 
                bodyWithAttachmentsContentType, 
                TransferEncoding.SevenBit, 
                false);
        }

        /*
         * returns signed content
         */
        private MessageContent SignContent(MessageContent unsignedContent)
        {
            if (From.SigningCertificate == null)
                throw new Exception("cannot sign message unless it contains a signing certificate");

            ContentType signedContentType = new ContentType("multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=SHA1; ");
            signedContentType.Boundary = "--BlackMail_signed=" + Guid.NewGuid().ToString().Replace("-", "");

            StringBuilder fullUnsignedMessageBuilder = new StringBuilder();
            fullUnsignedMessageBuilder.Append("Content-Type: ");
            fullUnsignedMessageBuilder.Append(unsignedContent.ContentType.ToString());
            fullUnsignedMessageBuilder.Append("\r\n");
            fullUnsignedMessageBuilder.Append("Content-Transfer-Encoding: ");
            fullUnsignedMessageBuilder.Append(MessageContent.GetTransferEncodingName(unsignedContent.TransferEncoding));
            fullUnsignedMessageBuilder.Append("\r\n\r\n");
            fullUnsignedMessageBuilder.Append(Encoding.ASCII.GetString(unsignedContent.Body));

            string fullUnsignedMessage = fullUnsignedMessageBuilder.ToString();

            byte[] signature = EncryptionHelper.GetSignature(fullUnsignedMessage, From.SigningCertificate, From.EncryptionCertificate);

            StringBuilder signedMessageBuilder = new StringBuilder();

            signedMessageBuilder.Append("--");
            signedMessageBuilder.Append(signedContentType.Boundary);
            signedMessageBuilder.Append("\r\n");
            signedMessageBuilder.Append(fullUnsignedMessage);
            signedMessageBuilder.Append("\r\n");
            signedMessageBuilder.Append("--");
            signedMessageBuilder.Append(signedContentType.Boundary);
            signedMessageBuilder.Append("\r\n");
            signedMessageBuilder.Append("Content-Type: application/x-pkcs7-signature; name=\"smime.p7s\"\r\n");
            signedMessageBuilder.Append("Content-Transfer-Encoding: base64\r\n");
            signedMessageBuilder.Append("Content-Disposition: attachment; filename=\"smime.p7s\"\r\n\r\n");
            signedMessageBuilder.Append(Convert.ToBase64String(signature, Base64FormattingOptions.InsertLineBreaks));
            signedMessageBuilder.Append("\r\n\r\n");
            signedMessageBuilder.Append("--");
            signedMessageBuilder.Append(signedContentType.Boundary);
            signedMessageBuilder.Append("--\r\n");

            return new MessageContent(Encoding.ASCII.GetBytes(
                signedMessageBuilder.ToString()),
                signedContentType,
                TransferEncoding.SevenBit,
                false);
        }

        /*
         * returns encrypted content
         */
        private MessageContent EncryptContent(MessageContent unencryptedContent)
        {
            X509Certificate2Collection encryptionCertificates = new X509Certificate2Collection();

            if (From.EncryptionCertificate == null)
                throw new Exception("no encryption certificate was specified");
            else
                encryptionCertificates.Add(From.EncryptionCertificate);

            foreach (IEnumerable<MailAddress> addressList in new IEnumerable<MailAddress>[] { To, Cc, Bcc })
            {
                foreach (MailAddress address in addressList)
                {
                    if (address.EncryptionCertificate == null)
                        throw new Exception("one or more recipients did not have encryption certificates specified");
                    else
                        encryptionCertificates.Add(address.EncryptionCertificate);
                }
            }

            ContentType encryptedContentType = new ContentType("application/x-pkcs7-mime; smime-type=enveloped-data; name=\"smime.p7m\"");

            StringBuilder fullUnencryptedMessageBuilder = new StringBuilder();
            fullUnencryptedMessageBuilder.Append("Content-Type: ");
            fullUnencryptedMessageBuilder.Append(unencryptedContent.ContentType.ToString());
            fullUnencryptedMessageBuilder.Append("\r\n");
            fullUnencryptedMessageBuilder.Append("Content-Transfer-Encoding: ");
            fullUnencryptedMessageBuilder.Append(MessageContent.GetTransferEncodingName(unencryptedContent.TransferEncoding));
            fullUnencryptedMessageBuilder.Append("\r\n\r\n");
            fullUnencryptedMessageBuilder.Append(Encoding.ASCII.GetString(unencryptedContent.Body));

            string fullUnencryptedMessage = fullUnencryptedMessageBuilder.ToString();

            byte[] encryptedBytes = EncryptionHelper.EncryptMessage(fullUnencryptedMessage, encryptionCertificates);

            return new MessageContent(encryptedBytes,
                encryptedContentType,
                TransferEncoding.Base64,
                false);
        }

        #region [ -- dispose pattern -- ]

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                if (_message != null)
                    _message.Dispose();
        }

        #endregion
    }
}

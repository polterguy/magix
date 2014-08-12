/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 * 
 * This file is based upon the work done by Bert Johnson in OpaqueMail at 
 * http://opaquemail.org/
 * 
 * Parts of the file is hence Copyright © Bert Johnson (http://bertjohnson.net/) 
 * of Bkip Inc. (http://bkip.com/)
 * 
 * The library OpaqueMail was forked to fix bugs, and to make sure it was 
 * compatible with older frameworks than 4.5
 * 
 * OpaqueMail is licensed as MIT License (http://mit-license.org/).
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OpaqueMail.Net
{
    public class SmtpClient : System.Net.Mail.SmtpClient
    {
        private byte[] _buffer = new byte[Constants.HUGEBUFFERSIZE];
        private Dictionary<string, X509Certificate2> _smimeCertificateCache = new Dictionary<string, X509Certificate2>();

        protected string _smimeAlternativeViewBoundaryName = "OpaqueMail-alternative-boundary";
        protected string _smimeBoundaryName = "OpaqueMail-boundary";
        protected string _smimeSignedCmsBoundaryName = "OpaqueMail-signature-boundary";
        protected string _smimeTripleSignedCmsBoundaryName = "OpaqueMail-triple-signature-boundary";

        public AlgorithmIdentifier SmimeAlgorithmIdentifier = new AlgorithmIdentifier(new Oid("2.16.840.1.101.3.4.1.42"));
        public X509Certificate2Collection SmimeValidCertificates = null;

        public SmtpClient()
            : base()
        {
            RandomizeBoundaryNames();
        }

        public SmtpClient(string host)
            : base(host)
        {
            RandomizeBoundaryNames();
        }

        public SmtpClient(string host, int port)
            : base(host, port)
        {
            RandomizeBoundaryNames();
        }

        public void Send(MailMessage message)
        {
            // If not performing any S/MIME encryption or signing, use the default System.Net.Mail.SmtpClient Send() method.
            if (!message.SmimeSigned && !message.SmimeEncryptedEnvelope && !message.SmimeTripleWrapped)
                base.Send(message);
            else
                SmimeSend(message);
        }

        public bool SmimeVerifyAllRecipientsHavePublicKeys(MailMessage message)
        {
            // Prepare recipient keys if this message will be encrypted.
            Dictionary<string, MailAddress> addressesNeedingPublicKeys;
            HashSet<string> addressesWithPublicKeys;
            ResolvePublicKeys(message, out addressesWithPublicKeys, out addressesNeedingPublicKeys);

            return addressesNeedingPublicKeys.Count < 1;
        }
        
        #region [ -- private methods -- ]

        private void RandomizeBoundaryNames()
        {
            string boundaryRandomness = "";
            Random randomGenerator = new Random();

            // Append 10 random characters.
            for (int i = 0; i < 10; i++)
            {
                int nextCharacter = randomGenerator.Next(1, 36);
                if (nextCharacter > 26)
                    boundaryRandomness += (char)(47 + nextCharacter);
                else
                    boundaryRandomness += (char)(64 + nextCharacter);
            }

            _smimeAlternativeViewBoundaryName += "-" + boundaryRandomness;
            _smimeBoundaryName += "-" + boundaryRandomness;
            _smimeSignedCmsBoundaryName += "-" + boundaryRandomness;
            _smimeTripleSignedCmsBoundaryName += "-" + boundaryRandomness;
        }

        private void ResolvePublicKeys(MailMessage message,
            out HashSet<string> addressesWithPublicKeys,
            out Dictionary<string, MailAddress> addressesNeedingPublicKeys)
        {
            // Initialize collections for all recipients.
            addressesWithPublicKeys = new HashSet<string>();
            addressesNeedingPublicKeys = new Dictionary<string, MailAddress>();

            MailAddressCollection[] addressRanges = new MailAddressCollection[] { message.To, message.CC, message.Bcc };
            foreach (MailAddressCollection addressRange in addressRanges)
            {
                foreach (MailAddress toAddress in addressRange)
                {
                    string canonicalToAddress = toAddress.Address.ToUpper();
                    if (_smimeCertificateCache.ContainsKey(canonicalToAddress))
                    {
                        if (!addressesWithPublicKeys.Contains(canonicalToAddress))
                            addressesWithPublicKeys.Add(canonicalToAddress);
                    }
                    else
                    {
                        if (!addressesNeedingPublicKeys.ContainsKey(canonicalToAddress))
                            addressesNeedingPublicKeys.Add(canonicalToAddress, toAddress);
                    }
                }
            }

            // If any addresses haven't been mapped to public keys, map them.
            if (addressesNeedingPublicKeys.Count > 0)
            {
                // Read from the Windows certificate store if valid certificates aren't specified.
                if (SmimeValidCertificates == null || SmimeValidCertificates.Count < 1)
                {
                    // Load from the current user.
                    X509Store store = new X509Store(StoreLocation.CurrentUser);
                    store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                    SmimeValidCertificates = store.Certificates;
                    store.Close();

                    // Add any tied to the local machine.
                    store = new X509Store(StoreLocation.LocalMachine);
                    store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
                    SmimeValidCertificates.AddRange(store.Certificates);
                    store.Close();
                }

                // Loop through certificates and check for matching recipients.
                foreach (X509Certificate2 cert in SmimeValidCertificates)
                {
                    // Look at certificates with e-mail subject names.
                    string canonicalCertSubject = "";
                    if (cert.Subject.StartsWith("E="))
                        canonicalCertSubject = cert.Subject.Substring(2).ToUpper();
                    else if (cert.Subject.StartsWith("CN="))
                        canonicalCertSubject = cert.Subject.Substring(3).ToUpper();
                    else
                        canonicalCertSubject = cert.Subject.ToUpper();

                    int certSubjectComma = canonicalCertSubject.IndexOf(",");
                    if (certSubjectComma > -1)
                        canonicalCertSubject = canonicalCertSubject.Substring(0, certSubjectComma);

                    // Only proceed if the key is for a recipient of this e-mail.
                    if (!addressesNeedingPublicKeys.ContainsKey(canonicalCertSubject))
                        continue;

                    // Verify the certificate chain.
                    if ((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireCertificateVerification) > 0)
                    {
                        if (!cert.Verify())
                            continue;
                    }

                    // Ensure valid key usage scenarios.
                    if ((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireKeyUsageOfDataEncipherment) > 0 ||
                        (message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireEnhancedKeyUsageofSecureEmail) > 0)
                    {
                        bool keyDataEncipherment = false, enhancedKeySecureEmail = false;
                        foreach (X509Extension extension in cert.Extensions)
                        {
                            if (!keyDataEncipherment && extension.Oid.FriendlyName == "Key Usage")
                            {
                                X509KeyUsageExtension ext = (X509KeyUsageExtension)extension;
                                if ((ext.KeyUsages & X509KeyUsageFlags.DataEncipherment) != X509KeyUsageFlags.None)
                                {
                                    keyDataEncipherment = true;

                                    if (!((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireEnhancedKeyUsageofSecureEmail) > 0))
                                        break;
                                }
                            }
                            if (!enhancedKeySecureEmail && extension.Oid.FriendlyName == "Enhanced Key Usage")
                            {
                                X509EnhancedKeyUsageExtension ext = (X509EnhancedKeyUsageExtension)extension;
                                OidCollection oids = ext.EnhancedKeyUsages;
                                foreach (Oid oid in oids)
                                {
                                    if (oid.FriendlyName == "Secure Email")
                                    {
                                        enhancedKeySecureEmail = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if ((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireKeyUsageOfDataEncipherment) > 0 && !keyDataEncipherment)
                            continue;
                        if ((message.SmimeEncryptionOptionFlags & SmimeEncryptionOptionFlags.RequireEnhancedKeyUsageofSecureEmail) > 0 && !enhancedKeySecureEmail)
                            continue;
                    }

                    // If we've made it this far, we can use the certificate for a recipient.
                    MailAddress originalAddress = addressesNeedingPublicKeys[canonicalCertSubject];
                    _smimeCertificateCache.Add(canonicalCertSubject, cert);
                    addressesWithPublicKeys.Add(canonicalCertSubject);
                    addressesNeedingPublicKeys.Remove(canonicalCertSubject);

                    // Shortcut to abort processing of additional certificates if all recipients are accounted for.
                    if (addressesNeedingPublicKeys.Count < 1)
                        break;
                }
            }
        }

        private void SmimeSend(MailMessage message)
        {
            if (message.To.Count + message.CC.Count + message.Bcc.Count < 1)
                throw new SmtpException("One or more recipients must be specified via the '.To', '.CC', or '.Bcc' collections.");

            if ((message.SmimeSigned || message.SmimeTripleWrapped) && message.SmimeSigningCertificate == null)
                throw new SmtpException("A signing certificate must be passed prior to signing.");

            // Ensure the rendering engine expects MIME encoding.
            message.Headers["MIME-Version"] = "1.0";

            // Generate a multipart/mixed message containing the e-mail's body, alternate views, and attachments.
            byte[] mimeBytes = message.MIMEEncode(_smimeBoundaryName, _smimeAlternativeViewBoundaryName);
            message.Headers["Content-Type"] = "multipart/mixed; boundary=\"" + _smimeBoundaryName + "\"";
            message.Headers["Content-Transfer-Encoding"] = "7bit";

            // Skip the MIME header.
            string defaultBodyContent = Encoding.UTF8.GetString(mimeBytes);
            message.Body = defaultBodyContent.Substring(defaultBodyContent.IndexOf("\r\n\r\n") + 4);

            bool successfullySigned = SmimeSignMessage(message, ref mimeBytes);
            bool successfullyEncrypted = SmimeEncryptEnvelope(message, ref mimeBytes, successfullySigned);

            if (successfullyEncrypted)
                SmimeTripleWrap(message, mimeBytes);

            SmimeSendRaw(message);
        }

        private bool SmimeSignMessage(MailMessage message, ref byte[] mimeBytes)
        {
            bool successfullySigned = false;
            if (message.SmimeSigned || message.SmimeTripleWrapped)
            {
                int unsignedSize = mimeBytes.Length;
                mimeBytes = SmimeSign(_buffer, mimeBytes, message, false);
                successfullySigned = mimeBytes.Length != unsignedSize;

                if (successfullySigned)
                {
                    // Remove any prior content dispositions.
                    if (message.Headers["Content-Disposition"] != null)
                        message.Headers.Remove("Content-Disposition");

                    message.Headers["Content-Type"] = 
                        "multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1;\r\n\tboundary=\"" + _smimeSignedCmsBoundaryName + "\"";
                    message.Headers["Content-Transfer-Encoding"] = "7bit";
                    message.Body = Encoding.UTF8.GetString(mimeBytes);
                }
            }
            return successfullySigned;
        }

        private bool SmimeEncryptEnvelope(MailMessage message, ref byte[] mimeBytes, bool successfullySigned)
        {
            bool successfullyEncrypted = false;
            if (message.SmimeEncryptedEnvelope || message.SmimeTripleWrapped)
            {
                int unencryptedSize = mimeBytes.Length;
                mimeBytes = SmimeEncryptEnvelope(mimeBytes, message, successfullySigned);
                successfullyEncrypted = mimeBytes.Length != unencryptedSize;

                // If the message won't be triple-wrapped, wrap the encrypted message with MIME.
                if (successfullyEncrypted && (!successfullySigned || !message.SmimeTripleWrapped))
                {
                    message.Headers["Content-Type"] = "application/pkcs7-mime; name=smime.p7m;\r\n\tsmime-type=enveloped-data";
                    message.Headers["Content-Transfer-Encoding"] = "base64";

                    message.Body = Functions.ToBase64String(mimeBytes) + "\r\n";
                }
            }
            return successfullyEncrypted;
        }

        private void SmimeTripleWrap(MailMessage message, byte[] mimeBytes)
        {
            if (message.SmimeTripleWrapped)
            {
                message.Headers["Content-Type"] = "multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1;\r\n\tboundary=\"" + _smimeTripleSignedCmsBoundaryName + "\"";
                message.Headers["Content-Transfer-Encoding"] = "7bit";

                message.Body = Encoding.UTF8.GetString(SmimeSign(_buffer, mimeBytes, message, true));
            }
            else
                message.Headers["Content-Disposition"] = "attachment; filename=smime.p7m";
        }

        private void SmimeSendRaw(MailMessage msg)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.Connect(Host, Port);
                using (Stream smtpStream = tcpClient.GetStream())
                {
                    using (StreamReader reader = new StreamReader(smtpStream))
                    {
                        using (StreamWriter writer = new StreamWriter(smtpStream))
                        {
                            writer.AutoFlush = true;
                            SendEHLO(reader, writer);
                            if (EnableSsl)
                                SMimeSendRawSSL(msg, smtpStream, reader, writer);
                            else
                                SmimeSendRaw(msg, reader, writer);
                        }
                    }
                }
            }
        }

        private void SendEHLO(StreamReader reader, StreamWriter writer)
        {
            string response = reader.ReadLine();
            writer.WriteLine("EHLO " + Host);
            char[] charBuffer = new char[Constants.SMALLBUFFERSIZE];
            int bytesRead = reader.Read(charBuffer, 0, Constants.SMALLBUFFERSIZE);
            response = new string(charBuffer, 0, bytesRead);
            if (!response.StartsWith("2"))
                throw new SmtpException("Unable to connect to remote server '" + Host + "'.  Sent 'EHLO' and received '" + response + "'.");
        }

        private void SMimeSendRawSSL(MailMessage msg, Stream smtpStream, StreamReader reader, StreamWriter writer)
        {
            writer.WriteLine("STARTTLS");
            string response = reader.ReadLine();
            if (!response.StartsWith("2"))
                throw new SmtpException("Unable to start TLS/SSL protection with '" + Host + "'.  Received '" + response + "'.");

            using (SslStream sslStream = new SslStream(smtpStream))
            {
                sslStream.AuthenticateAsClient(Host);

                using (StreamReader sslReader = new StreamReader(sslStream))
                {
                    using (StreamWriter sslWriter = new StreamWriter(sslStream))
                    {
                        sslWriter.AutoFlush = true;
                        SmimeSendRaw(msg, sslReader, sslWriter);
                    }
                }
            }
        }

        private void SmimeSendRaw(MailMessage message, StreamReader reader, StreamWriter writer)
        {
            if (Credentials != null)
                AuthenticateCredentials(reader, writer);

            StringBuilder rawHeaders = new StringBuilder();
            WriteFrom(message, reader, writer, rawHeaders);
            WriteRecipients(message, reader, writer, rawHeaders);
            WriteCarbonCopy(message, reader, writer, rawHeaders);
            WriteBlindCarbonCopy(message, reader, writer);
            WriteRawMessage(message, reader, writer, rawHeaders);

            writer.WriteLine("QUIT");
        }

        private void WriteRawMessage(MailMessage message, StreamReader reader, StreamWriter writer, StringBuilder rawHeaders)
        {
            // Send the raw message.
            writer.WriteLine("DATA");
            string response = reader.ReadLine();
            if (!response.StartsWith("3"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'DATA' and received '" + response + "'.");

            // If a read-only mail message is passed in with its raw headers and body, save a few steps by sending that directly.
            if (message is ReadOnlyMailMessage)
                writer.Write(((ReadOnlyMailMessage)message).RawHeaders + "\r\n" + ((ReadOnlyMailMessage)message).RawBody + "\r\n.\r\n");
            else
            {
                rawHeaders.Append(Functions.SpanHeaderLines("Subject: " + Functions.EncodeMailHeader(message.Subject)) + "\r\n");
                foreach (string rawHeader in message.Headers)
                {
                    switch (rawHeader.ToUpper())
                    {
                        case "BCC":
                        case "CC":
                        case "FROM":
                        case "SUBJECT":
                        case "TO":
                            break;
                        default:
                            rawHeaders.Append(Functions.SpanHeaderLines(rawHeader + ": " + message.Headers[rawHeader]) + "\r\n");
                            break;
                    }
                }
                writer.Write(rawHeaders.ToString() + "\r\n" + message.Body + "\r\n.\r\n");
            }

            response = reader.ReadLine();
            if (!response.StartsWith("2"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent message and received '" + response + "'.");
        }

        private void WriteBlindCarbonCopy(MailMessage message, StreamReader reader, StreamWriter writer)
        {
            foreach (MailAddress address in message.Bcc)
            {
                writer.WriteLine("RCPT TO:<" + address.Address + ">");
                string response = reader.ReadLine();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }
        }

        private void WriteCarbonCopy(MailMessage message, StreamReader reader, StreamWriter writer, StringBuilder rawHeaders)
        {
            if (message.CC.Count > 0)
                rawHeaders.Append(Functions.SpanHeaderLines("CC: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.CC))) + "\r\n");
            foreach (MailAddress address in message.CC)
            {
                writer.WriteLine("RCPT TO:<" + address.Address + ">");
                string response = reader.ReadLine();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }
        }

        private void WriteRecipients(MailMessage message, StreamReader reader, StreamWriter writer, StringBuilder rawHeaders)
        {
            if (message.To.Count > 0)
                rawHeaders.Append(Functions.SpanHeaderLines("To: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.To))) + "\r\n");
            foreach (MailAddress address in message.To)
            {
                writer.WriteLine("RCPT TO:<" + address.Address + ">");
                string response = reader.ReadLine();
                if (!response.StartsWith("2"))
                    throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'RCPT TO' and received '" + response + "'.");
            }
        }

        private void WriteFrom(MailMessage message, StreamReader reader, StreamWriter writer, StringBuilder rawHeaders)
        {
            string response;
            rawHeaders.Append(Functions.SpanHeaderLines("From: " + Functions.EncodeMailHeader(Functions.ToMailAddressString(message.From))) + "\r\n");
            writer.WriteLine("MAIL FROM:<" + message.From.Address + ">");
            response = reader.ReadLine();
            if (!response.StartsWith("2"))
                throw new SmtpException("Exception communicating with server '" + Host + "'.  Sent 'MAIL FROM' and received '" + response + "'.");
        }

        private void AuthenticateCredentials(StreamReader reader, StreamWriter writer)
        {
            string response;
            NetworkCredential cred = (NetworkCredential)Credentials;
            writer.WriteLine("AUTH LOGIN");
            response = reader.ReadLine();
            if (!response.StartsWith("3"))
                throw new SmtpException("Unable to authenticate with server '" + Host + "'.  Received '" + response + "'.");
            writer.WriteLine(Functions.ToBase64String(cred.UserName));
            response = reader.ReadLine();
            writer.WriteLine(Functions.ToBase64String(cred.Password));
            response = reader.ReadLine();
            if (!response.StartsWith("2"))
                throw new SmtpException("Unable to authenticate with server '" + Host + "'.  Received '" + response + "'.");
        }

        private byte[] SmimeEncryptEnvelope(byte[] contentBytes, MailMessage message, bool alreadySigned)
        {
            // Resolve recipient public keys.
            Dictionary<string, MailAddress> addressesNeedingPublicKeys;
            HashSet<string> addressesWithPublicKeys;
            ResolvePublicKeys(message, out addressesWithPublicKeys, out addressesNeedingPublicKeys);

            // Throw an error if we're unable to encrypt the message for one or more recipients and encryption is explicitly required.
            if (addressesNeedingPublicKeys.Count > 0)
            {
                // If the implementation requires S/MIME encryption (the default), throw an error if there's no certificate.
                if ((message.SmimeSettingsMode & SmimeSettingsMode.RequireExactSettings) > 0)
                {
                    StringBuilder exceptionMessage = new StringBuilder(Constants.TINYSBSIZE);
                    exceptionMessage.Append("Trying to send encrypted message to one or more recipients without a trusted public key.\r\nRecipients without public keys: ");
                    foreach (string addressNeedingPublicKey in addressesNeedingPublicKeys.Keys)
                        exceptionMessage.Append(addressNeedingPublicKey + ", ");
                    exceptionMessage.Remove(exceptionMessage.Length - 2, 2);

                    throw new SmtpException(exceptionMessage.ToString());
                }
                else
                    return contentBytes;
            }

            if (alreadySigned)
            {
                // If already signed, prepend S/MIME headers.
                StringBuilder contentBuilder = new StringBuilder(Constants.TINYSBSIZE);
                contentBuilder.Append("Content-Type: multipart/signed; protocol=\"application/x-pkcs7-signature\"; micalg=sha1;\r\n\tboundary=\"" + 
                    _smimeSignedCmsBoundaryName + "\"\r\n");
                contentBuilder.Append("Content-Transfer-Encoding: 7bit\r\n\r\n");

                contentBytes = Encoding.UTF8.GetBytes(contentBuilder.ToString() + Encoding.UTF8.GetString(contentBytes));
            }

            // Prepare the encryption envelope.
            ContentInfo contentInfo = new ContentInfo(contentBytes);
            EnvelopedCms envelope;

            // If a specific algorithm is specified, choose that.  Otherwise, negotiate which algorithm to use.
            if (SmimeAlgorithmIdentifier != null)
                envelope = new EnvelopedCms(contentInfo, SmimeAlgorithmIdentifier);
            else
                envelope = new EnvelopedCms(contentInfo);

            // Encrypt the symmetric session key using each recipient's public key.
            foreach (string addressWithPublicKey in addressesWithPublicKeys)
            {
                CmsRecipient recipient = new CmsRecipient(_smimeCertificateCache[addressWithPublicKey]);
                envelope.Encrypt(recipient);
            }

            return envelope.Encode();
        }

        private byte[] SmimeSign(byte[] buffer, byte[] contentBytes, MailMessage message, bool alreadyEncrypted)
        {
            if (message.SmimeSigningCertificate == null)
            {
                // If the implementation requires S/MIME signing (the default), throw an error if there's no certificate.
                if ((message.SmimeSettingsMode & SmimeSettingsMode.RequireExactSettings) > 0)
                    throw new SmtpException("Trying to send a signed message, but no signing certificate has been assigned.");
                else
                    return contentBytes;
            }

            // First, create a buffer for tracking the unsigned portion of this message.
            StringBuilder unsignedMessageBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            // If triple wrapping, the previous layer was an encrypted envelope and needs to be Base64 encoded.
            if (alreadyEncrypted)
            {
                unsignedMessageBuilder.Append("Content-Type: application/pkcs7-mime; smime-type=enveloped-data;\r\n\tname=\"smime.p7m\"\r\n");
                unsignedMessageBuilder.Append("Content-Transfer-Encoding: base64\r\n");
                unsignedMessageBuilder.Append("Content-Description: \"S/MIME Cryptographic envelopedCms\"\r\n");
                unsignedMessageBuilder.Append("Content-Disposition: attachment; filename=\"smime.p7m\"\r\n\r\n");

                unsignedMessageBuilder.Append(Functions.ToBase64String(contentBytes));
            }
            else
                unsignedMessageBuilder.Append(Encoding.UTF8.GetString(contentBytes));

            // Prepare the signing parameters.
            ContentInfo contentInfo = new ContentInfo(Encoding.UTF8.GetBytes(unsignedMessageBuilder.ToString()));
            SignedCms signedCms = new SignedCms(contentInfo, true);

            CmsSigner signer = new CmsSigner(message.SubjectIdentifierType, message.SmimeSigningCertificate);
            signer.IncludeOption = X509IncludeOption.WholeChain;

            // Sign the current time.
            if ((message.SmimeSigningOptionFlags & SmimeSigningOptionFlags.SignTime) > 0)
            {
                Pkcs9SigningTime signingTime = new Pkcs9SigningTime();
                signer.SignedAttributes.Add(signingTime);
            }

            // Encode the signed message.
            signedCms.ComputeSignature(signer);
            byte[] signedBytes = signedCms.Encode();

            // Embed the signed and original version of the message using MIME.
            StringBuilder messageBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            // Build the MIME message by embedding the unsigned and signed portions.
            messageBuilder.Append("This is a multi-part S/MIME signed message.\r\n\r\n");
            messageBuilder.Append("--" + (alreadyEncrypted ? _smimeTripleSignedCmsBoundaryName : _smimeSignedCmsBoundaryName) + "\r\n");
            messageBuilder.Append(unsignedMessageBuilder.ToString());
            messageBuilder.Append("\r\n--" + (alreadyEncrypted ? _smimeTripleSignedCmsBoundaryName : _smimeSignedCmsBoundaryName) + "\r\n");
            messageBuilder.Append("Content-Type: application/x-pkcs7-signature; smime-type=signed-data; name=\"smime.p7s\"\r\n");
            messageBuilder.Append("Content-Transfer-Encoding: base64\r\n");
            messageBuilder.Append("Content-Description: \"S/MIME Cryptographic signedCms\"\r\n");
            messageBuilder.Append("Content-Disposition: attachment; filename=\"smime.p7s\"\r\n\r\n");
            messageBuilder.Append(Functions.ToBase64String(signedBytes, 0, signedBytes.Length));
            messageBuilder.Append("\r\n--" + (alreadyEncrypted ? _smimeTripleSignedCmsBoundaryName : _smimeSignedCmsBoundaryName) + "--\r\n");

            return Encoding.UTF8.GetBytes(messageBuilder.ToString());
        }

        #endregion
    }
}

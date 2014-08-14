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
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

namespace BlackMail.smtp
{
    public class EncryptionHelper
    {
        /*
         * returns signed message
         */
        internal static byte[] GetSignature(string message, X509Certificate2 signingCertificate, X509Certificate2 encryptionCertificate)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            SignedCms signedCms = new SignedCms(new ContentInfo(messageBytes), true);

            CmsSigner cmsSigner = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, signingCertificate);
            cmsSigner.IncludeOption = X509IncludeOption.WholeChain;

            if (encryptionCertificate != null)
                cmsSigner.Certificates.Add(encryptionCertificate);

            Pkcs9SigningTime signingTime = new Pkcs9SigningTime();
            cmsSigner.SignedAttributes.Add(signingTime);

            signedCms.ComputeSignature(cmsSigner, false);

            return signedCms.Encode();
        }

        /*
         * returns encrypted message
         */
        internal static byte[] EncryptMessage(string message, X509Certificate2Collection encryptionCertificates)
        {
            byte[] messageBytes = Encoding.ASCII.GetBytes(message);

            EnvelopedCms envelopedCms = new EnvelopedCms(new ContentInfo(messageBytes));

            CmsRecipientCollection recipients = new CmsRecipientCollection(SubjectIdentifierType.IssuerAndSerialNumber, encryptionCertificates);

            envelopedCms.Encrypt(recipients);

            return envelopedCms.Encode();
        }
    }
}

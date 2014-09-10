/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Collections.Generic;
using sys = System.Security.Cryptography.X509Certificates;
using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Magix.Core;

namespace Magix.cryptography
{
	/*
	 * signs emails
	 */
    internal class CryptographyHelper
	{
        /*
         * returns true if email address can sign email
         */
        public static bool CanSign(string email)
        {
            try
            {
                MailboxAddress signer = new MailboxAddress("", email);
                TextPart entity = new TextPart("text");
                using (WindowsSecureMimeContext ctx = new WindowsSecureMimeContext(sys.StoreLocation.CurrentUser))
                {
                    MultipartSigned.Create(ctx, signer, DigestAlgorithm.Sha1, entity);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /*
         * returns true if email can be encrypted
         */
        public static string CanEncrypt(Node emails)
        {
            try
            {
                List<MailboxAddress> list = new List<MailboxAddress>();
                foreach (Node idx in emails)
                    list.Add(new MailboxAddress("", idx.Get<string>()));
                TextPart entity = new TextPart("text");
                using (WindowsSecureMimeContext ctx = new WindowsSecureMimeContext(sys.StoreLocation.CurrentUser))
                {
                    ApplicationPkcs7Mime.Encrypt(ctx, list, entity);
                }
                return null;
            }
            catch(Exception err)
            {
                return err.Message;
            }
        }

        /*
         * tries to sign a MimeEntity
         */
        public static MimeEntity SignEntity(MimeEntity entity, MailboxAddress signer)
        {
            using (WindowsSecureMimeContext ctx = new WindowsSecureMimeContext(sys.StoreLocation.CurrentUser))
            {
                return MultipartSigned.Create(ctx, signer, DigestAlgorithm.Sha1, entity);
            }
        }

        /*
         * tries to encrypt a MimeEntity
         */
        public static MimeEntity EncryptEntity(MimeEntity entity, IEnumerable<MailboxAddress> list)
        {
            using (WindowsSecureMimeContext ctx = new WindowsSecureMimeContext(sys.StoreLocation.CurrentUser))
            {
                return ApplicationPkcs7Mime.Encrypt(ctx, list, entity);
            }
        }

        /*
         * tries to sign and encrypt a MimeEntity
         */
        public static MimeEntity SignAndEncryptEntity(MimeEntity entity, MailboxAddress signer, IEnumerable<MailboxAddress> list)
        {
            using (WindowsSecureMimeContext ctx = new WindowsSecureMimeContext(sys.StoreLocation.CurrentUser))
            {
                return ApplicationPkcs7Mime.SignAndEncrypt(ctx, signer, DigestAlgorithm.Sha1, list, entity);
            }
        }

        /*
         * imports the given certificate file into certificate database
         */
        public static void ImportCertificate(string certificateFile, string password)
        {
            Node getBase = new Node();
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(CryptographyHelper),
                "magix.file.get-base-path",
                getBase);
            string basePath = getBase["path"].Get<string>();

            using (WindowsSecureMimeContext ctx = new WindowsSecureMimeContext(sys.StoreLocation.CurrentUser))
            {
                using (FileStream stream = File.OpenRead(basePath + certificateFile))
                {
                    if (!string.IsNullOrEmpty(password))
                        ctx.Import(stream, password);
                    else
                        ctx.Import(stream);
                }
            }
        }

        /*
         * removes all certificates matching subjectName from database
         */
        internal static void RemoveCertificates(string subjectName)
        {
            foreach (sys.StoreLocation idxStore in new sys.StoreLocation[] { sys.StoreLocation.CurrentUser })
            {
                sys.X509Store store = new sys.X509Store(idxStore);
                store.Open(sys.OpenFlags.ReadOnly);
                try
                {
                    sys.X509Certificate2Collection coll = store.Certificates.Find(sys.X509FindType.FindBySubjectName, subjectName, false);
                    if (coll.Count > 0)
                        store.RemoveRange(coll);
                }
                finally
                {
                    store.Close();
                }
            }
        }
    }
}

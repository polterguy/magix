/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
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
            foreach (sys.StoreLocation idxStore in new sys.StoreLocation[] { sys.StoreLocation.CurrentUser, sys.StoreLocation.LocalMachine })
            {
                sys.X509Store store = new sys.X509Store(idxStore);
                store.Open(sys.OpenFlags.ReadOnly);
                try
                {
                    sys.X509Certificate2Collection coll = store.Certificates.Find(sys.X509FindType.FindBySubjectName, email, true);
                    if (coll.Count > 0)
                    {
                        foreach (sys.X509Certificate2 idxCert in coll)
                        {
                            if (idxCert.HasPrivateKey)
                                return true;
                        }
                    }
                }
                finally
                {
                    store.Close();
                }
            }
            return false;
        }

        /*
         * returns true if email can be encrypted
         */
        public static string CanEncrypt(Node emails)
        {
            string retVal = null;
            sys.X509Store currentUserStore = new sys.X509Store(sys.StoreLocation.CurrentUser);
            sys.X509Store localMachineStore = new sys.X509Store(sys.StoreLocation.LocalMachine);
            currentUserStore.Open(sys.OpenFlags.ReadOnly);
            localMachineStore.Open(sys.OpenFlags.ReadOnly);
            try
            {
                foreach (Node idxEmail in emails)
                {
                    sys.X509Certificate2Collection coll = currentUserStore.Certificates.Find(sys.X509FindType.FindBySubjectName, idxEmail.Value, true);
                    if (coll.Count == 0)
                    {
                        coll = localMachineStore.Certificates.Find(sys.X509FindType.FindBySubjectName, idxEmail.Value, true);
                        if (coll.Count == 0)
                            retVal += "email '" + idxEmail.Value + "' does not have a valid certificate.&nbsp;&nbsp;";
                    }
                }
                return retVal;
            }
            finally
            {
                currentUserStore.Close();
                localMachineStore.Close();
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
    }
}


/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Collections.Generic;
using sys = System.Security.Cryptography.X509Certificates;
using sys2 = System.Security.Cryptography;
using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto;
using Magix.Core;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.Crypto.Parameters;

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
            if (emails.Count == 0)
                return "there are no recipients";
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

        /*
         * creates a certificate and a private key and installs into windows registry
         */
        public static void CreateCertificateKey(
            string subjectName, 
            string issuerName, 
            string signatureAlgorithm, 
            int strength, 
            DateTime begin, 
            DateTime end,
            string subjectCountryCode,
            string subjectOrganization,
            string subjectTitle,
            string issuerCountryCode,
            string issuerOrganization,
            string issuerTitle,
            string subjectCommonName,
            string issuerCommonName)
        {
            RsaKeyPairGenerator keyPairGenerator = new RsaKeyPairGenerator();
            SecureRandom secureRandom = new SecureRandom(new CryptoApiRandomGenerator());
            keyPairGenerator.Init(new KeyGenerationParameters(secureRandom, strength));
            AsymmetricCipherKeyPair asCipherKeyPair = keyPairGenerator.GenerateKeyPair();
            AsymmetricKeyParameter publicKey = asCipherKeyPair.Public;
            RsaPrivateCrtKeyParameters privateKey = (RsaPrivateCrtKeyParameters)asCipherKeyPair.Private;

            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), secureRandom);
            certificateGenerator.SetSerialNumber(serialNumber);

            string subjectNameString = "E=" + subjectName;
            if (subjectCommonName != null)
                subjectNameString += ", CN=" + subjectCommonName;
            if (subjectCountryCode != null)
                subjectNameString += ", C=" + subjectCountryCode;
            if (subjectOrganization != null)
                subjectNameString += ", O=" + subjectOrganization;
            if (subjectTitle != null)
                subjectNameString += ", T=" + subjectTitle;
            certificateGenerator.SetSubjectDN(new X509Name(subjectNameString));

            string issuerNameString = "E=" + issuerName;
            if (issuerCommonName != null)
                issuerNameString += ", CN=" + issuerCommonName;
            if (issuerCountryCode != null)
                issuerNameString += ", C=" + issuerCountryCode;
            if (issuerOrganization != null)
                issuerNameString += ", O=" + issuerOrganization;
            if (issuerTitle != null)
                issuerNameString += ", T=" + issuerTitle;
            certificateGenerator.SetIssuerDN(new X509Name(issuerNameString));

            certificateGenerator.SetNotBefore(begin);
            certificateGenerator.SetNotAfter(end);
            certificateGenerator.SetSignatureAlgorithm(signatureAlgorithm);
            certificateGenerator.SetPublicKey(publicKey);

            X509Certificate certificate = certificateGenerator.Generate(privateKey);
            sys.X509Certificate2 windowsCertificate = new sys.X509Certificate2(DotNetUtilities.ToX509Certificate(certificate));
            windowsCertificate.PrivateKey = ConvertToSystemKey(privateKey);

            InstallIntoRegistry(windowsCertificate);
        }

        /*
         * converts a bouncy castle private key to a windows private key
         */
        private static sys2.AsymmetricAlgorithm ConvertToSystemKey(RsaPrivateCrtKeyParameters privateKey)
        {
            sys2.CspParameters cspPars = new sys2.CspParameters
            {
                KeyContainerName = Guid.NewGuid().ToString(),
                KeyNumber = (int)sys2.KeyNumber.Exchange
            };

            sys2.RSACryptoServiceProvider rsaCryptoProvider = new sys2.RSACryptoServiceProvider(cspPars);
            sys2.RSAParameters rsaParameters = new sys2.RSAParameters
            {
                Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                P = privateKey.P.ToByteArrayUnsigned(),
                Q = privateKey.Q.ToByteArrayUnsigned(),
                DP = privateKey.DP.ToByteArrayUnsigned(),
                DQ = privateKey.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKey.QInv.ToByteArrayUnsigned(),
                D = privateKey.Exponent.ToByteArrayUnsigned(),
                Exponent = privateKey.PublicExponent.ToByteArrayUnsigned()
            };

            rsaCryptoProvider.ImportParameters(rsaParameters);
            return rsaCryptoProvider;
        }

        /*
         * installs certificate and key into windows registry
         */
        private static void InstallIntoRegistry(sys.X509Certificate2 windowsCertificate)
        {
            sys.X509Store store = new sys.X509Store();
            store.Open(sys.OpenFlags.ReadWrite);
            try
            {
                store.Add(windowsCertificate);
            }
            finally
            {
                store.Close();
            }
        }
    }
}


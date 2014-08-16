/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 * 
 * this file is based upon work done by Jeffrey Steadfast from the 
 * Xamarin team
 * 
 * file was forked due to that the original file didn't allow usage 
 * of DataEncipherment certificates in the search logic in the 
 * GetCmsRecipientCertificate method
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Pkix;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509.Store;

using RealCmsSigner = System.Security.Cryptography.Pkcs.CmsSigner;
using RealCmsRecipient = System.Security.Cryptography.Pkcs.CmsRecipient;
using RealCmsRecipientCollection = System.Security.Cryptography.Pkcs.CmsRecipientCollection;
using RealX509KeyUsageFlags = System.Security.Cryptography.X509Certificates.X509KeyUsageFlags;

using MimeKit.IO;
using MimeKit.Cryptography;
using MimeKit;

namespace Magix.cryptography
{
    public class WindowsSecureMimeContext : SecureMimeContext
    {
        public WindowsSecureMimeContext(StoreLocation location)
        {
            StoreLocation = location;

            // System.Security does not support Camellia...
            Disable(EncryptionAlgorithm.Camellia256);
            Disable(EncryptionAlgorithm.Camellia192);
            Disable(EncryptionAlgorithm.Camellia192);

            // ...or CAST5...
            Disable(EncryptionAlgorithm.Cast5);

            // ...or IDEA...
            Disable(EncryptionAlgorithm.Idea);
        }

        public WindowsSecureMimeContext()
            : this(StoreLocation.CurrentUser)
        { }

        public StoreLocation StoreLocation
        {
            get;
            private set;
        }

        #region implemented abstract members of SecureMimeContext

        protected override Org.BouncyCastle.X509.X509Certificate GetCertificate(IX509Selector selector)
        {
            var storeNames = new[] { StoreName.My, StoreName.AddressBook, StoreName.TrustedPeople, StoreName.Root };

            foreach (var storeName in storeNames)
            {
                var store = new X509Store(storeName, StoreLocation);

                store.Open(OpenFlags.ReadOnly);

                try
                {
                    foreach (var certificate in store.Certificates)
                    {
                        var cert = DotNetUtilities.FromX509Certificate(certificate);
                        if (selector == null || selector.Match(cert))
                            return cert;
                    }
                }
                finally
                {
                    store.Close();
                }
            }

            return null;
        }

        protected override AsymmetricKeyParameter GetPrivateKey(IX509Selector selector)
        {
            var store = new X509Store(StoreName.My, StoreLocation);

            store.Open(OpenFlags.ReadOnly);

            try
            {
                foreach (var certificate in store.Certificates)
                {
                    if (!certificate.HasPrivateKey)
                        continue;

                    var cert = DotNetUtilities.FromX509Certificate(certificate);

                    if (selector == null || selector.Match(cert))
                    {
                        var pair = DotNetUtilities.GetKeyPair(certificate.PrivateKey);
                        return pair.Private;
                    }
                }
            }
            finally
            {
                store.Close();
            }

            return null;
        }

        protected override Org.BouncyCastle.Utilities.Collections.HashSet GetTrustedAnchors()
        {
            var storeNames = new StoreName[] { StoreName.TrustedPeople, StoreName.Root };
            var anchors = new Org.BouncyCastle.Utilities.Collections.HashSet();

            foreach (var storeName in storeNames)
            {
                var store = new X509Store(storeName, StoreLocation);

                store.Open(OpenFlags.ReadOnly);

                foreach (var certificate in store.Certificates)
                {
                    var cert = DotNetUtilities.FromX509Certificate(certificate);
                    anchors.Add(new TrustAnchor(cert, null));
                }

                store.Close();
            }

            return anchors;
        }

        protected override IX509Store GetIntermediateCertificates()
        {
            var storeNames = new StoreName[] { StoreName.My, StoreName.AddressBook, StoreName.TrustedPeople, StoreName.Root };
            var intermediate = new X509CertificateStore();

            foreach (var storeName in storeNames)
            {
                var store = new X509Store(storeName, StoreLocation);

                store.Open(OpenFlags.ReadOnly);

                foreach (var certificate in store.Certificates)
                {
                    var cert = DotNetUtilities.FromX509Certificate(certificate);
                    intermediate.Add(cert);
                }

                store.Close();
            }

            return intermediate;
        }

        protected override IX509Store GetCertificateRevocationLists()
        {
            // TODO: figure out how other Windows apps keep track of CRLs...
            var crls = new List<X509Crl>();

            return X509StoreFactory.Create("Crl/Collection", new X509CollectionStoreParameters(crls));
        }

        X509Certificate2 GetCmsRecipientCertificate(MailboxAddress mailbox)
        {
            var store = new X509Store(StoreName.My, StoreLocation);
            var now = DateTime.Now;

            store.Open(OpenFlags.ReadOnly);

            try
            {
                foreach (var certificate in store.Certificates)
                {
                    if (certificate.NotBefore > now || certificate.NotAfter < now)
                        continue;

                    if (certificate.GetNameInfo(X509NameType.EmailName, false) != mailbox.Address)
                        continue;

                    return certificate;
                }
            }
            finally
            {
                store.Close();
            }

            throw new CertificateNotFoundException(mailbox, "A valid certificate could not be found.");
        }

        static EncryptionAlgorithm[] DecodeEncryptionAlgorithms(byte[] rawData)
        {
            using (var memory = new MemoryStream(rawData, false))
            {
                using (var asn1 = new Asn1InputStream(memory))
                {
                    var algorithms = new List<EncryptionAlgorithm>();
                    var sequence = asn1.ReadObject() as Asn1Sequence;

                    if (sequence == null)
                        return null;

                    for (int i = 0; i < sequence.Count; i++)
                    {
                        var identifier = Org.BouncyCastle.Asn1.X509.AlgorithmIdentifier.GetInstance(sequence[i]);
                        EncryptionAlgorithm algorithm;

                        if (TryGetEncryptionAlgorithm(identifier, out algorithm))
                            algorithms.Add(algorithm);
                    }

                    return algorithms.ToArray();
                }
            }
        }

        protected override MimeKit.Cryptography.CmsRecipient GetCmsRecipient(MailboxAddress mailbox)
        {
            var certificate = GetCmsRecipientCertificate(mailbox);
            var cert = DotNetUtilities.FromX509Certificate(certificate);
            var recipient = new MimeKit.Cryptography.CmsRecipient(cert);

            foreach (var extension in certificate.Extensions)
            {
                if (extension.Oid.Value == "1.2.840.113549.1.9.15")
                {
                    var algorithms = DecodeEncryptionAlgorithms(extension.RawData);

                    if (algorithms != null)
                        recipient.EncryptionAlgorithms = algorithms;

                    break;
                }
            }

            return recipient;
        }

        RealCmsRecipient GetRealCmsRecipient(MailboxAddress mailbox)
        {
            return new RealCmsRecipient(GetCmsRecipientCertificate(mailbox));
        }

        RealCmsRecipientCollection GetRealCmsRecipients(IEnumerable<MailboxAddress> mailboxes)
        {
            var recipients = new RealCmsRecipientCollection();

            foreach (var mailbox in mailboxes)
                recipients.Add(GetRealCmsRecipient(mailbox));

            return recipients;
        }

        X509Certificate2 GetCmsSignerCertificate(MailboxAddress mailbox)
        {
            var store = new X509Store(StoreName.My, StoreLocation);
            var now = DateTime.Now;

            store.Open(OpenFlags.ReadOnly);

            try
            {
                foreach (var certificate in store.Certificates)
                {
                    if (certificate.NotBefore > now || certificate.NotAfter < now)
                        continue;

                    var usage = certificate.Extensions[X509Extensions.KeyUsage.Id] as X509KeyUsageExtension;
                    if (usage != null && (usage.KeyUsages & RealX509KeyUsageFlags.DigitalSignature) == 0)
                        continue;

                    if (!certificate.HasPrivateKey)
                        continue;

                    if (certificate.GetNameInfo(X509NameType.EmailName, false) != mailbox.Address)
                        continue;

                    return certificate;
                }
            }
            finally
            {
                store.Close();
            }

            throw new CertificateNotFoundException(mailbox, "A valid signing certificate could not be found.");
        }

        protected override MimeKit.Cryptography.CmsSigner GetCmsSigner(MailboxAddress mailbox, DigestAlgorithm digestAlgo)
        {
            var certificate = GetCmsSignerCertificate(mailbox);
            var pair = DotNetUtilities.GetKeyPair(certificate.PrivateKey);
            var cert = DotNetUtilities.FromX509Certificate(certificate);
            var signer = new MimeKit.Cryptography.CmsSigner(cert, pair.Private);
            signer.DigestAlgorithm = digestAlgo;
            return signer;
        }

        RealCmsSigner GetRealCmsSigner(MailboxAddress mailbox, DigestAlgorithm digestAlgo)
        {
            var signer = new RealCmsSigner(GetCmsSignerCertificate(mailbox));
            signer.DigestAlgorithm = new Oid(GetDigestOid(digestAlgo));
            signer.IncludeOption = X509IncludeOption.ExcludeRoot;
            return signer;
        }

        protected override void UpdateSecureMimeCapabilities(Org.BouncyCastle.X509.X509Certificate certificate, EncryptionAlgorithm[] algorithms, DateTime timestamp)
        {
            // TODO: implement this - should we add/update the X509Extension for S/MIME Capabilities?
        }

        byte[] ReadAllBytes(Stream stream)
        {
            if (stream is MemoryBlockStream)
                return ((MemoryBlockStream)stream).ToArray();

            if (stream is MemoryStream)
                return ((MemoryStream)stream).ToArray();

            using (var memory = new MemoryBlockStream())
            {
                stream.CopyTo(memory, 4096);
                return memory.ToArray();
            }
        }

        public override ApplicationPkcs7Mime EncapsulatedSign(MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
        {
            if (signer == null)
                throw new ArgumentNullException("signer");

            if (content == null)
                throw new ArgumentNullException("content");

            var contentInfo = new ContentInfo(ReadAllBytes(content));
            var cmsSigner = GetRealCmsSigner(signer, digestAlgo);
            var signed = new SignedCms(contentInfo, false);
            signed.ComputeSignature(cmsSigner);
            var signedData = signed.Encode();

            return new ApplicationPkcs7Mime(SecureMimeType.SignedData, new MemoryStream(signedData, false));
        }

        public override MimePart Sign(MailboxAddress signer, DigestAlgorithm digestAlgo, Stream content)
        {
            if (signer == null)
                throw new ArgumentNullException("signer");

            if (content == null)
                throw new ArgumentNullException("content");

            var contentInfo = new ContentInfo(ReadAllBytes(content));
            var cmsSigner = GetRealCmsSigner(signer, digestAlgo);
            var signed = new SignedCms(contentInfo, true);
            signed.ComputeSignature(cmsSigner);
            var signedData = signed.Encode();

            return new ApplicationPkcs7Signature(new MemoryStream(signedData, false));
        }

        public override MimeEntity Decrypt(Stream encryptedData)
        {
            if (encryptedData == null)
                throw new ArgumentNullException("encryptedData");

            var enveloped = new EnvelopedCms();

            enveloped.Decode(ReadAllBytes(encryptedData));
            enveloped.Decrypt();

            var decryptedData = enveloped.Encode();

            var memory = new MemoryStream(decryptedData, false);

            return MimeEntity.Load(memory, true);
        }

        public override void Import(Org.BouncyCastle.X509.X509Certificate certificate)
        {
            if (certificate == null)
                throw new ArgumentNullException("certificate");

            var store = new X509Store(StoreName.AddressBook, StoreLocation);

            store.Open(OpenFlags.ReadWrite);
            store.Add(new X509Certificate2(certificate.GetEncoded()));
            store.Close();
        }

        public override void Import(X509Crl crl)
        {
            if (crl == null)
                throw new ArgumentNullException("crl");

            // TODO: figure out where to store the CRLs...
        }

        public override void Import(Stream stream, string password)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            if (password == null)
                throw new ArgumentNullException("password");

            byte[] rawData;

            if (stream is MemoryBlockStream)
            {
                rawData = ((MemoryBlockStream)stream).ToArray();
            }
            else if (stream is MemoryStream)
            {
                rawData = ((MemoryStream)stream).ToArray();
            }
            else
            {
                using (var memory = new MemoryBlockStream())
                {
                    stream.CopyTo(memory, 4096);
                    rawData = memory.ToArray();
                }
            }

            var store = new X509Store(StoreName.My, StoreLocation);
            var certs = new X509Certificate2Collection();

            store.Open(OpenFlags.ReadWrite);
            certs.Import(rawData, password, X509KeyStorageFlags.UserKeySet);
            store.AddRange(certs);
            store.Close();
        }

        #endregion
    }
}

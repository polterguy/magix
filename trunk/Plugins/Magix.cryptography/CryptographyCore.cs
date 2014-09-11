/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using sys2 = System.Security.Cryptography.X509Certificates;
using MimeKit;
using MimeKit.Cryptography;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Crypto.Prng;
using Magix.Core;

namespace Magix.cryptography
{
    internal class CryptographyCore : ActiveController
    {
        /*
         * creates a certificate and a private key
         */
        [ActiveEvent(Name = "magix.cryptography.create-certificate-key")]
        private static void magix_cryptography_create_certificate_key(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.Contains("inspect"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.create-certificate-key-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.create-certificate-key-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string subjectName = Expressions.GetExpressionValue<string>(ip["subject-name"].Get<string>(), Dp(e.Params), ip, false);
            string subjectCommonName = ip["subject-name"].ContainsValue("common-name") ?
                Expressions.GetExpressionValue<string>(ip["subject-name"]["common-name"].Get<string>(), dp, ip, false, null) : null;
            string subjectCountryCode = ip["subject-name"].ContainsValue("country-code") ?
                Expressions.GetExpressionValue<string>(ip["subject-name"]["country-code"].Get<string>(), dp, ip, false, null) : null;
            string subjectOrganization = ip["subject-name"].ContainsValue("organization") ?
                Expressions.GetExpressionValue<string>(ip["subject-name"]["organization"].Get<string>(), dp, ip, false, null) : null;
            string subjectTitle = ip["subject-name"].ContainsValue("title") ?
                Expressions.GetExpressionValue<string>(ip["subject-name"]["title"].Get<string>(), dp, ip, false, null) : null;

            string issuerName = Expressions.GetExpressionValue<string>(ip["issuer-name"].Get<string>(), Dp(e.Params), ip, false);
            string issuerCommonName = ip["issuer-name"].ContainsValue("common-name") ?
                Expressions.GetExpressionValue<string>(ip["issuer-name"]["common-name"].Get<string>(), dp, ip, false, null) : null;
            string issuerCountryCode = ip["issuer-name"].ContainsValue("country-code") ?
                Expressions.GetExpressionValue<string>(ip["issuer-name"]["country-code"].Get<string>(), dp, ip, false, null) : null;
            string issuerOrganization = ip["issuer-name"].ContainsValue("organization") ?
                Expressions.GetExpressionValue<string>(ip["issuer-name"]["organization"].Get<string>(), dp, ip, false, null) : null;
            string issuerTitle = ip["issuer-name"].ContainsValue("title") ?
                Expressions.GetExpressionValue<string>(ip["issuer-name"]["title"].Get<string>(), dp, ip, false, null) : null;

            string signatureAlgorithm = Expressions.GetExpressionValue<string>(ip.GetValue<string>("signature-algorithm", null), dp, ip, false, "SHA512WithRSA");
            int strength = Expressions.GetExpressionValue<int>(ip.GetValue<string>("strength", null), dp, ip, false, 2048);
            DateTime begin = Expressions.GetExpressionValue<DateTime>(ip.GetValue<string>("begin", null), dp, ip, false, DateTime.Now.Date);
            DateTime end = Expressions.GetExpressionValue<DateTime>(ip.GetValue<string>("end", null), dp, ip, false, DateTime.Now.Date.AddYears(3));

            CryptographyHelper.CreateCertificateKey(
                subjectName, 
                issuerName, 
                signatureAlgorithm, 
                strength, 
                begin, 
                end, 
                subjectCountryCode,
                subjectOrganization,
                subjectTitle,
                issuerCountryCode,
                issuerOrganization,
                issuerTitle,
                subjectCommonName,
                issuerCommonName);
        }

        /*
         * checks to see if email can be signed
         */
        [ActiveEvent(Name = "magix.cryptography.can-sign")]
        private static void magix_cryptography_can_sign(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.Contains("inspect"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.can-sign-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.can-sign-sample]");
                return;
            }

            string emailAdr = Expressions.GetExpressionValue<string>(ip["email"].Get<string>(), Dp(e.Params), ip, false);

            ip["value"].Value = CryptographyHelper.CanSign(emailAdr);
        }

        /*
         * checks to see if email can be encrypted
         */
        [ActiveEvent(Name = "magix.cryptography.can-encrypt")]
        private static void magix_cryptography_can_encrypt(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.Contains("inspect"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.can-encrypt-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.can-encrypt-sample]");
                return;
            }

            Node emails = null;
            if (ip.ContainsValue("emails"))
                emails = Expressions.GetExpressionValue<Node>(ip["emails"].Get<string>(), Dp(e.Params), ip, false);
            else
                emails = ip["emails"];

            string canEncrypt = CryptographyHelper.CanEncrypt(emails);

            if (canEncrypt == null)
                ip["value"].Value = true;
            else
            {
                ip["value"].Value = false;
                ip["value"]["message"].Value = canEncrypt;
            }
        }

        /*
         * imports a certificate into database
         */
        [ActiveEvent(Name = "magix.cryptography.import-certificate")]
        private static void magix_cryptography_import_certificate(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.Contains("inspect"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.import-certificate-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.import-certificate-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string certificateFile = Expressions.GetExpressionValue<string>(ip["certificate"].Get<string>(), dp, ip, false);
            string password = Expressions.GetExpressionValue<string>(ip["password"].Get<string>(), dp, ip, false);

            CryptographyHelper.ImportCertificate(certificateFile, password);
        }

        /*
         * removes a certificate from database by subject name
         */
        [ActiveEvent(Name = "magix.cryptography.remove-certificates")]
        private static void magix_cryptography_remove_certificates(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.Contains("inspect"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.remove-certificates-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography.remove-certificates-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string subjectName = Expressions.GetExpressionValue<string>(ip["subject-name"].Get<string>(), dp, ip, false);

            CryptographyHelper.RemoveCertificates(subjectName);
        }

        /*
         * signs a mime entity
         */
        [ActiveEvent(Name = "magix.cryptography._sign-mime-entity")]
        private static void magix_cryptography__sign_mime_entity(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.Contains("inspect"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography._sign-mime-entity-dox].value");
                return;
            }

            MimeEntity entity = ip["mime-entity"].Get<MimeEntity>();
            string emailAdr = ip["email-lookup"].Get<string>();
            MailboxAddress signer = new MailboxAddress("", emailAdr);

            ip["mime-entity"].Value = CryptographyHelper.SignEntity(entity, signer);
        }

        /*
         * signs a mime entity
         */
        [ActiveEvent(Name = "magix.cryptography._encrypt-mime-entity")]
        private static void magix_cryptography__encrypt_mime_entity(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.Contains("inspect"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography._encrypt-mime-entity-dox].value");
                return;
            }

            MimeEntity entity = ip["mime-entity"].Get<MimeEntity>();
            List<MailboxAddress> list = new List<MailboxAddress>();
            foreach (Node idx in ip["email-lookups"])
                list.Add(new MailboxAddress("", idx.Get<string>()));

            ip["mime-entity"].Value = CryptographyHelper.EncryptEntity(entity, list);
        }

        /*
         * signs and encrypts a mime entity
         */
        [ActiveEvent(Name = "magix.cryptography._sign_and_encrypt-mime-entity")]
        private static void magix_cryptography__sign_and_encrypt_mime_entity(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.Contains("inspect"))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.cryptography",
                    "Magix.cryptography.hyperlisp.inspect.hl",
                    "[magix.cryptography._sign_and_encrypt-mime-entity-dox].value");
                return;
            }

            MimeEntity entity = ip["mime-entity"].Get<MimeEntity>();
            List<MailboxAddress> list = new List<MailboxAddress>();
            foreach (Node idx in ip["email-lookups"])
                list.Add(new MailboxAddress("", idx.Get<string>()));

            string emailAdr = ip["email-lookup"].Get<string>();
            MailboxAddress signer = new MailboxAddress("", emailAdr);

            ip["mime-entity"].Value = CryptographyHelper.SignAndEncryptEntity(entity, signer, list);
        }
    }
}

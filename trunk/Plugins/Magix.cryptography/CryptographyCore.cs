/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using MimeKit;
using MimeKit.Cryptography;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.cryptography
{
    internal class CryptographyCore : ActiveController
    {
        /*
         * checks to see if email can be signed
         */
        [ActiveEvent(Name = "magix.cryptography.can-sign")]
        private static void magix_cryptography_can_sign(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.ContainsValue("inspect"))
            {
                ActiveController.AppendInspect(ip["inspect"], @"returns true if email can be signed

will check to see if email can be signed, and if so, return [value] as true.  expects 
to be given a [email] email address, which it uses as lookup to find an ssl certificate 
and private key

[email] can be either an expression or a constant");
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
            if (ip.ContainsValue("inspect"))
            {
                ActiveController.AppendInspect(ip["inspect"], @"returns true if email can be encrypted

will check to see if email can be encrypted, and if so, return [value] as true.  expects 
to be given a list of emails as [emails], which it uses as lookup to make sure all recipients 
have valid certificates

[emails] can be either a list of children nodes, or have its value point to another node 
which is to be used as the list of nodes containing the emails to verify have certificates

if encryptiong cannot be performed, then [message] will be returned as the child of [value] 
explaining why");
                return;
            }

            Node emails = null;
            if (ip.ContainsValue("emails"))
                emails = Expressions.GetExpressionValue<Node>(ip["emails"].Get<string>(), Dp(e.Params), ip, false);
            else
                emails = ip["emails"];
            if (emails.Count == 0)
            {
                ip["value"].Value = false;
                ip["value"]["message"].Value = "no recipients to encrypt for";
            }
            else
            {
                string canEncrypt = CryptographyHelper.CanEncrypt(emails);
                if (canEncrypt == null)
                    ip["value"].Value = true;
                else
                {
                    ip["value"].Value = false;
                    ip["value"]["message"].Value = canEncrypt;
                }
            }
        }

        /*
         * imports a certificate into database
         */
        [ActiveEvent(Name = "magix.cryptography.import-certificate")]
        private static void magix_cryptography_import_certificate(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.ContainsValue("inspect"))
            {
                ActiveController.AppendInspect(ip["inspect"], @"imports a certificate into certificate database

will import the given [certificate] file into the certificate database.  if 
[password] is given, it will be used as password to extract the certificate");
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
            if (ip.ContainsValue("inspect"))
            {
                ActiveController.AppendInspect(ip["inspect"], @"removes certificates from certificate database

will remove all given certificates from the certificate database according to 
[subject-name]");
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
            if (ip.ContainsValue("inspect"))
            {
                ActiveController.AppendInspect(ip["inspect"], @"signs a mime entity if possible

will attempt to sign the given MimeKit [mime-entity], and return the signed 
entity as [mime-entity].  pass in [email-lookup] which the event will use to 
search for a valid certificate to use for signing

this active event is not supposed to be raise from anything but C# code, 
since it requires an object of type MimeEntity to be passed in");
                return;
            }

            MimeEntity entity = ip["mime-entity"].Get<MimeEntity>();
            string emailAdr = ip["email-lookup"].Get<string>();
            MailboxAddress signer = new MailboxAddress("", emailAdr);
            MimeEntity signed = CryptographyHelper.SignEntity(entity, signer);
            if (signed != null)
            {
                ip["success"].Value = true;
                ip["mime-entity"].Value = signed;
            }
        }

        /*
         * signs a mime entity
         */
        [ActiveEvent(Name = "magix.cryptography._encrypt-mime-entity")]
        private static void magix_cryptography__encrypt_mime_entity(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.ContainsValue("inspect"))
            {
                ActiveController.AppendInspect(ip["inspect"], @"encrypts a mime entity if possible

will attempt to encrypt the given MimeKit [mime-entity], and return the encrypted 
entity as [mime-entity].  pass in [email-lookups] which the event will use to 
search for valid certificates to use for signing

this active event is not supposed to be raise from anything but C# code, 
since it requires an object of type MimeEntity to be passed in");
                return;
            }

            MimeEntity entity = ip["mime-entity"].Get<MimeEntity>();
            List<MailboxAddress> list = new List<MailboxAddress>();
            foreach (Node idx in ip["email-lookups"])
            {
                list.Add(new MailboxAddress("", idx.Get<string>()));
            }

            MimeEntity encrypted = CryptographyHelper.EncryptEntity(entity, list);
            if (encrypted != null)
            {
                ip["success"].Value = true;
                ip["mime-entity"].Value = encrypted;
            }
        }

        /*
         * signs a mime entity
         */
        [ActiveEvent(Name = "magix.cryptography._sign_and_encrypt-mime-entity")]
        private static void magix_cryptography__sign_and_encrypt_mime_entity(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ip.ContainsValue("inspect"))
            {
                ActiveController.AppendInspect(ip["inspect"], @"signs and encrypts a mime entity if possible

will attempt to sign and encrypt the given MimeKit [mime-entity], and return 
the entity as [mime-entity].  pass in [email-lookups] which the event will use to 
search for valid certificates to use for signing and [email-lookup] as the email 
address for the signer

this active event is not supposed to be raise from anything but C# code, 
since it requires an object of type MimeEntity to be passed in");
                return;
            }

            MimeEntity entity = ip["mime-entity"].Get<MimeEntity>();
            List<MailboxAddress> list = new List<MailboxAddress>();
            foreach (Node idx in ip["email-lookups"])
            {
                list.Add(new MailboxAddress("", idx.Get<string>()));
            }

            string emailAdr = ip["email-lookup"].Get<string>();
            MailboxAddress signer = new MailboxAddress("", emailAdr);

            MimeEntity encrypted = CryptographyHelper.SignAndEncryptEntity(entity, signer, list);
            if (encrypted != null)
            {
                ip["success"].Value = true;
                ip["mime-entity"].Value = encrypted;
            }
        }
    }
}

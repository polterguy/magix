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
         * signs a mime entity
         */
        [ActiveEvent(Name = "magix.cryptography._sign-mime-entity")]
        public static void magix_cryptography__sign_mime_entity(object sender, ActiveEventArgs e)
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
        public static void magix_cryptography__encrypt_mime_entity(object sender, ActiveEventArgs e)
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
        public static void magix_cryptography__sign_and_encrypt_mime_entity(object sender, ActiveEventArgs e)
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

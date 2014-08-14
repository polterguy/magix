/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Net;
using System.Web;
using System.Configuration;
using System.Collections.ObjectModel;
using sys = System.Security.Cryptography.X509Certificates;
using Magix.Core;
using Org.BouncyCastle.X509;

namespace Magix.email
{
	/*
	 * email common
	 */
	internal class MailCommon
	{
        /*
         * registers security context
         */
        public static sys.X509Certificate2 FindCertificate(string email)
        {
            sys.X509Store localStore = new sys.X509Store(sys.StoreName.My);
            localStore.Open(sys.OpenFlags.ReadOnly | sys.OpenFlags.OpenExistingOnly);
            try
            {
                sys.X509Certificate2Collection matches = localStore.Certificates.Find(
                    sys.X509FindType.FindBySubjectName,
                    email,
                    true);

                if (matches.Count > 0)
                {
                    return matches[0];
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                localStore.Close();
            }
        }
   }
}


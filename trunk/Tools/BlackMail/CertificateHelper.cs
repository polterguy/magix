/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace BlackMail
{
    /*
     * helper for retrieving and storing certificates
     */
    public static class CertificateHelper
    {
        /*
         * returns a certificate by subject name
         */
        public static X509Certificate2 GetCertificateBySubjectName(StoreLocation location, string subjectName)
        {
            X509Store store = new X509Store(location);
            store.Open(OpenFlags.OpenExistingOnly | OpenFlags.ReadOnly);
            X509Certificate2Collection certs = store.Certificates.Find(X509FindType.FindBySubjectName, subjectName, true);
            store.Close();

            if (certs.Count < 1)
                return null;
            else
                return certs[0];
        }
    }
}

/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Configuration;
using System.Security.Cryptography;
using Magix.Core;

namespace Magix.web
{
	/*
	 * encryption core
	 */
	internal sealed class EncryptCore : ActiveController
	{
		/*
		 * creates a hashed string from any string
		 */
		[ActiveEvent(Name = "magix.encryption.hash-string")]
		private void magix_encryption_hash_string(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.encryption.hash-string-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.encryption.hash-string-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("value"))
                throw new ArgumentException("no [value] given to [magix.encryption.hash-string]");
            string value = Expressions.GetExpressionValue<string>(ip["value"].Get<string>(), dp, ip, false);

            ip["hash"].Value = HashMD5(value);
		}

        private string HashMD5(string input)
        {
            using (MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] hashed = md5.ComputeHash(System.Text.Encoding.ASCII.GetBytes(input));
                StringBuilder builder = new StringBuilder();
                for (int idx = 0; idx < hashed.Length; idx++)
                {
                    builder.Append(hashed[idx].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}


/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using Magix.Core;

namespace Magix.data
{
	/*
	 * data storage
	 */
	internal sealed class DataCore : ActiveController
	{
        /*
         * slurps up everything from database
         */
        [ActiveEvent(Name = "magix.core.application-startup")]
        public static void magix_core_application_startup(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.application-startup-dox].Value");
                return;
            }
            Database.Initialize();
        }

		/*
		 * loads an object from database
		 */
		[ActiveEvent(Name = "magix.data.load")]
		public static void magix_data_load(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.load-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.load-sample]");
                return;
			}

            if (ip.Contains("id") && ip.Contains("prototype"))
                throw new ArgumentException("cannot use both [id] and [prototype] in [magix.data.load]");

            Node dp = Dp(e.Params);

			Node prototype = null;
            string id = null;
            if (ip.Contains("prototype"))
            {
                if (ip.ContainsValue("prototype"))
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                else
                    prototype = ip["prototype"];
            }
            else if (ip.ContainsValue("id"))
                id = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;
            else
                throw new ArgumentException("either [prototype] or [id] is needed for [magix.data.load]");

			int start = 0;
            if (ip.ContainsValue("start"))
                start = int.Parse(Expressions.GetExpressionValue(ip["start"].Get<string>(), dp, ip, false) as string);

			int end = -1;
            if (ip.ContainsValue("end"))
                end = int.Parse(Expressions.GetExpressionValue(ip["end"].Get<string>(), dp, ip, false) as string);

			if (id != null && (start != 0 || end != -1 || prototype != null))
				throw new ArgumentException("if you supply an [id], then [start], [end] and [prototype] cannot be defined");

            Database.LoadItems(ip, prototype, id, start, end);
		}

		/*
		 * saves an object to database
		 */
		[ActiveEvent(Name = "magix.data.save")]
		public static void magix_data_save(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.save-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.save-sample]");
				return;
			}

            if (!ip.Contains("value"))
				throw new ArgumentException("[value] must be given to [magix.data.save]");

            Node value = null;

            Node dp = Dp(e.Params);
            if (ip.ContainsValue("value"))
                value = (Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as Node).Clone();
            else
                value = ip["value"].Clone();

            if (ip.Contains("id"))
            {
                string id = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;
                Database.SaveById(value, id);
            }
            else
                ip["id"].Value = Database.SaveNewObject(value, null);
		}

		/*
		 * removes an object from database
		 */
		[ActiveEvent(Name = "magix.data.remove")]
		public static void magix_data_remove(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.remove-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.remove-sample]");
                return;
			}

            if (ip.Contains("id") && ip.Contains("prototype"))
                throw new ArgumentException("cannot use both [id] and [prototype] in [magix.data.load]");

            Node dp = Dp(e.Params);

            Node prototype = null;
            if (ip.Contains("prototype"))
            {
                if (ip.ContainsValue("prototype"))
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                else
                    prototype = ip["prototype"];
            }

            if (!ip.ContainsValue("id") && prototype == null)
				throw new ArgumentException("missing [id] or [prototype] while trying to remove object");

            if (ip.Contains("id"))
            {
                string id = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;
                Database.RemoveById(id);
            }
            else
                Database.RemoveByPrototype(prototype);
        }

		/*
		 * counts objects in database
		 */
		[ActiveEvent(Name = "magix.data.count")]
		public static void magix_data_count(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(e.Params))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.count-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.count-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            Node prototype = null;
            if (ip.Contains("prototype"))
            {
                if (ip.ContainsValue("prototype"))
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                else
                    prototype = ip["prototype"];
            }
            Database.CountRecords(ip, prototype);
		}
	}
}


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
                    "[magix.data.application-startup-dox].value");
                return;
            }

            Database.Initialize();
        }

        /*
         * creates a database transaction
         */
        [ActiveEvent(Name = "magix.data.transaction")]
        public static void magix_data_transaction(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.transaction-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.transaction-sample]");
                return;
            }

            Database.ExecuteTransaction(e.Params);
        }

        /*
         * commits a database transaction
         */
        [ActiveEvent(Name = "magix.data.commit")]
        public static void magix_data_commit(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.commit-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.commit-sample]");
                return;
            }

            Database.Commit(e.Params);
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
                    "[magix.data.load-dox].value");
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
                    prototype = Expressions.GetExpressionValue<Node>(ip["prototype"].Get<string>(), dp, ip, false);
                else
                    prototype = ip["prototype"];
            }
            else if (ip.ContainsValue("id"))
            {
                id = Expressions.GetFormattedExpression("id", e.Params, "");
            }
            else
                throw new ArgumentException("either [prototype] or [id] is needed for [magix.data.load]");

            int start = 0;
            if (ip.ContainsValue("start"))
                start = Expressions.GetExpressionValue<int>(ip["start"].Get<string>(), dp, ip, false);

            int end = -1;
            if (ip.ContainsValue("end"))
                end = Expressions.GetExpressionValue<int>(ip["end"].Get<string>(), dp, ip, false);

            if (id != null && (start != 0 || end != -1 || prototype != null))
                throw new ArgumentException("if you supply an [id], then [start], [end] and [prototype] cannot be defined");

            Guid transaction = Guid.Empty;
            if (e.Params.ContainsValue("_database-transaction"))
                transaction = e.Params["_database-transaction"].Get<Guid>();

            Database.LoadItems(ip, prototype, id, start, end, transaction);
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
                    "[magix.data.save-dox].value");
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
                value = Expressions.GetExpressionValue<Node>(ip["value"].Get<string>(), dp, ip, false).Clone();
            else
                value = ip["value"].Clone();

            Guid transaction = Guid.Empty;
            if (e.Params.ContainsValue("_database-transaction"))
                transaction = e.Params["_database-transaction"].Get<Guid>();

            if (ip.Contains("id"))
            {
                string id = Expressions.GetFormattedExpression("id", e.Params, "");
                Database.SaveById(value, id, transaction);
            }
            else
                ip["id"].Value = Database.SaveNewObject(value, transaction);
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
                    "[magix.data.remove-dox].value");
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
                    prototype = Expressions.GetExpressionValue<Node>(ip["prototype"].Get<string>(), dp, ip, false);
                else
                    prototype = ip["prototype"];
            }

            if (!ip.ContainsValue("id") && prototype == null)
                throw new ArgumentException("missing [id] or [prototype] while trying to remove object");

            Guid transaction = Guid.Empty;
            if (e.Params.ContainsValue("_database-transaction"))
                transaction = e.Params["_database-transaction"].Get<Guid>();

            if (ip.Contains("id"))
            {
                string id = Expressions.GetFormattedExpression("id", e.Params, "");
                ip["affected-records"].Value = Database.RemoveById(id, transaction);
            }
            else
                ip["affected-records"].Value = Database.RemoveByPrototype(prototype, transaction);
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
                    "[magix.data.count-dox].value");
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
                    prototype = Expressions.GetExpressionValue<Node>(ip["prototype"].Get<string>(), dp, ip, false);
                else
                    prototype = ip["prototype"];
            }

            Guid transaction = Guid.Empty;
            if (e.Params.ContainsValue("_database-transaction"))
                transaction = e.Params["_database-transaction"].Get<Guid>();

            Database.CountRecords(ip, prototype, transaction);
        }
    }
}


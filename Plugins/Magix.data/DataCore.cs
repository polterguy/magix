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

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id") && !ip.Contains("prototype") && !ip.Contains("or"))
                throw new ArgumentException("either [id], [prototype], [or] or any combination of all is needed for [magix.data.load]");

            string id = Expressions.GetFormattedExpression("id", e.Params, null);
            Guid transaction = e.Params.GetValue("_database-transaction", Guid.Empty);

            if (ip.Contains("prototype") || ip.Contains("or") || ip.Contains("not"))
            {
                bool caseSensitivePrototype = Expressions.GetExpressionValue<bool>(ip.GetValue("case", "true"), dp, ip, false);
                bool metaData = Expressions.GetExpressionValue<bool>(ip.GetValue("meta-data", "true"), dp, ip, false);
                string sortBy = ip.GetValue("sort", "");
                bool descending = true;
                if (!string.IsNullOrEmpty(sortBy))
                    descending = Expressions.GetExpressionValue<bool>(ip["sort"].GetValue("descending", "false"), dp, ip, false);
                Database.Load(
                    ip,
                    GetPrototype(ip, dp),
                    Expressions.GetExpressionValue<int>(ip.GetValue("start", "0"), dp, ip, false),
                    Expressions.GetExpressionValue<int>(ip.GetValue("end", "-1"), dp, ip, false),
                    transaction,
                    ip.GetValue("only-id", false),
                    caseSensitivePrototype,
                    sortBy,
                    descending,
                    id,
                    metaData);
            }
            else
                Database.Load(ip, id, transaction);
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

            if (!ip.Contains("value") && !ip.Contains("objects"))
                throw new ArgumentException("[value] or [objects] must be given to [magix.data.save]");

            Node dp = Dp(e.Params);

            Guid transaction = Guid.Empty;
            if (e.Params.ContainsValue("_database-transaction"))
                transaction = e.Params["_database-transaction"].Get<Guid>();

            if (ip.Contains("value"))
            {
                Node value = null;
                if (ip.ContainsValue("value"))
                    value = Expressions.GetExpressionValue<Node>(ip["value"].Get<string>(), dp, ip, false).Clone();
                else
                    value = ip["value"].Clone();

                if (ip.Contains("id"))
                {
                    string id = Expressions.GetFormattedExpression("id", e.Params, "");
                    Database.SaveById(value, id, transaction);
                }
                else
                    ip["id"].Value = Database.SaveNewObject(value, transaction);
            }
            else
            {
                Database.SaveNewObjects(ip["objects"], transaction);
            }
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

            Node dp = Dp(e.Params);

            if (!ip.Contains("id") && !ip.Contains("prototype") && !ip.Contains("or"))
                throw new ArgumentException("either [id], [prototype], [or] or any combination of all is needed for [magix.data.remove]");

            string id = Expressions.GetFormattedExpression("id", e.Params, null);
            Node prototype = GetPrototype(ip, dp);

            Guid transaction = e.Params.GetValue("_database-transaction", Guid.Empty);

            if (ip.Contains("prototype") || ip.Contains("or") || ip.Contains("not"))
            {
                bool caseSensitivePrototype = Expressions.GetExpressionValue<bool>(ip.GetValue("case", "true"), dp, ip, false);
                ip["affected-records"].Value = Database.RemoveByPrototype(prototype, transaction, caseSensitivePrototype, id);
            }
            else
                ip["affected-records"].Value = Database.RemoveById(id, transaction);
        }

        /*
         * counts objects in database
         */
        [ActiveEvent(Name = "magix.data.count")]
        public static void magix_data_count(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
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

            string id = ip.GetValue<string>("id", null);
            Node prototype = GetPrototype(ip, dp);
            bool caseSensitivePrototype = Expressions.GetExpressionValue<bool>(ip.GetValue("case", "true"), dp, ip, false);

            Guid transaction = e.Params.GetValue("_database-transaction", Guid.Empty);

            ip["count"].Value = Database.CountRecords(ip, prototype, transaction, caseSensitivePrototype, id);
        }

        /*
         * helper to retrieve prototype, will return null if no prototype exist
         */
        private static Node GetPrototype(Node ip, Node dp)
        {
            Node prototype = new Node();
            foreach (Node idxIpChild in ip)
            {
                if (idxIpChild.Name == "or" || idxIpChild.Name == "prototype")
                {
                    // this is an "or"'ed prototype
                    if (idxIpChild.Value != null)
                        prototype["or"].Add(Expressions.GetExpressionValue<Node>(idxIpChild.Get<string>(), dp, ip, false).Clone());
                    else
                        prototype["or"].Add(idxIpChild.Clone());
                }
                else if (idxIpChild.Name == "not")
                {
                    // this is an "or"'ed prototype
                    if (idxIpChild.Value != null)
                        prototype["not"].Add(Expressions.GetExpressionValue<Node>(idxIpChild.Get<string>(), dp, ip, false).Clone());
                    else
                        prototype["not"].Add(idxIpChild.Clone());
                }
            }
            return prototype.Count == 0 ? null : prototype;
        }
    }
}


/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using Magix.Core;

namespace Magix.ms.sql
{
    /*
     * class wrapping microsoft sql server, and the ability to select and do updates towards it
     */
    public class SqlCore : ActiveController
    {
        /*
         * selects from ms sql server
         */
        [ActiveEvent(Name = "magix.ms.sql.select")]
        public static void microsoft_sql_select(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ms.sql",
                    "Magix.ms.sql.sql.hyperlisp.inspect.hl",
                    "[magix.ms.sql.select-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ms.sql",
                    "Magix.ms.sql.hyperlisp.inspect.hl",
                    "[magix.ms.sql.select-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.Contains("connection"))
                throw new ArgumentException("you need to supply a [connection] to connect to a database");
            string connectionString = Expressions.GetExpressionValue(ip["connection"].Get<string>(), dp, ip, false) as string;
            if (connectionString.IndexOf("web.config:") == 0)
                connectionString = ConfigurationManager.ConnectionStrings[connectionString.Replace("web.config:", "")].ConnectionString;

            if (!ip.Contains("sql"))
                throw new ArgumentException("you need to supply a [sql] to know what query to run");
            string query = Expressions.GetExpressionValue(ip["sql"].Get<string>(), dp, ip, false) as string;

            int start = -1;
            if (ip.Contains("start"))
                start = int.Parse(Expressions.GetExpressionValue(ip["start"].Get<string>(), dp, ip, false) as string);

            int end = -1;
            if (ip.Contains("end"))
                end = int.Parse(Expressions.GetExpressionValue(ip["end"].Get<string>(), dp, ip, false) as string);

            bool count = true;
            if (ip.Contains("count"))
                count = ip["count"].Get<bool>();

            bool blobs = false;
            if (ip.Contains("blobs"))
                blobs = ip["blobs"].Get<bool>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                if (ip.Contains("params"))
                {
                    foreach (Node idx in ip["params"])
                    {
                        cmd.Parameters.AddWithValue("@" + idx.Name, idx.Value);
                    }
                }
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    int idxRecordNo = 0;
                    while (reader.Read())
                    {
                        if (start == -1 || idxRecordNo >= start)
                        {
                            if (end == -1 || idxRecordNo < end)
                            {
                                for (int idxNo = 0; idxNo < reader.VisibleFieldCount; idxNo++)
                                {
                                    if (reader[idxNo] == DBNull.Value)
                                        ip["result"][idxRecordNo.ToString()][reader.GetName(idxNo)].Value = null;
                                    else
                                    {
                                        if (!blobs && reader[idxNo] is byte[])
                                            continue;
                                        ip["result"][idxRecordNo.ToString()][reader.GetName(idxNo)].Value = reader[idxNo];
                                    }
                                }
                            }
                            else if (!count)
                                break;
                        }
                        idxRecordNo += 1;
                    }
                    if (count)
                        ip["record-count"].Value = idxRecordNo;
                }
            }
        }

        /*
         * selects table as file
         */
        [ActiveEvent(Name = "magix.ms.sql.select-as-text")]
        public static void microsoft_sql_select_as_text(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Microsoft.sql",
                    "Microsoft.sql.hyperlisp.inspect.hl",
                    "[magix.ms.sql.select-as-text-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Microsoft.sql",
                    "Microsoft.sql.hyperlisp.inspect.hl",
                    "[magix.ms.sql.select-as-text-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.Contains("file"))
                throw new ArgumentException("you need to supply a [file] parameter to [magix.ms.sql.select-as-text]");

            if (!ip["file"].Contains("connection"))
                throw new ArgumentException("you need to supply a [connection] to connect to a database");

            string connectionString = Expressions.GetExpressionValue(ip["file"]["connection"].Get<string>(), dp, ip, false) as string;
            if (connectionString.IndexOf("web.config:") == 0)
                connectionString = ConfigurationManager.ConnectionStrings[connectionString.Replace("web.config:", "")].ConnectionString;

            if (!ip["file"].Contains("sql"))
                throw new ArgumentException("you need to supply a [sql] to know what query to run");

            string query = Expressions.GetExpressionValue(ip["file"]["sql"].Get<string>(), dp, ip, false) as string;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                if (ip["file"].Contains("params"))
                {
                    foreach (Node idx in ip["file"]["params"])
                    {
                        cmd.Parameters.AddWithValue("@" + idx.Name, idx.Value);
                    }
                }
                connection.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.FieldCount > 1)
                        throw new ArgumentException("select statement to [magix.ms.sql.load-as-file] returned multiple result columns");

                    StringBuilder builder = new StringBuilder();
                    while (reader.Read())
                    {
                        builder.Append(reader[0] + "\r\n");
                    }
                    ip["value"].Value = builder.ToString();
                }
            }
        }

        /*
         * updates and executes sql to ms sql server
         */
        [ActiveEvent(Name = "magix.ms.sql.execute")]
        public static void microsoft_sql_execute(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Microsoft.sql",
                    "Microsoft.sql.hyperlisp.inspect.hl",
                    "[magix.ms.sql.execute-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Microsoft.sql",
                    "Microsoft.sql.hyperlisp.inspect.hl",
                    "[magix.ms.sql.execute-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.Contains("connection"))
                throw new ArgumentException("you need to supply a [connection] to connect to a database");
            string connectionString = Expressions.GetExpressionValue(ip["connection"].Get<string>(), dp, ip, false) as string;
            if (connectionString.IndexOf("web.config:") == 0)
                connectionString = ConfigurationManager.ConnectionStrings[connectionString.Replace("web.config:", "")].ConnectionString;

            if (!ip.Contains("sql"))
                throw new ArgumentException("you need to supply a [sql] to know what query to run");
            string query = Expressions.GetExpressionValue(ip["sql"].Get<string>(), dp, ip, false) as string;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, connection);
                if (ip.Contains("params"))
                {
                    foreach (Node idx in ip["params"])
                    {
                        cmd.Parameters.AddWithValue("@" + idx.Name, idx.Value);
                    }
                }
                connection.Open();
                ip["result"].Value = cmd.ExecuteNonQuery();
            }
        }

        /*
         * plugin for the desktop
         */
        [ActiveEvent(Name = "magix.admin.desktop-shortcuts.magix.ms.sql")]
        public static void magix_admin_desktop_shortcuts_microsoft_sql(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Microsoft.sql",
                    "Microsoft.sql.hyperlisp.inspect.hl",
                    "[magix.admin.desktop-shortcuts.magix.ms.sql-dox].Value");
                return;
            }

            ip["launch-microsoft-sql-server-manager"]["text"].Value = "ms sql query tool";
            ip["launch-microsoft-sql-server-manager"]["category"].Value = "applications";
            ip["launch-microsoft-sql-server-manager"]["code"]["execute-script"]["file"].Value = "plugin:magix.file.load-from-resource";
            ip["launch-microsoft-sql-server-manager"]["code"]["execute-script"]["file"]["assembly"].Value = "Microsoft.sql";
            ip["launch-microsoft-sql-server-manager"]["code"]["execute-script"]["file"]["resource-name"].Value = "Microsoft.sql.hyperlisp.ms-sql-query.hl";
        }
    }
}


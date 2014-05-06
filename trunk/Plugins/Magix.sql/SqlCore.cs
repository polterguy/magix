/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Data.SqlClient;
using Magix.Core;
using System.Configuration;

namespace Magix.sql
{
    /**
     * class wrapping microsoft sql server, and the ability to select and do updates towards it
     */
    public class SqlCore : ActiveController
    {
        /**
         * selects from ms sql server
         */
        [ActiveEvent(Name = "microsoft.sql.select")]
        public static void microsoft_sql_select(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>returns an sql query as a list of nodes, 
underneath the [result] node, from a microsoft sql server</p><p>add up parameters underneath 
the [params] node, and make sure you have a valid connection string to an ms sql database in 
your [connection] parameter.&nbsp;&nbsp;put the actual sql query in the [query] parameter 
node</p><p>both [query] and [connection] can be either expressions or constant values.&nbsp;
&nbsp;if you wish, you can de-reference a connection string from your web.config file, instead 
of typing in the connection string in code by prefixing the [connection] value with web.config:
NamedConnection, and such reference the connection string from your web.config called 
""NamedConnection""</p><p>thread safe</p>";
                e.Params["microsoft.sql.select"]["connection"].Value = "Data Source=(localdb)\\v11.0;Initial Catalog=Northwind;Integrated Security=True";
                e.Params["microsoft.sql.select"]["query"].Value = "select * from Customers where ContactTitle=@ContactTitle";
                e.Params["microsoft.sql.select"]["params"]["ContactTitle"].Value = "owner";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Get<Node>();

            if (!ip.Contains("connection"))
                throw new ArgumentException("you need to supply a [connection] to connect to a database");
            string connectionString = Expressions.GetExpressionValue(ip["connection"].Get<string>(), dp, ip, false) as string;
            if (connectionString.IndexOf("web.config:") == 0)
                connectionString = ConfigurationManager.ConnectionStrings[connectionString.Replace("web.config:", "")].ConnectionString;

            if (!ip.Contains("query"))
                throw new ArgumentException("you need to supply a [query] to know what query to run");
            string query = Expressions.GetExpressionValue(ip["query"].Get<string>(), dp, ip, false) as string;

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
                        for (int idxNo = 0; idxNo < reader.VisibleFieldCount; idxNo++)
                        {
                            ip["result"][idxRecordNo.ToString()][reader.GetName(idxNo)].Value = reader[idxNo];
                        }
                        idxRecordNo += 1;
                    }
                }
            }
        }

        /**
         * updates and executes sql to ms sql server
         */
        [ActiveEvent(Name = "microsoft.sql.execute")]
        public static void microsoft_sql_execute(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>executes a non-query sql statement towards 
a microsoft sql server, and returns the number of rows affected in the [result] node</p><p>
add up parameters underneath the [params] node, and make sure you have a valid connection 
string to an ms sql database in your [connection] parameter.&nbsp;&nbsp;put the actual sql 
query in the [query] parameter node</p><p>both [query] and [connection] can be either 
expressions or constant values.&nbsp;&nbsp;if you wish, you can de-reference a connection 
string from your web.config file, instead of typing in the connection string in code by 
prefixing the [connection] value with web.config:NamedConnection, and such reference the 
connection string from your web.config called ""NamedConnection""</p><p>thread safe</p>";
                e.Params["microsoft.sql.execute"]["connection"].Value = "Data Source=(localdb)\\v11.0;Initial Catalog=Northwind;Integrated Security=True";
                e.Params["microsoft.sql.execute"]["query"].Value = "update Customers set ContactTitle='big boss' where ContactTitle=@ContactTitle";
                e.Params["microsoft.sql.execute"]["params"]["ContactTitle"].Value = "Owner";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Get<Node>();

            if (!ip.Contains("connection"))
                throw new ArgumentException("you need to supply a [connection] to connect to a database");
            string connectionString = Expressions.GetExpressionValue(ip["connection"].Get<string>(), dp, ip, false) as string;
            if (connectionString.IndexOf("web.config:") == 0)
                connectionString = ConfigurationManager.ConnectionStrings[connectionString.Replace("web.config:", "")].ConnectionString;

            if (!ip.Contains("query"))
                throw new ArgumentException("you need to supply a [query] to know what query to run");
            string query = Expressions.GetExpressionValue(ip["query"].Get<string>(), dp, ip, false) as string;

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

        /**
         * plugin for the desktop
         */
        [ActiveEvent(Name = "magix.admin.desktop-shortcuts.microsoft.sql")]
        public static void magix_admin_desktop_shortcuts_microsoft_sql(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>returns the desktop shortcut for 
the microsoft.sql components</p><p>thread safe</p>";
                return;
            }

            Node ip = Ip(e.Params);
            ip["launch-microsoft-sql-server-manager"]["text"].Value = "ms sql query tool";
            ip["launch-microsoft-sql-server-manager"]["category"].Value = "tools";
            ip["launch-microsoft-sql-server-manager"]["code"]["execute-file"].Value = "system42/admin/tools/ms-sql-query.hl";
        }
    }
}


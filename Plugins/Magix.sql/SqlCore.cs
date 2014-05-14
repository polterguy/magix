/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;
using Magix.Core;

namespace Magix.sql
{
    /*
     * class wrapping microsoft sql server, and the ability to select and do updates towards it
     */
    public class SqlCore : ActiveController
    {
        /*
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
""NamedConnection""</p><p>if you supply a [start] and [end] node, then the query will only 
return the subsection of result inbetween the integer value from [start] and [end], which is 
useful for queries returning huge results.&nbsp;&nbsp;[start] and [end] are optional, and if 
they are not given, the query will return all records matching your sql.&nbsp;&nbsp;[start] 
and [end] can be either constants or expressions.&nbsp;&nbsp;if you supply a [start] and [end] 
node, then the active event will return the number of records totally in the query in the 
[record-count] return node, unless you also supply a [count] node, with the value of false
</p><p>thread safe</p>";
                e.Params["microsoft.sql.select"]["connection"].Value = "Data Source=(localdb)\\v11.0;Initial Catalog=Northwind;Integrated Security=True";
                e.Params["microsoft.sql.select"]["start"].Value = 0;
                e.Params["microsoft.sql.select"]["end"].Value = 20;
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

            int start = -1;
            if (ip.Contains("start"))
                start = ip["start"].Get<int>();

            int end = -1;
            if (ip.Contains("end"))
                end = ip["end"].Get<int>();

            bool count = true;
            if (ip.Contains("count"))
                count = ip["count"].Get<bool>();

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
                                    ip["result"][idxRecordNo.ToString()][reader.GetName(idxNo)].Value = reader[idxNo];
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
        [ActiveEvent(Name = "microsoft.sql.select-as-text")]
        public static void microsoft_sql_select_as_text(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>selects sql select queries as plain text</p>
<p>this is useful to supply as a plugin for [magix.file.load], since it can transparently load 
information from a database and treat it as if it was a file object.&nbsp;&nbsp;add up parameters 
beneath the [file]/[params] node, and make sure you have a valid connection string to an ms sql database 
in your [file]/[connection] parameter.&nbsp;&nbsp;put the actual sql query in the [file]/[query] parameter 
node, and make sure your query only returns one column.&nbsp;&nbsp;the result from the query, will 
be appended into the [value] node, with a carriage return following every result, making possible 
to treat multiple rows from a database as if it was one piece of text</p><p>both [query] and 
[connection] can be either expressions or constant values.&nbsp;&nbsp;if you wish, you can 
de-reference a connection string from your web.config file, instead of typing in the connection 
string in code by prefixing the [connection] value with web.config:NamedConnection, and such 
reference the connection string from your web.config called ""NamedConnection""</p><p>thread 
safe</p>";
                e.Params["microsoft.sql.load-as-file"]["file"]["connection"].Value = "Data Source=(localdb)\\v11.0;Initial Catalog=Northwind;Integrated Security=True";
                e.Params["microsoft.sql.load-as-file"]["file"]["query"].Value = "select * from Customers where ContactTitle=@ContactTitle";
                e.Params["microsoft.sql.load-as-file"]["file"]["params"]["ContactTitle"].Value = "owner";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Get<Node>();

            if (!ip.Contains("file"))
                throw new ArgumentException("you need to supply a [file] parameter to [microsoft.sql.select-as-text]");

            if (!ip["file"].Contains("connection"))
                throw new ArgumentException("you need to supply a [connection] to connect to a database");

            string connectionString = Expressions.GetExpressionValue(ip["file"]["connection"].Get<string>(), dp, ip, false) as string;
            if (connectionString.IndexOf("web.config:") == 0)
                connectionString = ConfigurationManager.ConnectionStrings[connectionString.Replace("web.config:", "")].ConnectionString;

            if (!ip["file"].Contains("query"))
                throw new ArgumentException("you need to supply a [query] to know what query to run");

            string query = Expressions.GetExpressionValue(ip["file"]["query"].Get<string>(), dp, ip, false) as string;

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
                        throw new ArgumentException("select statement to [microsoft.sql.load-as-file] returned multiple result columns");

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
            ip["launch-microsoft-sql-server-manager"]["code"]["execute-script"]["file"].Value = "plugin:magix.file.load-from-resource";
            ip["launch-microsoft-sql-server-manager"]["code"]["execute-script"]["file"]["assembly"].Value = "Magix.sql";
            ip["launch-microsoft-sql-server-manager"]["code"]["execute-script"]["file"]["resource-name"].Value = "Magix.sql.hyperlisp.ms-sql-query.hl";
        }
    }
}


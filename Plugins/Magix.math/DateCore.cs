/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using System.Globalization;

namespace Magix.date
{
	/*
	 * contains the magix.date main active events
	 */
	public class DateCore : ActiveController
	{
        /*
         * returns the "now" date
         */
        [ActiveEvent(Name = "magix.date.now")]
        public static void magix_date_now(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.date.now-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.date.now-sample]");
                return;
            }

            ip["value"].Value = DateTime.Now;
        }

        /*
         * formats date and returns as text
         */
        [ActiveEvent(Name = "magix.date.format")]
        public static void magix_date_format(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.date.format-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.date.format-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            DateTime date = Expressions.GetExpressionValue<DateTime>(ip["date"].Get<string>(), dp, ip, false);

            bool useLocalCulture = ip.GetValue("current-culture", false);

            ip["value"].Value = date.ToString(
                Expressions.GetExpressionValue<string>(ip["format"].Get<string>(), dp, ip, false),
                useLocalCulture ? CultureInfo.CurrentCulture : CultureInfo.InvariantCulture);
        }

        /*
         * formats date and returns as text
         */
        [ActiveEvent(Name = "magix.date.format-offset")]
        public static void magix_date_format_offset(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.date.format-offset-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.date.format-offset-sample]");
                return;
            }

            Node dp = Dp(e.Params);
            DateTime date = Expressions.GetExpressionValue<DateTime>(ip["date"].Get<string>(), dp, ip, false);
            CreateTimeStampFromNow(ip, date);
        }

        /*
         * returns timestamp from now
         */
        private static void CreateTimeStampFromNow(Node ip, DateTime date)
        {
            if (date < DateTime.Now)
            {
                TimeSpan span = DateTime.Now - date;
                if (span.TotalDays > 730)
                    ip["value"].Value = string.Format("{0} years ago", (int)(span.TotalDays / 365));
                else if (span.TotalDays > 365)
                    ip["value"].Value = "1 year ago";
                else if (span.TotalDays > 62)
                    ip["value"].Value = string.Format("{0} months ago", (int)(span.TotalDays / 30));
                else if (span.TotalDays > 31)
                    ip["value"].Value = "1 month ago";
                else if (span.TotalDays > 13)
                    ip["value"].Value = string.Format("{0} weeks ago", (int)(span.TotalDays / 7));
                else if (span.TotalDays > 7)
                    ip["value"].Value = "1 week ago";
                else if (span.TotalHours > 47)
                    ip["value"].Value = string.Format("{0} days ago", (int)(span.TotalHours / 24));
                else if (span.TotalHours > 24)
                    ip["value"].Value = "1 day ago";
                else if (span.TotalMinutes > 119)
                    ip["value"].Value = string.Format("{0} hours ago", (int)(span.TotalMinutes / 60));
                else if (span.TotalMinutes > 60)
                    ip["value"].Value = "1 hour ago";
                else if (span.TotalMinutes > 5)
                    ip["value"].Value = string.Format("{0} minutes ago", (int)span.TotalMinutes);
                else
                    ip["value"].Value = "just now";
            }
            else
            {
                TimeSpan span = date - DateTime.Now;
                if (span.TotalDays > 730)
                    ip["value"].Value = string.Format("{0} years from now", (int)(span.TotalDays / 365));
                else if (span.TotalDays > 365)
                    ip["value"].Value = "1 year from now";
                else if (span.TotalDays > 62)
                    ip["value"].Value = string.Format("{0} months from now", (int)(span.TotalDays / 30));
                else if (span.TotalDays > 31)
                    ip["value"].Value = "1 month from now";
                else if (span.TotalDays > 13)
                    ip["value"].Value = string.Format("{0} weeks from now", (int)(span.TotalDays / 7));
                else if (span.TotalDays > 7)
                    ip["value"].Value = "1 week from now";
                else if (span.TotalHours > 47)
                    ip["value"].Value = string.Format("{0} days from now", (int)(span.TotalHours / 24));
                else if (span.TotalHours > 24)
                    ip["value"].Value = "1 day from now";
                else if (span.TotalMinutes > 119)
                    ip["value"].Value = string.Format("{0} hours from now", (int)(span.TotalMinutes / 60));
                else if (span.TotalMinutes > 60)
                    ip["value"].Value = "1 hour from now";
                else if (span.TotalMinutes > 5)
                    ip["value"].Value = string.Format("{0} minutes from now", (int)span.TotalMinutes);
                else
                    ip["value"].Value = "now";
            }
        }
    }
}


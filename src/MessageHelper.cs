﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HL7.Dotnetcore
{
    public static class MessageHelper
    {
        private static string[] lineSeparators = { "\r\n", "\n\r", "\r", "\n" };

        public static List<string> SplitString(string strStringToSplit, string splitBy, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return strStringToSplit.Split(new string[] { splitBy }, splitOptions).ToList();
        }

        public static List<string> SplitString(string strStringToSplit, char chSplitBy, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return strStringToSplit.Split(new char[] { chSplitBy }, splitOptions).ToList();
        }
        
        public static List<string> SplitString(string strStringToSplit, char[] chSplitBy, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return strStringToSplit.Split(chSplitBy, splitOptions).ToList();
        }

        public static List<string> SplitMessage(string message)
        {
            return message.Split(lineSeparators, StringSplitOptions.None).Where(m => !string.IsNullOrWhiteSpace(m)).ToList();
        }

        public static string LongDateWithFractionOfSecond(DateTime dt)
        {
            return dt.ToString("yyyyMMddHHmmss.FFFF");
        }

        public static string[] ExtractMessages(string messages)
        {
            var expr = "\x0B(.*?)\x1C\x0D";
            var matches = Regex.Matches(messages, expr, RegexOptions.Singleline);
            
            var list = new List<string>();
            foreach (Match m in matches)
                list.Add(m.Groups[1].Value);

            return list.ToArray();
        }

        public static DateTime? ParseDateTime(string dateTimeString)
        {
            TimeSpan offset;
            return ParseDateTime(dateTimeString, out offset);
        }

        public static DateTime? ParseDateTime(string dateTimeString, out TimeSpan offset)
        {
            var expr = @"^\s*((?:19|20)[0-9]{2})(?:(1[0-2]|0[1-9])(?:(3[0-1]|[1-2][0-9]|0[1-9])(?:([0-1][0-9]|2[0-3])(?:([0-5][0-9])(?:([0-5][0-9](?:\.[0-9]{1,4})?)?)?)?)?)?)?(?:([+-][0-1][0-9]|[+-]2[0-3])([0-5][0-9]))?\s*$";
            var matches = Regex.Matches(dateTimeString, expr, RegexOptions.Singleline);

            try
            {
                if (matches.Count != 1)
                    return null;
                
                var groups = matches[0].Groups;
                int year = int.Parse(groups[1].Value);
                int month = groups[2].Success ? int.Parse(groups[2].Value) : 1;
                int day = groups[3].Success ? int.Parse(groups[3].Value) : 1;
                int hours = groups[4].Success ? int.Parse(groups[4].Value) : 0;
                int mins = groups[5].Success ? int.Parse(groups[5].Value) : 0;

                float fsecs = groups[6].Success ? float.Parse(groups[6].Value) : 0;
                int secs = (int)Math.Truncate(fsecs);
                int msecs = (int)Math.Truncate(fsecs * 1000) % 1000;

                int tzh = groups[7].Success ? int.Parse(groups[7].Value) : 0;
                int tzm = groups[8].Success ? int.Parse(groups[8].Value) : 0;
                offset = new TimeSpan(tzh, tzm, 0);
                
                return new DateTime(year, month, day, hours, mins, secs, msecs);
            }
            catch
            {
                return null;
            }
        }
    }
}

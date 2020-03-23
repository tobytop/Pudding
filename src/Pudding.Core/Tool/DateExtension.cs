using System;

namespace Pudding.Core.Tool
{
    public static class DateExtension
    {
        public static long ToIntMs(this DateTime time)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            return (long)(time - startTime).TotalMilliseconds;
        }

        public static long ToIntS(this DateTime time)
        {
            DateTime startTime = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            return (long)(time - startTime).TotalSeconds;
        }

        public static DateTime ToDateMs(this long time)
        {
            DateTime dateTimeStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            long lTime = long.Parse(time + "0000");
            TimeSpan toNow = new TimeSpan(lTime);
            return dateTimeStart.Add(toNow);
        }

        public static DateTime ToDateS(this long time)
        {
            DateTime dateTimeStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);
            TimeSpan toNow = new TimeSpan(time);
            return dateTimeStart.Add(toNow);
        }
    }
}

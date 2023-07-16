using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace smsCore.Data.Helpers
{
    public class DateTimeHelper
    {
        public static string dateFormat => "dd/MM/yyyy";

        public static DateTime ConvertDate(string date, bool defaultDate = false, string format = "", bool dontAppendCurrentTime = false)
        {

            if (string.IsNullOrEmpty(format)) format = dateFormat;
            try
            {
                DateTime dt = DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
                if (dontAppendCurrentTime == false & dt.TimeOfDay == TimeSpan.MinValue)
                {
                    dt = dt.Add(DateTime.Now.TimeOfDay);
                }
                return dt;
            }
            catch//(Exception ex) 
            {
                if (defaultDate)
                    return DateTime.Now;
                else return DateTime.MinValue;
                ////throw ex;
            }
        }
    }
}
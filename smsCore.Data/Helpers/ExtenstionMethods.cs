using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;
using Models;
using PropertyAttributes = System.Reflection.PropertyAttributes;

namespace smsCore.Data.Helpers
{
    public static class ExtenstionMethods
    {
        public static List<dynamic> ToDynamicList(this DataTable table)
        {
            List<dynamic> list = new List<dynamic>();

            foreach (DataRow row in table.Rows)
            {
                dynamic obj = new System.Dynamic.ExpandoObject();

                foreach (DataColumn col in table.Columns)
                {
                    var propertyName = col.ColumnName;
                    var propertyValue = row[col];

                    ((IDictionary<string, object>)obj)[propertyName] = propertyValue;
                }

                list.Add(obj);
            }

            return list;
        }

        public static string[] MonthNames =
        {
            "January", "February", "March", "April", "May", "june", "July", "August", "September", "October",
            "November", "December"
        };

        public static DataTable AsDataTable<T>(this IEnumerable<T> enumberable)
        {
            DataTable table = new DataTable("Generated");
            T first = enumberable.FirstOrDefault();
            if (first == null)
                return table;

            PropertyInfo[] properties = first.GetType().GetProperties();
            foreach (PropertyInfo pi in properties)
                if (pi.Name != string.Empty)
                    if (!table.Columns.Contains(pi.Name))
                    {
                        table.Columns.Add(pi.Name, Nullable.GetUnderlyingType(pi.PropertyType) ?? pi.PropertyType);
                        //table.Columns[pi.Name].AllowDBNull = true;
                    }
            foreach (T t in enumberable)
            {
                DataRow row = table.NewRow();
                foreach (PropertyInfo pi in properties)
                    row[pi.Name] = t.GetType().InvokeMember(pi.Name, BindingFlags.GetProperty, null, t, null) ?? DBNull.Value;

                table.Rows.Add(row);
            }

            return table;
        }
        public static int ToInt(this string m)
        {
            m = m.ToLower();
            //switch (m.ToLower())
            {
                if (m.Contains("jan"))
                    return 1;
                if (m.Contains("feb"))
                    return 2;
                if (m.Contains("march"))
                    return 3;
                if (m.Contains("april"))
                    return 4;
                if (m.Contains("may"))
                    return 5;
                if (m.Contains("june"))
                    return 6;
                if (m.Contains("july"))
                    return 7;
                if (m.Contains("august"))
                    return 8;
                if (m.Contains("septem"))
                    return 9;
                if (m.Contains("october"))
                    return 10;
                if (m.Contains("november"))
                    return 11;
                if (m.Contains("december"))
                    return 12;

            }
            return 0;
        }
        public static int DayToInt(this string d)
        {
            int re = 0;
            switch (d.ToLower())
            {
                case "sunday":
                    re = 1;
                    break;
                case "monday":
                    re = 2;
                    break; ;
                case "tuesday":
                    re = 3;
                    break;
                case "wednesday":
                    re = 4;
                    break;
                case "thursday":
                    re = 5;
                    break;
                case "friday":
                    re = 6;
                    break;
                case "saturday":
                    re = 7;
                    break;
            }
            return re;
        }
        public static DataTable ToVertical(this DataTable t)
        {
            DataTable newTable = new DataTable();

            int cols = t.Columns.Count;
            int rows = t.Rows.Count;
            if (rows == 1)
            {
                newTable.Columns.Add("fields1");
                newTable.Columns.Add("values1");
                newTable.Columns.Add("fields2");
                newTable.Columns.Add("Values2");
                for (int i = 0; i < cols; i++)
                {
                    DataRow ro = newTable.NewRow();
                    string colname = t.Columns[i].ColumnName;
                    string value = t.Rows[0][i].ToString();
                    ro[0] = colname;
                    ro[1] = value;
                    colname = "";
                    value = "";
                    i++;
                    if (i != cols)
                    {
                        colname = t.Columns[i].ColumnName;
                        value = t.Rows[0][i].ToString();
                    }
                    ro[2] = colname;
                    ro[3] = value;

                    newTable.Rows.Add(ro);
                }
            }
            else
            {
                newTable.Columns.Add("fields");
                for (int i = 0; i < rows - 1; i++)
                    newTable.Columns.Add("value" + i + 1);
            }



            return newTable;
        }

        public static DataTable ToSingleVertical(this DataTable t)
        {
            DataTable newTable = new DataTable();

            int cols = t.Columns.Count;
            int rows = t.Rows.Count;
            newTable.Columns.Add("fields1");
            newTable.Columns.Add("values1");
            for (int i = 0; i < cols; i++)
            {
                DataRow ro = newTable.NewRow();
                string colname = t.Columns[i].ColumnName;
                string value = t.Rows[0][i].ToString();
                ro[0] = colname;
                ro[1] = value;
                newTable.Rows.Add(ro);
            }
            return newTable;
        }

        public static string ToPosition(this int p)
        {
            string pos = "";
            switch (p)
            {
                case 0:
                    pos = "Fail";
                    break;
                case 1:
                    pos = "1st";
                    break;
                case 2:
                    pos = "2nd";
                    break;
                case 3:
                    pos = "3rd";
                    break;
                default:
                    pos = p + "th";
                    break;
            }

            return pos;
        }
        public static string GetDayNameFromInt(this int index)
        {
            return System.Globalization.DateTimeFormatInfo.InvariantInfo.DayNames[index];
        }


        public static string ToFormattedString(this int number)
        {
            string returnStr = number.ToString();

            switch (returnStr.Length)
            {
                case 1:
                    return "000" + returnStr;
                case 2:
                    return "00" + returnStr;
                case 3:
                    return "0" + returnStr;
                default: return returnStr;
            }
        }

        public static DayOfWeek GetDayOfWeekFromName(string day)
        {
            DayOfWeek dayOfWeek = DayOfWeek.Friday;
            switch (day)
            {
                case "Sunday":
                    dayOfWeek = DayOfWeek.Sunday;
                    break;

                case "Monday":
                    dayOfWeek = DayOfWeek.Monday;
                    break;

                case "Tuesday":
                    dayOfWeek = DayOfWeek.Tuesday;
                    break;

                case "Wednesday":
                    dayOfWeek = DayOfWeek.Wednesday;
                    break;

                case "Thursday":
                    dayOfWeek = DayOfWeek.Thursday;
                    break;

                case "Friday":
                    dayOfWeek = DayOfWeek.Friday;
                    break;

                case "Saturday":
                    dayOfWeek = DayOfWeek.Saturday;
                    break;
            }
            return dayOfWeek;
        }


        public static string[] GetDays { get { return System.Globalization.DateTimeFormatInfo.InvariantInfo.DayNames; } }
        public static string[] GetMonths { get { return System.Globalization.DateTimeFormatInfo.InvariantInfo.MonthNames; } }
        //private static readonly SchoolEntities Db = new SchoolEntities();

        public static DateTime Now()
        {
            var date1 = DateTime.UtcNow;
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            var date2 = TimeZoneInfo.ConvertTime(date1, tz);
            return date2;
        }

        public static DateTime ToMyTime(this DateTime date1)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            var date2 = TimeZoneInfo.ConvertTime(date1, tz);
            return date2;
        }

        //public static string GetPhoto(int regno)
        //{
        //    var photo = Db.StudentPhotos.AsNoTracking().Where(w => w.Student.RegistrationNo == regno);
        //    if (!photo.Any()) return "/Uploads/images/user.png";
        //    var ph = photo.ToList().LastOrDefault()?.StudentImage;

        //    return GetPhoto(ph);
        //}
        //public static string GetPhoto(string code)
        //{
        //    var photo = Db.tbl_Employee.AsNoTracking().Where(w => w.employeeCode == code);
        //    if (!photo.Any()) return "/Uploads/images/user.png";
        //    var ph = photo.ToList().LastOrDefault()?.Photo;

        //    return GetPhoto(ph);
        //}
        //public static string GetPhoto(byte[] myByteArray)
        //{
        //    var photo = "/Uploads/images/user.png";
        //    if (myByteArray != null)
        //        photo = "data:image/jpeg;base64," + Convert.ToBase64String(myByteArray);
        //    return photo;
        //}


        //public static bool IsAuthorized1(this EnumManager.Modules module, EnumManager.Actions action)
        //{
        //    return ClaimHelper.GetRolesFromClaims().Contains("Admin") || ClaimHelper.GetAuthenticatedFormFromClaims1()
        //        .Any(w => w.formId == module.GetId() && w.action == action.ToString());
        //}


        public static string ToWord(this int position)
        {
            var pos = "Fail";

            if (position == 1)
                pos = "1st";
            else if (position == 2)
                pos = "2nd";
            else if (position == 3)
                pos = "3rd";
            else if (position > 3) pos = position + "th";
            return pos;
        }


        //public static DataTable AsDataTable<T>(this IEnumerable<T> enumberable)
        //{
        //    var table = new DataTable("Generated");

        //    var first = enumberable.FirstOrDefault();
        //    if (first == null)
        //        return table;

        //    var properties = first.GetType().GetProperties();
        //    foreach (var pi in properties)
        //        if (pi.Name != string.Empty)
        //            if (!table.Columns.Contains(pi.Name))
        //                table.Columns.Add(pi.Name, pi.PropertyType);

        //    foreach (var t in enumberable)
        //    {
        //        var row = table.NewRow();
        //        foreach (var pi in properties)
        //            row[pi.Name] = t.GetType().InvokeMember(pi.Name, BindingFlags.GetProperty, null, t, null);

        //        table.Rows.Add(row);
        //    }

        //    return table;
        //}

        private static T GetItem<T>(this DataRow dr)
        {
            var temp = typeof(T);
            var obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            foreach (var pro in temp.GetProperties())
                if (pro.Name == column.ColumnName)
                    try
                    {
                        pro.SetValue(obj, dr[column.ColumnName], null);
                    }
                    catch
                    {
                    }
                else
                    continue;

            return obj;
        }

        public static object Validate(this object o, List<string> excludedProperties = null)
        {
            var ps = o.GetType().GetProperties();
            foreach (var p in ps)
            {
                if (p.GetValue(o, null) != null)
                    continue;
                if (!excludedProperties.Contains(p.Name))
                    if (p.CanWrite)
                    {
                        object value = null;
                        var type = p.PropertyType;
                        if (type == typeof(int) || type == typeof(long) || type == typeof(double) ||
                            type == typeof(decimal) || type == typeof(short) || type == typeof(int) ||
                            type == typeof(long))
                            value = 0;
                        else if (type == typeof(string))
                            value = string.Empty;
                        else if (type == typeof(bool))
                            value = false;
                        else if (type == typeof(DateTime))
                            value = DateTime.Now;
                        p.SetValue(o, value, null);
                    }
            }

            return o;
        }

        public static decimal GetId(this EnumManager.Modules module, bool fromdb = false)
        {
            decimal id = 0;
            if (fromdb)
            {
                //var modules = new string[] { };
                //Db.AppForms.ToList()
                  //  .FirstOrDefault(w => w.FormName.ToLower() == module.ToString().ToLower());
                //if (modules != null) id = modules.ID;
            }
            else
            {
                return (decimal)module; // Convert.ToDecimal(((int)module).ToString());
            }

            return id;
        }

        public static decimal[] GetId(this EnumManager.Modules[] module, bool fromdb = false)
        {
            decimal[] id = //{ };
            //id = fromdb
            //    ? Db.AppForms.ToList()
            //        .Where(w => module.Select(s => s.ToString().ToLower()).Contains(w.FormName.ToLower()))
            //        .Select(s => s.ID).ToArray()
                //: 
                module.Select(s => (decimal)s).ToArray();
            return id.ToArray();
        }

        #region "Convert DataTable to List<dynamic>"

        //public static List<dynamic> ToDynamicList(this DataTable dt)
        //{
        //    var cols = dt.Columns.Cast<DataColumn>().Select(column => column.ColumnName).ToList();
        //    return ToDynamicList(ToDictionary(dt), getNewObject(cols));
        //}

        public static List<Dictionary<string, object>> ToDictionary(DataTable dt)
        {
            var columns = dt.Columns.Cast<DataColumn>();
            var Temp = dt.AsEnumerable().Select(dataRow => columns.Select(column =>
                    new {Column = column.ColumnName, Value = dataRow[column]})
                .ToDictionary(data => data.Column, data => data.Value)).ToList();
            return Temp.ToList();
        }

        public static List<dynamic> ToDynamicList(List<Dictionary<string, object>> list, Type TypeObj)
        {
            dynamic temp = new List<dynamic>();
            foreach (var step in list)
            {
                var Obj = Activator.CreateInstance(TypeObj);
                var properties = Obj.GetType().GetProperties();
                var DictList = step;
                foreach (var keyValuePair in DictList)
                foreach (var property in properties)
                    if (property.Name == keyValuePair.Key)
                    {
                        property.SetValue(Obj, keyValuePair.Value.ToString(), null);
                        break;
                    }

                temp.Add(Obj);
            }

            return temp;
        }

        //private static Type getNewObject(List<string> list)
        //{
        //    var assemblyName = new AssemblyName();
        //    assemblyName.Name = "tmpAssembly";
        //    var assemblyBuilder = Thread.GetDomain().DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        //    var module = assemblyBuilder.DefineDynamicModule("tmpModule");
        //    var typeBuilder = module.DefineType("WebgridRowCellCollection", TypeAttributes.Public);
        //    foreach (var step in list)
        //    {
        //        var propertyName = step;
        //        var field = typeBuilder.DefineField(propertyName, typeof(string), FieldAttributes.Public);
        //        var property = typeBuilder.DefineProperty(propertyName, PropertyAttributes.None, typeof(string),
        //            new[] {typeof(string)});
        //        var GetSetAttr = MethodAttributes.Public | MethodAttributes.HideBySig;
        //        var currGetPropMthdBldr =
        //            typeBuilder.DefineMethod("get_value", GetSetAttr, typeof(string), Type.EmptyTypes);
        //        var currGetIL = currGetPropMthdBldr.GetILGenerator();
        //        currGetIL.Emit(OpCodes.Ldarg_0);
        //        currGetIL.Emit(OpCodes.Ldfld, field);
        //        currGetIL.Emit(OpCodes.Ret);
        //        var currSetPropMthdBldr =
        //            typeBuilder.DefineMethod("set_value", GetSetAttr, null, new[] {typeof(string)});
        //        var currSetIL = currSetPropMthdBldr.GetILGenerator();
        //        currSetIL.Emit(OpCodes.Ldarg_0);
        //        currSetIL.Emit(OpCodes.Ldarg_1);
        //        currSetIL.Emit(OpCodes.Stfld, field);
        //        currSetIL.Emit(OpCodes.Ret);
        //        property.SetGetMethod(currGetPropMthdBldr);
        //        property.SetSetMethod(currSetPropMthdBldr);
        //    }

        //    var obj = typeBuilder.CreateType();
        //    return obj;
        //}

        #endregion
    }
}
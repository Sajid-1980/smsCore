using Microsoft.EntityFrameworkCore;
using Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace smsCore.Data.Helpers
{
    public  class Extensions1
    {
        ClsBussinessSetting setting;
        SchoolEntities db;
        public Extensions1(ClsBussinessSetting _setting, SchoolEntities _db)
        {
            setting = _setting;
            db = _db;
        }

        public  ClsBussinessSetting GetConfig(PublicVariables.EnumConfigurations configurations, int campusId)
        {
            setting.CampusId=campusId;
           return setting.Read(configurations.ToString());
        }
        public  string GetConfigValue(PublicVariables.EnumConfigurations configurations, int campusId)
        {
            setting.CampusId = campusId;
            var data= setting.Read(configurations.ToString());
            if (data == null)
                return null;
            else return data.PropertyValue.Trim();
        }
        public  object GetConfigValue(PublicVariables.EnumConfigurations configurations, Type T,int campusId)
        {
            setting.CampusId = campusId;
            return setting.ReadWithType(configurations.ToString(), T);
        }

        public int GetForEmployeeId(PublicVariables.AttendanceType attendance)
        {
            string type = attendance.ToString();
            var data = db.EmployeeAttendanceTypes.ToList().Where(w => w.AttendanceName.Trim().ToLower() == type.Trim().ToLower()).FirstOrDefault();
            if (data == null)
            {

                using (var transaction = db.Database.BeginTransaction())
                {
                    data = new EmployeeAttendanceType() { AttendanceName = type.ToString(), Code = attendance.ToString().Substring(0, 2), FineInDays = 0, YearlyAllowed = 0, ID = (int)attendance };
                    db.EmployeeAttendanceTypes.Add(data);
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT EmployeeAttendanceType ON;");
                    db.SaveChanges();
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT EmployeeAttendanceType OFF");
                    transaction.Commit();
                }
            }
            return data.ID;
        }
        public  int GetForStudentId(PublicVariables.AttendanceType attendance)
        {
            
            //string type = attendance == PublicVariables.AttendanceType.LateComing ? "Late Coming" : attendance.ToString();
            string type = attendance.ToString();
            var data = db.StudentAttendanceTypes.ToList().Where(w => w.AttendanceName.Trim().ToLower() == type.Trim().ToLower()).FirstOrDefault();
            if (data == null)
            {

                using (var transaction = db.Database.BeginTransaction())
                {
                    data = new StudentAttendanceType() { AttendanceName = type, Code = attendance.ToString().Substring(0, 2), ID = (int)attendance };
                    db.StudentAttendanceTypes.Add(data);
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT StudentAttendanceTypes ON;");
                    db.SaveChanges();
                    db.Database.ExecuteSqlRaw("SET IDENTITY_INSERT StudentAttendanceTypes OFF");
                    transaction.Commit();
                }
            }
            return data.ID;
        }

        public  string GetPhoto(int regno)
        {
            var photo = db.StudentPhotos.AsNoTracking().Where(w => w.Student.RegistrationNo == regno);
            if (!photo.Any()) return "/Uploads/images/user.png";
            var ph = photo.ToList().LastOrDefault()?.StudentImage;

            return GetPhoto(ph);
        }
        public  string GetPhoto(string code)
        {
            var photo = db.tbl_Employee.AsNoTracking().Where(w => w.employeeCode == code);
            if (!photo.Any()) return "/Uploads/images/user.png";
            var ph = photo.ToList().LastOrDefault()?.Photo;

            return GetPhoto(ph);
        }
        public  string GetPhoto(byte[] myByteArray)
        {
            var photo = "/Uploads/images/user.png";
            if (myByteArray != null)
                photo = "data:image/jpeg;base64," + Convert.ToBase64String(myByteArray);
            return photo;
        }
        //public  IEnumerable<object[]> ExecuteQuery(this schoolEntities ctx, string query)
        //{

        //    System.Windows.Forms.MessageBox.Show(query);
        //    using (DbCommand cmd = ctx.Connection.CreateCommand())
        //    {
        //        cmd.CommandText = query;
        //        ctx.Connection.Open();
        //        using (DbDataReader rdr =
        //            cmd.ExecuteReader(CommandBehavior.CloseConnection))
        //        {
        //            while (rdr.Read())
        //            {
        //                object[] res = new object[rdr.FieldCount];
        //                rdr.GetValues(res);
        //                yield return res;
        //            }
        //        }
        //    }
        //}
        public string GetGrades(double m, int ExamRulId)
        {
            var rule = db.ExamsRules.Where(ex => ex.ID == ExamRulId).FirstOrDefault();
            string GradeKey = "Fail";
            if (rule != null)
            {
               // System.Windows.Forms.MessageBox.Show(m.ToString());
                if (m > rule.APlus)
                    GradeKey = "A+";
                else if (m >= rule.A && m < rule.APlus)
                    GradeKey = "A";
                else if (m >= rule.BPlus && m < rule.A)
                    GradeKey = "B+";
                else if (m >= rule.B && m < rule.BPlus)
                    GradeKey = "B";
                else if (m >= rule.C && m < rule.B)
                    GradeKey = "C";
                else if (m >= rule.D && m < rule.C)
                    GradeKey = "D";
                else if (m < rule.E)
                    GradeKey = "E";
            }
            return GradeKey;
        }

       

        //public  List<T> AsDataTable<T>(this DataTable tab)
        //{

        //    List<T> newList = new List<T>();

        //    tab.Rows.OfType<DataRow>().Select(dr => dr.Field<T>(columnName)).ToList();

        //    PropertyInfo[] properties = first.GetType().GetProperties();
        //    foreach (PropertyInfo pi in properties)
        //        table.Columns.Add(pi.Name, pi.PropertyType);

        //    foreach (T t in enumberable)
        //    {
        //        DataRow row = table.NewRow();
        //        foreach (PropertyInfo pi in properties)
        //            row[pi.Name] = t.GetType().InvokeMember(pi.Name, BindingFlags.GetProperty, null, t, null);
        //        table.Rows.Add(row);
        //    }

        //    return table;
        //}

        //public  ParameterFields ToParameters(this Dictionary<string, object> dic)
        //{
        //    ParameterFields pfds = new ParameterFields();

        //    foreach (KeyValuePair<string, object> k in dic)
        //    {
        //        ParameterField InvAdress = new ParameterField();
        //        ParameterDiscreteValue Add = new ParameterDiscreteValue();
        //        InvAdress.Name = k.Key;
        //        Add.Value = k.Value;
        //        InvAdress.CurrentValues.Add(Add);
        //        pfds.Add(InvAdress);
        //    }

        //    return pfds;
        //}


        public  void SecureMe()
        {

        }

       


    }
}

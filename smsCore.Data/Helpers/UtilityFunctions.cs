using Models;
using smsCore.Data.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.NetworkInformation;

namespace Utilities
{
   public class UtilityFunctions
    {
       
        private readonly SchoolEntities objdb;
        private readonly DatabaseAccessSql dba;
        private readonly ClsBussinessSetting setting;
        public UtilityFunctions(SchoolEntities _db, DatabaseAccessSql _dba, ClsBussinessSetting _setting)
        {
            objdb = _db;
            dba = _dba;
            setting = _setting;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="RegistrationNos"></param>
        /// <param name="campusIds"></param>
        /// <param name="isExpell">null if all, true for current, false for ex students</param>
        /// <param name="type">admissionid,regno,studentid. Default value is admissionid</param>
        /// <returns>return array based on type</returns>
        public int[] ParseAdmIDs(string RegistrationNos, int[] campusIds = null, bool? isExpell = null, string type = "admissionid")
        {
            bool[] isexp = { true, false };
            if (isExpell.HasValue)
            {
                isexp = new bool[] { isExpell.Value };
            }
            string adms = RegistrationNos;
            int[] ids = { };
            var students = objdb.Admissions.Where(w => campusIds.Contains(w.CampuseID) && isexp.Contains(w.IsExpell));
            if (adms.Contains(","))
            {
                List<int> id = new List<int>();
                string[] idsStr = adms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in idsStr)
                {
                    id.Add(int.Parse(s.Replace(",", "").Trim()));

                }
                students = students.Where(w => id.Contains(w.Student.RegistrationNo));
                //ids = id.ToArray();
            }
            else
            {
                try
                {
                    string[] idsStr = adms.Contains("-") ? adms.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { adms, adms };
                    int id1 = (idsStr[0].Contains("-")) ? int.Parse(idsStr[0].Replace("-", "").Trim()) : int.Parse(idsStr[0].Trim());
                    int id2 = (idsStr[1].Contains("-")) ? int.Parse(idsStr[1].Replace("-", "").Trim()) : int.Parse(idsStr[1].Trim());
                    students = students.Where(w => w.Student.RegistrationNo >= id1 && w.Student.RegistrationNo <= id2);
                }
                catch { }
            }
            if (type=="admissionid")
            {
                ids=students.Select(s => s.ID).ToArray();
            }
            else if (type=="regno")
            {
                ids = students.Select(s => s.Student.RegistrationNo).Distinct().ToArray();
            }
            else if (type=="studentid")
            {
                ids= students.Select(s => s.StudentID).Distinct().ToArray();
            }
            return ids;
        }
        /// <summary>
        /// Fetch registration no against the multiple registration nos
        /// </summary>
        /// <param name="RegistrationNos"></param>
        /// <param name="isExpell"></param>
        /// <param name="returnRegNo"></param>
        /// <returns>Registration No array int[]</returns>
        public int[] ParseRegNos(string RegistrationNos,int[] campusIds=null, bool? isExpell = null, bool returnRegNo = true)
        {
            bool[] isexp = { true, false };
            if (isExpell.HasValue)
            {
                isexp = new bool[] { isExpell.Value };
            }
            string adms = RegistrationNos;
            int[] ids = { };
            if (adms.Contains(","))
            {
                List<int> id = new List<int>();
                string[] idsStr = adms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in idsStr)
                {
                    id.Add(int.Parse(s.Replace(",", "").Trim()));

                }
                ids = objdb.Admissions.Where(w => campusIds.Contains(w.CampuseID) && isexp.Contains(w.IsExpell) && id.Contains(w.Student.RegistrationNo)).Select(s => s.Student.RegistrationNo).ToArray();
            }
            else
            {
                try
                {
                    string[] idsStr = adms.Contains("-") ? adms.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { adms, adms };
                    int id1 = (idsStr[0].Contains("-")) ? int.Parse(idsStr[0].Replace("-", "").Trim()) : int.Parse(idsStr[0].Trim());
                    int id2 = (idsStr[1].Contains("-")) ? int.Parse(idsStr[1].Replace("-", "").Trim()) : int.Parse(idsStr[1].Trim());
                    ids = objdb.Admissions.Where(w => campusIds.Contains(w.CampuseID) && isexp.Contains(w.IsExpell) && w.Student.RegistrationNo >= id1 && w.Student.RegistrationNo <= id2).Select(s => s.Student.RegistrationNo).ToArray();
                }
                catch { }
            }
            return ids;
        }

        public static (int[],string) ParseRegNosAsItis(string RegistrationNos)
        {
            string adms = RegistrationNos;

            List<int> ids = new List<int>(); 
            string method = "";
            if (adms.Contains(","))
            {
                string[] idsStr = adms.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in idsStr)
                {
                    ids.Add(int.Parse(s.Replace(",", "").Trim()));
                }
                method = "comma";
            }
            else
            {
                try
                {
                    string[] idsStr = adms.Contains("-") ? adms.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { adms, adms };
                    int id1 = (idsStr[0].Contains("-")) ? int.Parse(idsStr[0].Replace("-", "").Trim()) : int.Parse(idsStr[0].Trim());
                    int id2 = (idsStr[1].Contains("-")) ? int.Parse(idsStr[1].Replace("-", "").Trim()) : int.Parse(idsStr[1].Trim());
                    ids.Add(id1);
                    ids.Add(id2);
                }
                catch { }
            }
            return (ids.ToArray(), method);
        }

        public int[] parseStaffIds(string staffIds, int[] campusIds=null)
        {
            int[] ids = new int[] { };
            List<tbl_Employee> stf = new List<tbl_Employee>();
            string adms = staffIds;

                if (adms.Contains(","))
                {
                    List<decimal> id = new List<decimal>();
                    string[] idsStr = adms.Split(',');
                    foreach (var s in idsStr)
                    {
                        id.Add(decimal.Parse(s.Replace(",", "").Trim()));

                    }
                    ids = objdb.tbl_Employee.Where(w => campusIds.Contains(w.CampusID) && id.Contains(w.Id) && w.LeavedStaffs.Count > 1).Select(s => s.Id).ToArray();
                }
                else if (adms.Contains("-"))
                {
                    try
                    {
                        string[] idsStr = adms.Contains("-") ? adms.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { adms, adms };
                        int id1 = (idsStr[0].Contains("-")) ? int.Parse(idsStr[0].Replace("-", "").Trim()) : int.Parse(idsStr[0].Trim());
                        int id2 = (idsStr[1].Contains("-")) ? int.Parse(idsStr[1].Replace("-", "").Trim()) : int.Parse(idsStr[1].Trim());
                        ids = objdb.tbl_Employee.Where(w => campusIds.Contains(w.CampusID) && w.Id >= id1 && w.Id <= id2).Select(s => s.Id).ToArray();
                    }
                    catch { }
                }
                else
                {
                    ids = new int[] { int.Parse(adms.ToString()) };
                }
            

            return ids;
        }

        public int[] parseStaffCodes(string staffIds, int[] campusIds=null)
        {
            int[] ids = new int[] { };
            List<tbl_Employee> stf = new List<tbl_Employee>();
            string adms = staffIds;

            if (adms.Contains(","))
            {
                List<int> id = new List<int>();
                string[] idsStr = adms.Split(',');
                ids = objdb.tbl_Employee.Where(w => campusIds.Contains(w.CampusID) && idsStr.Contains(w.employeeCode) && w.LeavedStaffs.Count == 0).Select(s => s.Id).ToArray();
            }
            else if (adms.Contains("-"))
            {
                try
                {
                    string[] idsStr = adms.Contains("-") ? adms.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries) : new string[] { adms, adms };
                    int id1 = (idsStr[0].Contains("-")) ? int.Parse(idsStr[0].Replace("-", "").Trim()) : int.Parse(idsStr[0].Trim());
                    int id2 = (idsStr[1].Contains("-")) ? int.Parse(idsStr[1].Replace("-", "").Trim()) : int.Parse(idsStr[1].Trim());
                    ids = objdb.tbl_Employee.Where(w => campusIds.Contains(w.CampusID) && w.Id >= id1 && w.Id <= id2 && w.LeavedStaffs.Count == 0).Select(s => s.Id).ToArray();
                }
                catch { }
            }
            else
            {
                ids = objdb.tbl_Employee.Where(w => campusIds.Contains(w.CampusID) && adms == w.employeeCode && w.LeavedStaffs.Count == 0).Select(s => s.Id).ToArray();
            }


            return ids;
        }


        public  string GetPunctualityRemarks(double percentage)
        {
            var remarksDet = objdb.ExamRemarksDetails.ToList();
            string remarks = "";
            if (percentage >= 95)
            {
                remarks = remarksDet.Where(w => w.Code == 1).FirstOrDefault().Remarks;
            }
            if (percentage >= 90 && percentage < 95)
            {
                remarks = remarksDet.Where(w => w.Code == 2).FirstOrDefault().Remarks;
            }
            if (percentage >= 80 && percentage < 90)
            {
                remarks = remarksDet.Where(w => w.Code == 3).FirstOrDefault().Remarks;
            }
            if (percentage >= 70 && percentage < 80)
            {
                remarks = remarksDet.Where(w => w.Code == 4).FirstOrDefault().Remarks;
            }
            if (percentage < 69)
            {
                remarks = remarksDet.Where(w => w.Code == 5).FirstOrDefault().Remarks;
            }
            return remarks;
        }

        public string GetWorkingDaysInMonth() {
            string ret = "";
            try
            {
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
                int daysinmonth = DateTime.DaysInMonth(year, month);
                string qrymnth = @"SELECT COUNT(ID) AS LeaveSchdule FROM SchoolLeaveSchedule WHERE (MONTH(date) = " + month + ") and (Year(date) = " + year + ")";
                DataTable tab = dba.CreateTable(qrymnth);
                int holydays = int.Parse(tab.Rows[0][0].ToString());

                ret="Working days in current Month : " + (daysinmonth - holydays).ToString() + "/" + daysinmonth;

                return ret;}
            catch { }
            return "";
        }
        public string GetRemainingWorkingDaysInMonth()
        {
            string ret = "";
            try
            {
                int year = DateTime.Now.Year;
                int month = DateTime.Now.Month;
                int day = DateTime.Today.Day;
                int RemainingDaysInMonth = DateTime.DaysInMonth(year, month) - day;

                string qrymnth = @"SELECT COUNT(ID) AS LeaveSchdule FROM SchoolLeaveSchedule WHERE (MONTH(date) = " + month + ") and (Year(date) = " + year + ") AND (Day(date) > " + day+ ")";
                DataTable tab = dba.CreateTable(qrymnth);
                int holydays = int.Parse(tab.Rows[0][0].ToString());
                var holidaysInMonth = objdb.SchoolLeaveSchedules.Where(w =>w.IsHoliday && w.date.Month == month && w.date.Year == year).Count();

                ret = "Remaining Working days in current Month : " + (RemainingDaysInMonth - holydays).ToString() + "/" + (DateTime.DaysInMonth(year, month) - holidaysInMonth).ToString();

                return ret;
            }
            catch { }
            return "";
        }

        public string GetWorkingDaysInYear()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            int day = DateTime.Today.Day;
            int daysinyear = 0;
            if (DateTime.IsLeapYear(DateTime.Now.Year))
            {
                daysinyear = 366;
            }
            else daysinyear = 365;

            string qry = @"SELECT COUNT(ID) AS LeaveSchdule FROM SchoolLeaveSchedule WHERE (Year(date) = " + year + ") ";
            DataTable dtab = dba.CreateTable(qry);
            int holydaysinyaer = int.Parse(dtab.Rows[0][0].ToString());

            return "Working days in current session : " + (daysinyear - holydaysinyaer).ToString() + "/" + daysinyear;
        }
        public string GetRemainingWorkingDaysInYear()
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Today.Month;
            int day = DateTime.Today.Day;
            int RemainingDaysInYear = 0;
            int totalDaysInYear = 0;
            if (DateTime.IsLeapYear(DateTime.Now.Year))
            {
                totalDaysInYear = 366;
                RemainingDaysInYear = 366 - DateTime.Today.DayOfYear;
            }
            else
            {
                totalDaysInYear = 365;
                RemainingDaysInYear = 365 - DateTime.Today.DayOfYear;
            }
            int holydaysinyaer = objdb.SchoolLeaveSchedules.Where(w =>w.IsHoliday && w.date.Year == year).ToList().Where(w=> w.date.Date > DateTime.Today.Date).ToList().Count();
            int totalHolidaysInYear = objdb.SchoolLeaveSchedules.Where(w => w.IsHoliday && w.date.Year == year).ToList().Count();
            return "Remaining Working days in current session : " + (RemainingDaysInYear - holydaysinyaer).ToString() + "/" + (totalDaysInYear - totalHolidaysInYear).ToString();
        }

        public static string? GetMacAdress()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(n => n.OperationalStatus == OperationalStatus.Up)
                .FirstOrDefault()?
                .GetPhysicalAddress()
                .ToString();
        }

        public byte[] ImageToByteArraybyImageConverter(System.Drawing.Image image)
        {
            ImageConverter imageConverter = new ImageConverter();
            byte[] imageByte = (byte[])imageConverter.ConvertTo(image, typeof(byte[]));
            return imageByte;
        }
        //public static void AddValue(string key, string value)
        //{
        //    Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
        //    config.AppSettings.Settings.Add(key, value);
        //    config.Save(ConfigurationSaveMode.Minimal);
        //}
        //public static string ReadValue(string key)
        //{
        //string value=ConfigurationManager.AppSettings[key];
        //    return value;
        //}
    }
}

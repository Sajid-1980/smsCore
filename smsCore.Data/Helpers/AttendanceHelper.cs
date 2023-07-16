using Models;
using System;
using System.Data;
using System.Linq;

namespace Utilities
{

    public class AttendanceHelper
    {
        SchoolEntities objdb ;
        DatabaseAccessSql dba ;

        public AttendanceHelper(SchoolEntities objdb, DatabaseAccessSql dba)
        {
            this.objdb = objdb;
            this.dba = dba;
        }

        float percentage = 0;
        public string GetMonthlyAttendance(int stdid, int month, int year, char code) 
        {
            string q = @"SELECT COUNT(StudentAttendences.ID) AS Present, Admissions.StudentID FROM            StudentAttendanceTypes INNER JOIN
                         StudentAttendences ON StudentAttendanceTypes.ID = StudentAttendences.AttendanceTypeID INNER JOIN
                         Admissions ON StudentAttendences.AdmissionID = Admissions.ID
                        WHERE (MONTH(StudentAttendences.AttendanceDate) = "+month+") AND (YEAR(StudentAttendences.AttendanceDate) = "+year+") AND (Admissions.StudentID = "+stdid+") AND (StudentAttendanceTypes.Code = '"+code+"') GROUP BY Admissions.StudentID";
            DataTable data= dba.CreateTable(q);
            present= int.Parse(data.Rows[0][0].ToString());

            string qry = @"SELECT COUNT(ID) AS LeaveSchdule FROM SchoolLeaveSchedule WHERE (MONTH(date) = "+month+") and (Year(date) = "+year+")";
            DataTable dtab= dba.CreateTable(qry);
            int holydays = int.Parse(dtab.Rows[0][0].ToString());

            int totaldays = DateTime.DaysInMonth(year, month);
            int schoolopendays = totaldays - holydays;

            percentage = (present / schoolopendays) * 100;

            return present + "/" + schoolopendays;
        }

        float schoolopendays = 0, totaldays = 0, present=0;
        int oldstdid = 0;
        string attendance = "0";
        public int[] GetAttendanceTillDate(int AdmissionID,DateTime date,string code)
        {
            int[] attendances = new int[2];
            var TotalAttendances = objdb.StudentAttendences.Where(w => w.AdmissionID == AdmissionID && w.AttendanceDate.Date <= date);
            attendances[0] = TotalAttendances.Count();

            var totalLeaves = objdb.SchoolLeaveSchedules.Where(w =>w.IsHoliday && w.date <= date).Count();
            attendances[1] = 0;
            
            return attendances;
        }

        public string GetYearlyAttendance(int stdid, DateTime ExamDate, char code)
        {
            totaldays = 0; schoolopendays = 0;
            int holydays = 0;

            //DateTime fristattendancedate = objdb.StudentAttendences.ToList().Where(w => w.Admission.IsExpell == false && w.Admission.StudentID == stdid && w.AttendanceDate.Year == ExamDate.Year).Min().AttendanceDate;
            string qqq = @"SELECT CONVERT(date, StudentAttendences.AttendanceDate, 103) AS AttendanceDate
FROM            Admissions INNER JOIN StudentAttendences ON Admissions.ID = StudentAttendences.AdmissionID
WHERE        (Admissions.IsExpell = 0) AND (Admissions.StudentID = "+stdid+") AND (YEAR(StudentAttendences.AttendanceDate) = "+ExamDate.Year+") ORDER BY AttendanceDate";
            DataTable datadate=dba.CreateTable(qqq);
            if (datadate.Rows.Count > 0)
            {

                DateTime fristattendancedate = DateTime.ParseExact(datadate.Rows[0]["AttendanceDate"].ToString(), "dd/MM/yyyy hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture);
                string days = DateTime.DaysInMonth(ExamDate.Year, ExamDate.Month).ToString("00");

                //int fristmonthdays = int.Parse(days) - fristattendancedate.Day;
                string dt = string.Format("{0}/{1}/{2}", days, ExamDate.Month.ToString("00"), ExamDate.Year);

                DateTime lastattendancedate = DateTime.ParseExact(dt, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                //System.Windows.MessageBox.Show(lastattendancedate.ToString("dd/MM/yyyy"));

                string q = @"SELECT COUNT(StudentAttendences.ID) AS Present, Admissions.StudentID FROM            StudentAttendanceTypes INNER JOIN
                         StudentAttendences ON StudentAttendanceTypes.ID = StudentAttendences.AttendanceTypeID INNER JOIN
                         Admissions ON StudentAttendences.AdmissionID = Admissions.ID
                        WHERE  (YEAR(StudentAttendences.AttendanceDate) = " + ExamDate.Year + ") AND (Admissions.StudentID = " + stdid + ") AND (StudentAttendanceTypes.Code = '" + code + "') AND (CONVERT(Date, StudentAttendences.AttendanceDate, 103)  <= CONVERT(Date, '" + lastattendancedate.ToString("dd/MM/yyyy") + "', 103)) GROUP BY Admissions.StudentID";
                DataTable data = dba.CreateTable(q);
                present = float.Parse(data.Rows[0][0].ToString());


                string qry = @"SELECT COUNT(ID) AS LeaveSchdule FROM SchoolLeaveSchedule
                         WHERE (CONVERT(Date, date, 103) >= CONVERT(DATE, '" + fristattendancedate.ToString("dd/MM/yyyy") + "', 103)) AND (CONVERT(Date, date, 103) <= CONVERT(DATE, '" + lastattendancedate.ToString("dd/MM/yyyy") + "', 103))";
                DataTable dtab = dba.CreateTable(qry);

                holydays = int.Parse(dtab.Rows[0]["LeaveSchdule"].ToString());

                for (int i = fristattendancedate.Month; i <= ExamDate.Month; i++)
                {
                    totaldays += DateTime.DaysInMonth(ExamDate.Year, i);
                }

                schoolopendays += totaldays - (fristattendancedate.Day - 1) - holydays;
                                
                attendance = present + "/" + schoolopendays;
                percentage = (present / schoolopendays) * 100;
                oldstdid = stdid;
            }
            return attendance;
        }
        public float getpercentage()
        {
            //System.Windows.MessageBox.Show(percentage.ToString());
            return percentage;
        }
    }
}

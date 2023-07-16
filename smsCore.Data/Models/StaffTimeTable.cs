using System;
using System.Linq;
using Models;

namespace smsCore.Data.Models
{
    public class StaffTimeTable
    {
        private readonly SchoolEntities objdb ;

        public StaffTimeTable(SchoolEntities _db)
        {
            objdb = _db;
        }

        public StaffTimeTable(int staffid, string subjectName, int classid, string className, int secid,
            string sectionName, int subjectID, int campusID, string campusName)
        {
            StaffID = staffid;
            SubjectName = subjectName;
            ClassID = classid;
            SectionID = secid;
            ClassName = className;
            SectionName = sectionName;
            SubjectID = subjectID;
            CampusID = campusID;
            CampusName = campusName;
        }

        public int StaffID { get; set; }
        public int Period { get; set; }

        public string StaffName
        {
            get
            {
                var staf = objdb.tbl_Employee.Where(s => s.Id == StaffID).FirstOrDefault();
                return staf == null ? "No Teacher" : staf.employeeName;
            }
        }

        public string PeriodNo { get; set; }
        public string TimeTo { get; set; }
        public string TimeFrom { get; set; }
        public string SubjectName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }
        public int ClassID { get; set; }
        public int SectionID { get; set; }
        public int SubjectID { get; set; }
        public int CampusID { get; set; }
        public string CampusName { get; set; }


        public DateTime TimeToDate => DateTime.Parse(TimeTo);
        public DateTime TimeFromDate => DateTime.Parse(TimeFrom);
    }

    public class TimeTableOtherPeriods
    {
        public int PeriodNo { get; set; }
        public int Duration { get; set; }
        public string Description { get; set; }
    }
}
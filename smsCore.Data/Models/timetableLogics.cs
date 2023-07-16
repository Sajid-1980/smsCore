using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Newtonsoft.Json;

namespace smsCore.Data.Models
{
    public class TimeTableConfig
    {
        public TimeTableConfig()
        {
        }

        public TimeTableConfig(int campusid, bool loadFromDb)
        {
            //CampusID = campusid;
            if (loadFromDb)
            {
                //var setting = new ClsBussinessSetting(CampusID);
                //var startTime = setting.ReadWithType("FirstPeriod", typeof(DateTime));
                //SchoolStartTime = (DateTime) (startTime.ToString() == "0" ? DateTime.Now : startTime);
                //DurationofPeriod = (int) setting.ReadWithType("PeriodDuration", typeof(int));
                //var otherPeriod = setting.ReadWithType("OtherPeriods", typeof(string)).ToString();
                //PerDayPeriod = (int) setting.ReadWithType("PerDayPeriod", typeof(int));
                //try
                //{
                //    otherPeriods = JsonConvert.DeserializeObject<List<TimeTableOtherPeriods>>(otherPeriod);
                //}
                //catch
                //{
                //}
            }
        }

        public int CampusID { get; set; }
        public int PerDayPeriod { get; set; }
        public int DurationofPeriod { get; set; }
        public DateTime SchoolStartTime { get; set; }
        public string SchoolStratTimeStr => SchoolStartTime.ToString("hh:mm tt");
        public List<TimeTableOtherPeriods> otherPeriods { get; set; }
    }

    public class timetableLogics
    {
        private static Random random = new Random();

        private readonly TimeTableConfig config = new TimeTableConfig();

        private readonly SchoolEntities objdb;
        public timetableLogics(SchoolEntities _db)
        {
            objdb = _db;
        }

        private List<StaffTimeTable> timetable = new List<StaffTimeTable>();

        //public timetableLogics()
        //{
        //   config.otherPeriods = new List<TimeTableOtherPeriods>();
        //}
        //int.Parse(NoOperday), int.Parse(periodDuration), DateTime.Parse(firstPeriod), int.Parse(BreakDuration), int.Parse(breakPeriod)
        public timetableLogics(TimeTableConfig _config)
        {
            config = _config;
        }

        public List<StaffTimeTable> MakeTimeTable(List<TimeTablePreView> TeachingSubjects, int campusId)
        {
            timetable = new List<StaffTimeTable>();

            var Classes = objdb.ClassSections.Where(w => w.CampusID == campusId).Select(s =>
                    new {ClassSectionID = s.ID, s.Class.ClassName, s.ClassID, s.SectionID, s.Section.SectionName})
                .OrderBy(c => c.ClassID).ToList();

            var ClassLoopIndex = 0;
            foreach (var C in Classes)
            {
                var subjects = TeachingSubjects
                    .Where(t => t.CampusID == campusId && t.ClassID == C.ClassID && t.SectionID == C.SectionID).ToList()
                    .OrderBy(o => new Guid());
                if (subjects.Count() > 0)
                    foreach (var others in config.otherPeriods)
                    {
                        var AutoTimeTableOther = new StaffTimeTable(0, others.Description, C.ClassID, C.ClassName,
                            C.SectionID, C.SectionName, 0, subjects.FirstOrDefault().CampusID,
                            subjects.FirstOrDefault().CampusName);
                        AutoTimeTableOther.Period = others.PeriodNo;
                        timetable.Add(AutoTimeTableOther);
                    }

                var CurrentPeriodNo = 1;
                foreach (var s in subjects)
                {
                    if (config.PerDayPeriod != 0) //implementation of no of period per day 
                        if (CurrentPeriodNo > config.PerDayPeriod)
                            break;

                    CurrentPeriodNo = GetPeriod(CurrentPeriodNo);

                    var StaffId = s.StaffID == null ? 0 : (int) s.StaffID.Value;
                    var AutoTimeTable = new StaffTimeTable(StaffId, s.SubjectName, s.ClassID, s.ClassName, s.SectionID,
                        s.SectionName, s.SubjectID, s.CampusID, s.CampusName);

                    if (ClassLoopIndex == 0)
                    {
                        AutoTimeTable.Period = CurrentPeriodNo;
                    }
                    else
                    {
                        var NewPeriodNo = GetPeriod(CurrentPeriodNo, StaffId, s.ClassID, s.SectionID);

                        AutoTimeTable.Period = NewPeriodNo;
                    }

                    timetable.Add(AutoTimeTable);
                    CurrentPeriodNo++;
                }

                ClassLoopIndex++;
            }

            var timeFrom = DateTime.Now;
            var timeTo = DateTime.Now;

            foreach (var t in timetable.Select(s => s.Period).OrderBy(s => s).Distinct())
            {
                var others = config.otherPeriods.Where(w => w.PeriodNo == t).FirstOrDefault();

                if (t == 1) //implementation of timing system e.g duration of period
                {
                    if (others != null)
                    {
                        timeFrom = config.SchoolStartTime;
                        timeTo = config.SchoolStartTime.AddMinutes(others.Duration);
                    }
                    else
                    {
                        timeFrom = config.SchoolStartTime;
                        timeTo = config.SchoolStartTime.AddMinutes(config.DurationofPeriod);
                    }
                }
                else
                {
                    timeFrom = timeTo;
                    if (others != null)
                        timeTo = timeFrom.AddMinutes(others.Duration);
                    else
                        timeTo = timeFrom.AddMinutes(config.DurationofPeriod);
                }

                var ps = timetable.Where(w => w.Period == t);

                foreach (var p in ps)
                {
                    p.TimeFrom = timeFrom.ToString("hh:mm tt");
                    p.TimeTo = timeTo.ToString("hh:mm tt");
                }
            }

            return timetable;
        }

        public bool IsEngaged(int StaffID, int pno)
        {
            return timetable.Where(t => t.StaffID == StaffID && t.Period == pno).Count() > 0;
        }

        private int GetPeriod(int pno)
        {
            if (config.otherPeriods.Where(w => w.PeriodNo == pno).Count() > 0)
            {
                pno++;
                GetPeriod(pno);
            }

            return pno;
        }

        private int GetPeriod(int pno, int staffid, int classId, int sectionId)
        {
            if (staffid == 0)
                return pno;

            var ps = timetable
                .Where(t => t.StaffID == (staffid > 0 ? staffid : t.StaffID) ||
                            t.ClassID == classId && t.SectionID == sectionId).Select(t => t.Period);
            //select all the rows where this staff member is busy
            if (ps.Count() > 0)
            {
                var r = pno;
                var a = timetable.Where(t => !ps.Contains(t.Period)).OrderBy(t => t.Period).ToList();
                if (a.Count() > 0)
                    r = a.First().Period;
                else
                    r = timetable.Max(m => m.Period) + 1;
                return r;
            }

            return pno;
        }
    }
}
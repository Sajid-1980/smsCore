using System.Collections.Generic;
using System.Linq;
using Models;
using smsCore.Data.Models;
using Utilities;

namespace smsCore.Data.Helpers
{
    public class TimeTableHelper
    {
        private readonly SchoolEntities db ;
        public TimeTableHelper(SchoolEntities _db)
        {
            db = _db;
        }

        public List<StaffTimeTable> AutoGenerateTimeTable(TimeTableConfig config)
        {
            var finalTimeTable = new List<StaffTimeTable>();

            var tx = new timetableLogics(config);
            var TeachingSubjects = new List<TimeTablePreView>();

            var lists = db.ClassSections
                .Select(s => new {s.ID, s.Class.ClassName, s.ClassID, s.Section.SectionName, s.SectionID}).ToList();

            foreach (var section in lists)
            {
                var subjects = db.TeachingSubjects.Where(w => w.ClassSectionID == section.ID && w.CloseDate == null)
                    .ToList();
                foreach (var subject in subjects)
                    TeachingSubjects.Add(new TimeTablePreView
                    {
                        ClassID = section.ClassID,
                        ClassName = section.ClassName.Trim(),
                        SectionID = section.SectionID,
                        SectionName = section.SectionName.Trim(),
                        SubjectID = subject.SubjectId,
                        SubjectName = subject.Subject.SubjectName.Trim(),
                        StaffID = subject.StaffID
                    });
            }

            finalTimeTable = tx.MakeTimeTable(TeachingSubjects, config.CampusID).OrderBy(o => o.Period).ToList();
            foreach (var tt in finalTimeTable)
            {
                if (!config.otherPeriods.Select(s => s.Description).Contains(tt.SubjectName))
                    tt.SubjectName = tt.SubjectName + "\n[" + tt.StaffName + "]";
                tt.PeriodNo = tt.Period.ToPosition() + "\n" + tt.TimeFrom + " TO " + tt.TimeTo;
            }

            return finalTimeTable;
        }
    }
}
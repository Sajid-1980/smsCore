using System;
using System.Collections.Generic;

namespace smsCore.Data.Models
{
    public class TimeTablePreView
    {
        #region Factory Method

        /// <summary>
        ///     Create a new TimeTablePreView object.
        /// </summary>
        /// <param name="className">Initial value of the ClassName property.</param>
        /// <param name="sectionName">Initial value of the SectionName property.</param>
        /// <param name="subjectName">Initial value of the SubjectName property.</param>
        /// <param name="classID">Initial value of the ClassID property.</param>
        /// <param name="subjectID">Initial value of the SubjectID property.</param>
        /// <param name="sectionID">Initial value of the SectionID property.</param>
        public static TimeTablePreView CreateTimeTablePreView(string className, string sectionName, string subjectName,
            int classID, int subjectID, int sectionID, int campusID, string campusName)
        {
            var timeTablePreView = new TimeTablePreView();

            timeTablePreView.ClassName = className;
            timeTablePreView.SectionName = sectionName;
            timeTablePreView.SubjectName = subjectName;
            timeTablePreView.CampusName = campusName;
            timeTablePreView.ClassID = classID;
            timeTablePreView.SubjectID = subjectID;
            timeTablePreView.SectionID = sectionID;
            timeTablePreView.CampusID = campusID;
            return timeTablePreView;
        }

        #endregion

        #region Primitive Properties

        public string CampusName
        {
            get => _CampusName;
            set
            {
                if (_CampusName != value)
                {
                    OnCampusNameChanging(value);
                    _CampusName = value;
                    OnCampusNameChanged();
                }
            }
        }

        private string _CampusName;

        private void OnCampusNameChanging(string value)
        {
            throw new NotImplementedException();
        }

        private void OnCampusNameChanged()
        {
            throw new NotImplementedException();
        }

        public int CampusID
        {
            get => _CampusID;
            set
            {
                OnCampusIDChanging(value);
                _CampusID = value;
                OnCampusIDChanged();
            }
        }

        private int _CampusID;

        private void OnCampusIDChanging(int value)
        {
            throw new NotImplementedException();
        }

        private void OnCampusIDChanged()
        {
            throw new NotImplementedException();
        }


        public string ClassName
        {
            get => _ClassName;
            set
            {
                if (_ClassName != value)
                {
                    OnClassNameChanging(value);
                    _ClassName = value;
                    OnClassNameChanged();
                }
            }
        }

        private string _ClassName;

        private void OnClassNameChanging(string value)
        {
            throw new NotImplementedException();
        }

        private void OnClassNameChanged()
        {
            throw new NotImplementedException();
        }

        public string SectionName
        {
            get => _SectionName;
            set
            {
                if (_SectionName != value)
                {
                    OnSectionNameChanging(value);
                    _SectionName = value;
                    OnSectionNameChanged();
                }
            }
        }

        private string _SectionName;

        private void OnSectionNameChanging(string value)
        {
            throw new NotImplementedException();
        }

        private void OnSectionNameChanged()
        {
            throw new NotImplementedException();
        }

        public string SubjectName
        {
            get => _SubjectName;
            set
            {
                if (_SubjectName != value)
                {
                    OnSubjectNameChanging(value);
                    _SubjectName = value;
                    OnSubjectNameChanged();
                }
            }
        }

        private string _SubjectName;

        private void OnSubjectNameChanging(string value)
        {
            throw new NotImplementedException();
        }

        private void OnSubjectNameChanged()
        {
            throw new NotImplementedException();
        }

        public decimal? StaffID
        {
            get => _StaffID;
            set
            {
                OnStaffIDChanging(value);
                _StaffID = value;
                OnStaffIDChanged();
            }
        }

        private decimal? _StaffID;

        private void OnStaffIDChanging(decimal? value)
        {
            throw new NotImplementedException();
        }

        private void OnStaffIDChanged()
        {
            throw new NotImplementedException();
        }

        public int ClassID
        {
            get => _ClassID;
            set
            {
                if (_ClassID != value)
                {
                    OnClassIDChanging(value);
                    _ClassID = value;
                    OnClassIDChanged();
                }
            }
        }

        private int _ClassID;

        private void OnClassIDChanging(int value)
        {
            throw new NotImplementedException();
        }

        private void OnClassIDChanged()
        {
            throw new NotImplementedException();
        }

        public int SubjectID
        {
            get => _SubjectID;
            set
            {
                if (_SubjectID != value)
                {
                    OnSubjectIDChanging(value);
                    _SubjectID = value;
                    OnSubjectIDChanged();
                }
            }
        }

        private int _SubjectID;

        private void OnSubjectIDChanging(int value)
        {
            throw new NotImplementedException();
        }

        private void OnSubjectIDChanged()
        {
            throw new NotImplementedException();
        }


        public int SectionID
        {
            get => _SectionID;
            set
            {
                if (_SectionID != value)
                {
                    OnSectionIDChanging(value);
                    _SectionID = value;
                    OnSectionIDChanged();
                }
            }
        }

        private int _SectionID;

        private void OnSectionIDChanging(int value)
        {
            throw new NotImplementedException();
        }

        private void OnSectionIDChanged()
        {
            throw new NotImplementedException();
        }

        #endregion
    }


    public class Employee_t
    {
        public int EmployeeName { get; set; }
        public string EmployeeId { get; set; }
        public bool IsBusy { get; set; }
        public List<string> TeachingSubjects { get; set; }
        public List<string> ExperinceSubjects { get; set; }
    }

    public class Subjects_t
    {
        public string SubjectName { get; set; }
        public int Id { get; set; }
        public bool IsBusy { get; set; }
    }

    public class Periods_t
    {
        public string PeriodName { get; set; }
        public int Id { get; set; }
        public bool IsBusy { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class Classes_t
    {
        public string ClassName { get; set; }
        public int Id { get; set; }
        public bool IsBusy { get; set; }
    }
}
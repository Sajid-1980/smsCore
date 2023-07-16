namespace smsCore.Data.Models
{
    public class PartialCampusViewModels
    {

        public string ParentContainerID { get; set; } = string.Empty;

        public string ColCssClass { get; set; } = "col-12 col-sm-6 col-md-4 col-lg-3";
        public string ExamColCssClass { get; set; } = "col-12 col-sm-6 col-md col-lg";

        public bool ForSearch { get; set; } = false;

        public bool AddNew { get; set; } = false;

        public bool ShowSession { get; set; } = false;

        public bool ShowCampus { get; set; } = false;
        public string CampusLoadCallBack { get; set; }

        public bool ShowClasses { get; set; } = false;
        public bool ShowClasseForClassTeacher { get; set; } = true;

        public bool ShowGroups { get; set; } = false;

        public bool ShowSections { get; set; } = false;
        /// <summary>
        /// Show Exam Held
        /// </summary>
        public bool ShowExams { get; set; } = false;
        /// <summary>
        /// Show list of exams
        /// </summary>
        public bool ShowExamsList { get; set; } = false;
        public bool AddNewExam { get; set; } = false;

        public bool ShowSubjects { get; set; } = false;

        public bool ShowFeeGroup { get; set; } = false;
        public bool ShowFreeFeeGroup { get; set; } = false;
        public bool ShowEmployee { get; set; }
        public decimal EmployeeID { get; set; } = -1;
        public int SessionID { get; set; } = -1;

        public int CampusID { get; set; } = -1;

        public int ClassID { get; set; } = -1;

        public int SectionID { get; set; } = -1;

        public int ExamID { get; set; } = -1;

        public int SubjectID { get; set; } = -1;

        public int FeeGroupID { get; set; } = -1;

        public string SessionFieldName { get; set; } = "Session";
        public string EmployeeFieldName { get; set; } = "Employee";

        public string CampusFieldName { get; set; } = "Campus";

        public string ClassesFieldName { get; set; } = "Class";

        public string GroupsFieldName { get; set; } = "Group";

        public string SectionsFieldName { get; set; } = "Section";

        public string ExamsFieldName { get; set; } = "Exam";

        public string SubjectsFieldName { get; set; } = "Subject";

        public string FeeGroupFieldName { get; set; } = "FeeGroup";
    }
}
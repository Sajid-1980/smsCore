using System;

namespace smsCore.Data.Models.ViewModels
{
    public class TeachingSubjectEditViewModel
    {
        public string CurrentTeacherName { get; set; }
        public string CurrentTeacherCNIC { get; set; }
        public string CurrentTeacherDesignation { get; set; }
        public byte[] CurrentTeacherPhoto { get; set; }
        public string NewTeacherName { get; set; }
        public string NewTeacherCNIC { get; set; }
        public string NewTeacherDesignation { get; set; }
        public byte[] NewTeacherPhoto { get; set; }
        public string Campus { get; set; }
        public string Class { get; set; }
        public string Subject { get; set; }
        public string Section { get; set; }

        public DateTime StartDate { get; set; }

        public int campusId { get; set; }
        public int ClassSectionId { get; set; }
        public int subjectId { get; set; }
        public int staffId { get; set; }
    }
}
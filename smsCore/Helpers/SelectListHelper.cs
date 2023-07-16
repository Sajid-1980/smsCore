using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Models;

namespace smsCore.Data.Helpers
{
    public  class SelectListHelper
    {
        readonly SchoolEntities db;
        public SelectListHelper(SchoolEntities _db) { db = _db; }
        public  SelectList GetOustandingFeeOfStudentSelectList(int regno, object selectedVlue = null)
        {
            
            var data = db.FeeSlips.Where(w => w.Admission.Student.RegistrationNo == regno &&
                                              (w.FeeSlipReceipts.Count == 0 ||
                                               w.FeeSlipReceipts.DefaultIfEmpty().Select(s => s.Amount).Sum() <
                                               w.FeeSlipDetails.Select(s => s.Amount).Sum())).Select
                (s => new {s.Id, ForMonth = s.ForMonth.ToString("MMMM, yyyy")}).ToList();
            var list = new SelectList(data, "Id", "ForMonth", selectedVlue);
            return list;
        }

        public  SelectList GetCampusSelectList(object selectedVlue = null)
        {
            
            var select = db.Campuses.Select(s => new {s.ID, s.CampusName}).ToList();
            var list = new SelectList(select, "ID", "CampusName", selectedVlue);

            return list;
        }


        public  SelectList GetClassSelectList(object selectedVlue = null)
        {
            
            var select = db.Classes.Select(s => new {s.ID, s.ClassName}).ToList();
            var list = new SelectList(select, "ID", "ClassName", selectedVlue);

            return list;
        }


        public  SelectList GetSectionSelectList(object selectedVlue = null)
        {
            
            var select = db.Sections.Select(s => new {s.ID, s.SectionName}).ToList();
            var list = new SelectList(select, "ID", "SectionName", selectedVlue);

            return list;
        }

        public  SelectList GetSectionFeeGroupList(object selectedVlue = null)
        {
            
            var select = db.FeeGroups.Select(s => new {s.ID, s.FeeGroupName}).ToList();
            var list = new SelectList(select, "ID", "FeeGroupName");

            return list;
        }

        public SelectList GetExamList(object selectedVlue = null)
        {

            var select = db.Exams.Select(s => new { s.ID, s.ExamName }).ToList();
            var list = new SelectList(select, "ID", "ExamName");

            return list;
        }


        public SelectList GetSectionFeeTypeList(object selectedVlue = null)
        {
            
            var select = db.FeeTypes.Select(s => new {s.Id, s.TypeName}).ToList();
            var list = new SelectList(select, "id", "TypeName");

            return list;
        }

        public  SelectList GetPaymentSelectList(object selectedVlue = null)
        {
            
            var select = db.FeeSlipPaymentMethods.Select(s => new {s.Id, s.PaymentMethodName}).ToList();
            var list = new SelectList(select, "Id", "PaymentMethodName");

            return list;
        }
    }
}
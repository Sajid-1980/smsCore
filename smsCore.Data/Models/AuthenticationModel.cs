//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using sms.Models;
//using Models;
//using _models = Models;

//namespace sms.Models
//{
//    public static class AuthenticationModel
//    {
//        private static _models.SchoolEntities database = new _models.SchoolEntities();

//        public static bool Add(_models.Authentication entity)
//        {
//            database.Authentications.Add(entity);
//            try
//            {
//                database.SaveChanges();
//                return true;
//            }
//            catch
//            {
//                return false;
//            }
//        }

//        public static bool Edit(_models.Authentication entity)
//        {
//            //_models.Authentication auth = database.Authentications.Where(w => w.ID == entity.ID).FirstOrDefault();
//            //auth.ViewRecords = entity.ViewRecords;
//            //auth.EditRecords = entity.EditRecords;
//            //auth.DeleteRecords = entity.DeleteRecords;
//            //auth.AddRecords = entity.AddRecords;
//            try
//            {
//                database.SaveChanges();
//                return true;
//            }
//            catch { return false; }
//        }

//        public static bool SaveOrUpdate(_models.Authentication entity)
//        {
//            bool success = false;
//            if (entity.ID < 1)
//            {
//                success = Add(entity);
//            }
//            else
//            {
//                success = Edit(entity);
//            }
//            return success;
//        }

//        public class authenticationClass
//        {
//            public int ID { get; set; }
//            public string RoleId { get; set; }
//            public decimal FormID { get; set; }
//            public string FormName { get; set; }
//            public string Description { get; set; }
//            public bool? ViewRecords { get; set; }
//            public bool? AddRecords { get; set; }
//            public bool? EditRecords { get; set; }
//            public bool? DeleteRecords { get; set; }
//        }

//        public static List<authenticationClass> GetAll(string RoleId)
//        {
//            var list = database.AppForms.Select(s => new authenticationClass
//            {
//                RoleId = RoleId,
//                FormID = s.ID,
//                ID = s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault() == null ? -1 : s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault().ID,
//                FormName = s.FormName,
//                Description = s.Description,
//                //AddRecords = s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault() == null ? false : s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault().AddRecords,
//                //EditRecords = s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault() == null ? false : s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault().EditRecords,
//                //DeleteRecords = s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault() == null ? false : s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault().DeleteRecords,
//                //ViewRecords = s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault() == null ? false : s.Authentications.Where(w => w.RoleID == RoleId).FirstOrDefault().ViewRecords
//            }
//            );
//            return list.ToList();
//        }

//        public static Authentication GetByID(int id)
//        {
//            return database.Authentications.Where(w => w.ID == id).FirstOrDefault();
//        }


//    }
//}


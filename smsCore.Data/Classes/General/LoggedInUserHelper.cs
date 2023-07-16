using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace Models
{
    public class LoggedInUserHelper
    {
        private readonly SchoolEntities database ;
        public LoggedInUserHelper(SchoolEntities db) { database = db; }    
        public LoggedInUserHelper(string? userid) {  }    

        public LoggedInUserHelper GetLoggedInUserHelper(string UserId)
        {

            var user = database.UserDefinitions.AsNoTracking().FirstOrDefault(w => w.UserId == UserId);
            if (user != null)
            {
                if (user.UserType.ToLower().Trim() == "c")
                {
                    var c = database.Campuses.AsNoTracking().FirstOrDefault(w => w.ID == user.FK);
                    BasicUserType = EnumManager.BasicUserType.Campus;
                    Email = c.emailId;
                    primaryId = c.ID;
                    this.UserId = UserId;
                    FullName = c.CampusName;
                    CampusId = new[] {c.ID};
                    ProfilePic = "~/Uploads/images/user.png";
                }
                else if (user.UserType.ToLower().Trim() == "p".ToLower().Trim())
                {
                    var c = database.Students.AsNoTracking().FirstOrDefault(w => w.ID == user.FK);
                    BasicUserType = EnumManager.BasicUserType.Parent;
                    Email = c.Email;
                    primaryId = c.ID;
                    this.UserId = UserId;
                    this.CNIC = c.CNIC.Trim();
                    this.ParentClassSections = database.Admissions.Where(w => !w.IsExpell && w.Student.CNIC == CNIC).Select(s => s.ClassSectionID).ToArray();
                    CampusId = new[] {c.Admissions.LastOrDefault().CampuseID};
                    FullName = c.FName;
                    try
                    {
                        var base64 = Convert.ToBase64String(c.FatherPhoto);
                        ProfilePic = string.Format("data:image;base64,{0}", base64);
                    }
                    catch
                    {
                        ProfilePic = "~/Uploads/images/user.png";
                    }
                }
                else if (user.UserType.ToLower().Trim() == "s")
                {
                    var c = database.Students.AsNoTracking().FirstOrDefault(w => w.ID == user.FK);
                    BasicUserType = EnumManager.BasicUserType.Student;
                    Email = c.Email;
                    primaryId = c.ID;
                    this.UserId = UserId;
                    FullName = c.StudentName;
                    this.RegNo=c.RegistrationNo;
                    this.ClassSectionId = c.Admissions.Where(w => !w.IsExpell).Select(s => s.ClassSectionID).FirstOrDefault();
                    CampusId = new[] {c.Admissions.LastOrDefault().CampuseID};
                    try
                    {
                        var base64 = Convert.ToBase64String(c.StudentPhotos.Where(w => !w.IsReplaced)
                            .Select(s => s.StudentImage).FirstOrDefault());
                        ProfilePic = string.Format("data:image;base64,{0}", base64);
                    }
                    catch
                    {
                        ProfilePic = "~/Uploads/images/user.png";
                    }
                }
                else if (user.UserType.ToLower().Trim() == "e")
                {
                    var c = database.tbl_Employee.AsNoTracking().FirstOrDefault(w => w.Id == user.FK);
                    BasicUserType = EnumManager.BasicUserType.Employee;
                    Email = c.email;
                    primaryId = int.Parse(c.Id.ToString());
                    this.UserId = UserId;
                    FullName = c.employeeName;
                    CampusId = new[] {c.CampusID};
                    try
                    {
                        var base64 = Convert.ToBase64String(c.Photo);
                        ProfilePic = string.Format("data:image;base64,{0}", base64);
                    }
                    catch
                    {
                        ProfilePic = "~/Uploads/images/user.png";
                    }
                }
            }
            else
            {
                BasicUserType = EnumManager.BasicUserType.User;
                Email = string.Empty;
                this.UserId = UserId;
                ProfilePic = string.Empty;
                FullName = string.Empty;
                this.UserId = UserId;
                CampusId = database.Campuses.AsNoTracking().Select(s => s.ID).ToArray();
            }
            return this;
        }

        /// <summary>
        /// only for parent user
        /// </summary>
        public int[] ParentClassSections { get; set; }
        /// <summary>
        /// only for student users
        /// </summary>
        public int ClassSectionId { get; set; }
        public int[] CampusId { get; set; }
        public int primaryId { get; set; }
        public string FullName { get; set; }
        public string ProfilePic { get; set; }
        public string Email { get; set; }
        public string UserId { get; set; }
        public string CNIC { get; set; }
        public int RegNo { get; set; }
        public EnumManager.BasicUserType BasicUserType { get; set; }
    }
}
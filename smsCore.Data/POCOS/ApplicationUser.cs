using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ApplicationUser : IdentityUser
    {
        public ApplicationUser()
        {
            this.DOBChanges = new HashSet<DOBChanx>();
            this.ExamRemarks = new HashSet<ExamRemark>();
            this.ExpellDetails = new HashSet<ExpellDetail>();
            this.FineDeductions = new HashSet<FineDeduction>();
            this.IssuedBoardCertificates = new HashSet<IssuedBoardCertificate>();
            this.IssuedBooks = new HashSet<IssuedBook>();
            this.LibraryIssuedBooks = new HashSet<LibraryIssuedBook>();
            this.IssuedBooksToStaffs = new HashSet<IssuedBooksToStaff>();
            this.LibraryIssuedBooksToStaffs = new HashSet<LibraryIssuedBooksToStaff>();
            this.IssuedCertificates = new HashSet<IssuedCertificate>();
            this.LeavedStaffs = new HashSet<LeavedStaff>();
            this.MessageRecords = new HashSet<MessageRecord>();
            this.ProfessionalTrainings = new HashSet<ProfessionalTraining>();
            this.Qualifications = new HashSet<Qualification>();
            this.Results = new HashSet<Result>();
            this.SelectedForCadetColleges = new HashSet<SelectedForCadetCollege>();
            this.EmployeeShortLeaves = new HashSet<EmployeeShortLeave>();
            this.EmployeeAttendances = new HashSet<EmployeeAttendance>();
            this.EmployeeAwards = new HashSet<EmployeeAward>();
            this.StudentAttendences = new HashSet<StudentAttendence>();
            this.TimeConsumptionForResultEntries = new HashSet<TimeConsumptionForResultEntry>();
            this.UnwantedClients = new HashSet<UnwantedClient>();
            this.CampusUsers = new HashSet<CampusUser>();
        }

        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    //var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
        //    // Add custom user claims here
        //    //var db = new SchoolEntities();
        //    //var UserId = userIdentity.GetUserId();

        //    var UserId=await  manager.GetUserIdAsync();
            
        //    var helperUser = new LoggedInUserHelper(UserId);

        //    userIdentity.AddClaim(new Claim("BasicUserType", helperUser.BasicUserType.ToString()));
        //    userIdentity.AddClaim(new Claim("CampusIds", string.Join(",", helperUser.CampusId)));
        //    userIdentity.AddClaim(new Claim("ProfilePic", helperUser.ProfilePic));
        //    userIdentity.AddClaim(new Claim("FullName", helperUser.FullName));
        //    if (helperUser.Email != null) userIdentity.AddClaim(new Claim("Email", helperUser.Email));

        //    userIdentity.AddClaim(new Claim("primaryId", helperUser.primaryId.ToString()));

        //    return userIdentity;
        //}


        public virtual ICollection<DOBChanx> DOBChanges { get; set; }
        public virtual ICollection<CampusUser> CampusUsers { get; set; }

        public virtual ICollection<ExamRemark> ExamRemarks { get; set; }

        public virtual ICollection<ExpellDetail> ExpellDetails { get; set; }

        public virtual ICollection<FineDeduction> FineDeductions { get; set; }

        public virtual ICollection<IssuedBoardCertificate> IssuedBoardCertificates { get; set; }

        public virtual ICollection<IssuedBook> IssuedBooks { get; set; }

        public virtual ICollection<LibraryIssuedBook> LibraryIssuedBooks { get; set; }

        public virtual ICollection<IssuedBooksToStaff> IssuedBooksToStaffs { get; set; }

        public virtual ICollection<LibraryIssuedBooksToStaff> LibraryIssuedBooksToStaffs { get; set; }

        public virtual ICollection<IssuedCertificate> IssuedCertificates { get; set; }

        public virtual ICollection<LeavedStaff> LeavedStaffs { get; set; }

        public virtual ICollection<MessageRecord> MessageRecords { get; set; }

        public virtual ICollection<ProfessionalTraining> ProfessionalTrainings { get; set; }

        public virtual ICollection<Qualification> Qualifications { get; set; }

        public virtual ICollection<Result> Results { get; set; }

        public virtual ICollection<SelectedForCadetCollege> SelectedForCadetColleges { get; set; }

        public virtual ICollection<EmployeeShortLeave> EmployeeShortLeaves { get; set; }

        public virtual ICollection<EmployeeAttendance> EmployeeAttendances { get; set; }

        public virtual ICollection<EmployeeAward> EmployeeAwards { get; set; }

        public virtual ICollection<StudentAttendence> StudentAttendences { get; set; }

        public virtual ICollection<TimeConsumptionForResultEntry> TimeConsumptionForResultEntries { get; set; }

        public virtual ICollection<UnwantedClient> UnwantedClients { get; set; }

        
    }

}

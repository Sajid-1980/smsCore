using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace smsCore.Data.Models
{
    public class ClassSubjectResponse
    {
        public string SubjectName { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Qualification { get; set; }
    }
    public class ClassTeacherResponse
    {
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Qualification { get; set; }
    }
    public class SheduleResponse
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool IsHoliday { get; set; }
        public string Color { get; set; }
    }
    public class StudentComplaignResponse
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public System.DateTime EntryDate { get; set; }
        public string Particular { get; set; }
        public string Ctype { get; set; }
        public decimal EmployeeId { get; set; }
    }
    public class TimeTableResponse
    {
        public string SubjectName { get; set; }
        public int Pno { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public string TeacherName { get; set; }
    }
    public class DateSheetResponse
    {
        public string SubjectName { get; set; }
        public DateTime ExamDate { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public string ExamName { get; set; }
    }
    public class StudentProfileResponse
    {
        public bool IsCurrentStudent { get; set; }
        public string CampusName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }

        public string StudentName { get; set; }
        public string FName { get; set; }
        public string GuardianName { get; set; }
        public string StdRelation { get; set; }
        public string Email { get; set; }
        public string Religion { get; set; }
        public string Nationality { get; set; }
        public string MotherTongue { get; set; }
        public string LastIntitution { get; set; }
        public string Games { get; set; }
        public System.DateTime DOB { get; set; }
        public string PostalAddress { get; set; }
        public string OfficeTelephone { get; set; }
        public string PermenantAddress { get; set; }
        public string FatherQualification { get; set; }
        public string FatherProfession { get; set; }
        public string FatherNatureOfJob { get; set; }
        public string FatherDepartment { get; set; }
        public string MotherName { get; set; }
        public string MotherQualification { get; set; }
        public string MotherProfession { get; set; }
        public string MissingDocuments { get; set; }
        public string CNIC { get; set; }
        public int RegistrationNo { get; set; }
        public string Sex { get; set; }
        public string ResidanceTelephone { get; set; }
        public string Remarks { get; set; }
        public string GeneralRemarks { get; set; }
        public string DateForSubmission { get; set; }
        public string FatherAlive { get; set; }
        public string LiveWith { get; set; }
        public string Domicile { get; set; }
        public System.DateTime AdmissionDate { get; set; }
        public string AdmittedClass { get; set; }
        public Nullable<int> ApplyLateFee { get; set; }
        public string StudentCNIC { get; set; }

        public List<StudentMobileResponse> Mobiles { get; set; }

    }

    public partial class StudentMobileResponse
    {
        public int ID { get; set; }
        public bool IsDefault { get; set; }
        public string MobileHolder { get; set; }
        public string MobileNo { get; set; }
        public string Relation { get; set; }
        public int StudentID { get; set; }

    }
    public class AttendanceResponse
    {
        public bool IsTodayHoliday { get; set; }

        public string TodayStatus { get; set; }
        public decimal MonthPresent { get; set; }
        public string MonthAbsent { get; set; }
        public string MonthLeave { get; set; }
        public decimal MonthPercnet
        {
            get
            {
                return (MonthPresent / TotalWorkingDaysMonth) * 100;

            }
        }

        public decimal YearPresent { get; set; }
        public string YearAbsent { get; set; }
        public string YearLeave { get; set; }
        public decimal YearPercnet
        {
            get
            {
                return (YearPresent / TotalWorkingDaysYear) * 100;

            }
        }

        public decimal TotalWorkingDaysMonth { get; set; }
        public decimal TotalWorkingDaysYear { get; set; }

    }


    public class ResultResponse
    {
        public string ExamName2 { get; set; }
        public double GrantTotal { get; set; }
        public double TotalObtained { get; set; }
        public double TotalPercentage { get; set; }
        public string TotalStatus { get; set; }
        public IEnumerable<SubjectResult> Results { get; set; }

    }
    public class SubjectResult
    {
        public string SubjectName { get; set; }
        public string ObtainedMarks { get; set; }
        public double Percentage { get; set; }
        public double TotalMarks { get; set; }
        public string Status { get; set; }
    }
    public class FeeResponse
    {
        public string ForMonth { get; set; }
        public decimal Total { get; set; }
        public decimal Received { get; set; }
        public string ReceivedDate { get; set; }
        public decimal Balance { get; set; }
    }
    public class FeeInfo
    {
        public decimal Receiveable { get; set; }
        public decimal LasPayment { get; set; }
        public string LastPaymentDate { get; set; }

        public decimal CurrentMonthAmount { get; set; }
        public string CurrentMonthStatus { get; set; }
    }

    public class MessageSubscription
    {
        public int ReceiveFee { get; set; }
        public int IssuedFee { get; set; }
        public int Result { get; set; }

        public int Attendance { get; set; }
        public bool IsMessageActive { get; set; }

        public string ReceiveFeeCharges { get; set; }
        public string IssuedFeeCharges { get; set; }
        public string ResultCharges { get; set; }
        public string AttendanceCharges { get; set; }
    }
    public class BaseQueryModel<TModel>
    {
        public BaseQueryModel()
        {
        }

        public TModel Data { get; set; }
        public List<KeyValueApi> FormValues { get; set; }
        public PictureQueryModel UploadPicture { get; set; }
    }

    public class KeyValueApi
    {
        public KeyValueApi() { }

        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class PictureQueryModel
    {
        public PictureQueryModel() { }

        public string Base64Image { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public int LengthInBytes { get; set; }
    }

    public class LoginModel
    {

        public string Email { get; set; }
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
    public class GenericResponseModel<TResult>
    {
        public GenericResponseModel()
        {
            ErrorList = new List<string>();
        }
        public string Message { get; set; }

        public List<string> ErrorList { get; set; }
        public TResult Data { get; set; }
    }
    public class UserViewModel
    {
        public string FK { get; set; }
        public string UserType { get; set; }
        public String GroupId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string CurrentRoom { get; set; }
        public string Device { get; set; }

        public bool IsAdmin { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string CNIC { get; set; }
    }
    public class RoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
    public class MessageViewModel
    {
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string From { get; set; }
        public string FromUserId { get; set; }
        public string To { get; set; }
        public string Avatar { get; set; }
        public bool IsMine { get; set; }
        public bool IsRoom { get; set; }
        public string type { get; set; }
    }
    public class AppConfigurationModel
    {
        public AppConfigurationModel()
        {
        }
        public string SchoolName { get; set; }
        public string Logo { get; set; }
        public bool ShowHomepageSlider { get; set; }
        public bool SliderAutoPlay { get; set; }
        public int SliderAutoPlayTimeout { get; set; }
        public string AndroidVersion { get; set; }
        public bool AndriodForceUpdate { get; set; }
        public string PlayStoreUrl { get; set; }
        public string IOSVersion { get; set; }
        public bool IOSForceUpdate { get; set; }
        public string AppStoreUrl { get; set; }
        public string LogoUrl { get; set; }
        public string PrimaryThemeColor { get; set; }
        public string TopBarBackgroundColor { get; set; }
        public string TopBarTextColor { get; set; }
        public string BottomBarBackgroundColor { get; set; }
        public string BottomBarActiveColor { get; set; }
        public string BottomBarInactiveColor { get; set; }
        public string GradientStartingColor { get; set; }
        public string GradientMiddleColor { get; set; }
        public string GradientEndingColor { get; set; }
        public bool GradientEnabled { get; set; }


    }
    public class AuthenticationResult
    {
        public string Token { get; set; }
        public UserInfo UserInfo { get; set; }
        public List<StudentInfo> Students { get; set; }
    }
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string[] Roles { get; set; }
    }

    public class StudentInfo
    {
        public int RegNo { get; set; }
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string Picture { get; set; }
        public bool IsCurrent { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int CampusId { get; set; }
        public string CampusName { get; set; }
        public int ClassSectionId { get; set; }
        public string ClassTeacher { get; set; }
    }
    public class DistributorResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ChartData
    {
        public int SortOrder { get; set; }
        public string XValue { get; set; }

        public decimal YValue { get; set; }

        public int shops { get; set; }

        public int productive { get; set; }

        public decimal LTR { get; set; }
        public decimal CTN { get; set; }
        public decimal SaleValue { get; set; }

    }

    public class AppStartModel
    {
        public int DeviceTypeId { get; set; }
        public string SubscriptionId { get; set; }
    }

    public class MessageModel
    {
        public int ID { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        public bool IsMine { get; set; }
        public string Message { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ReadDate { get; set; }
        public string MessageType { get; set; }
        public bool IsDeleted { get; set; }
        public string AuthorName { get; set; }
        public string AuthorPic { get; set; }
    }
}

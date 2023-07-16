using Models;

namespace smsCore.Data.Helpers
{
    public enum NotificationType { error, info }

    public enum NotificationCategory
    {
        InvalidDateInFee,
        OverDueFee,
        NoFeeForStudent,
        NoGradingRuleForExam,
        DoubleSheetForExam,
        FeeDiscountOverThen100Percent,
        TimeTableOverlapping,
    }
    public class NotificationHelper
    {
        SchoolEntities obj;
        public NotificationHelper(SchoolEntities db)
        {
            obj = db;
        }
        //public void GetInvalidDateInFee()
        //{

        //    List<NotificationVm> list = new List<NotificationVm>();
        //    DateTime dt = DateTime.MinValue.Date;
        //    var invalids = obj.FeeSlips.Where(w => w.ForMonth.Date ==dt || w.LastFineDate.Date==dt || w.DueDate.Date==dt);
        //    if (invalids.Any())
        //    {
        //        if (invalids.Where(w => w.ForMonth.Date==dt).Any())
        //        {
        //            var not = new NotificationVm { Message="There are Few records in Fee which has invalid Month.", NotificationCategory=NotificationCategory.InvalidDateInFee, NotificationType=NotificationType.error };
        //            list.Add(not);
        //        }

        //        var invalid2 = invalids.Where(w => w.DueDate.Date==dt);
        //        if (invalid2.Any())
        //        {
        //            var months = string.Join(", ", invalid2.Select(s => s.ForMonth).Distinct().Select(s => s.ToString("MMMM, yyyy")));
        //            var not = new NotificationVm { Message=$"There are Few records in Fee which has invalid Due Date (01/01/01) for months (${months}). Please modify those records. You'll not be able to receive fee for this month.", NotificationCategory=NotificationCategory.InvalidDateInFee, NotificationType=NotificationType.error };
        //            list.Add(not);
        //        }
        //        var invalid3 = invalids.Where(w => w.LastFineDate.Date==dt);
        //        if (invalid3.Any())
        //        {
        //            var months = string.Join(", ", invalid3.Select(s => s.ForMonth).Distinct().Select(s => s.ToString("MMMM, yyyy")));
        //            var not = new NotificationVm { Message=$"There are Few records in Fee which has invalid Last Fine Date (01/01/01) for months (${months}). Please modify those records. You'll not be able to receive fee for this month.", NotificationCategory=NotificationCategory.InvalidDateInFee, NotificationType=NotificationType.error };
        //            list.Add(not);
        //        }
        //    }
        //    //IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SmsHub>();
        //    //SendNotification(hubContext, list);
        //}

        //private  void SendNotification(IHubContext context, List<NotificationVm> data)
        //{
        //    foreach (var d in data)
        //        context.Clients.All.notification(StaticResources.QeuedReturn(d));
        //}
        //public static void GetOverdueFee()
        //{
        //    //List<Notification> list = new List<Notification>();
        //    //SchoolEntities obj = new SchoolEntities();
        //    //DateTime dt = DateTime.Today.Date;
        //    //var invalids = obj.FeeSlips.Where(w => w.DueDate<dt && (w.FeeSlipReceipts==null || w.FeeSlipReceipts.Count()==0));
        //    //if (invalids.Any())
        //    //{
        //    //    var not = new Notification { Message=$"{invalids.Count()} Students has not paid there fees and are overdue.", NotificationCategory=NotificationCategory.InvalidDateInFee, NotificationType=NotificationType.error };
        //    //    list.Add(not);
        //    //}
        //    //IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SmsHub>();
        //    //SendNotification(hubContext, list);
        //}
        //public static void GetNoFeeForStudents()
        //{
        //    List<NotificationVm> list = new List<NotificationVm>();
        //    SchoolEntities obj = new SchoolEntities();
        //    DateTime dt = DateTime.Today.Date;
        //    int month = DateTime.Today.Month;
        //    int year = DateTime.Today.Year;

        //    var invalids = obj.Admissions.Where(w => !w.IsExpell && !w.Student.FreeAdmissions.Any() &&  !w.FeeSlips.Where(ww => ww.ForMonth.Month==month && ww.ForMonth.Year==year).Any());
        //    if (invalids.Any())
        //    {
        //        var not = new NotificationVm { Message=$"{invalids.Count()} Students has no fee slip for current month. Please add Fee slip for these students.", NotificationCategory=NotificationCategory.InvalidDateInFee, NotificationType=NotificationType.error };
        //        list.Add(not);
        //    }
        //    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SmsHub>();
        //    SendNotification(hubContext, list);

        //}
        //public static void GetNoGradingRuleForExam()
        //{
        //    List<NotificationVm> list = new List<NotificationVm>();
        //    SchoolEntities obj = new SchoolEntities();
        //    DateTime dt = DateTime.Today.Date;

        //    var invalids = obj.ExamHelds.Where(w => !w.ExamsRules.Any());
        //    if (invalids.Any())
        //    {
        //        var exams = string.Join("<br />", invalids.ToList().Select(s => s.Exam.ExamName +" "+ s.EntryDate.ToString("MMMM, yyyy")));
        //        var not = new NotificationVm { Message=$"{invalids.Count()} Exams has no grading rules defined.<br />{exams} You may face issue while adding and compling results.", NotificationCategory=NotificationCategory.InvalidDateInFee, NotificationType=NotificationType.error };
        //        list.Add(not);
        //    }
        //    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SmsHub>();
        //    SendNotification(hubContext, list);

        //}
        //public static void GetDoubleDateSheetForExam()
        //{
        //    List<NotificationVm> list = new List<NotificationVm>();
        //    SchoolEntities obj = new SchoolEntities();
        //    DateTime dt = DateTime.Today.Date;

        //    var invalids = obj.ExamDates.GroupBy(g => new { g.ExamHeldID, g.ClassSectionID, g.CampusID, g.SubjectID }).Where(w => w.Count()>1).Select(s => new
        //    {
        //        duplicate = s.Select(t => new { t.ExamHeld.Exam.ExamName, t.ExamHeld.EntryDate, t.Campus.CampusName, t.ClassSection.Class.ClassName, t.ClassSection.Section.SectionName, t.Subject.SubjectName }).FirstOrDefault(),
        //        dates = s.Select(t => new { t.ExamDate1, t.TotalMarks })
        //    });
        //    if (invalids.Any())
        //    {
        //        string message = string.Empty;
        //        foreach (var e in invalids)
        //        {
        //            message += $@"Exam: {e.duplicate.ExamName} {e.duplicate.EntryDate.ToString("MMMM, yyyy")}<br />
        //           Campus:  {e.duplicate.CampusName} <br /> Class: {e.duplicate.ClassName} <br /> Section: {e.duplicate.SectionName}";

        //            foreach (var d in e.dates)
        //            {
        //                message+=$"<br />Date: {d.ExamDate1.ToString("dd/MM/yyyy")} Marks: {d.TotalMarks}";
        //            }
        //        }
        //        var not = new NotificationVm { Message=$"{invalids.Count()} Subjects has more then one datesheet.<br />{message} <br />You may face issue while compling results.", NotificationCategory=NotificationCategory.InvalidDateInFee, NotificationType=NotificationType.error };
        //        list.Add(not);
        //    }
        //    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SmsHub>();
        //    SendNotification(hubContext, list);

        //}
        //public static void GetFeeDiscountOverThen100Percent()
        //{
        //    List<NotificationVm> list = new List<NotificationVm>();
        //    SchoolEntities obj = new SchoolEntities();
        //    DateTime dt = DateTime.Today.Date;

        //    var invalids = obj.FeeDiscounts.Where(w => w.Discount>100 && !w.DiscountInAmount).ToList().Select(s => s.Student.RegistrationNo);
        //    if (invalids.Any())
        //    {
        //        string message = string.Join(", ", invalids);

        //        var not = new NotificationVm { Message=$"{invalids.Count()} Students has more then 100% discount in fee.<br />{message} <br />You may face issue while preparing fee slip.", NotificationCategory=NotificationCategory.InvalidDateInFee, NotificationType=NotificationType.error };
        //        list.Add(not);
        //    }
        //    IHubContext hubContext = GlobalHost.ConnectionManager.GetHubContext<SmsHub>();
        //    SendNotification(hubContext, list);

        //}
    }

    public class NotificationVm
    {
        public string Message { get; set; }
        public NotificationType NotificationType { get; set; }
        public NotificationCategory NotificationCategory { get; set; }
    }
}
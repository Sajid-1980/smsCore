using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using Newtonsoft.Json;
using smsCore.Controllers;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using Utilities;

namespace smsCore.Controllers
{
    [Authorize]
    public class FeeReportsController : BaseController
    {
        
           
        public enum feetypes
        {
            none,
            received,
            outstanding
        }

     //   private static readonly ILog Log = LogManager.GetLogger(typeof(FeeController));

        private readonly SchoolEntities db;
        private readonly int[] _campusid;
        private readonly CurrentUser _user;
        private readonly UtilityFunctions _Utility;
        private readonly FeeSystemHelper _helper;
        private readonly SchoolEntities _entities;
        private readonly FeeLogics feeLogics;

        public FeeReportsController(SchoolEntities _db, CurrentUser user, UtilityFunctions Utility, FeeSystemHelper helper, SchoolEntities entities, FeeLogics _feeLogics)
        {
            db = _db;
            _user = user;
            _campusid = user.GetCampusIds();
            _Utility = Utility;
            _helper = helper;
            _entities = entities;
            feeLogics = _feeLogics;
        }

        // GET: FeeReports
        public IActionResult Index()
        {
            return View();
        }
       
        
        public IActionResult FeeSlip()
        {
            return View();
        }

        
        public IActionResult DuesStatement()
        {
            return View();
        }

        public JsonResult GetDuesStatementList(DataManagerRequest dm, int CampusId = -1, string fromdate = "", string todate = "", int clasId = -1, string regno = "", string category = "", string range = "")
        {
            //feetypes feetype = feetypes.none;
            bool[] isexpell = new bool[] { true, false };
            if (category.ToLower() == "presentstudents".ToLower())
                isexpell = new bool[] { false };
            else if (category.ToLower() == "exstudents".ToLower())
                isexpell = new bool[] { true };

            int[] ids = { };
            var clsID = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] { clasId };
            var camIds = CampusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] { CampusId };

            if (regno != "")
            {
                var adms = regno;
                bool? b = null;
                if (isexpell.Length!=2) b= isexpell[0];
                ids = _Utility.ParseAdmIDs(adms, camIds, b, "studentid");
            }
            else
            { 
                ids = db.Admissions.Where(v => isexpell.Contains(v.IsExpell) && clsID.Contains(v.ClassSection.ClassID) && camIds.Contains(v.CampuseID) && 
                !v.Student.Admissions.Where(w=> (w.Session > v.Session)).Any()).Select(s => s.StudentID).Distinct().ToArray();

                //ids = db.Admissions
                //        .Where(w => isexpell.Contains(w.IsExpell) &&
                //                    clsID.Contains(w.ClassSection.ClassID) && camIds.Contains(w.CampuseID))
                //        .Select(s => s.StudentID).Distinct().ToArray();
            }

            //get all the fee types from database to pivot columns
            //var feetypeNames = new SchoolEntities().FeeTypes.OrderBy(o => o.SortOrder).ToList().Select(w => w.TypeName.Trim()).ToArray();
            //conditon for not received
            //var condition = ""; // @" AND (dbo.GetTotalFeeAmount(dbo.Admissions.Id,Month(dbo.FeeSlips.ForMonth),year(dbo.FeeSlips.ForMonth))-ISNULL(dbo.FeeSlipReceipts.Amount, 0)> 0)";
            var list = db.FeeSlips.Where(w => ids.Contains(w.Admission.StudentID));
            if (range == "true")
            {
                var dtfromdate = DateTimeHelper.ConvertDate(fromdate);
                var dttodate = DateTimeHelper.ConvertDate(todate);

                var from_month = dtfromdate.Month;
                var from_year = dtfromdate.Year;

                var to_month = dttodate.Month;
                var to_year = dttodate.Year;
                list = list.Where(w => w.ForMonth.Month >= from_month && (w.ForMonth.Year >= from_year) &
                    (w.ForMonth.Month <= to_month) & (w.ForMonth.Year <= to_year));
            }
         // var helper = new FeeSystemHelper();
            var data = list.ToList().Select(s => new
            {
                ClassName =s.Admission.ClassSection.Class.ClassName,// s.Admission.Student.Admissions.Any(w => !w.IsExpell) ? s.Admission.Student.Admissions.FirstOrDefault(w => !w.IsExpell).ClassSection.Class.ClassName : s.Admission.Student.Admissions.OrderByDescending(d => d.Session).FirstOrDefault().ClassSection.Class.ClassName,
                s.Admission.Student.StudentName,
                s.Admission.Student.FName,
                ForMonth = s.ForMonth,
                DueDate = s.DueDate,
                AdmissionID = s.Admission.Student.RegistrationNo,
                Total = s.FeeSlipDetails.Select(ss => ss.Amount).DefaultIfEmpty(0).Sum()+_helper.GetLateReceivedFeeFine(s.Id, DateTime.Now),
                Received = s.FeeSlipReceipts.Select(ss => ss.Amount).DefaultIfEmpty(0).Sum(),
                Balance = s.FeeSlipDetails.Select(ss => ss.Amount).DefaultIfEmpty(0).Sum() + _helper.GetLateReceivedFeeFine(s.Id, DateTime.Now) - s.FeeSlipReceipts.Select(ss => ss.Amount).DefaultIfEmpty(0).Sum()
            }).Where(w => w.Balance > 0).Select(sss => new
            {
                sss.ClassName,
                sss.StudentName,
                sss.FName,
                sss.ForMonth,
                sss.DueDate,
                sss.AdmissionID,
                sss.Total,
                sss.Received,
                sss.Balance
            });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                dm.Sorted.Add(new Sort { Name = "ForMonth", Direction = "ascending" });
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Sorted == null  || dm.Sorted.Count == 0)
            {
                List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" }, new Sort { Name = "ForMonth", Direction = "ascending" } };
                sort.Add(new Sort { Name = "ForMonth", Direction = "ascending" });
                data = operation.PerformSorting(data, sort);
            }
            int count = data.Count();
            if (dm.Skip != 0)
            {
                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = count });
        }


        public IActionResult ReceivedFeeByUsers()
        {
            var campusId = _campusid;
            var userlist = db.Users.Select(s => new { s.Id, s.UserName }).ToList();
            if (_user.BasicUserType == EnumManager.BasicUserType.Campus)
                userlist = db.Users
                    .Where(w => campusId.Contains(w.CampusUsers.Select(ss => ss.CampusID).FirstOrDefault()))
                    .Select(s => new { s.Id, s.UserName }).ToList();
            userlist.Insert(0, new { Id = "0", UserName = "ALL" });
            ViewBag.userlist = new SelectList(userlist, "Id", "UserName");
            return View();
        }

        public JsonResult GetReceivedFeeByUsersList(DataManagerRequest dm,string fromdate, string todate, string user)
        {
            var dtfromdate = DateTimeHelper.ConvertDate(fromdate);
            var dttodate = DateTimeHelper.ConvertDate(todate);
            string[] userid = { };
            string[] cols = { };

            if (user != "0" && user != null)
                userid = new[] {user};
            else
                userid = db.Users.Select(s => s.Id).ToArray();

            var data = db.FeeSlipReceipts.AsNoTracking().Where(w =>
               w.EntryDate>= dtfromdate && w.EntryDate<= dttodate &&
                userid.Contains(w.UserId)).Select(s => new
            {
                s.FeeSlip.Admission.Student.RegistrationNo,
                s.FeeSlip.Admission.ClassSection.Class.ClassName,
                s.FeeSlip.Admission.Student.StudentName,
                s.Amount
            }).GroupBy(s => s.RegistrationNo).Select(s => new
            {
                AdmissionID = s.Key,
                s.FirstOrDefault().ClassName,
                s.FirstOrDefault().StudentName,
                Received = s.Sum(d => d.Amount)
            });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
                    data = operation.PerformSorting(data, sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });
        }

        
        public IActionResult ReceivedFee()
        {
            return View();
        }

        public JsonResult GetReceivedFeeForTransport(string fromdate = "", string todate = "", int clasId = 0,
            string regno = "", string category = "", string range = "")
        {
            var feetype = feetypes.received;

            var tab = new DataTable();
            int[] data = {0};
            int[] ids = { };
            var clsID = clasId == 0 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            bool? isexpell = null;
            if (regno != null)
            {
                var adms = regno;
                if (category.ToLower() == "PresentStudents".ToLower())
                    isexpell = false;
                else if (category.ToLower() == "ExStudents".ToLower())
                    isexpell = true;
                else
                    isexpell = null;
                ids = _Utility.ParseAdmIDs(adms, _campusid, isexpell);
            }
            else
            {
                if (category.ToLower() == "PresentStudents".ToLower())
                    ids = db.Admissions
                        .Where(w => w.Student.Admissions.Where(ww => ww.IsExpell == false).Count() > 0 &&
                                    clsID.Contains(w.ClassSection.ClassID)).OrderBy(o => new {o.Student.RegistrationNo})
                        .Select(s => s.ID).ToArray();
                else if (category.ToLower() == "Ex Students".ToLower())
                    ids = db.Admissions
                        .Where(w => w.Student.Admissions.Where(ww => ww.IsExpell == false).Count() < 1 &&
                                    !(w.Student.Admissions.Where(ww => ww.IsExpell == false).Count() > 0) &&
                                    clsID.Contains(w.ClassSection.ClassID)).OrderBy(o => new {o.Student.RegistrationNo})
                        .Select(s => s.ID).ToArray();
                else
                    ids = db.Admissions.Where(w => clsID.Contains(w.ClassSection.ClassID))
                        .OrderBy(o => new {o.Student.RegistrationNo}).Select(s => s.ID).ToArray();
            }

            //get all the fee types from database to pivot columns
            var feetypeNames = _entities.FeeTypes.OrderBy(o => o.SortOrder).ToList()
                .Select(w => w.TypeName.Trim()).ToArray();
            //conditon for not received
            var condition = "";
            // @" AND (dbo.GetTotalFeeAmount(dbo.Admissions.Id,Month(dbo.FeeSlips.ForMonth),year(dbo.FeeSlips.ForMonth))-ISNULL(dbo.FeeSlipReceipts.Amount, 0)> 0)";

            if (feetype == feetypes.received)
                //conditon for  received date
                condition = " AND (FeeSlipReceipts.EntryDate BETWEEN '" +
                            DateTime.Parse(fromdate).Date.ToString("MM/dd/yyyy") + "' AND '" +
                            DateTime.Parse(todate).Date.ToString("MM/dd/yyyy") + "')";
            if (range == "true")
                //conditon for fee month check
                condition += " AND (Month(dbo.FeeSlips.ForMonth) >= " + DateTime.Parse(fromdate).Date.Month +
                             " AND Year(dbo.FeeSlips.ForMonth) >=" + DateTime.Parse(todate).Date.Year +
                             " AND Month(dbo.FeeSlips.ForMonth) <= " + DateTime.Parse(fromdate).Date.Month +
                             " AND Year(dbo.FeeSlips.ForMonth) <=" + DateTime.Parse(todate).Date.Year + ")";


            var tabs = feeLogics.Construct(ids, feetype == feetypes.received, isexpell, FeeLogicIDTypes.admissionId,
                condition);
            var list = Json(new {Data = tab});
            return list;
        }

        
        public IActionResult ClassWiseListOfFeeGroup()
        {
            return View();
        }

        public JsonResult GetClassWiseListOfFeeGroup(DataManagerRequest dm, int CampusId = -1, int clasId = -1)
        {
            var camIds = new[] { CampusId };
            var clsIds = new[] { clasId };

            var data = db.Admissions.AsNoTracking()
                  .Where(w => w.IsExpell == false && camIds.Contains(w.CampuseID) &&
                              clsIds.Contains(w.ClassSection.ClassID)).Select(s => new
                              {
                                  AdmissionID = s.Student.RegistrationNo,
                                  s.Student.StudentName,
                                  s.Student.FName,
                                  FeeGroupName = s.Student.FreeAdmissions.Any()
                          ? s.ClassSection.Class.ClassFeeGroups.FirstOrDefault().FeeGroup.FeeGroupName
                          : "F"
                              });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
                    data = operation.PerformSorting(data, sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });
        }

        public IActionResult FeeGroupList()
        {
            var campusId = _campusid;
            var feegroup = db.FeeGroups.Select(s => new {s.ID, s.FeeGroupName}).ToList();
         // feegroup.Insert(0, new {ID = 0, FeeGroupName = "ALL"});
         //var list = new SelectList(feegroup, "ID", "FeeGroupName");
            ViewBag.feegroup = new SelectList(feegroup, "ID", "FeeGroupName");
            return View();
        }

        public JsonResult GetFeeGroupList(DataManagerRequest dm,int groupID)
        {
                try
                {
                var id = int.Parse(groupID.ToString());
                int[] clsIds = db.ClassFeeGroups.Where(w => w.FeeGroupID == id).Select(s => s.ClassID).ToArray();
                int[] ids = db.Admissions.Where(w=>clsIds.Contains( w.ClassSection.ClassID)).Select(t=>t.ID).ToArray();
                var datas = _helper.GetNetFeeOfStudents(ids);
               
                    DataOperations operation = new DataOperations();

                    if (dm.Search != null && dm.Search.Count > 0)
                    {
                        datas = operation.PerformSearching(datas, dm.Search);  //Search
                    }
                    if (dm.Where != null && dm.Where.Count > 0) //Filtering
                    {
                        datas = operation.PerformFiltering(datas, dm.Where, dm.Where[0].Operator);
                    }

                    if (dm.Sorted != null && dm.Sorted.Count > 0)
                    {
                        datas = operation.PerformSorting(datas, dm.Sorted);
                    }

                    if (dm.Skip != 0)
                    {
                        if (dm.Sorted.Count == 0)
                        {
                            List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
                            datas = operation.PerformSorting(datas, sort);
                        }

                        datas = operation.PerformSkip(datas, dm.Skip);
                    }

                    if (dm.Take != 0)
                    {
                        datas = operation.PerformTake(datas, dm.Take);
                    }

                    return Json(new { result = datas, count = datas.Count() });
                }
                catch(Exception ex)
                {
                    return Json(new { result = ex.Message.ToString() });
               
                }
        }

        
        public IActionResult DuesStatementAmount()
        {
            return View();
        }

        public JsonResult GetDuesStatementAmountdata(DataManagerRequest dm,int clasId = -1, int campusID = -1, int sectionId = -1, string category = "")
        {
            int[] data = {0};
            int[] ids = { };
            var clsID = clasId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {clasId};
            var campid = campusID == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusID};
            var secid = sectionId == -1 ? db.Sections.Select(s => s.ID).ToArray() : new[] {sectionId};

            bool[] isexpell = new bool[] { true, false };
            if (category.ToLower() == "presentstudents".ToLower())
                isexpell = new bool[] { false };
            else if (category.ToLower() == "exstudents".ToLower())
                isexpell = new bool[] { true };

           data = db.Admissions.Where(v => isexpell.Contains(v.IsExpell) && clsID.Contains(v.ClassSection.ClassID) && campid.Contains(v.CampuseID) &&
                !v.Student.Admissions.Where(w => (w.Session > v.Session)).Any()).Select(s => s.StudentID).Distinct().ToArray();

            var OutStandingFeeSlipids = feeLogics.GetOutStandingOfStudents(data);  //.GetOutStanding(data);

            var data1 = db.FeeSlips.AsNoTracking().Where(w => OutStandingFeeSlipids.Contains(w.Id))
                .GroupBy(w => w.Admission.Student.RegistrationNo).AsNoTracking().ToList().Select(s => new
                {
                    AdmissionID = s.FirstOrDefault().Admission.Student.RegistrationNo,
                    s.FirstOrDefault().Admission.Student.StudentName,
                    s.FirstOrDefault().Admission.Student.FName,
                    s.FirstOrDefault().Admission.ClassSection.Class.ClassName,
                    Amount =_helper .GetLateReceivedFeeFine(s.FirstOrDefault().Id, DateTime.Now)+s.Sum(ss => ss.FeeSlipDetails.Select(sss => sss.Amount).Sum()),
                    Balance = _helper.GetLateReceivedFeeFine(s.FirstOrDefault().Id, DateTime.Now)+ s.Sum(ss => ss.FeeSlipDetails.Select(sss => sss.Amount).Sum()) -
                              s.Sum(ss => ss.FeeSlipReceipts.Select(sss => sss.Amount).Sum()),
                    //LateFee=new FeeSystemHelper().GetLateReceivedFeeFine(s.FirstOrDefault().Id,DateTime.Now),
                    Received = s.Sum(ss => ss.FeeSlipReceipts.Select(sss => sss.Amount).Sum()),
                    mobileno =
                        s.FirstOrDefault().Admission.Student.StudentMobiles.Where(w => w.IsDefault).FirstOrDefault() ==
                        null
                            ? ""
                            : s.FirstOrDefault().Admission.Student.StudentMobiles.Where(w => w.IsDefault)
                                .FirstOrDefault().MobileNo
                });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data1 = operation.PerformSearching(data1, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data1 = operation.PerformFiltering(data1, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data1 = operation.PerformSorting(data1, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
                    data1 = operation.PerformSorting(data1, sort);
                }

                data1 = operation.PerformSkip(data1, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data1 = operation.PerformTake(data1, dm.Take);
            }

            return Json(new { result = data1, count = data1.Count() });
        }

        
        public IActionResult FeeRecordRegister()
        {
            return View();
        }

        public JsonResult GetFeeRecordRegister(DataManagerRequest dm,int CampusId, string regno = "-1", string session = "-1")
        {
            var camIds = CampusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {CampusId};
            var sessId = int.Parse(session) == -1
                ? db.FinancialYears.Select(s => s.financialYearId).ToArray()
                : new[] {int.Parse(session)};
            var ids = _Utility.ParseAdmIDs(regno, camIds);

            var data = db.FeeSlips.ToList()
                .Where(w => ids.Contains(w.AdmissionId) && sessId.Contains(w.Admission.Session)).Select(
                    s => new
                    {
                        AdmissionID = s.Admission.Student.RegistrationNo,
                        s.Admission.Student.StudentName,
                        s.Admission.Student.FName,
                        TelephoneResidance = s.Admission.Student.ResidanceTelephone,
                        TelephoneOff = s.Admission.Student.OfficeTelephone,
                        s.Admission.Student.PostalAddress,
                        MobileNo = s.Admission.Student.StudentMobiles.Where(w => w.IsDefault).Select(ss => ss.MobileNo)
                            .FirstOrDefault(),
                        s.Admission.ClassSection.Class.ClassName,
                        Total = s.FeeSlipDetails.Select(ss => ss.Amount).Sum(),
                        Received = s.FeeSlipReceipts.Select(ss => ss.Amount).Sum(),
                        ForMonthYear = s.ForMonth,
                        ReceivedDate = s.FeeSlipReceipts.Count > 0
                            ? s.FeeSlipReceipts.Select(ss => ss.EntryDate).FirstOrDefault().ToString()
                            : ""
                    });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
                    data = operation.PerformSorting(data, sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });

        }

        
        public IActionResult ExemptFromLateFee()
        {
            return View();
        }

        public JsonResult GetExemptFromLateFee(DataManagerRequest dm,int CampusId = -1)
        {
            var camIds = CampusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {CampusId};
            var data = db.Admissions
                .Where(w => w.IsExpell == false && w.Student.ApplyLateFee == 1 && camIds.Contains(w.CampuseID)).Select(
                    s => new
                    {
                        AdmissionID = s.Student.RegistrationNo,
                        s.Student.StudentName,
                        s.Student.FName,
                        s.ClassSection.Class.ClassName
                    });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
                    data = operation.PerformSorting(data, sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });


        }
        // scholarship List here
        public IActionResult SholarshipList()
        {
            return View();
        }

        public JsonResult SholarshipListData(int clsId, int cmpId, int regno = 0)
        {
            var data = db.Admissions.Where(w => !w.IsExpell && w.Student.FeeDiscounts.Count > 0);
            if (regno == 0)
                data = data.Where(w => w.ClassSection.ClassID == clsId && w.CampuseID == cmpId);
            else
                data = data.Where(w => w.Student.RegistrationNo == regno);

           var result=data.Select(s => new
            {
                AdmissionID = s.Student.RegistrationNo,
                s.CampuseID,
                s.ClassSection.ClassID,
                s.Student.StudentName,
                s.Student.FName,
                s.ClassSection.Class.ClassName,
                DiscountInAmount = s.Student.FeeDiscounts.Select(d => d.DiscountInAmount).FirstOrDefault(),
                DiscountAmount = s.Student.FeeDiscounts.Select(d => d.Discount).FirstOrDefault(),
                TotalFee = s.ClassSection.Class.ClassFeeGroups.Sum(f =>
                    f.FeeGroup.FeeStructures.Where(w => w.CampusID == s.CampuseID).Sum(m => m.Amount))
            }).ToList().Select(s => new
            {
                s.AdmissionID,
                s.CampuseID,
                s.ClassID,
                s.StudentName,
                s.FName,
                s.ClassName,
                s.DiscountInAmount,
                s.DiscountAmount,
                s.TotalFee,
                Percentage = s.DiscountInAmount == false ? 0 : s.DiscountAmount / s.TotalFee * 100
            });

            var sholar = Json(new {Data = result});
            return sholar;
        }

        public object SholarshipListDetails(int clsId, int cmpId)
        {
            var data = db.FeeDiscounts
                .Where(w => w.Student.Admissions.Where(a => a.ClassSection.ClassID == clsId && !a.IsExpell).Any())
                .Select(s => new
                {
                    s.Student.RegistrationNo,
                    s.Student.StudentName,
                    s.Student.FName,
                    s.FeeType.TypeName,
                    //s.Discount,
                    //s.DiscountInAmount,
                    Discount = s.DiscountInAmount
                        ? s.Discount / 100 * s.Discount
                        : s.FeeType.FeeStructures
                            .Where(w => w.CampusID == cmpId &&
                                        w.FeeGroup.ClassFeeGroups.Where(cf => cf.ClassID == clsId).Any())
                            .Select(fa => fa.Amount).DefaultIfEmpty(1).FirstOrDefault()
                }).AsDataTable();

            var pt = new Pivot(data);
            pt.PivotData("TypeName", "Discount", AggregateFunction.Last, "RegistrationNo", "StudentName", "FName");

            return JsonConvert.SerializeObject(data);
        }
        //sholarship list end here

        //Received Fee List
        public ActionResult FeeStatementByClass()
        {
            return View();
        }

        public JsonResult FeeStatementByClassData(DataManagerRequest dm,int campusId, int classId, int month, int year)
        {
            var data = db.FeeSlips.Where(w =>
                w.ForMonth.Year == year && w.FeeSlipReceipts.Count() > 0 &&
                w.Admission.CampuseID == campusId && w.Admission.ClassSection.ClassID == classId && w.ForMonth.Month == month &&
                !w.Admission.IsExpell).Select(s => new
                {
                    AdmissionID = s.Admission.Student.RegistrationNo,
                    s.Admission.Student.StudentName,
                    s.Admission.Student.FName,
                    Mobile = s.Admission.Student.StudentMobiles.Where(w => w.IsDefault).Select(m => m.MobileNo)
                    .FirstOrDefault(),
                    Total = s.FeeSlipDetails.Select(f => f.Amount).DefaultIfEmpty(0).Sum() +_helper.GetLateReceivedFeeFine(s.Id, DateTime.Now),
                    Received = s.FeeSlipReceipts.Select(f => f.Amount).DefaultIfEmpty(0).Sum()
                });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
                    data = operation.PerformSorting(data, sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });


        }

        public IActionResult FeeDueStatementByClass()
        {
            return View();
        }

        public JsonResult FeeDueStatementByClassList(DataManagerRequest dm,int campusId, int classId, int month, int year)
        {
            var data = db.FeeSlips.Where(w => w.ForMonth.Month == month && w.ForMonth.Year == year &&
                                             w.FeeSlipDetails.Sum(st => st.Amount) > w.FeeSlipReceipts
                                                 .Select(st => st.Amount).DefaultIfEmpty(0).Sum()
                                             && w.Admission.CampuseID == campusId &&
                                             w.Admission.ClassSection.ClassID == classId && !w.Admission.IsExpell)
                .Select(s => new
                {
                    AdmissionID = s.Admission.Student.RegistrationNo,
                    s.Admission.Student.StudentName,
                    s.Admission.Student.FName,
                    Mobile = s.Admission.Student.StudentMobiles.Where(w => w.IsDefault).Select(m => m.MobileNo)
                        .FirstOrDefault(),
                    Total = s.FeeSlipDetails.Select(f => f.Amount).DefaultIfEmpty(0).Sum(),
                    Received = s.FeeSlipReceipts.Select(f => f.Amount).DefaultIfEmpty(0).Sum(),
                    Balance = s.FeeSlipDetails.Select(ss => ss.Amount).DefaultIfEmpty(0).Sum() -
                              s.FeeSlipReceipts.Select(ss => ss.Amount).DefaultIfEmpty(0).Sum()
                });
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }

            if (dm.Sorted != null && dm.Sorted.Count > 0)
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }

            if (dm.Skip != 0)
            {
                if (dm.Sorted.Count == 0)
                {
                    List<Sort> sort = new List<Sort>() { new Sort { Name = "AdmissionID", Direction = "ascending" } };
                    data = operation.PerformSorting(data, sort);
                }

                data = operation.PerformSkip(data, dm.Skip);
            }

            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            return Json(new { result = data, count = data.Count() });
        }
    }
}
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Newtonsoft.Json;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Text.Json.Serialization;
using System.Text.Json;
using Utilities;


namespace sms.WebApiControllers
{
    [Authorize]
    //[RoutePrefix("api/fee")]
    [Route("api/fee")]
    public class FeeApiController : Controller
    {
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;
        private readonly UtilityFunctions UtilityFunctions;
        private readonly DatabaseAccessSql dba;
        private readonly Extensions1 _methods;
        public FeeApiController(SchoolEntities _db,CurrentUser Cuser, UtilityFunctions utilityFunctions,DatabaseAccessSql _dba, Extensions1 methods)
        {
            db = _db;
            _user = Cuser;
            UtilityFunctions = utilityFunctions;
            dba = _dba;
            _methods = methods;
        }

        [HttpGet]
        [Route("feegroups/{feegroupId}")]
        //public JsonResult GetFeeGrups(bool search, int feegroupId = 0)
        //{
        //    if (feegroupId == 0)
        //    {
        //        var campuses = db.FeeGroups.AsNoTracking().Select(s => new { id = s.ID, text = s.FeeGroupName }).ToList();
        //        if (search) campuses.Insert(0, new { id = -1, text = "All" });
        //        return new JsonResult { Data = campuses, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        //    }

        //    //var feegr = db.FeeGroups.Where(w=>w.ID== feegroupId).FirstOrDefault();
        //    var newwww = db.FeeGroups.AsNoTracking()
        //        .Select(s => new { id = s.ID, text = s.FeeGroupName, selected = s.ID == feegroupId }).ToList();

        //    if (search) newwww.Insert(0, new { id = -1, text = "All", selected = false });
        //    //var camp = new SelectList(newwww, "id", "text", feegroupId);

        //    return new JsonResult { Data = newwww, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        //}

        public IActionResult GetFeeGrups(bool search, int feegroupId = 0)
        {
            if (feegroupId == 0)
            {
                var campuses = db.FeeGroups.AsNoTracking().Select(s => new { id = s.ID, text = s.FeeGroupName }).ToList();
                if (search) campuses.Insert(0, new { id = -1, text = "All" });
                return (IActionResult)Json(campuses);
            }

            var newwww = db.FeeGroups.AsNoTracking()
                .Select(s => new { id = s.ID, text = s.FeeGroupName, selected = s.ID == feegroupId }).ToList();

            if (search) newwww.Insert(0, new { id = -1, text = "All", selected = false });

            return (IActionResult)Json(newwww);
        }

        [HttpGet]
        [Route("getfeeGroup/{clas}")]
        public int getfeeGroup(int clas, int campusid)
        {
            var feegr = db.ClassFeeGroups.AsNoTracking().Where(w => w.ClassID == clas && w.CampusID == campusid)
                .Select(s => s.FeeGroupID).FirstOrDefault();
            return feegr;
        }


        //[System.Web.Http.Route("payment-history")]
        //public JqGridJsonResult GetPaymentHistory(JqGridRequest request)
        //{
        //    //var clasfeeGroup = db.ClassFeeGroups.ToList();
        //    var FeeSlipReceipt = db.FeeSlipReceipts.AsNoTracking().ToList();
        //    var totalRecords = FeeSlipReceipt.Count();
        //    //Prepare JqGridData instance
        //    var response = new JqGridResponse
        //    {
        //        //Total pages count
        //        TotalPagesCount = (int) Math.Ceiling(totalRecords / (float) request.RecordsCount),

        //        //Page number
        //        PageIndex = request.PageIndex,
        //        //Total records counts
        //        TotalRecordsCount = totalRecords
        //        //Footer data
        //        //UserData = new { QuantityPerUnit = "Avg Unit Price:", UnitPrice = _productsRepository.GetProducts(filterExpression).Average(p => p.UnitPrice) }
        //    };
        //    var pno = request.PageIndex;
        //    var perpage = request.RecordsCount;
        //    var skip = perpage * pno;

        //    IEnumerable<JqGridRecord> data = null;
        //    //var n = _context.Sector.OrderBy(.OrderByDescending(ToLambda<T>(propertyName));
        //    //            if (request.SortingOrder == JqGridSortingOrders.Asc)
        //    //.OrderBy(request.SortingName) For Month Campus Entry Date  User id  Edit By Edit On Last Fine Date

        //    data = FeeSlipReceipt.Skip(skip).Take(perpage).ToList().Select(s =>
        //        new JqGridRecord(s.Id.ToString(),
        //            new {s.Id, s.Amount, s.EntryDate, s.FeeSlipPaymentMethod.PaymentMethodName}));

        //    //data = FeeSlipReceipt.Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.Id.ToString(), new { s.Amount,s.EntryDate,s.FeeSlipPaymentMethod.PaymentMethodName, action = $"<a title=\"Edit\" href=\"{Url.Action("FeeSlipDetailEdit", "Fee", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("FeeSlipDetailDelete", "Fee", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> " }));
        //    ////else
        //    //    data = _context.Employee.OrderByDescending(request.SortingName).Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.Id.ToString(), new { s.Id, s.EmployeeName, s.Cnic, s.Address, s.MobileNo, s.Email, action = $"<a title=\"Placement\" href=\"{Url.Action("CreatePlacement", "Employees", new { employeeId = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-plus\"></i></a> <a title=\"Edit\" href=\"{Url.Action("Edit", "Employees", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("Delete", "Employees", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a>" }));

        //    foreach (var d in data) response.Records.Add(d);
        //    //response.Records = data;
        //    response.Reader.RepeatItems = false;
        //    return new JqGridJsonResult {Data = response};
        //    //return db.Companies.ToList();
        //}

        //[System.Web.Http.Route("GetPackages")]
        //public string getpackage()
        //{
        //    var dtblSalaryPackage = new DataTable();
        //    var spSalaryPackage = new SalaryPackageSP();
        //    var data = spSalaryPackage.SalaryPackageViewAllForMonthlySalarySettings().AsEnumerable().Select(s => new
        //    {
        //        salaryPackageId = (decimal) s["salaryPackageId"],
        //        salaryPackageName = s["salaryPackageName"].ToString()
        //    });
        //    var package = new SelectList(data, "salaryPackageId", "salaryPackageName");
        //    return JsonConvert.SerializeObject(package);
        //}

        [HttpGet]
        [Route("get-fee-month/{regno}")]
        public string GetFeeMonth(string regno)
        {
            var campusId = _user.GetCampusIds();
            var id = UtilityFunctions.ParseAdmIDs(regno, campusId);
            

            var months = db.FeeSlips.AsNoTracking().Where(w => id.Contains(w.AdmissionId)).OrderBy(o=>o.ForMonth).Select(s=>new {s.Id, s.ForMonth }).ToList()
                .Select(s => new {id = s.Id, text = s.ForMonth.ToString("MMMM, yyyy")}).ToList();
            return JsonConvert.SerializeObject(months);
        }
        [HttpPost]
        [Route("get-student-fee-for-month/{feeslipId}")]
        public JsonResult GetStudentFeeListForJq(DataManagerRequest dm ,int feeslipId)
        {
            //var clasfeeGroup = db.ClassFeeGroups.ToList();
            var data = db.FeeSlipDetails.Where(w => w.FeeSlipId == feeslipId).Select(s => new
            {
                s.Id, s.FeeType.TypeName, s.Amount, s.Discount,
                s.DiscountInAmount
            });
            // return Json<object>(new { result = data, count = data.Count() });
            return Json(new { data });

        }
        [HttpGet]
        [Route("GetAdvanceMonth")]
        public string GetAdvanceMonth(int regno, int year)
        {
            //var year = _db.tbl_FinancialYear.AsNoTracking().Where(w => w.IsCurrent.HasValue && w.IsCurrent.Value).FirstOrDefault().toDate.Value.Year;
            var campusId = _user.GetCampusIds();
            var regnos = UtilityFunctions.ParseRegNos(regno.ToString(), campusId);
            if (regnos.Count() <= 0) return "Invalid Data";
            var monthsExisting = db.FeeSlips.AsNoTracking()
                .Where(w => w.Admission.Student.RegistrationNo == regno && w.ForMonth.Year == year &&
                            !w.Admission.IsExpell).Select(s => s.ForMonth.Month).ToList();
            var listd = new List<dynamic>();
            //var monthsExisting = getmonth.Select(s => s.Month);
            for (var i = 1; i <= 12; i++)
            {
                if (monthsExisting.Contains(i))
                    continue;

                listd.Add(new {id = i, text = ExtenstionMethods.MonthNames[i - 1]});
            }

            return JsonConvert.SerializeObject(listd);
        }
        [HttpGet]
        [Route("GetamountofFee")]
        public string getamountofFee(int regno, string months)
        {
            var clasid = db.Admissions.AsNoTracking().Where(w => !w.IsExpell && w.Student.RegistrationNo == regno)
                .Select(s => new
                {
                    s.ClassSection.ClassID, s.CampuseID,
                    FeegroupId = s.ClassSection.Class.ClassFeeGroups
                        .Where(w => w.ClassID == s.ClassSection.ClassID && w.CampusID == s.CampuseID)
                        .Select(ss => ss.FeeGroupID).FirstOrDefault()
                }).FirstOrDefault();
            if (clasid == null) return "Invalid data.";
            var amount = db.FeeStructures.AsNoTracking()
                .Where(w => w.CampusID == clasid.CampuseID && w.FeeGroupId == clasid.FeegroupId && w.IsActive &&
                            w.Revised == false).Select(s => s.Amount).DefaultIfEmpty(0)
                .Sum(); //== null?0: db.FeeStructures.Where(w => w.CampusID == clasid.CampuseID && w.FeeGroupId == clasid.FeegroupId && w.IsActive == true && w.Revised == false).Sum(s => s.Amount);

            var monthsplit = months.Split(',');
            decimal totalamount = 0;
            for (var i = 0; i < monthsplit.Count(); i++) totalamount += amount;
            return JsonConvert.SerializeObject(totalamount);
        }
        [HttpGet]
        [Route("GetFeetypeAmount")]
        
        public string getFeetypeAmount(int typeId, int campus, int classes)
        {
            var feegroupId = db.ClassFeeGroups.AsNoTracking().Where(w => w.ClassID == classes && w.CampusID == campus)
                .FirstOrDefault().FeeGroupID;
            if (feegroupId <= 0) return "Invalid data.";
            var amount = db.FeeStructures.AsNoTracking()
                .Where(w => w.CampusID == campus && w.FeeTypeId == typeId && w.FeeGroupId == feegroupId && w.IsActive &&
                            w.Revised == false).Select(s => s.Amount).FirstOrDefault();
            return JsonConvert.SerializeObject(amount);
        }
        [HttpGet]
        [Route("GetFeetype")]
        public string getFeetype()
        {
            var type = db.FeeTypes.Select(s => new {id = s.Id, text = s.TypeName}).ToList();
            return JsonConvert.SerializeObject(type);
        }
        [HttpGet]

        [Route("GetexamFeeforStudent")]
        public string getexamFeeforStudent(int regno)
        {
            var clasid = db.Admissions.AsNoTracking().Where(w => !w.IsExpell && w.Student.RegistrationNo == regno)
                .Select(s => new
                {
                    s.ClassSection.ClassID, s.CampuseID,
                    FeegroupId = s.ClassSection.Class.ClassFeeGroups.Select(f => f.FeeGroupID).FirstOrDefault()
                }).FirstOrDefault();
            if (clasid == null) return "Invalid data.";
            var amount = db.FeeStructures.AsNoTracking()
                .Where(w => w.CampusID == clasid.CampuseID && w.FeeGroupId == clasid.CampuseID && w.IsActive &&
                            w.Revised == false && w.FeeTypeId == 1).Select(s => s.Amount).FirstOrDefault();
            return JsonConvert.SerializeObject(amount);
        }
        [HttpGet]

        [Route("GetexamFeeforclass")]
        public string getexamFeeforclass(int classId, int campusId)
        {
            var classIds = classId == -1 ? db.Classes.Select(s => s.ID).ToArray() : new[] {classId};
            var cmpus = campusId == -1 ? db.Campuses.Select(s => s.ID).ToArray() : new[] {campusId};
            var clasid = db.Admissions.AsNoTracking()
                .Where(w => !w.IsExpell && classIds.Contains(w.ClassSection.ClassID) && cmpus.Contains(w.CampuseID))
                .Select(s => new
                {
                    s.ClassSection.ClassID, s.CampuseID,
                    FeegroupId = s.ClassSection.Class.ClassFeeGroups.Select(f => f.FeeGroupID).FirstOrDefault()
                }).FirstOrDefault();
            if (clasid == null) return "Invalid data.";
            var amount = db.FeeStructures
                .Where(w => w.CampusID == clasid.CampuseID && w.FeeGroupId == clasid.CampuseID && w.IsActive &&
                            w.Revised == false && w.FeeTypeId == 1).Select(s => s.Amount).FirstOrDefault();
            return JsonConvert.SerializeObject(amount);
        }
        [HttpGet]
        [Route("get-receiveable-fee")]
        public JsonResult GetRecieveFeeByIndividualStd(int regno, string receivedDate, int campusID = -1)
        {

            var collection2 = new List<SqlParameter>();

            var startdate2 = new SqlParameter("regno", regno);

            collection2.Add(startdate2);

            var tabArrears = dba.ExecuteSP("[dbo].[FeeSystemGetAllReceiveableAmount]", collection2);
            //var options = new JsonSerializerOptions
            //{
            //    ReferenceHandler = ReferenceHandler.Preserve,
            //    MaxDepth = 64 // Set the maximum depth to a larger value if needed
            //};
            //var result = new JsonResult(tabArrears, options);
            //return result;
            var result = new JsonResult(tabArrears);
            return result;
        }

        [HttpPost]
        [Route("get-student-receiveables")]
        public JsonResult GetIndividualStdJQ(DataManagerRequest dm, int regno, string ReceivedDate)
        {
            var collection2 = new List<SqlParameter>();

            var startdate2 = new SqlParameter("regno", regno);

            collection2.Add(startdate2);

            var tabArrears = dba.ExecuteSP("[dbo].[FeeSystemGetAllReceiveable]", collection2);
            DataColumn dc = new DataColumn();
            dc.ColumnName = "Received";
            dc.DataType = typeof(int);
            dc.DefaultValue = 0;
            tabArrears.Columns.Add(dc);
            DataView dv = tabArrears.AsDataView();
            if (tabArrears.Columns.Contains("ForMonth"))
                dv.Sort = "ForMonth ASC";
            tabArrears=dv.ToTable();
           //return Json(new { result = tabArrears, count = tabArrears.Rows.Count });
           return Json(new { tabArrears });
        }

        [HttpPost]
        [Route("get-student-received-fee-for-month/{feeslipId}")]
        public JsonResult GetPaymentHistoryForJq(DataManagerRequest dm, int feeslipId)
        {
            var data = db.FeeSlipReceipts.AsNoTracking().Where(w => w.FeeSlipId == feeslipId).Select(s => new
            {
                s.Id,
                s.Amount,
                s.EntryDate,
                s.FeeSlipPaymentMethod.PaymentMethodName,
            });
           // return Json<object>(new { result = data, count = data.Count() });
           return Json(new {data });
             }
        [HttpGet]
        [Route("get-student-Info")]
        public string Getstudentinfo(int regno)
        {
            var photo = _methods.GetPhoto(regno);
            var student = db.Admissions.AsNoTracking().Where(w => w.Student.RegistrationNo == regno).Select(s => new
            {
                StudentPhotos = photo, s.Student.StudentName, s.Campus.CampusName, s.ClassSection.Class.ClassName,
                s.ClassSection.Section.SectionName, s.Student.FName,
                mobile = s.Student.StudentMobiles.FirstOrDefault().MobileNo == null
                    ? ""
                    : s.Student.StudentMobiles.FirstOrDefault().MobileNo
            }).FirstOrDefault();
            return JsonConvert.SerializeObject(student);
        }
        [HttpPost]
        [Route("get-student-fee-for-session")]
        public JsonResult GetStudentFeeForSession(int studentId, int session)
        {
            var data = db.FeeSlips.Where(w => w.Admission.StudentID == studentId && w.Admission.Session == session).Select(s => new
            {
                s.ForMonth,
                Total = s.FeeSlipDetails.Sum(f => f.Amount),
                Received = s.FeeSlipReceipts.Select(f=>f.Amount).DefaultIfEmpty(0).Sum(),
                ReceivedDate = s.FeeSlipReceipts.Select(f => f.EntryDate).DefaultIfEmpty().FirstOrDefault()
            }).OrderBy(o=>o.ForMonth).ToList().Select(s => new
            {
                s.ForMonth,
                s.Total,
                s.Received,
                ReceivedDate=s.ReceivedDate ==DateTime.MinValue?string.Empty:s.ReceivedDate.ToString("dd/MM/yyyy"),
                Balance = s.Total - s.Received
            });
            //return Json<object>(new { result = data, count = data.Count() });
            return Json(new {data });   
        }
        [HttpGet]
        [Route("get-student-Info-complaint")]
        public string Getstudentinfocomp(int regno)
        {
            var photo = _methods.GetPhoto(regno);
            var user = _user.dec_primaryId;
            var classectionId = db.TeachingClasses.Where(w => w.StaffID == user).Select(s => s.ClassSectionId).ToList();
            var student = db.Admissions.AsNoTracking()
                .Where(w => w.Student.RegistrationNo == regno && classectionId.Contains(w.ClassSectionID)).Select(s =>
                    new
                    {
                        StudentPhotos = photo,
                        s.Student.StudentName,
                        s.Campus.CampusName,
                        s.ClassSection.Class.ClassName, s.ClassSection.Section.SectionName,
                        s.Student.FName,
                        mobile = s.Student.StudentMobiles.FirstOrDefault().MobileNo == null
                            ? ""
                            : s.Student.StudentMobiles.FirstOrDefault().MobileNo
                    }).FirstOrDefault();
            return JsonConvert.SerializeObject(student);
        }
    }
}
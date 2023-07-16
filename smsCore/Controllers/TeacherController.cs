using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using smsCore.Data;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
 
namespace smsCore.Controllers
{
    public class TeacherController : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        //private ApplicationSignInManager _signInManager;
        //private ApplicationUserManager _userManager;

        // GET: Teacher
        private readonly SchoolEntities db;// = new SchoolEntities();
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly CurrentUser _user;
        private readonly DatabaseAccessSql dba;



        public TeacherController(SignInManager<ApplicationUser> signInManager , UserManager<ApplicationUser> userManager , SchoolEntities _db , RoleManager<IdentityRole> _roleManager, CurrentUser user, DatabaseAccessSql _dba)
        {
            dba = _dba;
            _signInManager = signInManager;
            _userManager = userManager;
            db = _db;
            roleManager = _roleManager;
            _user = user;

        }

        //public TeacherController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        //{
        //    _userManager = userManager;
        //    _signInManager = signInManager;
        //    roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        //}

        //public ApplicationSignInManager SignInManager
        //{
        //    get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
        //    private set => _signInManager = value;
        //}

        //public ApplicationUserManager UserManager
        //{
        //    get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    private set => _userManager = value;
        //}

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult TeacherProfile()
        {
            var userId = _user.UserID;
            var id = db.UserDefinitions.Where(w => w.UserId == userId && w.UserType == "E").Select(s => s.FK)
                .FirstOrDefault();
            ;
            var data = db.tbl_Employee.Where(w => w.Id == id).FirstOrDefault();

            return View(data);
        }

        public IActionResult Teacheruser(decimal id)
        {
            ViewBag.Id = id;
            return View();
        }

        //public string Teacherusers(RegisterViewModel model)
        //{
        //    if (Request.Form["Id"].ToString() == null) return "The Data is Not Valid";
        //    var Id = int.Parse(Request.Form["Id"].ToString());
        //    var checkexist = db.UserDefinitions.Where(w => w.FK == Id && w.UserType.ToLower() == "e").ToList();
        //    if (checkexist.Count() > 0) return "User is already Exist";

        //    try
        //    {
        //        var user = new ApplicationUser
        //        { UserName = model.Email, Email = model.Email, PhoneNumber = "", EmailConfirmed = false };

        //        var result = _userManager.Create(user, model.Password);
        //        if (result.Errors.Count() > 0) return result.Errors.FirstOrDefault();

        //        try
        //        {
        //            var roles = roleManager.FindByNameAsync("Teacher");
        //            if (roles == null) roleManager.Create(new IdentityRole { Name = "Teacher" });
        //            _userManager.AddToRolesAsync(user.Id, "Teacher");
        //        }
        //        catch
        //        {
        //            return "An error occured while trying to create the user account.";
        //        }

        //        if (result.Succeeded)
        //            try
        //            {
        //                var definition = new UserDefinition();
        //                definition.FK = Id;
        //                definition.UserId = user.Id;
        //                definition.UserType = "E";
        //                db.UserDefinitions.Add(definition);
        //                db.SaveChanges();
        //                return "success";
        //            }
        //            catch (Exception ex)
        //            {
        //                try
        //                {
        //                    _userManager.Delete(user);
        //                }
        //                catch
        //                {
        //                }

        //                return ex.Message;
        //            }

        //        ModelState.AddModelError("", result.Errors.FirstOrDefault());
        //    }
        //    catch (Exception e)
        //    {
        //        return e.Message;
        //    }

        //    return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
        //        .ErrorMessage;
        //}

        public IActionResult Application()
        {
            return View();
        }

        [HttpPost]
        public string Application(ApplicationLeave leave)
        {
            var userId = _user.UserID;
            var empId = db.UserDefinitions.Where(w => w.UserId == userId && w.UserType == "E").Select(s => s.FK)
                .FirstOrDefault();
            leave.StaffId = empId;
            leave.Status = null;
            leave.UserId = userId;
            leave.Comments = string.Empty;
            db.ApplicationLeaves.Add(leave);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "success";
        }

        public string getattendance(decimal id)
        {
            decimal[] staffid = { id };

            var criteria = "";
            //string shortcriteria = "";
            var data = db.EmployeeAttendances.Where(w => staffid.Contains(w.StaffID)).Select(s =>
                    new { s.ID, s.tbl_Employee.employeeName, s.AttendanceDate, s.EmployeeAttendanceType.AttendanceName })
                .ToList();

            var q =
                @"WITH d(ID, StaffName, typee,date,ForMonth) AS (SELECT tbl_Employee.Id, tbl_Employee.employeename as StaffName, EmployeeAttendanceType.AttendanceName,attendancedate,CONVERT(CHAR(4), attendancedate, 100) + CONVERT(CHAR(4), attendancedate, 120)
FROM tbl_Employee INNER JOIN EmployeeAttendance ON tbl_Employee.Id = EmployeeAttendance.StaffID INNER JOIN
EmployeeAttendanceType ON EmployeeAttendance.AttendanceTypeID = EmployeeAttendanceType.ID Where tbl_Employee.Id in (" +
                string.Join(",", staffid) + ")  " + criteria +
                ") SELECT *,0 as Total FROM d pivot(count(date) for typee in(Present,Absent,Leave,[Late coming],[Short leave]))  as pt";
            var tab = dba.CreateTable(q);

            string[] exCols = { "ID", "StaffName", "Total" };
            for (var i = 0; i < tab.Rows.Count; i++)
            {
                double total = 0;
                for (var j = 0; j < tab.Columns.Count; j++)
                    try
                    {
                        if (!exCols.Contains(tab.Columns[j].ColumnName))
                            total += Convert.ToDouble(tab.Rows[i][j].ToString());
                    }
                    catch
                    {
                    }

                tab.Rows[i]["Total"] = total;
                var dt = DateTime.Parse(tab.Rows[i]["ForMonth"].ToString());
                var StaffID = int.Parse(tab.Rows[i]["ID"].ToString());
                tab.Rows[i]["Short leave"] = db.EmployeeShortLeaves.Where(w => w.StaffID == StaffID).ToList()
                    .Where(w => w.OutTime.Date.Month == dt.Month && w.OutTime.Date.Year == dt.Year).ToList().Count();
            }

            var jsonString = JsonConvert.SerializeObject(tab);
            return jsonString;
        }

        //public IActionResult GetApplication(JqGridRequest request)
        //{
        //    var stafID = _user.UserID;
        //    var application = db.ApplicationLeaves.Where(w => w.UserId == stafID).ToList();
        //    var totalRecords = application.Count();
        //    //Prepare JqGridData instance
        //    var response = new JqGridResponse
        //    {
        //        //Total pages count
        //        TotalPagesCount = (int)Math.Ceiling(totalRecords / (float)request.RecordsCount),

        //        //Page number
        //        PageIndex = request.PageIndex,
        //        //Total records count
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
        //    //.OrderBy(request.SortingName)
        //    data = application.Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.Id.ToString(),
        //        new
        //        {
        //            s.Id,
        //            s.tbl_Employee.employeeName,
        //            s.Comments,
        //            DateTo = s.DateTo.ToString("dd MMM yyyy"),
        //            DateFrom = s.DateFrom.ToString("dd MMM yyyy"),
        //            s.Description,
        //            Status = s.Status == true
        //                ? "<i class=\"fa fa-check-circle\" style=\"color:green\"></i>"
        //                : "<i class=\"fa fa-times-circle\" style=\"color:red\"></i>",
        //            action =
        //                $"<a title=\"Edit\" href=\"{Url.Action("Edit", "Teacher", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("Delete", "Teacher", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a> "
        //        }));
        //    //else
        //    //    data = _context.Employee.OrderByDescending(request.SortingName).Skip(skip).Take(perpage).ToList().Select(s => new JqGridRecord(s.Id.ToString(), new { s.Id, s.EmployeeName, s.Cnic, s.Address, s.MobileNo, s.Email, action = $"<a title=\"Placement\" href=\"{Url.Action("CreatePlacement", "Employees", new { Id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-plus\"></i></a> <a title=\"Edit\" href=\"{Url.Action("Edit", "Employees", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-edit\"></i></a> <a title=\"Delete\" href=\"{Url.Action("Delete", "Employees", new { id = s.Id })}\" class=\"edit ajax ml-1\"><i class=\"fa fa-trash\"></i></a>" }));

        //    foreach (var d in data) response.Records.Add(d);
        //    //response.Records = data; 
        //    response.Reader.RepeatItems = false;
        //    return Json(new{ Data = response });
        //    //return db.Companies.ToList();
        //}
    }
}
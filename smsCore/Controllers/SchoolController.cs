using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;

namespace smsCore.Controllers
{
    [CompressContent]
    [Authorize]
    public class SchoolController : BaseController
    {
 
        private SignInManager<ApplicationUser> _signInManager;
        private UserManager<ApplicationUser> _userManager;
        private readonly SchoolEntities db ;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly CurrentUser CurrentUser;



        public SchoolController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,RoleManager<IdentityRole> _role , SchoolEntities _db, CurrentUser _CurrentUser)
        {
            CurrentUser = _CurrentUser;
            db = _db;
            _userManager = userManager;
            _signInManager = signInManager;
            roleManager = _role;
        }

        // GET: School
        public IActionResult Index()
        {
            //CompanyInfo info = new CompanyInfo();
            //CompanySP sP = new CompanySP();

            //sP.CompanyAdd(info);
            return View();
        }
        //[AllowAnonymous]
        //public async Task<string> AddMenu()
        //{
        //    SeedDataHelper _data = new SeedDataHelper();
        //    await _data.SeedData();
        //    return "";
        //}

       

        
        [HttpGet]
        public async Task<IActionResult> EditSchool()
        {
            //var sP = new CompanySP();
            //var _db = new SchoolEntities();
            var model =await db.Company.FirstOrDefaultAsync();
            //var model =await db.Company.DefaultIfEmpty().Select(s => new CompanyInfo
            //{
            //    Address = s.address,
            //    BooksBeginingFrom = s.booksBeginingFrom.Value,
            //    CompanyId = s.companyId,
            //    CompanyName = s.companyName,
            //    Country = s.country,
            //    Cst = s.cst,
            //    CurrencyId = s.currencyId.HasValue ? s.currencyId.Value : 0,
            //    CurrentDate = s.currentDate.HasValue ? s.currentDate.Value : DateTime.Today,
            //    EmailId = s.emailId,
            //    FinancialYearFrom = s.financialYearFrom.Value,
            //    Logo = s.logo,
            //    MailingName = s.mailingName,
            //    Mobile = s.mobile,
            //    Pan = s.pan,
            //    Phone = s.phone,
            //    Pin = s.phone,
            //    State = s.state,
            //    Tin = s.tin,
            //    Web = s.web
            //}).FirstOrDefaultAsync();
            var country = new List<string> {"Pakistan", "China", "India"};

            ViewBag.countryName = country;

            return View(model);
        }

        
        [HttpPost]
        public async Task< string> EditSchool(tbl_Company model)
        {
            string message;

            var info =await db.Company.FindAsync(model.companyId);

                info.booksBeginingFrom =
                    DateTimeHelper.ConvertDate(Request.Form["booksBeginingFrom"]);
                info.financialYearFrom =
                    DateTimeHelper.ConvertDate(Request.Form["financialYearFrom"]);

            info.address = model.address ?? string.Empty;
            info.companyName = model.companyName ?? string.Empty;
            info.country = model.country ?? string.Empty;
            info.cst = model.cst ?? string.Empty;
            info.currencyId = model.currencyId ?? 7;
            info.currentDate = DateTime.Now;
            info.emailId = model.emailId ?? string.Empty;
            info.extra1 = string.Empty;
            info.extra2 = string.Empty;
            info.mailingName = model.mailingName ?? string.Empty;
            info.mobile = model.mobile ?? string.Empty;
            info.pan = model.pan ?? string.Empty;
            info.phone = model.phone ?? string.Empty;
            info.pin = model.pin ?? string.Empty;
            info.state = model.state ?? string.Empty;
            info.tin = model.tin ?? string.Empty;
            info.web = model.web ?? string.Empty;



            //string filename = StudentPhotoFile.FileName;
            if (Request.Form.Files["CompanyLogoFile"] != null)
            {
                var CompanyLogoFile = Request.Form.Files["CompanyLogoFile"];
                byte[] Logo;
                using (var inputStream = CompanyLogoFile.OpenReadStream())
                {
                    var memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }

                    Logo = memoryStream.ToArray();
                }

                info.logo = Logo;
            }


            try
            {
                await db.SaveChangesAsync();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }

        
        public IActionResult CreateCampus()
        {
            return View();
        }


        [HttpPost]
        public async Task<string> CreateCampus(Campus campus)
        {
            var email = Request.Form["Email"];
            var password = Request.Form["Password"];
            var user = new ApplicationUser
            { UserName = email, Email = email };

            var userValid = await _userManager.UserValidators.FirstOrDefault().ValidateAsync(_userManager, user);
            if (!userValid.Succeeded)
            {
                return userValid.Errors.FirstOrDefault().Description;
            }

            var validator = await _userManager.PasswordValidators.FirstOrDefault().ValidateAsync(_userManager, user, password);
            if (!validator.Succeeded)
            {
                return validator.Errors.FirstOrDefault().Description;
            }
            try
            {
                var exist = db.Campuses.Any(w => w.CampusName.ToLower() == campus.CampusName.ToLower());
                if (!exist)
                {
                    campus.address = campus.address == null ? string.Empty : campus.address;
                    campus.emailId = email;
                    campus.mailingName = campus.mailingName == null ? string.Empty : campus.mailingName;
                    campus.mobile = campus.mobile == null ? string.Empty : campus.mobile;
                    campus.phone = campus.phone == null ? string.Empty : campus.phone;
                    campus.web = campus.web == null ? string.Empty : campus.web;
                    db.Campuses.Add(campus);
                    db.SaveChanges();
                }
                else
                {
                    return "The Campus Name Already Exist";
                }

                var result =await _userManager.CreateAsync(user, password);
                if (result.Errors.Count() > 0)
                {
                    db.Campuses.Remove(campus);
                    db.SaveChanges();
                    return result.Errors.FirstOrDefault().Description;
                }
                result =await _userManager.AddToRoleAsync(user, "Admin");
                if (!result.Succeeded)
                {
                    db.Campuses.Remove(campus);
                    await db.SaveChangesAsync();
                    return "An error occured while trying to create the user account.";
                }

                var definition = new UserDefinition();
                definition.FK = campus.ID;
                definition.UserId = user.Id;
                definition.UserType = "C";
                db.UserDefinitions.Add(definition);
                await db.SaveChangesAsync();
                return "success";
            }
            catch (Exception ex)
            {
                db.Campuses.Remove(campus);
                await db.SaveChangesAsync();
                await _userManager.DeleteAsync(user);
                return ex.Message;
            }

        }

        public ActionResult CampusList(DataManagerRequest request)
        {
            var types = db.Campuses.AsNoTracking().Where(w => w.CampusName != "Default").Select(s => new
                {
                    s.ID, s.CampusName, s.address, s.web, s.mailingName, s.mobile, s.phone, s.emailId
            });

            return Json(new { result = types, count = types.Count() });     
        }

        
        public IActionResult CampusEdit(int id)
        {
            var item = db.Campuses.Where(w => w.ID == id).FirstOrDefault();
            return View(item);
        }

        
        [HttpPost]
        public string CampusEdit(Campus campus)
        {
            if (string.IsNullOrEmpty(campus.CampusName)) return "Invalid campus name.";
            var item = db.Campuses.Find(campus.ID);
            try
            { 
                if (item != null)
                {
                    item.mailingName = campus.mailingName == null ? string.Empty : campus.mailingName;
                    item.CampusName = campus.CampusName;
                    item.phone = campus.phone == null ? string.Empty : campus.phone;
                    item.mobile = campus.mobile == null ? string.Empty : campus.mobile;
                    item.web = campus.web == null ? string.Empty : campus.web;
                    item.address = campus.address == null ? string.Empty : campus.address;
                    db.SaveChanges();
                    return "success";
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "failed";
        }

        
        public IActionResult feepaymentMethod()
        {
            return View();
        }

        
        [HttpPost]
        public string feepaymentMethod(FeeSlipPaymentMethod feeSlipPaymentMethod)
        {
            if (Request.Form["Campus"].ToString() == null) return "Campus is not Selected";
            var campus = int.Parse(Request.Form["Campus"].ToString());
            try
            {
                feeSlipPaymentMethod.CampusID = campus;
                db.FeeSlipPaymentMethods.Add(feeSlipPaymentMethod);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return "success";
        }

        
        public IActionResult FeepaymentmethodEdit(int id)
        {
            var data = db.FeeSlipPaymentMethods.Where(w => w.Id == id).FirstOrDefault();
            return View(data);
        }

        
        [HttpPost]
        public string FeepaymentmethodEdit(FeeSlipPaymentMethod feeSlipPayment)
        {
            var data = db.FeeSlipPaymentMethods.Find(feeSlipPayment.Id);

            if (Request.Form["Campus"].FirstOrDefault() == null) return "Campus is not Selected";
            var campus = int.Parse(Request.Form["Campus"].ToString());
            if (data != null)
            {
                data.PaymentMethodName = feeSlipPayment.PaymentMethodName;
                data.CampusID = campus;
                data.Id = feeSlipPayment.Id;
                data.AllowPartialPayment = feeSlipPayment.AllowPartialPayment;
                db.SaveChanges();
                return "success";
            }

            return "failed";
        }

        
        public IActionResult Deletepaymentmethod(int id = 0)
        {
            var feeSlipPayment = db.FeeSlipPaymentMethods.Find(id);
            if (feeSlipPayment == null) return NotFound();
            return View(feeSlipPayment);
        }

        //
        // POST: /Admin/Students/Delete/5
        
        [HttpPost]
        [ActionName("Deletepaymentmethod")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var feeSlip = db.FeeSlipPaymentMethods.Find(id);
            db.FeeSlipPaymentMethods.Remove(feeSlip);
            db.SaveChanges();
            return RedirectToAction("feepaymentMethod");
        }


        [HttpPost]
        public JsonResult PaymentMethodList(DataManagerRequest request)
        {
            var campusids = CurrentUser.GetCampusIds();
            var feepayment = db.FeeSlipPaymentMethods.AsNoTracking().Where(w =>
                    campusids.Contains(w.CampusID) && w.PaymentMethodName.ToLower() != "Default Payment".ToLower()).Select(s=>new { 
                    s.Id, s.PaymentMethodName, s.AllowPartialPayment
                    });

            return Json(new { result=feepayment, count=feepayment.Count() });
        }

        public IActionResult ApproveApp()
        {
            return View();
        }

        public string AdminApproveApp(int id, string status, string descr)
        {
            var messag = "";
            var app = db.ApplicationLeaves.Find(id);
            if (app != null)
            {
                if (status == "true")
                    app.Status = true;
                else
                    app.Status = false;
                try
                {
                    app.Comments = descr;
                    db.SaveChanges();
                    messag = "success";
                }
                catch (Exception ex)
                {
                    messag = ex.Message;
                }
            }

            return messag;
        }

        public IActionResult UnAproveApplicationList(DataManagerRequest gridRequest)
        {
            var Leaveapprove = db.ApplicationLeaves.OrderByDescending(d => d.DateFrom).Select(s => new
            {
                s.Id,
                s.tbl_Employee.employeeName,
                s.DateFrom,
                s.DateTo,
                s.Status
            });

            return Json(new { result = Leaveapprove, count = Leaveapprove.Count() });
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using sms.Helpers;
using smsCore.Data;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using Utilities;

namespace smsCore.Controllers
{
    [Authorize]
    public class ApplicationConfigurationController : BaseController
    {
        private readonly SchoolEntities db ;
        private readonly ClsBussinessSetting setting;
        private readonly CurrentUser CurrentUser;
        public ApplicationConfigurationController(SchoolEntities _db,ClsBussinessSetting _setting, CurrentUser _CurrentUser)
        {
            db = _db;
            setting = _setting;
            CurrentUser = _CurrentUser;
        }
        // Application Configuration setting region

        #region

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        public IActionResult FeeAndTerrifSetup(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/feeandterrif.cshtml");
        }

        public IActionResult MessagingEmailSetup(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/messagingemail.cshtml");
        }
        public IActionResult ViewExamSetup(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/examsetup.cshtml");
        }
        public IActionResult SchoolLogoSignature(int campusId)
        {
            ViewBag.signaturephoto = db.SchoolLogoes.Where(w => w.ID == 3).Select(p => p.LOGO).FirstOrDefault();
            ViewBag.Backgroundphoto = db.SchoolLogoes.Where(w => w.ID == 4).Select(p => p.LOGO).FirstOrDefault();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/signatures.cshtml");
        }
        public IActionResult SchoolInfo(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/basicinfo.cshtml");
        }
        public IActionResult FounderInfo(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.Founderphoto = db.SchoolLogoes.Where(w => w.ID == 2).Select(p => p.LOGO).FirstOrDefault();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/founder.cshtml");
        }
        public IActionResult Certificates(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/certificate.cshtml");
        }
        public IActionResult SchoolAttendanceSchedule(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/schedules.cshtml");
        }
        public IActionResult Library(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/library.cshtml");
        }
        public IActionResult MessageTemplate(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/template.cshtml");
        }
        public IActionResult AccountSetting(int campusId)
        {
            setting.CampusId = campusId;
            ViewBag.settings = setting.ReadAll();
            ViewBag.CampusId = campusId;
            return View("~/Views/ApplicationConfiguration/partials/accounts.cshtml");
        }
        [HttpPost]
        public string Create(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;

            setting.WriteorUpdate("LateFee", Request.Form["LateFee"]);
            setting.WriteorUpdate("StudentAbsentFine", Request.Form["StudentAbsentFine"]);
            setting.WriteorUpdate("StudentLateComingFine", Request.Form["StudentLateComingFine"]);
            setting.WriteorUpdate("AttendanceCharges", Request.Form["AttendanceCharges"]);
            setting.WriteorUpdate("ResultCharges", Request.Form["ResultCharges"]);
            setting.WriteorUpdate("FeeReceivedCharges", Request.Form["FeeReceivedCharges"]);
            setting.WriteorUpdate("FeeIssuedCharges", Request.Form["FeeIssuedCharges"]);
            setting.WriteorUpdate("StaffAbsentFine", Request.Form["StaffAbsentFine"]);
            setting.WriteorUpdate("StaffLateComingFine", Request.Form["StaffLateComingFine"]);
            setting.WriteorUpdate("bankdetails", Request.Form["bankdetails"]);
            setting.WriteorUpdate("ApplyDailyLateFee", Request.Form["ApplyDailyLateFee"]);
            setting.WriteorUpdate("LateReceivedFeeFine", Request.Form["LateReceivedFeeFine"]);

            return message;
        }

        [HttpPost]
        public string MessagingEmailSetup(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;

            setting.WriteorUpdate("SMSServerUrl", Request.Form["txtSMSServerUrl"]);
            setting.WriteorUpdate("SMSRate", Request.Form["txtSmsRate"]);
            setting.WriteorUpdate("MSISDN", Request.Form["txtMsisdn"]);
            setting.WriteorUpdate("password", Request.Form["txtSmsPassword"]);
            setting.WriteorUpdate("Mask", Request.Form["txtMask"]);
            setting.WriteorUpdate("smtpServer", Request.Form["txtSmtpServer"]);
            setting.WriteorUpdate("UseSSlForMail", Request.Form["cbxUseSSlForMail"].FirstOrDefault() != null ? "true" : "false");
            setting.WriteorUpdate("Port", Request.Form["txtPort"]);
            setting.WriteorUpdate("SchoolEmail", Request.Form["txtSchoolEmail"]);
            setting.WriteorUpdate("EmailPassword", Request.Form["txtEmailPassword"]);
            setting.WriteorUpdate("Remarks", Request.Form["txtRemarks"]);
            setting.WriteorUpdate("StaffWaitingListMessage", Request.Form["txtStaffWaitingListMessage"]);


            return message;
        }

        [HttpPost]
        public string ViewExamSetup(SMSApplicationProperty model)
        {
            string message;
            var input = Request.Form["select"].ToString().Split(',');
            var regno = Request.Form["regno"].ToString().Split(',');
            for (var i = 0; i < regno.Count(); i++)
            {
                var exam = db.ExamHelds.Find(int.Parse(regno[i]));
                if (input[i] == "1")
                    exam.VeiwAble = true;
                else
                    exam.VeiwAble = false;
            }

            try
            {
                db.SaveChanges();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }

        public IActionResult GetViewExamList(DataManagerRequest dm)
        {

                var data = db.ExamHelds.AsNoTracking().ToList().Select(s => new
                {
                    ID = s.ID,
                    ExamName = s.Exam.ExamName,
                    VeiwAble = s.VeiwAble
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
                        List<Sort> sort = new List<Sort>() { new Sort { Name = "ID", Direction = "ascending" } };
                        data = operation.PerformSorting(data, sort);
                    }
                    data = operation.PerformSkip(data, dm.Skip);
                }
                int count = data.Count();
                if (dm.Take != 0)
                {
                    data = operation.PerformTake(data, dm.Take);
                }
                if (dm.RequiresCounts)
                {
                    return Json(new { result = data, count = count });
                }
                else
                {
                    return Json(data);
                }                
        }

        [HttpPost]
        public string SchoolLogoSignature(SMSApplicationProperty model)
        {
            var message = "success";

            var SignatureFile = Request.Form.Files["SignatureFile"];
            var Signature = db.SchoolLogoes.Where(w => w.ID == 3).FirstOrDefault();
            byte[] Signaturephoto;
            if (SignatureFile != null)
            {
                using (var inputStream = SignatureFile.OpenReadStream())
                {
                    var memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }

                    Signaturephoto = memoryStream.ToArray();
                }

                if (Signature != null)
                {
                    Signature.LOGO = Signaturephoto;
                }
                else
                {
                    Signature = new SchoolLogo();
                    Signature.ID = 3;
                    Signature.LOGO = Signaturephoto;
                    db.SchoolLogoes.Add(Signature);
                }
            }

            var BackgroundmageFile = Request.Form.Files["BackgroundmageFile"];
            var Backgroundmage = db.SchoolLogoes.Where(w => w.ID == 4).FirstOrDefault();
            byte[] Backgroundphoto;
            if (BackgroundmageFile != null)
            {
                using (var inputStream = BackgroundmageFile.OpenReadStream())
                {
                    var memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }

                    Backgroundphoto = memoryStream.ToArray();
                }

                if (Backgroundmage != null)
                {
                    Backgroundmage.LOGO = Backgroundphoto;
                }
                else
                {
                    Backgroundmage = new SchoolLogo();
                    Backgroundmage.ID = 4;
                    Backgroundmage.LOGO = Backgroundphoto;
                    db.SchoolLogoes.Add(Backgroundmage);
                }
            }

            try
            {
                db.SaveChanges();

                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }

        [HttpPost]
        public string SchoolInfo(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;

            setting.WriteorUpdate("PrincipalMobile", Request.Form["txtPrincipalMobile"]);
            setting.WriteorUpdate("AffiliatedWith", Request.Form["txtAffiliatedWith"]);
            setting.WriteorUpdate("InstCodeSSC", Request.Form["txtInstCodeSSC"]);
            setting.WriteorUpdate("InstCodeHSSC", Request.Form["txtInstCodeHSSC"]);
            setting.WriteorUpdate("SchoolEstablishedOn", Request.Form["txtSchoolEstablishedOn"]);
            setting.WriteorUpdate("SchoolRegistrationNo", Request.Form["txtSchoolRegistrationNo"]);
            setting.WriteorUpdate("PlayStoreLink", Request.Form["txtPlayStoreLink"]);
            setting.WriteorUpdate("AppleStoreLink", Request.Form["txtAppleStoreLink"]);

            return message;
        }

        [HttpPost]
        public string FounderInfo(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;

            setting.WriteorUpdate("FounderName", Request.Form["txtFounderName"]);
            setting.WriteorUpdate("FounderRemarks", Request.Form["txtFounderRemarks"]);

            var BackgroundmageFile = Request.Form.Files["FounderPictureFile"];
            var FounderPicture = db.SchoolLogoes.Where(w => w.ID == 2).FirstOrDefault();
            byte[] FounderPhoto;
            if (BackgroundmageFile != null)
            {
                using (var inputStream = BackgroundmageFile.OpenReadStream())
                {
                    var memoryStream = inputStream as MemoryStream;
                    if (memoryStream == null)
                    {
                        memoryStream = new MemoryStream();
                        inputStream.CopyTo(memoryStream);
                    }

                    FounderPhoto = memoryStream.ToArray();
                }

                if (FounderPicture != null)
                {
                    FounderPicture.LOGO = FounderPhoto;
                }
                else
                {
                    FounderPicture = new SchoolLogo();
                    FounderPicture.ID = 2;
                    FounderPicture.LOGO = FounderPhoto;
                    db.SchoolLogoes.Add(FounderPicture);
                }

                try
                {
                    db.SaveChanges();
                    message = "success";
                }
                catch (Exception aa)
                {
                    message = aa.Message;
                }
            }
            else
            {
                message = "Data Save Successfully But Image Is Null";
            }
            return message;
        }


        [HttpPost]
        public string Certificates(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;

            setting.WriteorUpdate("BirhtCertificateTitle", Request.Form["txtBirhtCertificateTitle"]);
            setting.WriteorUpdate("BirhtCertificateBody", Request.Form["txtBirhtCertificateBody"]);
            setting.WriteorUpdate("NoObjectionCertificateTitle", Request.Form["txtNoObjectionCertificateTitle"]);
            setting.WriteorUpdate("NoObjectionCertificateBody", Request.Form["txtNoObjectionCertificateBody"]);
            setting.WriteorUpdate("ExprienceCertificateTitle", Request.Form["txtExprienceCertificateTitle"]);
            setting.WriteorUpdate("ExperienceCertificateBody", Request.Form["txtExprienceCertificateBody"]);
            setting.WriteorUpdate("CharacterCertificateTitle", Request.Form["txtCharacterCertificateTitle"]);
            setting.WriteorUpdate("CharacterCertificateBody", Request.Form["txtCharacterCertificateBody"]);
            setting.WriteorUpdate("ParacticalCertificateTitle", Request.Form["txtParacticalCertificateTitle"]);
            setting.WriteorUpdate("ParacticalCertificateBody", Request.Form["txtParacticalCertificateBody"]);
            setting.WriteorUpdate("ConcernCertificateTitle", Request.Form["txtConcernCertificateTitle"]);
            setting.WriteorUpdate("ConcernCertificateBody", Request.Form["txtConcernCertificateBody"]);
            setting.WriteorUpdate("PerformanceCertificateTitle", Request.Form["txtPerformanceCertificateTitle"]);
            setting.WriteorUpdate("PerformanceCertificateBody", Request.Form["txtPerformanceCertificateBody"]);
            setting.WriteorUpdate("RelieveCertificateTitle", Request.Form["txtRelieveTitle"]);
            setting.WriteorUpdate("RelieveCertificateBody", Request.Form["txtRelieveBody"]);

            return message;
        }

        [HttpPost]
        public string SchoolAttendanceSchedule(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;
            setting.WriteorUpdate("OTstudent", Request.Form["dtpschoolOtstudent"]);
            setting.WriteorUpdate("CTstudent", Request.Form["dtpscholCtstudent"]);
            setting.WriteorUpdate("ATstudent", Request.Form["dtpattendancetimestudent"]);
            setting.WriteorUpdate("OTetaff", Request.Form["dtpschoolOtstaff"]);
            setting.WriteorUpdate("CTstafff", Request.Form["dtpschoolStstaff"]);
            setting.WriteorUpdate("ATstaff", Request.Form["dtpattendanceStaff"]);
            setting.WriteorUpdate("FirstPeriod", Request.Form["dtpFirstPeriod"]);
            setting.WriteorUpdate("SendStaffAttendance",
                Request.Form["cbxSendStaffAttendance"].FirstOrDefault() != null ? "true" : "false");
            setting.WriteorUpdate("StaffAttendanceMobileNo", Request.Form["cbxSendStaffAttendance"].FirstOrDefault() != null ? Request.Form["txtStaffAttendanceMobileNo"] : "");
            return message;
        }

        [HttpPost]
        public string Library(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;

            setting.WriteorUpdate("StaffLibraryNOCTitle", Request.Form["txtStaffLibraryNOCtitle"]);
            setting.WriteorUpdate("StaffLibraryNOCBody", Request.Form["txtStaffLibraryNOCBody"]);
            setting.WriteorUpdate("StudentLibraryNOCTitle", Request.Form["txtStudentLibraryNOCTitle"]);
            setting.WriteorUpdate("StudentLibraryNOCBody", Request.Form["txtStudentLibraryNOCBody"]);

            return message;
        }

        [HttpPost]
        public string MessageTemplate(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;

            setting.WriteorUpdate("PresentMessage", Request.Form["txtPresentMessage"]);
            setting.WriteorUpdate("AbsentMessage", Request.Form["txtAbsentMessage"]);
            setting.WriteorUpdate("LeaveMessage", Request.Form["txtLeaveMessage"]);
            setting.WriteorUpdate("SentIssuedFeeTemplate", Request.Form["txtSentIssuedFeeTemplate"]);
            setting.WriteorUpdate("FeeReceivedMessageTemplate", Request.Form["txtFeeReceivedMessageTemplate"]);
            setting.WriteorUpdate("FeeIssuedMessageRemarks", Request.Form["txtFeeIssuedMessageRemarks"]);

            return message;
        }

        [HttpPost]
        public string AccountSetting(SMSApplicationProperty model)
        {
            var message = "success";
            var campusId = int.Parse(Request.Form["CampusID"]);
            setting.CampusId = campusId;

            setting.WriteorUpdate("Payroll", Request.Form["cbxPayroll"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("Budget", Request.Form["cbxBudget"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("Tax", Request.Form["cbxTax"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("MultiCurrency", Request.Form["cbxMultiCurrency"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("BillByBill", Request.Form["cbxBillByBill"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("AllowZeroValueEntry", Request.Form["cbxAllowZeroValueEntry"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowCurrencySymbol", Request.Form["cbxShowCurrencySymbol"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("TickPrintAfterSave", Request.Form["cbxTickPrintAfterSave"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("AutomaticProductCodeGeneration",
                Request.Form["cbxAutomaticProductCodeGeneration"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("Barcode", Request.Form["cbxBarcode"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("AllowBatch", Request.Form["cbxAllowBatch"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("AllowSize", Request.Form["cbxAllowSize"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("AllowModelNo", Request.Form["cbxAllowModelNo"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("AllowGodown", Request.Form["cbxAllowGodown"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("AllowRack", Request.Form["cbxAllowRack"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowSalesRate", Request.Form["cbxShowSalesRate"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowMRP", Request.Form["cbxShowMRP"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowPurchaseRate", Request.Form["cbxShowPurchaseRate"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowUnit", Request.Form["cbxShowUnit"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowSize", Request.Form["cbxShowSize"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowModelNo", Request.Form["cbxShowModelNo"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowDiscountAmount", Request.Form["cbxShowDiscountAmount"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowProductCode", Request.Form["cbxShowProductCode"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowBrand", Request.Form["cbxShowBrand"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("ShowDiscountPercentage",
                Request.Form["cbxShowDiscountPercentage"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("AddConfirmationFor", Request.Form["cbxAddConfirmationFor"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("Add", Request.Form["cbxAdd"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("Edit", Request.Form["checkBox2"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("Delete", Request.Form["checkBox1"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("Close", Request.Form["cbxClose"].FirstOrDefault() != null ? "Yes" : "No");
            setting.WriteorUpdate("DirectPrint", Request.Form["cbxDirectPrint"].FirstOrDefault() != null ? "Yes" : "No");


            setting.WriteorUpdate("Printer", Request.Form["cmbPrinter"]);
            setting.WriteorUpdate("DefualtInvoiceType", Request.Form["cmbDefualtInvoiceType"]);
            setting.WriteorUpdate("NegativeCashTransaction", Request.Form["cmbNegativeCashTransaction"]);
            setting.WriteorUpdate("StockValueCalculationMethod", Request.Form["cmbStockValueCalculationMethod"]);
            setting.WriteorUpdate("NegativeStockStatus", Request.Form["cmbNegativeStockStatus"]);
            return message;
        }

        #endregion

        // Session Details Region

        #region

        [HttpGet]
        public IActionResult NewAcademicSession()
        {
            return View();
        }

        [HttpPost]
        public string NewAcademicSession(tbl_FinancialYear model)
        {
            string message;
            model = new tbl_FinancialYear();
            model.financialYearId = DateTimeHelper.ConvertDate(Request.Form["FromDate"]).Year;
            var toDate = DateTimeHelper.ConvertDate(Request.Form["ToDate"]);
            if(toDate==DateTime.MinValue)
            {
                return "Please select a valid start date";
            }
            var fromDate = DateTimeHelper.ConvertDate(Request.Form["FromDate"]);
            if (fromDate == DateTime.MinValue)
            {
                return "Please select a valid end date";
            }
            if (toDate <= fromDate) return "From Date should be less than To Date.";
            var cbxIsCurrent = !string.IsNullOrEmpty(Request.Form["CurrentSession"]);
            var found = db.FinancialYears.Where(w => w.fromDate == fromDate && w.toDate == toDate).Any();
            if (found)
                return "Session already exist.";
            model.fromDate = fromDate;
            model.toDate =toDate;
            model.EntryDate = DateTime.Now;
            model.ModifiedOn = DateTime.Now;
            model.UserId = CurrentUser.UserID;
            model.ModifiedBy = CurrentUser.UserID;
            model.IsCurrent = cbxIsCurrent;
            try
            {
                db.FinancialYears.Add(model);
                db.SaveChanges();
                if(model.IsCurrent)
                {
var existings=                    db.FinancialYears.Where(w => w.Id != model.Id);
                    foreach(var e in existings)
                    {
                        e.IsCurrent = false;
                    }
                    db.SaveChanges();
                }
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }


        [HttpPost]
        public JsonResult SessionListFor(DataManagerRequest request)
        {
            
            var data = db.FinancialYears.Select(x =>
                new
                {
                    x.Id,
                    FinancialYearId = x.financialYearId,
                    FromDate = x.fromDate,
                    ToDate = x.toDate,
                    IsCurrent = x.IsCurrent
                });


            return Json(new { result = data, count = data.Count() });
        }

        public string CurrentSessionCheck(int Id)
        {
            string message;
            var model = db.FinancialYears.Find(Id);
            var existings = db.FinancialYears.Where(w => w.Id != model.Id);
            foreach (var e in existings)
            {
                e.IsCurrent = false;
            }
            model.IsCurrent = true;
            db.SaveChanges();
            try
            {
                db.SaveChanges();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }


        #endregion
    }
}
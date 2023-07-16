using Models;
using sms.Helpers;
using sms.Models;
using sms.Hubs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using Utilities;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.SignalR;
using smsCore.Data.Helpers;
using smsCore.Data.Models;

namespace smsCore.BackgroundTasks
{
    public class AddFeeBackgroundTask
    {
        private readonly SchoolEntities db;
        private readonly IHubContext<SmsHub> hubContext;
        private readonly LedgerPostingSP spLedgerPosting;
        private readonly DatabaseAccessSql sql;
        private readonly ClsBussinessSetting setting;
        private readonly StaticResources _resource;
        private readonly MessagingSystem ms;
        public AddFeeBackgroundTask(LedgerPostingSP _ledgerPosting,IHubContext<SmsHub> _hub, SchoolEntities _db, DatabaseAccessSql _sql, StaticResources resource,ClsBussinessSetting _setting, MessagingSystem _ms)
        {
            spLedgerPosting = _ledgerPosting;
            hubContext = _hub;
            db = _db;
            sql = _sql;
            _resource = resource;
            setting = _setting;
            ms = _ms;
        }

        public  void AddFeeSlip(bool OverWrite, int[] campusids, int[] classids, int month, int year, string userId, DateTime _DueDate, DateTime _LastFineDate, bool AddTransport, bool AddSmsCharges, List<OptionalFee> optionalFee, bool sendMessage)
        {
            try
            {
                SendNotification(hubContext, new { title = "Fee Slip Generation Task", type = "info", message = "Fee slip generation task started", percent = 1 });
                var message = string.Empty;

                
                var AdmissionIDs = (from ss in db.Admissions
                                    where classids.Contains(ss.ClassSection.ClassID) && campusids.Contains(ss.CampuseID) &&
                                          ss.Student.FreeAdmissions.Count == 0 && ss.IsExpell == false
                                    orderby ss.Student.RegistrationNo
                                    select ss.ID).ToArray();

                var total = AdmissionIDs.Count();
                if (AdmissionIDs.Count() < 1)
                {
                    SendNotification(hubContext, new { title = "Fee Slip Generation Task", type = "error", message = "Task Completed with message: No Student found.", percent = 100 });
                    return;
                    //return message;
                }

                var firstdate = new DateTime(year, month, 1);
                var lastdate = firstdate.AddMonths(1)
                    .AddDays(-1);

                var startdate = new SqlParameter("startDate", SqlDbType.Date);
                startdate.Value = firstdate;

                var enddate = new SqlParameter("endDate", SqlDbType.Date);
                enddate.Value = lastdate;
                var admissionIds = new SqlParameter("admissionIds", string.Join(",", AdmissionIDs));
                var collection = new List<SqlParameter>();
                collection.Add(startdate);
                collection.Add(enddate);
                collection.Add(admissionIds);
                var data = sql.ExecuteSP("GetFeeEntryStrucure", collection);

                SendNotification(hubContext, new { type = "info", message = "", percent = 20 });
                if (data.Rows.Count == 0)
                {
                    message = "No Fee Structure found.";
                    SendNotification(hubContext, new { title = "Fee Slip Generation Task", type = "error", message = "Task Completed with message: No Fee Structure found.", percent = 100 });
                    return;
                }

                var Quries = new List<string>();

                var ids = data.AsEnumerable().Select(s => s["AdmissionID"]).Distinct().ToArray();

                var feeSlips = new List<FeeSlip>();
                int total_records = ids.Count();

                if (_DueDate.Date==DateTime.MinValue)
                {
                    _DueDate = firstdate.AddDays(10);
                }

                if (_LastFineDate.Date==DateTime.MinValue)
                {
                    _LastFineDate = _DueDate.AddDays(10);
                }

                //Parallel.For(0, ids.Count(), index =>
                for (var index = 0; index < ids.Count(); index++)
                {
                    var id = int.Parse(ids[index].ToString().Trim());
                    var rows = data.AsEnumerable().Where(w => w["AdmissionID"].ToString().Trim() == id.ToString().Trim());

                    var fineId = (int)PublicVariables.FeeTypes.AttendanceFine;
                    var FeeSlip = db.FeeSlips
                        .Where(w => w.AdmissionId == id && w.ForMonth.Month == month && w.ForMonth.Year == year)
                        .FirstOrDefault();
                    var editMode = false;
                    editMode = FeeSlip != null && OverWrite;
                    if (FeeSlip == null)
                    {
                        FeeSlip = new FeeSlip() { AdmissionId = int.Parse(id.ToString()), EntryDate = DateTime.Now, UserId = userId };
                        db.FeeSlips.Add(FeeSlip);
                        feeSlips.Add(FeeSlip);
                    }
                    else if (FeeSlip != null && !OverWrite)
                    {
                        continue;
                    }

                    FeeSlip.DueDate = _DueDate.Date;
                    FeeSlip.ForMonth = firstdate;
                    FeeSlip.LastFineDate = _LastFineDate.Date;
                    var AttendanceFine = decimal.Parse(rows.FirstOrDefault()["AttendanceFine"].ToString());
                    var FinesSlip = FeeSlip.FeeSlipDetails.Where(w => w.FeeTypeId == fineId).FirstOrDefault();
                    var addFine = false;
                    if (AttendanceFine > 0)
                    {

                        if (FinesSlip == null)
                        {
                            FinesSlip = new FeeSlipDetail();
                            if (editMode)
                            {
                                FinesSlip.FeeSlipId=FeeSlip.Id;
                                db.FeeSlipDetails.Add(FinesSlip);
                            }
                            else FeeSlip.FeeSlipDetails.Add(FinesSlip);

                            addFine = true;
                        }
                        else if (OverWrite)
                        {
                            addFine = true;
                        }

                        if (addFine)
                        {

                            FinesSlip.Amount = AttendanceFine;
                            FinesSlip.Discount = 0;// decimal.Parse(rows.FirstOrDefault()["Discount"].ToString());
                            FinesSlip.DiscountInAmount = false;
                            //bool.Parse(rows.FirstOrDefault()["DiscountInAmount"].ToString());
                            FinesSlip.FeeTypeId = (int)PublicVariables.FeeTypes.AttendanceFine;
                        }

                    }


                    if (AddTransport)
                    {
                        var transportAmount = decimal.Parse(rows.FirstOrDefault()["TransportFare"].ToString());
                        var trasnId = (int)PublicVariables.FeeTypes.Transport;
                        var TransportSlip = FeeSlip.FeeSlipDetails.Where(w => w.FeeTypeId == trasnId).FirstOrDefault();
                        if (transportAmount > 0)
                        {
                            addFine = false;
                            if (TransportSlip == null)
                            {
                                TransportSlip = new FeeSlipDetail();
                                if (editMode)
                                {
                                    TransportSlip.FeeSlipId=FeeSlip.Id;
                                    db.FeeSlipDetails.Add(TransportSlip);
                                }
                                else FeeSlip.FeeSlipDetails.Add(TransportSlip);
                                addFine = true;
                            }
                            else if (OverWrite)
                            {
                                addFine = true;
                            }

                            if (addFine)
                            {
                                TransportSlip.Amount = transportAmount;

                                TransportSlip.Discount = decimal.Parse(rows.FirstOrDefault()["Discount"].ToString());
                                TransportSlip.DiscountInAmount =
                                    bool.Parse(rows.FirstOrDefault()["DiscountInAmount"].ToString());
                                TransportSlip.FeeTypeId = (int)PublicVariables.FeeTypes.Transport;
                            }
                        }
                    }


                    if (AddSmsCharges)
                    {
                        var amount = decimal.Parse(rows.FirstOrDefault()["SMSRate"].ToString());
                        var smsid = (int)PublicVariables.FeeTypes.SMSCharges;

                        var SMSCharges = FeeSlip.FeeSlipDetails.Where(w => w.FeeTypeId == smsid).FirstOrDefault();
                        if (amount > 0 || SMSCharges != null)
                        {
                            //decimal smsCharges = decimal.Parse(rows.FirstOrDefault()["SMSRate"].ToString());


                            addFine = false;
                            if (SMSCharges == null)
                            {
                                SMSCharges = new FeeSlipDetail();
                                if (editMode)
                                {
                                    SMSCharges.FeeSlipId=FeeSlip.Id;
                                    db.FeeSlipDetails.Add(SMSCharges);
                                }
                                else FeeSlip.FeeSlipDetails.Add(SMSCharges);
                                addFine = true;
                            }
                            else if (OverWrite)
                            {
                                addFine = true;
                            }

                            if (addFine)
                            {
                                SMSCharges.Amount = amount;
                                SMSCharges.Discount = decimal.Parse(rows.FirstOrDefault()["Discount"].ToString());
                                SMSCharges.DiscountInAmount =
                                    bool.Parse(rows.FirstOrDefault()["DiscountInAmount"].ToString());
                                SMSCharges.FeeTypeId = (int)PublicVariables.FeeTypes.SMSCharges;
                            }
                        }
                    }


                    foreach (var m in optionalFee)
                    {
                        if (!m.Select || !(m.Amount > 0))
                            continue;
                        var optionalexistings = FeeSlip.FeeSlipDetails.ToList()
                            .Where(w => w.FeeTypeId == m.Id).FirstOrDefault();
                        addFine = false;
                        if (optionalexistings == null)
                        {
                            optionalexistings = new FeeSlipDetail();
                            if (editMode)
                            {
                                optionalexistings.FeeSlipId=FeeSlip.Id;
                                db.FeeSlipDetails.Add(optionalexistings);
                            }
                            else FeeSlip.FeeSlipDetails.Add(optionalexistings);
                            addFine = true;
                        }
                        else if (OverWrite)
                        {
                            addFine = true;
                        }

                        if (addFine)
                        {
                            optionalexistings.Amount = m.Amount;
                            optionalexistings.Discount = 0;
                            optionalexistings.DiscountInAmount = false;
                            optionalexistings.FeeTypeId = m.Id;
                        }
                    }

                    foreach (var row in rows)
                    {
                        var amount = decimal.Parse(row["ActualAmount"].ToString());
                        var feeTypeId = int.Parse(row["FeeTypeId"].ToString());
                        var continues = false;
                        for (var it = 0; it < optionalFee.Count(); it++)
                            if (optionalFee[it].Select && (feeTypeId == optionalFee[it].Id ||
                                feeTypeId == (int)PublicVariables.FeeTypes.Transport ||
                                feeTypeId == (int)PublicVariables.FeeTypes.SMSCharges ||
                                feeTypeId == (int)PublicVariables.FeeTypes.AttendanceFine))
                            {
                                continues = true;
                                break;
                            }

                        if (continues) continue;
                        if (amount > 0)
                        {
                            addFine = false;
                            var existings = FeeSlip.FeeSlipDetails.Where(w => w.FeeTypeId == feeTypeId).FirstOrDefault();
                            if (existings == null)
                            {
                                existings = new FeeSlipDetail();
                                if (editMode)
                                {
                                    existings.FeeSlipId=FeeSlip.Id;
                                    db.FeeSlipDetails.Add(existings);
                                }
                                else FeeSlip.FeeSlipDetails.Add(existings);

                                addFine = true;
                            }
                            else if (OverWrite)
                            {
                                addFine = true;
                            }

                            if (addFine)
                            {
                                existings.Amount = amount;
                                existings.Discount = decimal.Parse(row["Discount"].ToString());
                                existings.DiscountInAmount = bool.Parse(row["DiscountInAmount"].ToString());
                                existings.FeeTypeId = feeTypeId;
                            }
                        }
                    }

                    int completed = (index / total_records) * 100;
                    if (completed > 100)
                        completed = 100;

                    SendNotification(hubContext, new { type = "info", message = "", percent = completed });
                    //if (editMode)
                    //    spLedgerPosting.FeeLedgerPostingF(FeeSlip.FeeSlipDetails.Sum(s => s.Amount), FeeSlip.Id,
                    //        FeeSlip.EntryDate, VoucherTypeId);


                }
                //);


                if (OverWrite)
                    db.ChangeTracker.DetectChanges();
                //else if (feeSlips.Count > 0)
                //    db.FeeSlips.AddRange(feeSlips);

                db.SaveChanges();
                if (sendMessage && feeSlips.Count>0)
                    BackgroundJob.Enqueue(() =>
                    SendIssuedMessageTask(feeSlips.Select(s => s.AdmissionId).ToArray(), campusids.FirstOrDefault(), false, firstdate)
                    );

                SendNotification(hubContext, new { title = "Fee slip generation Task", type = "info", message = "Task Completed Successfully.", percent = 100, completed = true });
                int VoucherTypeId = (int)PublicVariables.EnumVoucherTypeDefaults.Fee_Issued_Voucher;

                foreach (var feeslip in feeSlips)
                    spLedgerPosting.FeeLedgerPostingF(feeslip.FeeSlipDetails.Sum(s => s.Amount), feeslip.Id,
                        feeslip.EntryDate, VoucherTypeId);
                message = "success";
            }
            catch (Exception aa)
            {
                SendNotification(hubContext, new { type = "error", message = aa.Message, percent = 100, completed = true });
            }
        }

        private async void SendNotification(IHubContext<SmsHub> context, object data)
        {
          await  context.Clients.Group(HubSubscriptions.FeeEntry.ToString()).SendCoreAsync("notification", new object[] { data });//.notification(data);
        }

        public void SendIssuedMessageTask(int[] admisionIds, int campusId, bool remark, DateTime forMonth)
        {
            var data = db.FeeSlips.Where(w =>
                  admisionIds.Contains(w.AdmissionId) && w.ForMonth.Month == forMonth.Month && w.ForMonth.Year == forMonth.Year)
                   .Select(s => new
                   {
                       s.Admission.Student.StudentName,
                       RegistrationNo = s.Admission.Student.RegistrationNo,
                       CampusId = s.Admission.CampuseID,
                       SendRemarks = false,
                       s.DueDate,
                       s.Admission.StudentID,
                       Amount = s.FeeSlipDetails.Sum(d => d.Amount),
                       s.ForMonth
                   });

            try
            {
                SendNotification(hubContext, _resource.QeuedReturn("Send Issued Fee Message Task", StaticResources.ToastType.info, "Task Started.", 0, false));
                setting.CampusId = (campusId);

                string messageTemplate = (string)setting.ReadWithType("FeeSlipIssuedMessage", typeof(string));
                if (string.IsNullOrEmpty(messageTemplate))
                {
                    messageTemplate = "Dear Parents,\nFee Bill of {0} ({1}) for rupees {3}/- has been issued. Last date for payment of this bill without fine is {4}.";
                    setting.WriteorUpdate("FeeSlipIssuedMessage", messageTemplate);
                }
                if (remark)
                {
                    string remarks = (string)setting.ReadWithType("FeeSlipIssuedMessageRemarks", typeof(string));
                    if (!string.IsNullOrEmpty(remarks))
                    {
                        messageTemplate += " " + remarks;
                    }
                }
                foreach (var v in data)
                {
                    var substd =
                        sql.CreateTable("SELECT IssuedFee, StudentID FROM dbo.ComSubs WHERE (IssuedFee = 1) AND (StudentID = " + v.StudentID + ")");
                    if (substd.Rows.Count > 0)
                    {
                        var message = "";
                        message = string.Format(
                            messageTemplate,
                            v.StudentName, v.RegistrationNo,
                            v.ForMonth.ToString("MMMM, yyyy"), v.Amount.ToString(),
                            v.DueDate.ToString("dd/MM/yyyy"));
                        // message = HttpUtility.UrlEncode(message.Trim());
                        ms.sendMessage(v.RegistrationNo, message, true).Wait();
                    }
                }
                SendNotification(hubContext, _resource.QeuedReturn("Send Issued Fee Message Task", StaticResources.ToastType.info, "Task Completed.", 100, true));

                //Parallel.ForEach(data, (v) =>
                //{
                //    var dba = new DatabaseAccessSql(true);
                //    var substd =
                //        dba.CreateTable("SELECT IssuedFee, StudentID FROM dbo.ComSubs WHERE (IssuedFee = 1) AND (StudentID = " + v.StudentID + ")");
                //    if (substd.Rows.Count > 0)
                //    {
                //        var message = "";
                //        message = string.Format(
                //            messageTemplate,
                //            v.StudentName,v.RegistrationNo,
                //            v.ForMonth.ToString("MMMM, yyyy"), v.Amount.ToString(),
                //            v.DueDate.ToString("dd/MM/yyyy"));
                //        // message = HttpUtility.UrlEncode(message.Trim());
                //        ms.sendMessage(v.RegistrationNo, message, true).Wait();
                //        SendNotification(hubContext, StaticResources.QeuedReturn("Send Issued Fee Message Task", StaticResources.ToastType.info, "Task Completed.", 100, true));
                //    }

                //});

                //for (var i = 0; i < data.Count(); i++)
                //{
                //    var regno = data[i].RegistrationNo;
                //    var feedetails = db.FeeSlips.Where(w =>w.Admission.Student.RegistrationNo == regno && w.ForMonth.Month == forMonth.Month && w.ForMonth.Year == forMonth.Year).FirstOrDefault();
                //    if (feedetails==null)
                //        continue;

                //    var stdid = feedetails.Admission.StudentID;
                //    var dba = new DatabaseAccessSql(true);
                //    var substd =
                //        dba.CreateTable("SELECT IssuedFee, StudentID FROM dbo.ComSubs WHERE (IssuedFee = 1) AND (StudentID = " + stdid + ")");
                //    if (substd.Rows.Count > 0)
                //    {
                //        var message = "";
                //        message = string.Format(
                //            messageTemplate,
                //            feedetails.Admission.Student.StudentName, regno,
                //            feedetails.ForMonth.ToString("MMMM, yyyy"), feedetails.FeeSlipDetails.Sum(s => s.Amount).ToString(),
                //            feedetails.DueDate.ToString("dd/MM/yyyy"));



                //        // message = HttpUtility.UrlEncode(message.Trim());
                //        ms.sendMessage(regno, message, true).Wait();
                //        SendNotification(hubContext, StaticResources.QeuedReturn("Send Issued Fee Message Task", StaticResources.ToastType.info, "Task Completed.", 100, true));
                //    }
                //}
            }
            catch
            {
                SendNotification(hubContext, _resource.QeuedReturn("Send Public Message Task", StaticResources.ToastType.error, "Task Completed with error.", 100, true));
            }
        }

    }
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Models;

namespace smsCore.Data.Helpers
{
    public class FeeSystemHelper
    {

        private readonly SchoolEntities db ;
        private readonly ClsBussinessSetting setting;
        private readonly DatabaseAccessSql sql;
        private readonly CurrentUser _user;

        public FeeSystemHelper(SchoolEntities _db, ClsBussinessSetting _setting, DatabaseAccessSql sql, CurrentUser user)
        {
            db = _db;
            setting = _setting;
            this.sql = sql;
            _user = user;
        }
        public List<dynamic> GetFeeOfStudent(int sessionId, int[] admids, int[] campusIds)
        {
            var data2 = db.FeeSlips
                .Where(w => campusIds.Contains(w.Admission.CampuseID) && admids.Contains(w.AdmissionId) &&
                            w.Admission.Session == sessionId).ToList().Select(
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
                        Balance = s.FeeSlipDetails.Select(ss => ss.Amount).Sum() -
                                  s.FeeSlipReceipts.Select(ss => ss.Amount).Sum(),
                        ForMonthYear = s.ForMonth.ToString("MMMM, yyyy"),
                        ReceivedDate = s.FeeSlipReceipts.Count > 0
                            ? s.FeeSlipReceipts.Select(ss => ss.EntryDate).FirstOrDefault().ToString("dd-MMM-yyyy")
                            : ""
                    }
                ).ToList<dynamic>();

            return data2;
        }

        public decimal GetLateReceivedFeeFine(DateTime ReceivedDate, DateTime DueDate, DateTime LastFineDate,
            int CampusId)
        {
            decimal latefee = 0;

            setting.CampusId=CampusId;
            var _ApplyDailyLateFee = setting.Read("ApplyDailyLateFee");
            if (_ApplyDailyLateFee == null) setting.WriteorUpdate("ApplyDailyLateFee", "False");
            var ApplyDailyLateFee = (bool) setting.ReadWithType("ApplyDailyLateFee", typeof(bool));
            var _LateReceivedFeeFine = setting.Read("LateReceivedFeeFine");
            if (_LateReceivedFeeFine == null) setting.WriteorUpdate("LateReceivedFeeFine", "0");
            var LateReceivedFeeFine = (decimal) setting.ReadWithType("LateReceivedFeeFine", typeof(decimal));
            if (ReceivedDate > DueDate)
            {
                if (ApplyDailyLateFee)
                {
                    if (ReceivedDate > LastFineDate)
                    {
                        var days = (LastFineDate - DueDate).Days;
                        latefee = LateReceivedFeeFine * days;
                        return latefee;
                    }
                    else
                    {
                        var days = (ReceivedDate - DueDate).Days;
                        latefee = LateReceivedFeeFine * days;
                        return latefee;
                    }
                }

                return LateReceivedFeeFine;
            }

            return 0;
        }
        public decimal GetLateReceivedFeeFine(DateTime ReceivedDate, DateTime DueDate, DateTime LastFineDate,
            bool ApplyDailyLateFee,decimal LateReceivedFeeFine)
        {
            decimal latefee;

            
            if (ReceivedDate > DueDate)
            {
                if (ApplyDailyLateFee)
                {
                    if (ReceivedDate > LastFineDate)
                    {
                        var days = (LastFineDate - DueDate).Days;
                        latefee = LateReceivedFeeFine * days;
                        return latefee;
                    }
                    else
                    {
                        var days = (ReceivedDate - DueDate).Days;
                        latefee = LateReceivedFeeFine * days;
                        return latefee;
                    }
                }

                return LateReceivedFeeFine;
            }

            return 0;
        }

        public decimal GetLateReceivedFeeFine(int FeeSlipId, DateTime ReceivedDate)
        {
            var FeeSlip = db.FeeSlips.Find(FeeSlipId);
            if (FeeSlip == null)
                return 0;
            if (FeeSlip.Admission.Student.ApplyLateFee != null && FeeSlip.Admission.Student.ApplyLateFee == 1) return 0;
            var applied = FeeSlip.FeeSlipDetails.Where(w => w.FeeType.TypeName.Trim() == "Late Fee").FirstOrDefault();
            if (applied != null) return 0;
            if (FeeSlip.FeeSlipReceipts.Count > 0)
                return 0;

            return GetLateReceivedFeeFine(ReceivedDate, FeeSlip.DueDate, FeeSlip.LastFineDate,
                FeeSlip.Admission.CampuseID);
        }

        public decimal GetLateReceivedFeeFineForArrears(int FeeSlipId)
        {
            var FeeSlip = db.FeeSlips.Find(FeeSlipId);

            if (FeeSlip == null)
                return 0;
            if (FeeSlip.Admission.Student.ApplyLateFee != null && FeeSlip.Admission.Student.ApplyLateFee == 1) return 0;
            var applied = FeeSlip.FeeSlipDetails.Where(w => w.FeeType.TypeName.Trim() == "Late Fee").FirstOrDefault();
            if (applied != null) return 0;
            if (FeeSlip.FeeSlipReceipts.Count > 0)
                return 0;

            var lateFee = GetLateReceivedFeeFineOfArrears(DateTime.Today, FeeSlip.DueDate, FeeSlip.LastFineDate,
                FeeSlip.Admission.CampuseID);
            if (lateFee > 0)
            {
                var fineSlip = new FeeSlipDetail
                    {Amount = lateFee, Discount = 0, DiscountInAmount = false, FeeSlipId = FeeSlipId, FeeTypeId = 8};
                db.FeeSlipDetails.Add(fineSlip);
                db.SaveChanges();
            }

            return lateFee;
        }

        public decimal GetLateReceivedFeeFineOfArrears(DateTime ReceivedDate, DateTime DueDate,
            DateTime LastFineDate, int CampusId)
        {
            decimal latefee = 0;

            setting.CampusId = CampusId;
            var _ApplyDailyLateFee = setting.Read("ApplyDailyLateFee");
            if (_ApplyDailyLateFee == null) setting.WriteorUpdate("ApplyDailyLateFee", "False");
            var ApplyDailyLateFee = (bool) setting.ReadWithType("ApplyDailyLateFee", typeof(bool));


            var _LateReceivedFeeFine = setting.Read("LateReceivedFeeFine");
            if (_LateReceivedFeeFine == null) setting.WriteorUpdate("LateReceivedFeeFine", "0");
            var LateReceivedFeeFine = (decimal) setting.ReadWithType("LateReceivedFeeFine", typeof(decimal));
            if (ReceivedDate > DueDate)
            {
                if (ApplyDailyLateFee)
                {
                    if (ReceivedDate > LastFineDate)
                    {
                        var days = (LastFineDate - DueDate).Days;
                        latefee = LateReceivedFeeFine * days;
                        return latefee;
                    }
                    else
                    {
                        var days = (ReceivedDate - DueDate).Days;
                        latefee = LateReceivedFeeFine * days;
                        return latefee;
                    }
                }

                return LateReceivedFeeFine;
            }

            return 0;
        }

        public IEnumerable<ReceiveFeeFromStudentGridSource> GetReceiveableFeeOfStudent(int regno, DateTime ReceivedDate,
            int campusId)
        {
            setting.CampusId = campusId;
            //var _ApplyDailyLateFee = setting.Read("ApplyDailyLateFee");
            //if (_ApplyDailyLateFee == null) setting.WriteorUpdate("ApplyDailyLateFee", "False");
            var ApplyDailyLateFee = (bool)setting.ReadWithType("ApplyDailyLateFee", typeof(bool));
            //var _LateReceivedFeeFine = setting.Read("LateReceivedFeeFine");
            //if (_LateReceivedFeeFine == null) setting.WriteorUpdate("LateReceivedFeeFine", "0");
            var LateReceivedFeeFine = (decimal)setting.ReadWithType("LateReceivedFeeFine", typeof(decimal));

            IEnumerable<ReceiveFeeFromStudentGridSource> list = new List<ReceiveFeeFromStudentGridSource>();

            var campusIds = _user.GetCampusIds();
            
            if (campusId != -1) campusIds = new[] {campusId};
            list = db.FeeSlips
                .Where(w => campusIds.Contains(w.Admission.CampuseID) && w.Admission.Student.RegistrationNo == regno &&
                            (w.FeeSlipReceipts.Count == 0 || w.FeeSlipDetails.Sum(s => s.Amount) >
                                w.FeeSlipReceipts.Sum(s => s.Amount))).AsNoTracking().ToList().Select(s =>
                    new ReceiveFeeFromStudentGridSource
                    {
                        pk = s.Id,
                        Month = s.ForMonth.ToString("MMMM, yyyy"),
                        ReceiveableAmount = s.FeeSlipDetails.Sum(su => su.Amount) -
                                            s.FeeSlipReceipts.Sum(su => su.Amount),
                        Received = s.FeeSlipReceipts.Sum(su => su.Amount),
                        LateFee = GetLateReceivedFeeFine(ReceivedDate, s.DueDate, s.LastFineDate, ApplyDailyLateFee, LateReceivedFeeFine),
                        RegNo =regno
                        
                    });
            return list;
        }

        public IEnumerable<object> GetNetFeeOfStudents(int[] AdmissionIDs)
        {
            int year = DateTime.Now.Year;
            int month = DateTime.Now.Month;
            var firstdate = new DateTime(year, month, 1);
            var lastdate = firstdate.AddMonths(1)
                .AddDays(-1);

            var admissionIds = new SqlParameter("admissionIds", string.Join(",", AdmissionIDs));
            var collection = new List<SqlParameter>
            {
                admissionIds
            };
            var data = sql.ExecuteSP("GetAmountFeeEntryStrucure", collection).AsEnumerable().Select(s => new
            {
                RegistrationNo = s.Field<int>("RegistrationNo"),
                Name = s.Field<string>("StudentName"),
                FName = s.Field<string>("FName"),
                ClassName=s.Field<string>("ClassName"),
                Amount = s.Field<decimal>("Amount"),
                ODiscount = s.Field<decimal>("ODiscount"),
                DiscountInAmount = s.Field<bool>("DiscountInAmount"),
                Discount = s.Field<decimal>("Discount"),
                ActualAmount = s.Field<decimal>("ActualAmount"),
                TransportFee = s.Field<decimal>("TransportFare"),
            }).GroupBy(g => g.RegistrationNo).Select(s => new
            {
                RegistrationNo = s.Key,
                Name = s.FirstOrDefault().Name,
                Fname = s.FirstOrDefault().FName,
                ClassName=s.FirstOrDefault().ClassName,
                Discount = s.Sum(t => t.Discount),
                Total = s.Sum(t => t.Amount),
                ActualAmount = s.Sum(t => t.ActualAmount),
                Transport = s.Max(m => m.TransportFee),
                GrantTotal = s.Sum(t => t.ActualAmount) + s.Max(m => m.TransportFee)
            });

            return data;
        }
    }
}
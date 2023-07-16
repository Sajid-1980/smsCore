//This is a source code or part of Models project
//Copyright (C) 2018  Models
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Models
{
    public static class PublicVariables
    {
        //public static ErrorMessageInfo infoError = new ErrorMessageInfo();
        //public PublicVariables() {
        //   // infoError.PropertyChanged += InfoError_PropertyChanged;
        //}

        //private void InfoError_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{

        //}
        public static LoginType loginType = LoginType.User;
        public static int CampusId = -1;
        public static readonly string ActivationFileName = "default.sico";
        public static ExitCode CurrentApplicationCode = ExitCode.InvalidCode;

        //public static CompanyInfo CompanyInformation = new CompanyInfo();
        public static string _decCurrentUserId = "";
        //public static string _strCurrentUserId = "";

        public static decimal _decCurrentCompanyId = 0;
        public static DateTime _dtCurrentDate { get { return DateTime.Now; } set {  } }
        public static DateTime _dtFromDate;//financial year starting    
        public static DateTime _dtToDate;//financial year ending         
        public static int _decCurrentFinancialYearId = 0;
        public static bool isMessageAdd = true;
        public static bool isMessageEdit = true;
        public static bool isMessageDelete = true;
        public static bool isMessageClose = true;
        public static bool demoProject = true;
        public static decimal _decCurrencyId = 1;
        public static int _inNoOfDecimalPlaces = 2;
        public static string MessageToShow = string.Empty;
        public static string MessageHeadear = string.Empty;
        
        public static string appKey = string.Empty;

        public enum LoginType
        {
            User = 0, Campus = 1
        }

        public enum ExitCode
        {
            Activated = 5, InvalidCode = 3, InvalidSecurityFile = 4, TrialCode = 1
        }

        public enum PrivilidgeActionTypes
        {
            Any = 0, View = 1, Save = 2, Edit = 3, Delete = 4, none = -1
        }

        public enum AttendanceType
        {
            Present=1, Absent=2, Leave=3,LateComing=4
        }


        public enum FeeTypes
        {
            Transport = 4,SMSCharges = 6,AttendanceFine = 7
        }

        public enum EnumAccountGroup
        {
            Primary = 0,
            Capital_Account = 1,
            Loans_Liability = 2,
            Current_Liabilities = 3,
            Fixed_Assets = 4,
            Investments = 5,
            Current_Assets = 6,
            Branch_Divisions = 7,
            MiscExpenses_ASSET = 8,
            Suspense_AC = 9,
            Sales_Account = 10,
            Purchase_Account = 11,
            Direct_Income = 12,
            Direct_Expenses = 13,
            Indirect_Income = 14,
            Indirect_Expenses = 15,
            Reservers_Surplus = 16,
            Bank_OD_AC = 17,
            Secured_Loans = 18,
            UnSecured_Loans = 19,
            Duties_Taxes = 20,
            Provisions = 21,
            Sundry_Creditors = 22,
            Stock_in_Hand = 23,
            DepositsAssets = 24,
            Loans__AdvancesAsset = 25,
            Sundry_Debtors = 26,
            Cash_in_Hand = 27,
            Bank_Account = 28,
            Service_Account = 29
        }
        public enum EnumLedgerDefaults
        {
            Cash = 1,
            Profit_And_Loss = 2,
            Advance_Payment = 3,
            Salary = 4,
            Service_Account = 5,
            PDC_Payable = 6,
            PDC_Receivable = 7,
            Discount_Allowed = 8,
            Discount_Received = 9,
            Sales_Account = 10,
            Purchase_Account = 11,
            Forex_GainLoss = 12,
            School_Fee_Revenue=13,
            School_Fee_Receiveable = 14
        }
        public enum EnumVoucherTypeDefaults
        {
            Opening_Balance = 1,
            Opening_Stock = 2,
            Contra_Voucher = 3,
            Payment_Voucher = 4,
            Receipt_Voucher = 5,
            Journal_Voucher = 6,
            PDC_Payable = 7,
            PDC_Receivable = 8,
            PDC_Clearance = 9,
            Purchase_Order = 10,
            Material_Receipt = 11,
            Rejection_Out = 12,
            Purchase_Invoice = 13,
            Purchase_Return = 14,
            Sales_Quotation = 15,
            Sales_Order = 16,
            Delivery_Note = 17,
            Rejection_In = 18,
            Sales_Invoice = 19,
            Sales_Return = 20,
            Service_Voucher = 21,
            Credit_Note = 22,
            Debit_Note = 23,
            Stock_Journal = 24,
            Physical_Stock = 25,
            Daily_Salary_Voucher = 26,
            Monthly_Salary_Voucher = 27,
            Advance_Payment = 28,
            Fee_Receipt_Voucher = 29,
            Fee_Issued_Voucher = 30
        }

        public enum EnumConfigurations
        {
            LateFee = 1,
            MSISDN = 2,
            SMSRate = 3,
            password = 4,
            Mask = 5,
            Remarks = 6,
            AbsentFine = 7,
            AffiliatedWith = 10,
            InstCode = 11,
            FounderName = 12,
            SchoolRegistrationNo = 13,
            SchoolEstablishedOn = 14,
            InstCodeHSSC = 15,
            InstCodeSSC = 16,
            ATstaff = 17,
            CTstafff = 18,
            OTetaff = 19,
            ATstudent = 20,
            CTstudent = 21,
            OTstudent = 22,
            RelieveTitle = 23,
            PerformanceTitle = 24,
            ConcernTitle = 25,
            RelieveBody = 26,
            ParacticalTitle = 27,
            PerformanceBody = 28,
            ConcernBody = 29,
            ExperienceTitle = 30,
            ParacticalBody = 31,
            NOCTitle = 32,
            ExperienceBody = 33,
            NOCBody = 34,
            BirthTitle = 35,
            BirthBody = 36,
            FeeIssuedMesg = 37,
            EmailPassword = 38,
            SchoolEmail = 39,
            Port = 40,
            smtpServer = 41,
            FounderRemarks = 42,
            StudentWaitingListMessage = 43,
            StaffWaitingListMessage = 44,
            NoOfMistakcWihtoutFine = 45,
            MistakResultFine = 46,
            StaffLateComingFine = 47,
            StaffAbsentFine = 48,
            ResultCharges = 49,
            FeeReceivedCharges = 50,
            FeeIssuedCharges = 51,
            AttendanceCharges = 52,
            StudentLateComingFine = 53,
            StudentAbsentFine = 54,
            LeaveMessage = 55,
            AbsentMessage = 56,
            PresentMessage = 57,
            BGColor = 58,
            apptype = 59,
            FirstPeriodStart = 60,
            BreakDuration = 61,
            PeriodDuration = 62,
            BreakPeriod = 63,
            PerDayPeriod = 64,
            OtherPeriods = 65,
            HolidayConfigurationModel = 66,
            Payroll = 67,
            Budget = 68,
            Tax = 69,
            MultiCurrency = 70,
            BillByBill = 71,
            AllowZeroValueEntry = 72,
            ShowCurrencySymbol = 73,
            TickPrintAfterSave = 74,
            AutomaticProductCodeGeneration = 75,
            Barcode = 76,
            AllowBatch = 77,
            AllowSize = 78,
            AllowModelNo = 79,
            AllowGodown = 80,
            AllowRack = 81,
            ShowSalesRate = 82,
            ShowMRP = 83,
            ShowUnit = 84,
            ShowSize = 85,
            ShowModelNo = 86,
            ShowDiscountAmount = 87,
            ShowProductCode = 88,
            ShowBrand = 89,
            ShowDiscountPercentage = 90,
            ShowDiscountInPercentage = 91,
            Printer = 92,
            NegativeCashTransaction = 93,
            NegativeStockStatus = 94,
            StockValueCalculationMethod = 95,
            DirectPrint = 96,
            AddConfirmationFor = 97,
            Add = 98,
            Edit = 99,
            Delete = 100,
            Close = 101,
            ShowPurchaseRate = 102,
            DefualtInvoiceType = 103,
            UseSSlForMail = 104,
            SMSServerUrl = 105,
            SMSPassword = 106,
            PrincipalMobile = 107,
            PerformanceCertificateTitle = 109,
            ConcernCertificateTitle = 110,
            ParacticalCertificateTitle = 112,
            PerformanceCertificateBody = 113,
            ConcernCertificateBody = 114,
            ExprienceCertificateTitle = 115,
            ParacticalCertificateBody = 116,
            NoObjectionCertificateTitle = 117,
            ExprienceCertificateBody = 118,
            NoObjectionCertificateBody = 119,
            BirhtCertificateTitle = 120,
            BirhtCertificateBody = 121,
            StudentLibraryNOCTitle = 122,
            StudentLibraryNOCBody = 123,
            StaffLibraryNOCTitle = 124,
            StaffLibraryNOCBody = 125,
            FeeIssuedMessageRemarks = 126,
            SentIssuedFeeTemplate = 127,
            FeeReceivedMessageTemplate = 128,
            SendStaffAttendance=129,
            StaffAttendanceMobileNo=130
        }
    }
}

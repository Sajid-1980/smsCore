//This is a source code or part of Models project
//Copyright (C) 2018  Models

using System;    
using System.Collections.Generic;    
using System.Text;    
//<summary>    
//Summary description for PaymentMasterInfo    
//</summary>    
namespace smsCore.Data.Models
{
    public class PaymentMasterInfo    
{    
    private decimal _paymentMasterId;    
    private string _voucherNo;    
    private string _invoiceNo;    
    private decimal _suffixPrefixId;    
    private DateTime _date;    
    private int _ledgerId;    
    private decimal _totalAmount;    
    private string _narration;    
    private int _voucherTypeId;       
    private string _userId;    
    private decimal _financialYearId;    
    private DateTime _extraDate;    
    private string _extra1;    
    private string _extra2;    
    private string _chequeNo;    
    private DateTime _chequeDate;  
    public string ChequeNo    
    {    
        get { return _chequeNo; }    
        set { _chequeNo = value; }    
    }    
    public DateTime ChequeDate    
    {    
        get { return _chequeDate; }    
        set { _chequeDate = value; }    
    }  
    
    public decimal PaymentMasterId    
    {    
        get { return _paymentMasterId; }    
        set { _paymentMasterId = value; }    
    }    
    public string VoucherNo    
    {    
        get { return _voucherNo; }    
        set { _voucherNo = value; }    
    }    
    public string InvoiceNo    
    {    
        get { return _invoiceNo; }    
        set { _invoiceNo = value; }    
    }    
    public decimal SuffixPrefixId    
    {    
        get { return _suffixPrefixId; }    
        set { _suffixPrefixId = value; }    
    }    
    public DateTime Date    
    {    
        get { return _date; }    
        set { _date = value; }    
    }    
    public int LedgerId    
    {    
        get { return _ledgerId; }    
        set { _ledgerId = value; }    
    }    
    public decimal TotalAmount    
    {    
        get { return _totalAmount; }    
        set { _totalAmount = value; }    
    }    
    public string Narration    
    {    
        get { return _narration; }    
        set { _narration = value; }    
    }    
    public int VoucherTypeId    
    {    
        get { return _voucherTypeId; }    
        set { _voucherTypeId = value; }    
    }     
    public string UserId     
    {    
        get { return _userId; }    
        set { _userId = value; }    
    }    
    public decimal FinancialYearId    
    {    
        get { return _financialYearId; }    
        set { _financialYearId = value; }    
    }    
    public DateTime ExtraDate    
    {    
        get { return _extraDate; }    
        set { _extraDate = value; }    
    }    
    public string Extra1    
    {    
        get { return _extra1; }    
        set { _extra1 = value; }    
    }    
    public string Extra2    
    {    
        get { return _extra2; }    
        set { _extra2 = value; }    
    }

      public  IEnumerable<PaymentDetailsInfo> PaymentDetails { get; set; } = new List<PaymentDetailsInfo>();
    
}    
}

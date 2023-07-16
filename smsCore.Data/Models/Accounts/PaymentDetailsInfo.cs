//This is a source code or part of Models project
//Copyright (C) 2018  Models

using System;    
using System.Collections.Generic;    
using System.Text;    
//<summary>    
//Summary description for PaymentDetailsInfo    
//</summary>    
namespace smsCore.Data.Models
{
    public class PaymentDetailsInfo    
{    
    private int _paymentDetailsId;    
    private int _paymentMasterId;    
    private int _ledgerId;    
    private decimal _amount;
    private decimal _exchangeRateId;   
    private DateTime _extraDate;    
    private string _extra1;    
    private string _extra2;    
    
    public int PaymentDetailsId    
    {    
        get { return _paymentDetailsId; }    
        set { _paymentDetailsId = value; }    
    }    
    public int PaymentMasterId    
    {    
        get { return _paymentMasterId; }    
        set { _paymentMasterId = value; }    
    }    
    public int LedgerId    
    {    
        get { return _ledgerId; }    
        set { _ledgerId = value; }    
    }    
    public decimal Amount    
    {    
        get { return _amount; }    
        set { _amount = value; }    
    }

        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
    public decimal ExchangeRateId
    {
        get { return _exchangeRateId; }
        set { _exchangeRateId = value; }
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
    
}    
}

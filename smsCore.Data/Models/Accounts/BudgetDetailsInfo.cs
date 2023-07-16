//This is a source code or part of Models project
//Copyright (C) 2018  Models

using System;    
using System.Collections.Generic;    
using System.Text;    
//<summary>    
//Summary description for BudgetDetailsInfo    
//</summary>    
namespace smsCore.Data.Models
{
    public class BudgetDetailsInfo    
{    
    private decimal _budgetDetailsId;    
    private decimal _budgetMasterId;    
    private int _ledgerId;    
    private decimal _credit;    
    private decimal _debit;    
    private string _extra1;    
    private string _extra2;    
    private DateTime _extraDate;    
    
    public decimal BudgetDetailsId    
    {    
        get { return _budgetDetailsId; }    
        set { _budgetDetailsId = value; }    
    }    
    public decimal BudgetMasterId    
    {    
        get { return _budgetMasterId; }    
        set { _budgetMasterId = value; }    
    }    
    public int LedgerId    
    {    
        get { return _ledgerId; }    
        set { _ledgerId = value; }    
    }    
    public decimal Credit    
    {    
        get { return _credit; }    
        set { _credit = value; }    
    }    
    public decimal Debit    
    {    
        get { return _debit; }    
        set { _debit = value; }    
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
    public DateTime ExtraDate    
    {    
        get { return _extraDate; }    
        set { _extraDate = value; }    
    }    
    
        public string CrDr { get; set; }
}    
}


//This is a source code or part of Models project
//Copyright (C) 2018  Models
using System;    
using System.Collections.Generic;    
using System.Text;    
//<summary>    
//Summary description for MonthlySalaryInfo    
//</summary>    
namespace smsCore.Data.Models
{
    public class MonthlySalaryInfo    
{    
    private decimal _monthlySalaryId;    
    //private DateTime _date;
    private DateTime _salaryMonth;    
    private string _narration;    
    private DateTime _extraDate;    
    private string _extra1;    
    private string _extra2;    
    
    public decimal MonthlySalaryId    
    {    
        get { return _monthlySalaryId; }    
        set { _monthlySalaryId = value; }    
    }    
    //public DateTime Date    
    //{    
    //    get { return _date; }    
    //    set { _date = value; }    
    //}
    public DateTime SalaryMonth    
    {    
        get { return _salaryMonth; }    
        set { _salaryMonth = value; }    
    }    
    public string Narration    
    {    
        get { return _narration; }    
        set { _narration = value; }    
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

//This is a source code or part of Models project
//Copyright (C) 2018  Models

using System;    
using System.Collections.Generic;    
using System.Text;    
//<summary>    
//Summary description for SalaryPackageInfo    
//</summary>    
namespace smsCore.Data.Models
{
    public class SalaryPackageInfo    
{    
    
    
    public decimal SalaryPackageId    
    {
        get;
        set;
    }    
    public string SalaryPackageName    
    {
        get;
        set;
    }    
    public bool IsActive    
    {
        get;
        set;
    }    
    public string Narration    
    {
        get;
        set;
    }
    public decimal TotalAmount
    {
        get;
        set;
    }
    public DateTime ExtraDate    
    {
        get;
        set;  
    } 
   
    public string Extra1    
    {
        get;
        set; 
    }    
    public string Extra2    
    {
        get;
        set; 
    }    
    
}    
}

using System;
using System.Web;
using Models;

namespace smsCore.Data.Models
{
    public class BaseModel
    {
        //public BaseModel(SchoolEntities _database)
        //{
        //    database = _database;
        //}

        //public SchoolEntities database { get; set; }

        //public bool SaveChanges()
        //{
        //    HttpContext.Current.Session["error"] = null;
        //    var states = new ModelStateDictionary();
        //    try
        //    {
        //        database.SaveChanges();
        //        return true;
        //    }
        //    catch (DbEntityValidationException ba)
        //    {
        //        foreach (var a in ba.EntityValidationErrors)
        //        foreach (var c in a.ValidationErrors)
        //            states.AddModelError(c.PropertyName, c.ErrorMessage);
        //    }
        //    catch (Exception aa)
        //    {
        //        states.AddModelError("", aa.Message);
        //    }

        //    return false;
        //}
    }
}
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Models;
using smsCore.Data.Models;
using Microsoft.AspNetCore.Authorization;

namespace sms.WebApiControllers
{
    [Authorize]
    [Route("api/timetable")]
    public class TimeTableController : Controller
    {
        private readonly SchoolEntities db;

        public TimeTableController(SchoolEntities db)
        {
            this.db = db;
        }

        [Route("config/{campusId}")]
        //public JsonResult GetConfguration(int campusId)
        //{
        //    var result = new JsonResult();

        //    var model = new TimeTableConfig(campusId, true);

        //    result.Data = model;
        //    result.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
        //    return result;
        //}
        [HttpGet]
        [Route("GetConfguration")]
        public IActionResult GetConfguration(int campusId)
        {
            var model = new TimeTableConfig(campusId, true);
            return (IActionResult)Json(model);
        }

    }
}
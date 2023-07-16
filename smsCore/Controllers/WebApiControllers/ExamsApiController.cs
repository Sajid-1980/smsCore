using System.Collections.Generic;
using Models;
using SchoolManagementSystem.Helpers;
using smsCore.Data;
using Microsoft.AspNetCore.Mvc;

namespace sms.WebApiControllers
{
    [Route("api/exam")]
    public class ExamsApiController : Controller
    {
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;

        public ExamsApiController(SchoolEntities _db,CurrentUser Cuser)
        {
            db = _db;
            _user = Cuser;
        }


        [HttpPost]
        [Route("get-student-result-for-session")]
        public JsonResult GetStudentResultForSession(int studentId, int session)
        {
            var rs = new ResultSystem();
            var exam = rs.GetResultsbyStudent(studentId, session);
            
            return Json(new { result = exam, count = exam.Count });
        }
    }
}
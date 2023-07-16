using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using smsCore.Data;

namespace sms.WebApiControllers
{
    [Route("api/mycontroller")]
    [AllowAnonymous]
    public class BioMatrixAttendanceController : Controller
    {

        private readonly SchoolEntities db;
        private readonly CurrentUser _user;

        public BioMatrixAttendanceController(SchoolEntities _db,CurrentUser Cuser)
        {
            db = _db;
            _user = Cuser;
            
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("get-info")]
        public IActionResult GetInfo(string type, string Id)
        {
            try
            {
                if (type == "student")
                {
                }
                else if (type == "employee")
                {
                    var employee = db.tbl_Employee.AsNoTracking().Where(f => f.employeeCode == Id).Select(s => new
                    {
                        Code = s.employeeCode,
                        Name = s.employeeName,
                        s.Photo,
                        DesignationName = s.tbl_Designation.designationName,
                        s.Campus.CampusName
                    });
                    if (employee != null) return Ok(employee);
                }

                return Ok("not foud");
            }
            catch (Exception ee)
            {
                return BadRequest(ee.Message);
            }
        }


        [HttpPost]
        [Route("enroll")]
        public IActionResult Enroll(int Id, string type, string impression)
        {
            try
            {
                if (type == "student")
                {
                }
                else if (type == "employee")
                {
                    decimal.TryParse(Id.ToString(), out var employeeId);
                    var employee = db.tbl_Employee.AsNoTracking().FirstOrDefault(f => f.Id == employeeId);
                    if (employee != null)
                    {
                        var thumb = employee.EmployeeThumbImpressions.FirstOrDefault();
                        if (thumb == null)
                        {
                            thumb = new EmployeeThumbImpression();
                            thumb.StaffID = employee.Id;
                            db.EmployeeThumbImpressions.Add(thumb);
                        }

                        thumb.Thumb = impression;
                        thumb.Finger = 1;
                        db.SaveChanges();
                    }
                }

                return Ok(true);
            }
            catch (Exception ee)
            {
                return BadRequest(ee.Message);
            }
        }
    }
}
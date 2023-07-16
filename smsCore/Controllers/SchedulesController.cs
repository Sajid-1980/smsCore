using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;

using Newtonsoft.Json;
using smsCore.Data.Helpers;
using System.Xml.Linq;
using Utilities;

namespace smsCore.Controllers
{
    public class SchedulesController : BaseController
    {
        private readonly SchoolEntities db;
        private readonly ClsBussinessSetting _clsBussinessSetting;

        public SchedulesController(SchoolEntities _db, ClsBussinessSetting clsBussinessSetting)
        {
            db = _db;
            _clsBussinessSetting = clsBussinessSetting;
        }

        //
        // GET: /Admin/Schedules/

        public IActionResult Index()
        {
            return View();
        }
        
        public JsonResult GetEvents(DateTime start, DateTime end, int campusId)
        {
            //var startDate =DateTimeHelper.ConvertDate(start,format: "yyyy-MM-dd");
            //var endDate = DateTimeHelper.ConvertDate(end,format: "yyyy-MM-dd");
            var schecules = db.SchoolLeaveSchedules.Where(w => w.CampusID == campusId).ToList()
                .Where(w => w.date.Date >= start && w.date.Date < end).Select(s => new
                {
                    s.ID,
                    title = s.holidayName,
                    start = s.date.ToString("yyyy-MM-dd"),
                    isHOliday = s.IsHoliday,
                    color = s.Color
                });

            return Json(schecules);
        }

        [HttpPost]
        public IActionResult SetEvents(int id, string dt, string dt2, string title, int campusId, string color, bool isholiday)
        {
            if (id == 0)
                try
                {
                    id = db.SchoolLeaveSchedules.Max(m => m.ID) + 1;
                }
                catch
                {
                    id = 1;
                }

            var date1 = DateTimeHelper.ConvertDate(dt);
            var date2 = DateTimeHelper.ConvertDate(dt2);
            if (date2 > date1 && date2 != DateTime.MinValue)
            {
                for (DateTime cd = date1; cd <= date2;)
                {
                    var exist = db.SchoolLeaveSchedules.FirstOrDefault(w => w.date == cd);
                    if (exist == null)
                    {
                        exist = new SchoolLeaveSchedule();
                        exist.ID = id;
                        db.SchoolLeaveSchedules.Add(exist);
                        id++;
                    }
                    exist.date = cd;
                    exist.holidayName = title;
                    exist.CampusID = campusId;
                    exist.Color = color;
                    exist.IsHoliday = isholiday;

                    cd = cd.AddDays(1);
                }
            }
            else
            {
                var schedule = db.SchoolLeaveSchedules.Where(w => w.ID == id).FirstOrDefault();
                if (schedule == null)
                {

                    schedule = new SchoolLeaveSchedule();
                    schedule.ID = id;
                    db.SchoolLeaveSchedules.Add(schedule);

                }

                schedule.date = DateTimeHelper.ConvertDate(dt);// DateTime.Parse(dt);
                schedule.holidayName = title;
                schedule.CampusID = campusId;
                schedule.Color = color;
                schedule.IsHoliday = isholiday;
            }
            try
            {

                
                db.SaveChanges();
                //return new JsonResult { Data = new { message = "Save", id = 0 } };

                return Json(new
                {
                    Data = new {message="Save" , id=0}
                });
            }
            
            catch
            {
                return Json(new
                {
                    Data = new { message = "Unable to store event on selected date.", id = 0 }
                });
            }
        }

        [HttpPost]
        public IActionResult DeSetEvents(int id)
        {
            var schedule = db.SchoolLeaveSchedules.Where(w => w.ID == id).FirstOrDefault();
            if (schedule != null)
            {
                db.SchoolLeaveSchedules.Remove(schedule);
                try
                {
                    db.SaveChanges();
                    
                    return Json(new
                    {
                        Data = new { message = "Save "}
                    });
                }
                catch
                {
                    return Json(new
                    {
                        Data = new { message = "Unable to Save " }
                    });

                }
            }

            return Json(new
            {
                Data = new { message = "Invalid Request " }
            });
         }
        
        //
        // GET: /Admin/Schedules/Details/5

        public IActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Admin/Schedules/Create

        public IActionResult Create()
        {
            return View();
        }

        //
        // POST: /Admin/Schedules/Create

        [HttpPost]
        public IActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Admin/Schedules/Edit/5

        public IActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Admin/Schedules/Edit/5

        [HttpPost]
        public IActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Admin/Schedules/Delete/5

        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Admin/Schedules/Delete/5

        [HttpPost]
        public IActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        public ActionResult Holyday()
        {
            //var sp = new FinancialYearSP();
            //var session = sp.FinancialYearGetActive();
            var session = db.FinancialYears.Where(w => w.IsCurrent).FirstOrDefault();
            ViewBag.labelseesiontext = $"Session: {session.fromDate.ToString("MMMM, yyyy")} - {session.toDate.ToString("MMMM, yyyy")}";
            return View();
        }



        public string Holyday(SchoolLeaveSchedule leaveSchedule)
        {
            var session = db.FinancialYears.AsNoTracking().FirstOrDefault(w => w.IsCurrent);
            if (session == null) return "No current financial year is active.";
            var message = "";
            var configurationModel = new ConfigurationModel();

            var start = DateTime.Today;
            var applyFrom = Request.Form["applyFrom"].ToString();
            if (applyFrom == "ApplySession")
            {
                start = DateTime.Today;

                start = session.fromDate;
                configurationModel.WholeSession = true;
            }
            else if (applyFrom == "ApplyFromDate")
            {
                start = DateTime.Parse(Request.Form["ApplyFromDate"].ToString());
                configurationModel.CustomDate = true;
                configurationModel.SelectedCustomDate = start;
            }
            else if (applyFrom == "ApplyFromToday")
            {
                start = DateTime.Today.Date;
                configurationModel.CurrentDate = true;
            }

            var checkedListBox1 = Request.Form["Weeklyholiday"].ToString();
            var holidays = new List<DayOfWeek>();
            foreach (char checkedday in checkedListBox1)
            
             //   holidays.Add(Extensions.GetDayOfWeekFromName(checkedday));
            

            configurationModel.WeeklyHolidays = holidays;

            if (Request.Form["Campus"].ToString() == null) return "Campus Not Selected";
            var campus = int.Parse(Request.Form["Campus"].ToString());
            var id = 1;
            try
            {
                id = db.SchoolLeaveSchedules.Max(m => m.ID) + 1;
            }
            catch
            {
                id = 1;
            }


            for (var loop = start; loop <= session.toDate;)
            {
                if (holidays.Contains(loop.DayOfWeek))
                    if (!db.SchoolLeaveSchedules
                        .Where(a => a.CampusID == campus && a.date.Date == loop.Date).Any())
                    {
                        db.SchoolLeaveSchedules.Add(new SchoolLeaveSchedule
                        { ID = id, CampusID = campus, date = loop.Date, holidayName = "Weekend", IsHoliday = true, Color = "green" });
                        id++;
                    }

                loop = loop.AddDays(1);
            }

            try
            {
                db.SaveChanges();
                // var setting = new Data.Helpers.ClsBussinessSetting(campus);
                _clsBussinessSetting.CampusId = campus;

                _clsBussinessSetting.WriteorUpdate("HolidayConfigurationModel", JsonConvert.SerializeObject(configurationModel));
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }

            return message;
        }


        public class ConfigurationModel
        {
            public List<DayOfWeek> WeeklyHolidays { get; set; }
            public bool WholeSession { get; set; }
            public bool CurrentDate { get; set; }
            public bool CustomDate { get; set; }
            public DateTime SelectedCustomDate { get; set; }
            public DayOfWeek WeekStartFrom { get; set; }
        }

    }
}







using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using smsCore.Data;

namespace smsCore.Controllers
{
    [Authorize]
    public class VisitorController : BaseController
    {
        private readonly SchoolEntities db;
        private readonly StaticResources _resource;
        private readonly CurrentUser _user;

        public VisitorController(SchoolEntities _db, StaticResources resource,CurrentUser
             user)
        {
            db = _db;
            _resource = resource;
            _user = user;
        }
        
        #region Visitors
        public async Task<IActionResult> Index()
        {
            return View();
        }
        public async Task<IActionResult> Create(int id = 0)
        {
            return View();
        }
        
        public async Task<IActionResult> Edit(int id = 0)
        {
            var model = await db.Visitors.Where(w => w.Id == id).FirstOrDefaultAsync();
            return View("Create",model);
        }
        [HttpPost]
        public async Task<JsonResult> Create(Visitor model)
        {
            try
            {
                bool isNew = false;
                var exist = await db.Visitors.FirstOrDefaultAsync(w => w.Id == model.Id);
                if (exist == null)
                {
                    isNew = true;
                    exist = new Visitor() { CampusId= _user.SelectedCampusId, EntryDate = DateTime.Now, UserId = _user.UserID };
                    db.Visitors.Add(exist);
                }
                exist.Name = model.Name;
                exist.Relationship = model.Relationship;
                exist.StudentRegNo = model.StudentRegNo;
                exist.VistorType = model.VistorType;
                exist.Cnic = model.Cnic;
                exist.Contact = model.Contact;
                exist.Description = model.Description;
                exist.Date = DateTimeHelper.ConvertDate(Request.Form["VisitDate"].ToString());
                exist.CampusId = _user.SelectedCampusId;

                await db.SaveChangesAsync();
                return _resource.GetResult(true, "Info! Visitor Saved successfully.");
            }
            catch (Exception ex)
            {
                return _resource.GetResult(false, ex.InnerException == null ? ex.Message : ex.InnerException.Message);
            }
        }

        
        public JsonResult VisitorList(DataManagerRequest dm, string fromdate, string todate, int LedgerId, string name)
        {
            int campusid = _user.SelectedCampusId;
            var data = db.Visitors.Where(w => w.CampusId == campusid).Select(s => new
            {
                s.Id,
                s.Name,
                s.Date,s.Description,
                s.VistorType

            }).Where(w => w.Id > 0);
            var date1 = DateTimeHelper.ConvertDate(fromdate);
            var date2 = DateTimeHelper.ConvertDate(todate);
            if (date1 != DateTime.MinValue)
            {
                data = data.Where(w => w.Date >= date1);
            }
            if (date2 != DateTime.MinValue)
            {
                data = data.Where(w => w.Date <= date2);
            }
            if (!string.IsNullOrEmpty(name))
            {
                data = data.Where(w => w.Name.Contains(name));
            }
           
            DataOperations operation = new DataOperations();

            if (dm.Search != null && dm.Search.Count > 0)
            {
                data = operation.PerformSearching(data, dm.Search);  //Search
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                data = operation.PerformFiltering(data, dm.Where, dm.Where[0].Operator);
            }
            int count = data.Count();
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                data = operation.PerformSorting(data, dm.Sorted);
            }
            if (dm.Skip != 0)
            {
                if (dm.Sorted == null || dm.Sorted.Count == 0)
                {
                    List<Sort> srt = new List<Sort>() { };
                    srt.Add(new Sort { Name = "Date", Direction = "ascending" });
                    data = operation.PerformSorting(data, srt);
                }

                data = operation.PerformSkip(data, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                data = operation.PerformTake(data, dm.Take);
            }

            if (dm.RequiresCounts)
            {
                return Json(new { result =   data.ToList(), count = count });
            }
            else
            {
                return Json(new { result = data.ToList(), count = count });
            }
        }
        
        public async Task<JsonResult> DeleteVisitor(int id)
        {
            try
            {
                var exist = await db.Visitors.FirstOrDefaultAsync(w => w.Id == id);
                if (exist != null)
                {
                    db.Visitors.Remove(exist);
                    await db.SaveChangesAsync();
                }
                return _resource.GetResult(true, "success");
            }
            catch { }
            return _resource.GetResult(false, "failed");
        }

        #endregion


    }
}
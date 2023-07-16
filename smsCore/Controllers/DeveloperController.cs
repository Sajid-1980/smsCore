using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using System.Reflection;

namespace smsCore.Controllers
{
     
    public class DeveloperController : BaseController
    {
         private readonly SchoolEntities _context;

        public DeveloperController(SchoolEntities context)
        {
            _context = context;
        }


        // GET: Developer
        public IActionResult Index()
        {
            return View();
        }
        ////////////////////////////////////// ACTION PAGE SECTION ////////////////////////////
        /// Save Action Item Start ///
        public IActionResult Actions()
        {

            Assembly assembly = Assembly.GetExecutingAssembly();
            List<string> list = new List<string>();
            IEnumerable<Type> types = assembly.GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type)).OrderBy(x => x.Name);
            foreach (Type cls in types)
            {
                list.Add(cls.Name.Replace("Controller", ""));
            }
            ViewBag.Controllers = new SelectList(list.Select(s => new { Id = s, Text = s }), "Id", "Text");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Actions(MenuAction model)
        {
            var exist = await _context.MenuActions.FirstOrDefaultAsync(a => a.Name == model.Name);
            if (exist == null)
            {
                exist = new MenuAction();
                _context.MenuActions.Add(exist);
            }

            exist.Name = model.Name;
            exist.DisplayText = model.DisplayText;
            exist.Controller = model.Controller;
            exist._Type = model._Type;
            exist.IsActive = true;

            try
            {
                await _context.SaveChangesAsync();
                // return View("DragMenu");
                return Json(new { status = true, message = "Action saved successfully." });
            }
            catch
            {
                return Json(new { status = false, message = "Unable to save Action." });

            }

        }
        /// Save Action Item End ///
        public IActionResult DragMenu()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            List<string> list = new List<string>();
            IEnumerable<Type> types = assembly.GetTypes().Where(type => typeof(Controller).IsAssignableFrom(type)).OrderBy(x => x.Name);
            foreach (Type cls in types)
            {
                list.Add(cls.Name.Replace("Controller", ""));
            }
            ViewBag.Controllers = new SelectList(list.Select(s => new { Id = s, Text = s }), "Id", "Text");
            return View();
        }
        public JsonResult ActionList(string controllername)
        {
            return Json(new { status = true, data = _context.MenuActions.Where(w => w.Controller == controllername).AsNoTracking().Select(s => new { Name = s.DisplayText, Id = s.Id }).ToList() });
        }

        public JsonResult ParentList()
        {
            return Json(new { status = true, data = _context.MenuItems.AsNoTracking().Select(s => new { Name = s.Action.DisplayText, Id = s.Id }).ToList() });
        }

        [HttpPost]
        public async Task<JsonResult> MenuItems(MenuItem model)
        {

            var menuitems = await _context.MenuItems.FirstOrDefaultAsync(a => a.Id == model.Id);
            if (menuitems == null)
            {
                menuitems = await _context.MenuItems.FirstOrDefaultAsync(a => a.ActionId == model.ActionId);
                if (menuitems == null)
                {
                    menuitems = new MenuItem();
                    _context.MenuItems.Add(menuitems);
                }
                else
                {
                    return Json(new { status = false, message = "Selected action already exist." });
                }
            }
            menuitems.ParentId = model.ParentId;
            menuitems.Icon = model.Icon;
            menuitems.SortOrder = model.SortOrder;
            menuitems.IsActive = model.IsActive;
            menuitems.ActionId = model.ActionId;


            try
            {
                await _context.SaveChangesAsync();
                return Json(new { status = true, message = "Menu Items saved successfully.", id = menuitems.Id });
            }
            catch
            {
                return Json(new { status = false, message = "Unable to save Menu Items." });

            }

        }
        /// Save Menu Item End ///
        

        /// Delete Menu Item  ///
        public JsonResult MenuItemDelete(int menuid)
        {
            try
            {
                var childs = _context.MenuItems.Any(a => a.ParentId == menuid);
                if (childs)
                {
                    return Json( new { status = false, message = "Selected Item has child. Please remove child items first." });
                }
                var menu = _context.MenuItems.FirstOrDefault(r => r.Id == menuid);

                _context.MenuItems.Remove(menu);
                _context.SaveChanges();
                return Json(new { status = true, message = "Menu Item Successfully Deleted" } );
            }
            catch (Exception Ex)
            {
                return Json(new { status = false, message = Ex.ToString() } );
            }
        }

        public JsonResult LoadMenuItemForEdit(int id)
        {
            return Json(new { status = true, data = _context.MenuItems.Where(w => w.Id == id).Include(i => i.Action).Select(s => new { s.Id, s.ActionId, s.IsActive, s.ParentId, s.SortOrder, s.Icon, Controller = s.Action.Controller }).FirstOrDefault() });
        }
        [HttpPost]
        public async Task<JsonResult> SaveSorting(SortingRequest[] model)
        {
            int i = 0;
            foreach (var m in model)
            {
                var d = _context.MenuItems.Find(m.id);
                d.SortOrder = i;
                d.ParentId = m.parent_id;
                i++;
            }
            await _context.SaveChangesAsync();
            return Json(new { status = true, message = "Menu saved successfully." });
        }
    }

    public class SortingRequest
    {
        public int id { get; set; }
        public int parent_id { get; set; }
    }
}
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;
using smsCore.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using smsCore.Data.Models.ViewModels;
using smsCore.Data.Models;

namespace smsCore.Controllers
{   //[Authorize(Roles ="Admin")]
    //[CompressContent]
    [Authorize]
    public class UsersController : BaseController
    {
        //
        // GET: /Admin/Users/

        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        //private ApplicationSignInManager _signInManager;
        //private ApplicationUserManager _userManager;

        // GET: Teacher
        private readonly SchoolEntities db;// = new SchoolEntities();
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly CurrentUser _user;



        public UsersController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, SchoolEntities _db, RoleManager<IdentityRole> _roleManager, CurrentUser user)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            db = _db;
            roleManager = _roleManager;
            _user = user;

        }



        public IActionResult Index(bool formonly=false)
        {
            ViewBag.formonly = formonly;
            var model1 = db.Users.ToList();
            ViewBag.Roles = new SelectList(db.Roles.Select(s => new {Value = s.Name, Display = s.Name}), "Value",
                "Display");
            var list = db.Campuses.Select(s => new {Value = s.ID, Display = s.CampusName}).ToList();
            list.Insert(0, new {Value = -1, Display = "---Select Campus---"});
            ViewBag.Campus = new SelectList(list, "Value", "Display");
            return View();
        }
        
        public IActionResult UserList(DataManagerRequest request)
        {
            //var users = context.Users.ToList();
            var users = db.Users.ToList().Select(s => new
            {
                s.Id,
                s.UserName,
                role =  string.Join(", ",_userManager.GetRolesAsync(s).Result.ToArray())
                  ,
                campus = s.CampusUsers.Count > 0
                    ? s.CampusUsers.DefaultIfEmpty().Select(c => new
                    { c.CampusID, CampusName = c.Campus == null ? "" : c.Campus.CampusName })
                    : null
            }).ToList();

            if (_user.BasicUserType == EnumManager.BasicUserType.Campus)
            {
                var campusId = _user.GetCampusIds().FirstOrDefault();
                users = users.Where(w => w.campus != null && w.campus.Select(s => s.CampusID).Contains(campusId))
                    .ToList();
            }


            var data = users.ToList().Select(s => new
            {
                s.Id,
                s.UserName,
                s.role,
                campus = s.campus == null
                        ? ""
                        : s.campus.DefaultIfEmpty().Select(c => c.CampusName).FirstOrDefault()
            });

            return Json(new { result = data, count = data.Count() });
        }

        public async Task<string> Getrole(string username)
        {
            var result = "";
            try
            {
                var user = await _userManager.FindByNameAsync(username);
               var roles= (await _userManager.GetRolesAsync(user));
                result = string.Join(", ",roles);
            }
            catch
            {
            }

            return result;
        }

        
        
        public IActionResult Create(string role, string fk)
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.AddRecords))
            //{
            //    return View("UnAuthorize");
            //}

            ViewBag.role = role;
            ViewBag.fk = fk;
            return View();
        }
        
        
        [HttpPost]
        public async Task<string> Create(RegisterViewModel model)
        {
            var role = Request.Form["Roles"].ToString();
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.AddRecords))
            //{
            //    return View("UnAuthorize");
            //}
            if (role=="Parent" && !int.TryParse(Request.Form["Parents"], out int pid))
            {
                return "Please select a Parent to continue.";
            }
           
            else if (role=="Teacher" && !int.TryParse(Request.Form["Employees"], out int tid))
            {

              return "Please select an Employee to continue.";

            }
            else if (role=="Student" && !int.TryParse(Request.Form["Students"].ToString(), out int sid))
            {
                return "Please select a Student to continue.";
            }

            if (role=="Parent")
            {
                int.TryParse(Request.Form["Parents"].ToString(), out pid);
                var p = db.Students.AsNoTracking().Where(w => w.ID==pid).FirstOrDefault();
                if (p==null || string.IsNullOrEmpty(p.CNIC.Trim()))
                {
                    return "Selected parent has emppty CNIC.";
                }
            }

            var Campus = int.Parse(Request.Form["Campus"].ToString());
            if (ModelState.IsValid)
                // Attempt to register the user
                try
                {
                    var user = new ApplicationUser
                        {UserName = model.UserName, Email = model.Email, PhoneNumber = "", EmailConfirmed = false};

                    var result = await _userManager.CreateAsync(user, model.Password);
                    if (result.Errors.Count() > 0) return result.Errors.FirstOrDefault().Description;

                    try
                    {
                        await _userManager.AddToRoleAsync(user,role);

                        if (role=="Parent")
                        {
                            int.TryParse(Request.Form["Parents"].ToString(), out int p1id);
                            UserDefinition def = new UserDefinition { FK=p1id, UserId=user.Id, UserType="p" };
                            db.UserDefinitions.Add(def);
                            await db.SaveChangesAsync();
                        }
                        else if (role=="Teacher")
                        {
                            int.TryParse(Request.Form["Employees"].ToString(), out int t1id);
                            UserDefinition def = new UserDefinition { FK=t1id, UserId=user.Id, UserType="e" };
                            db.UserDefinitions.Add(def);
                            await db.SaveChangesAsync();
                        }
                        else if (role=="Student")
                        {
                            int.TryParse(Request.Form["Students"].ToString(), out int s1id);
                            UserDefinition def = new UserDefinition { FK=s1id, UserId=user.Id, UserType="s" };
                            db.UserDefinitions.Add(def);
                            await db.SaveChangesAsync();
                        }
                    }
                    catch
                    {
                        _userManager.DeleteAsync(user);
                        return "An error occured while trying to create the user account.";
                    }

                    if (result.Succeeded)
                        try
                        {
                            if (Campus > 0)
                            {
                                db.CampusUsers.Add(new CampusUser {CampusID = Campus, UserID = user.Id});
                                await db.SaveChangesAsync();
                            }

                            return "success";
                        }
                        catch
                        {
                           await _userManager.RemoveFromRoleAsync(user,role);
                            await _userManager.DeleteAsync(user); 
                            return "Unable to add user to campus.";
                        }

                    ModelState.AddModelError("", result.Errors.FirstOrDefault().Description);
                }
                catch (Exception e)
                {
                    ModelState.AddModelError("", e.Message);
                }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        
        public IActionResult Detail(int id = 0)
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.ViewRecords))
            //{
            //    return View("UnAuthorize");
            //}
            var model = _userManager.Users;
            if (model == null)
            {
            }

            return View(model);
        }

        
        public IActionResult Edit(string username)
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.EditRecords))
            //{
            //    return View("UnAuthorize");
            //}
            var model = db.Users.Where(w => w.UserName == username)
                .FirstOrDefault(); //  new Models.UsersContext().UserProfiles.Where(w => w.UserId == id).FirstOrDefault();

            if (model == null)
            {
            }

            return View(model);
        }

        
        [HttpPost]
        public IActionResult Edit(ApplicationUser user)
        {
            var users = db.Users.Where(w => w.Id == user.Id).FirstOrDefault();
            users.UserName = user.UserName;
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        
        public IActionResult Delete(string username)
        {
            var model = db.Users.Where(w => w.UserName == username)
                .FirstOrDefault(); // new Models.UsersContext().UserProfiles.Where(w => w.UserId == id).FirstOrDefault();
            if (model == null)
            {
            }

            return View(model);
        }

        
        [HttpPost]
        public async Task<string> Delete(ApplicationUser aspusers)
        {
            if (ModelState.IsValid)
                try
                {
                    var user = db.Users.Find(aspusers.Id);
                    var roleName = await _userManager.GetRolesAsync(user);
                    //for (var i = 0; i < roleName.Count(); i++)
                        await _userManager.RemoveFromRolesAsync(user, roleName);
                    var campususer = db.CampusUsers.Where(w => w.UserID == user.Id).FirstOrDefault();
                    db.CampusUsers.Remove(campususer);
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    return "delete";
                }

                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        public IActionResult UserDetails(string username)
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.AddRecords))
            //{
            //    return View("UnAuthorize");
            //}
            //ViewBag.rolename = username;
            //ViewBag.users = UserManager.Users.ToList().Where(w => UserManager.IsInRole(w.Id, username)).Select(s => s.UserName).ToArray();
            return View();
        }


        
        public IActionResult RoleList()
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.ViewRecords))
            //{
            //    return View("UnAuthorize");
            //}
            ViewBag.Roles = roleManager.Roles.Select(s => s.Name).ToArray();
            return View();
        }

        public IActionResult RolesList(DataManagerRequest request)
        {
            string[] PreBuiltRoles = new string[] { "Admin", "Developer", "Teacher", "Student","Parent" };
            var rolelist = db.Roles.Where(w => w.Name!="Developer").ToList().Select(s => new { s.Id, s.Name, isEditAble = !PreBuiltRoles.Contains(s.Name) });

            return Json(new { result = rolelist, count = rolelist.Count() });
        }

        
        public ActionResult CreateRole()
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.AddRecords))
            //{
            //    return View("UnAuthorize");
            //}

            return View();
        }

        
        [HttpPost]
        public async Task<string> CreateRole(RoleInfo roleInfo)
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.AddRecords))
            //{
            //    return View("UnAuthorize");
            //}
            var RoleName = Request.Form["RoleName"].ToString();
            var existname = db.Roles.Where(w => w.Name.ToLower() == RoleName.ToLower()).Select(s => s.Name)
                .FirstOrDefault();
            if (existname != null) return "Entered role already exists";
            if (RoleName.ToLower() == "admin" || await roleManager.RoleExistsAsync(RoleName))
                ModelState.AddModelError("RoleName", "Provided role name already exist.");
            try
            {
                await roleManager.CreateAsync(new IdentityRole {Name = RoleName}); // Roles.CreateRole(RoleName);
                return "success";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            //ViewBag.rolename = RoleName;
            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        
        public IActionResult EditRole(string role)
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.EditRecords))
            //{
            //    return View("UnAuthorize");
            //}
            var model = db.Roles.Where(w => w.Name == role).FirstOrDefault();
            return View(model);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<string> EditRole(IdentityRole role)
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.EditRecords))
            //{
            //    return View("UnAuthorize");
            //}
            var rolename = Request.Form["role"].ToString();
            var name = Request.Form["Name"].ToString();

            try
            {
                if (await roleManager.RoleExistsAsync(rolename))
                {
                    var toedit =await roleManager.FindByNameAsync(rolename);
                    toedit.Name = name;
                    await roleManager.UpdateAsync(toedit);
                    return "success";
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        
        public IActionResult DeleteRole(string id)
        {
            var model = db.Roles.Where(w => w.Id == id)
                .FirstOrDefault(); // new Models.UsersContext().UserProfiles.Where(w => w.UserId == id).FirstOrDefault();
            if (model == null)
            {
            }

            return View(model);
        }

        
        [HttpPost]
        public async Task<string> DeleteRole(IdentityRole role)
        {
            if (ModelState.IsValid)
                try
                {
                    var roles = db.Roles.Find(role.Id);

                    db.Roles.Remove(roles);
                    await db.SaveChangesAsync();
                }

                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }

            if (ModelState.IsValid) return "delete";

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        
        public IActionResult AssignRole(string role = "")
        {
            //ViewBag.roles = context.Roles.Where(w => w.Name == role).Select(s => s.Name).FirstOrDefault();
            var roles = db.Roles.Where(w => w.Name == role).FirstOrDefault();
            ViewBag.users =
                new SelectList(_userManager.Users, "Id", "UserName"); //new SelectList( Membership.GetAllUsers());
            return View(roles);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<string> AssignRole(IdentityRole role)
        {
            if (ModelState.IsValid)
                try
                {
                    var usersId = Request.Form["userId"].ToString();
                    var user =await  _userManager.FindByIdAsync(usersId);
                    await _userManager.AddToRoleAsync(user, role.Name);
                    return "success";
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }

            return ModelState.Where(w => w.Value.Errors.Count > 0).FirstOrDefault().Value.Errors.FirstOrDefault()
                .ErrorMessage;
        }

        
        public async Task<IActionResult> UsersInRole(string role)
        {
            //if (!Helpers.EnumManager.Modules.Attendance.IsAuthrozed(EnumManager.Actions.ViewRecords))
            //{
            //    return View("UnAuthorize");
            //}
            ViewBag.Rolename = role;
            ViewBag.users = (await _userManager.GetUsersInRoleAsync(role)).Select(s => s.UserName).ToArray();
            return View();
        }

        
        public async Task<IActionResult> RemoveUserFromRole(string username, string rolename)
        {
            try
            {
                var userId = await _userManager.FindByNameAsync(username);
                await _userManager.RemoveFromRoleAsync(userId, rolename);
            }
            catch
            {
            }

            return View("RoleList");
        }


        [HttpGet]
        public async Task<IActionResult> Authentications(string roleid)
        {
            ViewBag.roles = new SelectList(await db.Roles.Select(s => new { s.Id, s.Name }).ToListAsync(), "Id", "Name", roleid);
            ViewBag.roleId = roleid;
            
            return View();
        }

        public async Task<IActionResult> PrvilidgeList( DataManagerRequest dm, string id)
        {
            var privilidges = db.MenuActions.Include(i=>i.MenuItems).Where(w=>w._Type=="action").Select(s => new
            {
                Id = s.Id,
                Name = s.Name,
                DisplayText = s.DisplayText,
                _Type = s._Type,
                ParentId = s.MenuItems.Select(t=>t.ParentId).FirstOrDefault(),
                s.Controller,
                Allow = s.Privlidges.Where(w => w.RoleId == id && w.Allow).Any()
            });
            if (dm.Search != null)
            {
                DataOperations op = new DataOperations();
                privilidges = op.PerformSearching(privilidges, dm.Search);
            }
            
            return Json(new { result = privilidges, count = await privilidges.CountAsync() });
        }

        [HttpPost]
        public async Task<JsonResult> Privilidges(string id, List<PrivlidgeViewModel> changed)
        {
            List<int> parents = new List<int>();
            foreach (var m in changed)
            {
                var exist = await db.Privlidges.FirstOrDefaultAsync(a => a.RoleId == id & a.ActionId == m.Id);
                if (exist == null & m.Allow)
                {
                    exist = new Privlidge();
                    exist.RoleId = id;
                    exist.ActionId = m.Id;
                    exist.Allow = true;
                    db.Privlidges.Add(exist);
                }
                else if (exist != null)
                {
                    if (m.Allow)
                    {
                        exist.Allow = true;
                    }
                    else
                    {
                        db.Privlidges.Remove(exist);
                    }
                }
                if (!parents.Any(p => p == m.ParentId))
                    parents.Add(m.ParentId);
            }
            try
            {
                await db.SaveChangesAsync();
                foreach(var v in parents)
                {
                    var parentAction =await db.MenuItems.FindAsync(v);
                    if (parentAction != null)
                    {
                        var anyActiveChild =await db.Privlidges.AnyAsync(w => w.Action.MenuItems.Where(m => m.ParentId == v).Any() & w.Allow & w.RoleId == id);
                        var parentPrivilidgeExist =await db.Privlidges.FirstOrDefaultAsync(w => w.RoleId == id && w.ActionId==parentAction.Id); 
                        if(anyActiveChild)
                        {
                            if (parentPrivilidgeExist!=null)
                                parentPrivilidgeExist.Allow = true;
                            else
                            {
                                db.Privlidges.Add(new Privlidge { Allow = true, RoleId = id, ActionId = parentAction.ActionId });
                            }
                        }
                        else
                        {
                            db.Privlidges.Remove(parentPrivilidgeExist);
                        }
                        await db.SaveChangesAsync();
                    }
                }
                return Json(new { status = true, message = "Prvilidges saved successfully." });
            }
            catch
            {
                return Json(new { status = false, message = "Unable to save privilidges." });

            }
        }


        
        public ActionResult UnAuthorize()
        {
            return View();
        }
    }
}
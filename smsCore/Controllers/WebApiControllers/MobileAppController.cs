using Microsoft.AspNetCore.Identity;
using Models;
using SchoolManagementSystem.Helpers;
using smsCore;
using smsCore.Controllers;
using smsCore.Data;
using smsCore.Data.Helpers;
using System.Data;
using System.Data.Entity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Utilities;

namespace sms.WebApiControllers
{
    [Route("api")]
    [Authorize]
    public class MobileController : BaseController
    {
        private readonly SchoolEntities db;
        private readonly CurrentUser _user;
        private readonly FCMHelper fcmhelper;
        private readonly RoleManager<IdentityRole> role;
        private readonly StaticResources staticResources;
        private readonly AttendanceHelper attendanceHelper;
        private readonly ClsBussinessSetting clsBussinessSetting;
        private readonly IWebHostEnvironment env;
        private readonly UserManager<ApplicationUser> _userManager;

        //private readonly RoleManager<IdentityRole> _roleManager;
        //public ApplicationUserManager UserManager
        //{
        //    get
        //    {
        //        return _userManager ?? HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
        //    }
        //    private set
        //    {
        //        _userManager = value;
        //    }
        //}

        public MobileController(SchoolEntities _db, CurrentUser user, FCMHelper _fcmhelper, RoleManager<IdentityRole> _role, StaticResources _staticResources, AttendanceHelper _attendanceHelper, ClsBussinessSetting _clsBussinessSetting, IWebHostEnvironment _env, UserManager<ApplicationUser> userManager)
        {
            db = _db;
            _user = user;
            fcmhelper = _fcmhelper;
            role = _role;
            staticResources = _staticResources;
            attendanceHelper = _attendanceHelper;
            clsBussinessSetting = _clsBussinessSetting;
            env = _env;
            _userManager = userManager;

            // _roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
        }

        //[HttpPost]
        //[Route("account/auth")]
        //[AllowAnonymous]
        //public async Task<GenericResponseModel<AuthenticationResult>> Authenticate([FromBody]BaseQueryModel<LoginModel> model)
        //{
        //    var response = new GenericResponseModel<AuthenticationResult>();
        //    string user = model.Data.Email;
        //    string password = model.Data.Password;
        //    if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(password))
        //    {
        //        response.ErrorList.Add("User name or password is empty.");
        //        return response;
        //    }

        //    var userIdentity = await _userManager.FindAsync(user, password);


        //    if (userIdentity != null)
        //    {
        //        var identity = new ClaimsIdentity(Startup.OAuthBearerOptions.AuthenticationType);


        //        identity.AddClaim(new Claim(ClaimTypes.Name, user));
        //        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userIdentity.Id));
        //        AuthenticationResult result = new AuthenticationResult();
        //        string userId = userIdentity.Id;
        //        var roles = userIdentity.Roles.Select(s => s.RoleId);
        //        var r = db.Roles.Where(w => roles.Contains(w.Id)).Select(s => s.Name).ToArray();// string.Join(",", db.Roles.Where(w => roles.Contains(w.Id)).Select(s => s.Name));
        //        var helperUser = new LoggedInUserHelper(userId);
        //        if(helperUser.BasicUserType!=EnumManager.BasicUserType.Parent && helperUser.BasicUserType!=EnumManager.BasicUserType.Student)
        //        {
        //            response.ErrorList.Add("You are not authorized to login.");
        //            return response;
        //        }

        //        identity.AddClaim(new Claim("BasicUserType", helperUser.BasicUserType.ToString()));
        //        identity.AddClaim(new Claim("CampusIds", string.Join(",", helperUser.CampusId)));
        //        identity.AddClaim(new Claim("ProfilePic", helperUser.ProfilePic));
        //        identity.AddClaim(new Claim("FullName", helperUser.FullName));
        //        if (helperUser.Email != null) identity.AddClaim(new Claim("Email", helperUser.Email));
        //        else identity.AddClaim(new Claim("Email", helperUser.FullName));

        //        identity.AddClaim(new Claim("primaryId", helperUser.primaryId.ToString()));

        //        var userinfo = new UserInfo
        //        {
        //            Email = userIdentity.Email,
        //            FullName = helperUser.FullName,
        //            ProfilePicture = helperUser.ProfilePic,
        //            UserId = identity.GetUserId(),
        //            UserName = user,
        //            UserType = helperUser.BasicUserType.ToString(),
        //            Roles = r
        //        };

        //        result.UserInfo=userinfo;
        //        result.Students = GetStudentByParent(helperUser.BasicUserType, helperUser.CNIC, helperUser.RegNo);

        //        AuthenticationTicket ticket = new AuthenticationTicket(identity, new AuthenticationProperties());
        //        var currentUtc = new SystemClock().UtcNow;
        //        ticket.Properties.IssuedUtc = currentUtc;
        //        ticket.Properties.ExpiresUtc = currentUtc.Add(TimeSpan.FromDays(30));

        //        string AccessToken = Startup.OAuthBearerOptions.AccessTokenFormat.Protect(ticket);
        //        result.Token = AccessToken;
        //        response.Data=result;
        //        return response;
        //    }
        //    response.ErrorList.Add("User name and/or password is incorrect.");
        //    return response;
        //}

        [HttpPost]
        [Route("dashboard/appstart")]
        public async Task<GenericResponseModel<bool>> AppStart([FromBody] BaseQueryModel<AppStartModel> model)
        {
            var response = new GenericResponseModel<bool>();
            try
            {
                //var user = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                var user = User.FindFirstValue(ClaimTypes.NameIdentifier);


                var devices = db.MobileDevices.Where(w => w.UserId == user && w.DeviceTypeId == model.Data.DeviceTypeId && w.SubscriptionId == model.Data.SubscriptionId);

                MobileDevice device = devices.FirstOrDefault();
                if (device == null)
                {
                    device = new MobileDevice();
                    device.UserId = user;
                    device.DeviceTypeId = model.Data.DeviceTypeId;
                    db.MobileDevices.Add(device);
                }
                device.SubscriptionId = model.Data.SubscriptionId;
                device.LastConnected = DateTime.Now;

                await db.SaveChangesAsync();
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.ErrorList.Add(ex.Message);
                if (ex.InnerException != null)
                    response.ErrorList.Add(ex.InnerException.Message);
            }
            return response;
        }

        [HttpPost]
        [Route("dashboard/savemessage")]
        public async Task<GenericResponseModel<bool>> SaveMessage([FromBody] BaseQueryModel<MessageModel> model)
        {
            var response = new GenericResponseModel<bool>();
            try
            {

                string toId = model.Data.ToId?.Trim();
                if (string.IsNullOrEmpty(toId))
                {
                    //string role = _roleManager.FindByName("Admin").Id;
                    var userAdmin =await _userManager.GetUsersInRoleAsync("Admin");// _userManager.Users.ToList().Where(w => _userManager.IsInRole(w.Id, "Admin")).FirstOrDefault();
                    
                    if (userAdmin.FirstOrDefault() != null)
                        toId = userAdmin.FirstOrDefault().Id;
                }
                var user = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);//.Identity.GetUserId();

                MobileMessage message = new MobileMessage();
                message.EntryDate = DateTime.Now;
                message.FromId = user;
                message.ToId = toId;
                message.MessageType = model.Data.MessageType;
                message.Message = model.Data.Message;
                message.IsDeleted = false;
                message.ReadDate = DateTime.Now;
                db.MobileMessages.Add(message);
                await db.SaveChangesAsync();
                response.Data = true;
            }
            catch (Exception ex)
            {
                response.ErrorList = new List<string>();
                response.ErrorList.Add(ex.Message);
                if (ex.InnerException != null)
                    response.ErrorList.Add(ex.InnerException.Message);
                response.Data = false;
            }
            return response;
        }

        [HttpGet]
        [Route("student/messages")]
        public async Task<GenericResponseModel<IEnumerable<MessageModel>>> GetStudentComplaints(string touserid = "", int pno = 0)
        {
            int skip = 0;
            if (pno > 0)
            {
                skip = 5 * pno;
            }
            var response = new GenericResponseModel<IEnumerable<MessageModel>>();
            try
            {
                var user = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                //var data = await db.MobileMessages.Where(w => (w.FromId == user && w.ToId==touserid) || (w.FromId == touserid && w.ToId == user)).OrderByDescending(d => d.EntryDate).Select(s => new MessageModel
                var data = await db.MobileMessages.Where(w => (w.FromId == user) || (w.ToId == user)).OrderByDescending(d => d.EntryDate).Skip(skip).Take(5).Select(s => new MessageModel
                {
                    ID = s.ID,
                    EntryDate = s.EntryDate,
                    FromId = s.FromId,
                    ToId = s.ToId,
                    Message = s.Message,
                    MessageType = s.MessageType,
                    ReadDate = s.ReadDate,
                    IsDeleted = s.IsDeleted,
                    AuthorName = s.Author.UserName
                }).ToListAsync();

                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load message details.");
                return response;
            }
        }

        private string IdentityName
        {
            get { return User.Identity.Name; }
        }

        [NonAction]
        public IEnumerable<MessageViewModel> GetMessageHistory(string roomName, int pno, int skip, int take, DateTime lastTime)
        {
            IEnumerable<MessageViewModel> messageHistory = null;
            try
            {
                if (roomName == null)
                {
                    messageHistory = new List<MessageViewModel>();
                    return messageHistory;
                }

                string adminRoleId = db.Roles.Where(w => w.Name == "Admin").FirstOrDefault()?.Id;
                if (roomName != null)
                {
                    string touserid = roomName.Replace("U:", "").Trim();
                    var myId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                    //var myUserDetails = db.Users.AsNoTracking().Where(w => w.UserName == IdentityName).FirstOrDefault();
                    var query = db.MobileMessages.AsNoTracking().Where(w => w.FromId != "");

                    bool isRoom = int.TryParse(touserid, out int touserintid);
                    if (isRoom)
                        query = query.Where(w => w.ToId == roomName);
                    else
                    {
                        //var targetUser = db.Users.AsNoTracking().Where(w => w.Id == userName).FirstOrDefault();

                        query = query.Where(m => (m.FromId == myId && m.ToId == touserid) || (m.ToId == myId && m.FromId == touserid));
                    }
                    if (lastTime != DateTime.MinValue)
                    {
                        skip = 0;
                        lastTime = lastTime.ToUniversalTime().AddMilliseconds(25);
                        query = query.Where(w => w.EntryDate <= lastTime);
                    }
                    messageHistory = query
                         .OrderByDescending(m => m.EntryDate)
                         .Skip(skip)
                         .Take(take)
                         .AsEnumerable()
                         .Reverse()
                         .ToList().Select(s => new MessageViewModel
                         {
                             Avatar = "",
                             Content = s.Message,
                             From = s.Author.UserName,
                             FromUserId = s.Author.Id,
                             Timestamp = s.EntryDate.ToLocalTime(),
                             To = "",
                             //To = (s.FromId == myId && s.ToId == targetUser.Id) ? targetUser.UserName : myUserDetails.UserName,
                             IsMine = (s.FromId == myId),
                             type = (s.Message.EndsWith(".wav") || s.Message.EndsWith(".mp3") || s.Message.EndsWith(".ogg")) ? "audio" :
                             (s.Message.EndsWith(".jpg") || s.Message.EndsWith(".png") || s.Message.EndsWith(".gif") || s.Message.EndsWith(".bmp") || s.Message.EndsWith(".jpeg")) ? "image" : "text"
                         });
                }
            }
            catch //(Exception aa)
            {
                //string pause = "";
            }
            return messageHistory;// Mapper.Map<IEnumerable<Message>, IEnumerable<MessageViewModel>>(messageHistory);

        }

        [NonAction]
        public async Task<ApplicationUser> UserDetail(string userId)
        {
            return await _userManager.FindByIdAsync(userId);
        }

        [HttpGet]
        [Route("chat/user-message-hitory")]
        public async Task<GenericResponseModel<IEnumerable<MessageViewModel>>> GetMessageHistory(string touserid = "", int pno = 0, int take = 5, string lastMessageTime = "", string sort = "asc")
        {
            DateTime dt = DateTimeHelper.ConvertDate(lastMessageTime, format: "dd-MM-YYYY hh:mm tt", dontAppendCurrentTime: true);
            int skip = 0;
            if (pno > 0)
            {
                skip = 5 * pno;
            }
            var response = new GenericResponseModel<IEnumerable<MessageViewModel>>();
            try
            {

                var data = GetMessageHistory(touserid, pno, skip, take, dt);
                if (sort == "desc")
                {
                    data = data.OrderByDescending(d => d.Timestamp);
                }
                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load message details.");
                return response;
            }
        }

        [HttpGet]
        [Route("chat/user-message-group")]
        public async Task<GenericResponseModel<IEnumerable<RoomViewModel>>> GetMessageGroup(string touserid = "", int pno = 0, int take = 5)
        {
            int skip = 0;
            if (pno > 0)
            {
                skip = 5 * pno;
            }
            var response = new GenericResponseModel<IEnumerable<RoomViewModel>>();
            try
            {

                var data = GetGroups();

                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load message details.");
                return response;
            }
        }

        private class QueryData
        {
            public int Id { get; set; }
            public int RegNo { get; set; }
            public string Name { get; set; }
            public string Fname { get; set; }
            public string UserType { get; set; }
            public string Email { get; set; }
            public string Cnic { get; set; }
        }
        [NonAction]
        public async Task<IEnumerable<UserViewModel>> GetUsers(string roomName, int pno = 0, int take = 10)
        {
            //var onlineIds = conne.Select(s => s.UserId).ToArray();
            // roomname -1=all, -2=teacher, -3 = administration

            int skip = 10 * pno;

            int id = int.Parse(roomName);
            string adminRoleId = db.Roles.Where(w => w.Name == "Admin").FirstOrDefault()?.Id;

            List<UserViewModel> users = new List<UserViewModel>();
            IEnumerable<QueryData> datas = new List<QueryData>();

            if (id == -2)
            {
                //var def = db.UserDefinitions.AsNoTracking().Where(w => w.UserType == "t").ToList();

                datas = db.tbl_Employee.Where(w => w.LeavedStaffs.Count() == 0).Select(s => new QueryData
                {
                    Id = s.Id,
                    Name = s.employeeName,
                    Fname = s.employeeName,
                    UserType = "e",
                    Email = s.email
                });

            }
            else if (id == -3)
            {
                var roleNames = new string[] { "Teacher", "Parent", "Student" };
                var usersInRoles = new List<ApplicationUser>();

                foreach (var roleName in roleNames)
                {
                        var userInRole = await _userManager.GetUsersInRoleAsync(roleName);
                        usersInRoles.AddRange(userInRole);
                }

                var allusers = usersInRoles.Select(s => new UserViewModel
                {
                    Avatar = "",
                    CurrentRoom = roomName,
                    Device = "",
                    DisplayName = s.UserName,
                    UserType = "U",
                    Username = s.UserName,
                    UserId = s.Id,
                    GroupId = "",
                    IsAdmin = false
                }).ToList();
                return allusers;
            }
            else
            {
                var query = db.Admissions.AsNoTracking().Where(w => !w.IsExpell);
                if (id > 0)
                    query = query.Where(w => w.ClassSectionID == id);

                datas = query.OrderBy(o => o.Student.StudentName)
                    //.Skip(skip).Take(take)
                    .Select(s => new QueryData
                    {

                        Name = s.Student.StudentName,
                        Fname = s.Student.FName,
                        Id = s.StudentID,
                        UserType = "s",
                        Email = s.Student.Email,
                        Cnic = s.Student.CNIC,
                        RegNo = s.Student.RegistrationNo
                    }).ToList();

            }
            var def = db.UserDefinitions.AsNoTracking().Where(w => w.UserType != "C").ToList();
            var parents = new List<QueryData>();
            if (datas.Where(w => w.UserType == "s").Count() > 0)
            {

                foreach (var s in datas.OrderBy(o => o.Id))
                {
                    if (s.UserType == "s")
                    {
                        if (s.Cnic == string.Empty || parents.Where(w => w.Cnic == s.Cnic).Count() == 0)
                        {
                            parents.Add(new QueryData
                            {
                                UserType = "p",
                                Cnic = s.Cnic,
                                Email = s.Email,
                                Fname = s.Fname,
                                Name = s.Name,
                                RegNo = s.RegNo,
                                Id = s.Id
                            });
                        }
                    }
                }
            }

            datas = datas.Union(parents);
            foreach (var s in datas)
            {
                var user = def.FirstOrDefault(w => w.UserType == s.UserType && w.FK == s.Id);
                if (user == null)
                    user = new UserDefinition { UserType = s.UserType, ID = -1 };

                EnumManager.Imagetype img = EnumManager.Imagetype.Logo;

                if (user.UserType == "s") img = EnumManager.Imagetype.Students;
                else if (user.UserType == "p") img = EnumManager.Imagetype.Father;
                else if (user.UserType == "e") img = EnumManager.Imagetype.Employee;

                UserViewModel model = new UserViewModel();
                model.Status = "";
                model.Avatar = staticResources.GetImageUrl(s.Id.ToString(), img);
                model.DisplayName = user.UserType == "s" ? s.Name : s.Fname;
                model.Username = user.UserType == "s" ? s.Name : s.Fname;
                model.UserId = user.UserId;
                model.IsAdmin = false;
                model.UserType = user.UserType;
                model.CurrentRoom = roomName;
                model.FK = s.Id.ToString();
                model.Email = s.Email;
                model.CNIC = s.Cnic;
                users.Add(model);
            }
            return users;// _Connections.Where(u => u.CurrentRoom == roomName).ToList();
        }

        [HttpGet]
        [Route("chat/group-users")]
        public async Task<GenericResponseModel<IEnumerable<UserViewModel>>> GetUserGroup(string roomid)
        {

            var response = new GenericResponseModel<IEnumerable<UserViewModel>>();
            try
            {
                var data = await GetUsers(roomid);
                response.Data = data;
                return response;
            }
            catch (Exception ex)
            {
                response.ErrorList.Add(ex.Message);
                if (ex.InnerException != null)
                    response.ErrorList.Add(ex.InnerException.Message);
                return response;
            }
        }

        [NonAction]
        public IEnumerable<RoomViewModel> GetGroups()
        {
            //_Groups.Clear();
            var gorups = db.ClassSections.AsNoTracking().Select(s => new RoomViewModel { Id = s.ID, Count = s.Admissions.Where(w => !w.IsExpell).Count(), Name = s.Class.ClassName + "(" + s.Section.SectionName + ")" }).ToList();
            //if (User.IsInRole("Admin"))
            //{
            //    gorups = db.Groups.AsNoTracking().Select(s => new RoomViewModel { Id = s.Id, Name = s.Name });
            //}
            //foreach (var room in gorups)
            //{
            //    if (_Groups.Where(w => w.Id == room.Id).Count() == 0)
            //        _Groups.Add(room);
            //}
            gorups.Insert(0, new RoomViewModel { Id = -1, Name = "All Students", Count = db.Admissions.Where(w => !w.IsExpell).Count() });
            gorups.Insert(0, new RoomViewModel { Id = -2, Name = "Teachers", Count = db.Admissions.Where(w => !w.IsExpell).Count() });
            gorups.Insert(0, new RoomViewModel { Id = -3, Name = "Admininstration", Count = db.Admissions.Where(w => !w.IsExpell).Count() });

            return gorups.ToList();
        }

        [HttpPost]
        [Route("chat/upload-audio")]
        public async Task<GenericResponseModel<IList<string>>> UploadAudio(BaseQueryModel<List<IFormFile>> model)
        {
            GenericResponseModel<IList<string>> response = new GenericResponseModel<IList<string>>();
            try
            {

                string userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);// (ClaimTypes.NameIdentifier).Value;
                //var exte = Path.GetExtension(btnUpload.FileName);
                string fileName = "";
                int i = 1;
                List<string> paths = new List<string>();
                foreach (var f in model.Data)
                {
                    fileName = "uploads/voicenotes/" + userId + DateTime.Now.ToString("yyyyMdhhmmss") + "_" + i.ToString() + ".ogg";
                    //var filePath = HttpContext.Current.Server.MapPath(fileName);
                    string filePath = Path.Combine(env.ContentRootPath, fileName);

                    //f.SaveAs(filePath);
                    paths.Add(fileName);
                }

                response.Data = paths;
                return response;
            }
            catch (Exception ex)
            {
                response.ErrorList.Add(ex.Message);
                return response;
            }
        }

        [HttpPost]
        [Route("chat/upload-media")]
        public async Task<GenericResponseModel<IList<string>>> UploadMedia(BaseQueryModel<List<IFormFile>> model)
        {
            GenericResponseModel<IList<string>> response = new GenericResponseModel<IList<string>>();
            try
            {

                string userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);// (ClaimTypes.NameIdentifier).Value;
                //var exte = Path.GetExtension(btnUpload.FileName);
                string fileName = "";
                int i = 1;
                List<string> paths = new List<string>();
                foreach (var f in model.Data)
                {
                    var exte = Path.GetExtension(f.FileName);
                    fileName = "uploads/images/" + userId + DateTime.Now.ToString("yyyyMdhhmmss") + exte;
                    //var filePath = HttpContext.Current.Server.MapPath(fileName);
                    string filePath = Path.Combine(env.ContentRootPath, fileName);

                    //f.SaveAs(filePath);
                    paths.Add(fileName);
                }

                response.Data = paths;
                return response;
            }
            catch (Exception ex)
            {
                response.ErrorList.Add(ex.Message);
                return response;
            }
        }

        [HttpPost]
        [Route("notification/send")]
        public async Task<string> SendNotificationTest(string body, string title, string to, string type, string data = "")
        {
            var notify = new Notification()
            {
                Title = title,
                Body = body,
                NotificationType = type,
                Receipients = to,
                SentOn = DateTime.Now,
                SentByUser = User.FindFirstValue(ClaimTypes.NameIdentifier),
                ViewedBy = "",
                Data = data
            };
            db.Notifications.Add(notify);
            await db.SaveChangesAsync();

            var devices = db.MobileDevices.Where(s => s.SubscriptionId != "");
            if (!string.IsNullOrEmpty(to))
            {
                if (!int.TryParse(to, out int groupid))
                {
                    devices = devices.Where(w => w.UserId == to);
                }
                else
                {
                    var users =await GetUsers(to, 0, 1000000);
                    var ids = users.Select(s => s.UserId).ToArray();
                    devices = devices.Where(w => ids.Contains(w.UserId));
                }
            }

            string dt = "";
            var fcmids = devices.Select(s => s.SubscriptionId).ToArray();

            //foreach (var d in devices)
            if (fcmids.Count() > 0)
            {
                //dt = await helper.SendNotification(title, body, fcmids, data);
            }


            return dt;
        }

        [HttpGet]
        [Route("notification/all")]
        public async Task<GenericResponseModel<List<Notification>>> GetNotifications(int pno = 0, int take = 10)
        {
            int skip = 0;
            if (pno > 0)
            {
                skip = 5 * pno;
            }
            var response = new GenericResponseModel<List<Notification>>();
            string userid = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            LoggedInUserHelper helper = new LoggedInUserHelper(userid);
            List<string> groupIds = new List<string>();
            groupIds.Add(userid);
            if (helper.BasicUserType == EnumManager.BasicUserType.User)
            {
                groupIds.Add("-3");
            }
            else if (helper.BasicUserType == EnumManager.BasicUserType.Parent)
            {
                groupIds.Add("-1");
                foreach (var id in helper.ParentClassSections)
                {
                    groupIds.Add(id.ToString());
                }
            }
            else if (helper.BasicUserType == EnumManager.BasicUserType.Student)
            {
                groupIds.Add("-1");
                groupIds.Add(helper.ClassSectionId.ToString());
            }
            else if (helper.BasicUserType == EnumManager.BasicUserType.Employee)
            {
                groupIds.Add("-2");
            }
            var notifications = await db.Notifications.Where(w => groupIds.Contains(w.Receipients)).OrderByDescending(o => o.SentOn).Skip(skip).Take(take).ToListAsync();
            response.Data = notifications;

            return response;
        }


        [NonAction]
        public List<StudentInfo> GetStudentByParent(EnumManager.BasicUserType usertype, string cnic = "", int regno = 0)
        {
            List<StudentInfo> students = new List<StudentInfo>();
            if (usertype == EnumManager.BasicUserType.Parent)
            {
                students = db.Students.Where(w => w.CNIC == cnic).ToList().Select(s => new StudentInfo
                {
                    Id = s.ID,
                    RegNo = s.RegistrationNo,
                    StudentName = s.StudentName,
                    IsCurrent = s.Admissions.Where(w => !w.IsExpell).Any(),
                    ClassId = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.ClassID).FirstOrDefault(),
                    SectionId = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.SectionID).FirstOrDefault(),
                    ClassSectionId = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSectionID).FirstOrDefault(),
                    CampusId = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.CampusID).FirstOrDefault(),
                    ClassName = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.Class.ClassName).FirstOrDefault(),
                    SectionName = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.Section.SectionName).FirstOrDefault(),
                    CampusName = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.Campus.CampusName).FirstOrDefault(),
                    Picture = staticResources.GetImageUrl(s.ID.ToString(), EnumManager.Imagetype.Students),
                    //ClassTeacher = db.tbl_Employee.Where(w => w.Id == db.TeachingClasses.Where(v => v.ClassSectionId==s.Admissions.Where(av => !av.IsExpell).Select(a => a.ClassSectionID).FirstOrDefault()).Select(t => t.StaffID).FirstOrDefault()).Select(t => t.employeeName).FirstOrDefault()
                }).ToList();
            }
            else if (usertype == EnumManager.BasicUserType.Student)
            {

                students = db.Students.Where(w => w.RegistrationNo == regno).Select(s => new StudentInfo
                {
                    Id = s.ID,
                    RegNo = s.RegistrationNo,
                    StudentName = s.StudentName,
                    IsCurrent = s.Admissions.Where(w => !w.IsExpell).Any(),
                    ClassId = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.ClassID).FirstOrDefault(),
                    SectionId = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.SectionID).FirstOrDefault(),
                    ClassSectionId = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSectionID).FirstOrDefault(),
                    CampusId = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.CampusID).FirstOrDefault(),
                    ClassName = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.Class.ClassName).FirstOrDefault(),
                    SectionName = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.Section.SectionName).FirstOrDefault(),
                    CampusName = s.Admissions.Where(w => !w.IsExpell).Select(a => a.ClassSection.Campus.CampusName).FirstOrDefault(),
                    Picture = staticResources.GetImageUrl(s.ID.ToString(), EnumManager.Imagetype.Students),
                    //ClassTeacher = db.tbl_Employee.Where(w => w.Id == db.TeachingClasses.Where(v => v.ClassSectionId==s.Admissions.Where(av => !av.IsExpell).Select(a => a.ClassSectionID).FirstOrDefault()).Select(t => t.StaffID).FirstOrDefault()).Select(t => t.employeeName).FirstOrDefault()
                }).ToList();
            }

            return students;
        }

        [HttpGet]
        [Route("dashboard/fee")]
        public GenericResponseModel<FeeInfo> GetFeeForDashboard(int regno)
        {

            var response = new GenericResponseModel<FeeInfo>();

            var Receiveable = db.FeeSlips.Where(w => w.Admission.Student.RegistrationNo == regno && (w.FeeSlipReceipts.Count() == 0 || w.FeeSlipDetails.Select(t => t.Amount).DefaultIfEmpty(0).Sum() - w.FeeSlipReceipts.Select(t => t.Amount).DefaultIfEmpty(0).Sum() > 0)).Select(s =>
            s.FeeSlipDetails.Select(t => t.Amount).DefaultIfEmpty(0).Sum() - s.FeeSlipReceipts.Select(t => t.Amount).DefaultIfEmpty(0).Sum()
            ).DefaultIfEmpty(0).Sum();

            var LastPayment = db.FeeSlipReceipts.Where(w => w.FeeSlip.Admission.Student.RegistrationNo == regno).Select(s => new { s.Amount, Date = DbFunctions.TruncateTime(s.EntryDate) }).GroupBy(g => g.Date).Select(s => new
            {
                Date = s.Key,
                Amount = s.Sum(t => t.Amount)
            }).OrderByDescending(o => o.Date).FirstOrDefault();

            var month = DateTime.Now.Month;
            var year = DateTime.Now.Year;
            var Fee = db.FeeSlips.Where(w => w.Admission.Student.RegistrationNo == regno && w.ForMonth.Month == month && w.ForMonth.Year == year).Select(s => new
            {
                Amount = s.FeeSlipDetails.Sum(t => t.Amount),
                Status = s.FeeSlipDetails.Select(t => t.Amount).DefaultIfEmpty(0).Sum() - s.FeeSlipReceipts.Select(t => t.Amount).DefaultIfEmpty(0).Sum() > 0
            }).FirstOrDefault();
            response.Data = new FeeInfo
            {
                CurrentMonthAmount = Fee == null ? 0 : Fee.Amount,
                CurrentMonthStatus = Fee == null ? "" : (Fee.Status ? "Not Paid" : "Paid"),
                LasPayment = LastPayment == null ? 0 : LastPayment.Amount,
                LastPaymentDate = (LastPayment == null || !LastPayment.Date.HasValue) ? "" : LastPayment.Date.Value.ToString("dd, MMM yyyy"),
                Receiveable = Receiveable
            };

            return response;
        }

        [HttpGet]
        [Route("dashboard/message-subscription")]
        public GenericResponseModel<MessageSubscription> MessageSubscriptionDashboard(int stdid)
        {
            var response = new GenericResponseModel<MessageSubscription>();
            var subs = db.ComSubs.Where(w => w.StudentID == stdid).Select(s => new MessageSubscription
            {
                Attendance = s.Attendance,
                IsMessageActive = true,
                IssuedFee = s.IssuedFee,
                ReceiveFee = s.ReceiveFee,
                Result = s.Result,
            }).DefaultIfEmpty().FirstOrDefault();
            if (subs == null)
                subs = new MessageSubscription();

            var campusId = db.Admissions.Where(w => w.StudentID == stdid && !w.IsExpell).Select(s => new { s.CampuseID }).FirstOrDefault();
            if (campusId == null)
            {
                subs.AttendanceCharges = "0";
                subs.IssuedFeeCharges = "0";
                subs.ResultCharges = "0";
                subs.ReceiveFeeCharges = "0";
                subs.IsMessageActive = false;
            }
            else
            {
                //var setting.Campus = clsBussinessSetting.CampusId;
                clsBussinessSetting.CampusId = campusId.CampuseID;
                var settings = clsBussinessSetting.ReadAll();

                subs.AttendanceCharges = settings.Where(w => w.PropertyName == "AttendanceCharges").Select(s => s.PropertyValue).FirstOrDefault() ?? "0";
                subs.IssuedFeeCharges = settings.Where(w => w.PropertyName == "FeeIssuedCharges").Select(s => s.PropertyValue).FirstOrDefault() ?? "0";
                subs.ResultCharges = settings.Where(w => w.PropertyName == "ResultCharges").Select(s => s.PropertyValue).FirstOrDefault() ?? "0";
                subs.ReceiveFeeCharges = settings.Where(w => w.PropertyName == "FeeReceivedCharges").Select(s => s.PropertyValue).FirstOrDefault() ?? "0";
            }
            response.Data = subs;
            return response;
        }

        [HttpGet]
        [Route("fee/all")]
        public async Task<GenericResponseModel<List<FeeResponse>>> GetStudentFee(int studentId)
        {
            var response = new GenericResponseModel<List<FeeResponse>>();
            try
            {
                var data = (await db.FeeSlips.Where(w => w.Admission.StudentID == studentId).Select(s => new
                {
                    s.ForMonth,
                    Total = s.FeeSlipDetails.Sum(f => f.Amount),
                    Received = s.FeeSlipReceipts.Select(f => f.Amount).DefaultIfEmpty(0).Sum(),
                    ReceivedDate = s.FeeSlipReceipts.Select(f => f.EntryDate).DefaultIfEmpty().FirstOrDefault()
                }).OrderByDescending(o => o.ForMonth).ToListAsync()).Select(s => new FeeResponse
                {
                    ForMonth = s.ForMonth.ToString("MMM, yyyy"),
                    Total = s.Total,
                    Received = s.Received,
                    ReceivedDate = s.ReceivedDate == DateTime.MinValue ? string.Empty : s.ReceivedDate.ToString("dd/MM/yyyy"),
                    Balance = s.Total - s.Received
                }).ToList();
                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load fee details.");
                return response;
            }
        }

        [HttpGet]
        [Route("exam/all")]
        public GenericResponseModel<IEnumerable<ResultResponse>> GetStudentResult(int studentId, int session)
        {
            var response = new GenericResponseModel<IEnumerable<ResultResponse>>();
            try
            {
                var rs = new ResultSystem();
                var data = rs.GetResultsbyStudent(studentId, session);
                var d2 = data.GroupBy(g => g.ExamHeldID).Select(s => new ResultResponse
                {
                    ExamName2 = s.FirstOrDefault().ExamName2,
                    GrantTotal = s.FirstOrDefault().GrantTotal,
                    TotalObtained = s.FirstOrDefault().TotalObtained,
                    TotalPercentage = s.FirstOrDefault().TotalPercentage,
                    TotalStatus = s.FirstOrDefault().TotalStatus,
                    Results = s.Select(v => new SubjectResult
                    {
                        SubjectName = v.SubjectName,
                        ObtainedMarks = v.ObtainedMarks,
                        Percentage = v.Percentage,
                        TotalMarks = v.TotalMarks,
                        Status = v.Status
                    })
                });
                response.Data = d2;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load result details.");
                return response;
            }
        }

        [HttpGet]
        [Route("dashboard/attendance")]
        public async Task<GenericResponseModel<AttendanceResponse>> Attendance(int stdid)
        {
            var today = DateTime.Today.Date;
            var response = new GenericResponseModel<AttendanceResponse>();

            var campusId = db.Admissions.Where(w => w.StudentID == stdid && !w.IsExpell).Select(s => new { s.ID, s.CampuseID }).FirstOrDefault();


            var month = 0;// attendanceHelper.GetStudentAttendanceTotalInMonth(DateTime.Today, campusId.ID);
            var year = 0;// attendanceHelper.GetStudentAttendanceTotalInSession(0, campusId.ID);
            var totaldaysinmonth = 0;// attendanceHelper.GetTotalWorkingDaysInMonth(campusId.CampuseID, DateTime.Today);
            var totaldaysinyear = 0;// attendanceHelper.GetTotalWorkingDaysInSession(campusId.CampuseID);
            var todayHoliday = await db.SchoolLeaveSchedules.Where(w => w.IsHoliday && w.date == today).FirstOrDefaultAsync();
            bool isHoliday = false;
            string description = "";
            if (todayHoliday != null)
            {
                isHoliday = true;
                description = todayHoliday.holidayName;
            }
            else
            {
                var att = await db.StudentAttendences.Where(w => DbFunctions.TruncateTime(w.AttendanceDate) == today).Select(s => new { s.StudentAttendanceType.AttendanceName }).FirstOrDefaultAsync();
                if (att == null)
                {
                    description = "NA";
                }
                else
                {
                    description = att.AttendanceName;
                }
            }

            AttendanceResponse data = new AttendanceResponse
            {
                IsTodayHoliday = isHoliday,
                TodayStatus = description,

                //MonthPresent = (month.Where(w => w.Key == "Present").FirstOrDefault().Value + month.Where(w => w.Key == "Present").FirstOrDefault().Value),
                //MonthAbsent = month.Where(w => w.Key == "Absent").FirstOrDefault().Value.ToString(),
                //MonthLeave = month.Where(w => w.Key == "Leave").FirstOrDefault().Value.ToString(),
                //TotalWorkingDaysMonth = totaldaysinmonth,

                //TotalWorkingDaysYear = totaldaysinyear,
                //YearPresent = (month.Where(w => w.Key == "Present").FirstOrDefault().Value + month.Where(w => w.Key == "Present").FirstOrDefault().Value),
                //YearAbsent = month.Where(w => w.Key == "Absent").FirstOrDefault().Value.ToString(),
                //YearLeave = month.Where(w => w.Key == "Leave").FirstOrDefault().Value.ToString(),

            };
            response.Data = data;
            return response;
        }

        [HttpGet]
        [Route("student/profile")]
        public async Task<GenericResponseModel<StudentProfileResponse>> StudentProfile(int stdid)
        {
            var today = DateTime.Today.Date;
            var response = new GenericResponseModel<StudentProfileResponse>();

            var admission = db.Admissions.Where(w => w.StudentID == stdid && !w.IsExpell).Select(s => new
            {
                s.ID,
                s.CampuseID,
                s.ClassSection.Class.ClassName,
                s.ClassSection.Section.SectionName,
                s.Campus.CampusName
            }).FirstOrDefault();

            var data = (await db.Students.Where(w => w.ID == stdid).ToListAsync()).Select(s => new StudentProfileResponse
            {

                AdmissionDate = s.AdmissionDate,
                AdmittedClass = s.AdmittedClass,
                ApplyLateFee = s.ApplyLateFee,
                CampusName = admission?.CampusName,
                ClassName = admission?.ClassName,
                CNIC = s.CNIC,
                DOB = s.DOB,
                DateForSubmission = s.DateForSubmission,
                Domicile = s.Domicile,
                Email = s.Email,
                FatherAlive = s.FatherAlive,
                FatherDepartment = s.FatherDepartment,
                FatherNatureOfJob = s.FatherNatureOfJob,
                FatherProfession = s.FatherProfession,
                FatherQualification = s.FatherQualification,
                FName = s.FName,
                Games = s.Games,
                GeneralRemarks = s.GeneralRemarks,
                GuardianName = s.GuardianName,
                IsCurrentStudent = admission != null,
                LastIntitution = s.LastIntitution,
                LiveWith = s.LiveWith,
                MissingDocuments = s.MissingDocuments,
                MotherName = s.MotherName,
                MotherProfession = s.MotherProfession,
                MotherQualification = s.MotherQualification,
                MotherTongue = s.MotherTongue,
                Nationality = s.Nationality,
                OfficeTelephone = s.OfficeTelephone,
                PermenantAddress = s.PermenantAddress,
                PostalAddress = s.PostalAddress,
                RegistrationNo = s.RegistrationNo,
                Religion = s.Religion,
                Remarks = s.Remarks,
                ResidanceTelephone = s.ResidanceTelephone,
                SectionName = admission?.SectionName,
                Sex = s.Sex,
                StdRelation = s.StdRelation,
                StudentCNIC = s.StudentCNIC,
                StudentName = s.StudentName,

                Mobiles = s.StudentMobiles.Select(m => new StudentMobileResponse { ID = m.ID, IsDefault = m.IsDefault, MobileHolder = m.MobileHolder, MobileNo = m.MobileNo, Relation = m.Relation, StudentID = m.StudentID }).ToList()

            }).FirstOrDefault();
            response.Data = data;
            return response;
        }

        [HttpGet]
        [Route("exam/datesheet")]
        public async Task<GenericResponseModel<IEnumerable<DateSheetResponse>>> GetStudentDatesheet(int classSectionId)
        {
            var response = new GenericResponseModel<IEnumerable<DateSheetResponse>>();
            try
            {
                var exam = db.ExamHelds.Where(w => w.ExamDates.Where(v => v.ClassSectionID == classSectionId).Any()).OrderByDescending(d => d.EntryDate).Select(s => new { s.ID, s.EntryDate, s.Exam.ExamName }).FirstOrDefault();
                var data = await db.ExamDates.Where(w => w.ExamHeldID == exam.ID).Select(s => new DateSheetResponse
                {
                    SubjectName = s.Subject.SubjectName,
                    ExamDate = s.ExamDate1,
                    ExamName = exam.ExamName + " " + exam.EntryDate.ToString("MMMM, yyyy"),
                    TimeFrom = s.TimeFrom,
                    TimeTo = s.ToTime
                }).ToListAsync();

                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load result details.");
                return response;
            }
        }


        [HttpGet]
        [Route("student/timetable")]
        public async Task<GenericResponseModel<IEnumerable<TimeTableResponse>>> GetStudentTimeTable(int classSectionId)
        {
            var response = new GenericResponseModel<IEnumerable<TimeTableResponse>>();
            try
            {
                var cls = db.ClassSections.Where(w => w.ID == classSectionId).Select(s => new { s.ClassID, s.SectionID, s.CampusID }).FirstOrDefault();
                var data = await db.TimeTables.Where(w => w.ClassID == cls.ClassID && w.SectionID == cls.SectionID && w.CampusID == cls.CampusID).Select(s => new TimeTableResponse
                {
                    SubjectName = s.SubjectName,
                    Pno = s.Pno,
                    TeacherName = s.EmployeeName,
                    TimeFrom = s.TimeFrom,
                    TimeTo = s.TimeTo

                }).ToListAsync();

                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load result details.");
                return response;
            }
        }


        [HttpGet]
        [Route("student/complaints")]
        public async Task<GenericResponseModel<IEnumerable<StudentComplaignResponse>>> GetStudentComplaints(int stdid)
        {
            var response = new GenericResponseModel<IEnumerable<StudentComplaignResponse>>();
            try
            {
                var data = await db.StudentComplaigns.Where(w => w.StudentID == stdid).OrderByDescending(d => d.EntryDate).Select(s => new StudentComplaignResponse
                {
                    Ctype = s.Ctype,
                    Particular = s.Particular,
                    EntryDate = s.EntryDate,
                    EmployeeId = s.EmployeeId,
                    ID = s.ID
                }).ToListAsync();

                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load result details.");
                return response;
            }
        }



        [HttpGet]
        [Route("school/schedule")]
        public async Task<GenericResponseModel<IEnumerable<SheduleResponse>>> GetSchedule(int campusId, int year)
        {
            var response = new GenericResponseModel<IEnumerable<SheduleResponse>>();
            try
            {
                var data = await db.SchoolLeaveSchedules.Where(w => w.date.Year == year && w.CampusID == campusId).Select(s => new SheduleResponse { IsHoliday = s.IsHoliday, Color = s.Color, Date = s.date, Description = s.holidayName }).ToListAsync();
                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load result details.");
                return response;
            }
        }


        [HttpGet]
        [Route("school/classTeacher")]
        public async Task<GenericResponseModel<ClassTeacherResponse>> GetClassTeacher(int classSectionId)
        {
            var response = new GenericResponseModel<ClassTeacherResponse>();
            try
            {
                var emp = (await db.TeachingClasses.Where(w => w.ClassSectionId == classSectionId && w.IsActive).Select(s => s.StaffID).ToListAsync()).FirstOrDefault();
                var data = (await db.tbl_Employee.Where(w => w.Id == emp).ToListAsync()).Select(s => new ClassTeacherResponse
                {
                    Name = s.employeeName,
                    Picture = staticResources.GetImageUrl(s.Id.ToString(), EnumManager.Imagetype.Employee),
                    Qualification = s.qualification
                }).FirstOrDefault();
                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load result details.");
                return response;
            }
        }

        [HttpGet]
        [Route("school/subjectTeacher")]
        public async Task<GenericResponseModel<IEnumerable<ClassSubjectResponse>>> GetSubjectTeacher(int classSectionId)
        {
            var response = new GenericResponseModel<IEnumerable<ClassSubjectResponse>>();
            try
            {
                var data = (await db.TeachingSubjects.Where(w => w.ClassSectionID == classSectionId).ToListAsync()).Select(s => new ClassSubjectResponse
                {
                    SubjectName = s.Subject.SubjectName,
                    Name = s.tbl_Employee.employeeName,
                    Picture = staticResources.GetImageUrl(s.tbl_Employee.Id.ToString(), EnumManager.Imagetype.Employee),
                    Qualification = s.tbl_Employee.qualification
                });
                response.Data = data;
                return response;
            }
            catch
            {
                response.ErrorList.Add("Failed to load result details.");
                return response;
            }
        }
        //[HttpPost]
        //[ActionName("reset")]
        //[Authorize]
        //public string ChangePassword(string OldPassword, string NewPassword)
        //{
        //    var result = _userManager.ChangePassword(UserHelper.UserId, OldPassword, NewPassword);

        //    if (result.Succeeded)
        //    {
        //        return "Password changed successfully";
        //    }
        //    return result.Errors.FirstOrDefault();

        //}

        [HttpGet]
        [Route("account/validate-token")]
        public string ValidateToken()
        {
            var user = this.User.Identity;
            if (user != null)
                return string.Format("{0} - {1}", User.FindFirstValue(ClaimTypes.NameIdentifier), User.FindFirstValue(ClaimTypes.Name));
            else
                return "Unable to resolve user id";

        }

        [AllowAnonymous]
        [HttpGet]
        [Route("dashboard/applandingsetting")]
        public async Task<GenericResponseModel<AppConfigurationModel>> AppLandingSetting()
        {
            var response = new GenericResponseModel<AppConfigurationModel>();

            var info = await db.Company.FirstOrDefaultAsync();
            var model = new AppConfigurationModel
            {
                SchoolName = info.companyName,
                Logo = staticResources.GetImageUrl("", EnumManager.Imagetype.Logo),
                PrimaryThemeColor = "",// _webApiSettings.PrimaryThemeColor,
                BottomBarActiveColor = "",// _webApiSettings.BottomBarActiveColor,
                BottomBarInactiveColor = "",// _webApiSettings.BottomBarInactiveColor,
                BottomBarBackgroundColor = "",// _webApiSettings.BottomBarBackgroundColor,
                TopBarTextColor = "",// _webApiSettings.TopBarTextColor,
                TopBarBackgroundColor = "",// _webApiSettings.TopBarBackgroundColor,
                GradientStartingColor = "",// _webApiSettings.GradientStartingColor,
                GradientMiddleColor = "",// _webApiSettings.GradientMiddleColor,
                GradientEndingColor = "",// _webApiSettings.GradientEndingColor,
                GradientEnabled = true,// _webApiSettings.GradientEnabled,
                ShowHomepageSlider = true,// _webApiSettings.ShowHomepageSlider,
                SliderAutoPlay = true,// _webApiSettings.SliderAutoPlay,
                SliderAutoPlayTimeout = 300,// _webApiSettings.SliderAutoPlayTimeout,
                AndriodForceUpdate = true,// _webApiSettings.AndriodForceUpdate,
                AndroidVersion = "",// _webApiSettings.AndroidVersion,
                PlayStoreUrl = "",// _webApiSettings.PlayStoreUrl,
                IOSForceUpdate = true,// _webApiSettings.IOSForceUpdate,
                IOSVersion = "",// _webApiSettings.IOSVersion,
                AppStoreUrl = "",// _webApiSettings.AppStoreUrl,
                LogoUrl = "",// await _pictureService.GetPictureUrlAsync(_webApiSettings.LogoId, _webApiSettings.LogoSize),
            };


            response.Data = model;

            return (response);
        }



    }

    public class ClassSubjectResponse
    {
        public string SubjectName { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Qualification { get; set; }
    }
    public class ClassTeacherResponse
    {
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Qualification { get; set; }
    }
    public class SheduleResponse
    {
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public bool IsHoliday { get; set; }
        public string Color { get; set; }
    }
    public class StudentComplaignResponse
    {
        public int ID { get; set; }
        public int StudentID { get; set; }
        public System.DateTime EntryDate { get; set; }
        public string Particular { get; set; }
        public string Ctype { get; set; }
        public decimal EmployeeId { get; set; }
    }
    public class TimeTableResponse
    {
        public string SubjectName { get; set; }
        public int Pno { get; set; }
        public DateTime TimeFrom { get; set; }
        public DateTime TimeTo { get; set; }
        public string TeacherName { get; set; }
    }
    public class DateSheetResponse
    {
        public string SubjectName { get; set; }
        public DateTime ExamDate { get; set; }
        public string TimeFrom { get; set; }
        public string TimeTo { get; set; }
        public string ExamName { get; set; }
    }
    public class StudentProfileResponse
    {
        public bool IsCurrentStudent { get; set; }
        public string CampusName { get; set; }
        public string ClassName { get; set; }
        public string SectionName { get; set; }

        public string StudentName { get; set; }
        public string FName { get; set; }
        public string GuardianName { get; set; }
        public string StdRelation { get; set; }
        public string Email { get; set; }
        public string Religion { get; set; }
        public string Nationality { get; set; }
        public string MotherTongue { get; set; }
        public string LastIntitution { get; set; }
        public string Games { get; set; }
        public System.DateTime DOB { get; set; }
        public string PostalAddress { get; set; }
        public string OfficeTelephone { get; set; }
        public string PermenantAddress { get; set; }
        public string FatherQualification { get; set; }
        public string FatherProfession { get; set; }
        public string FatherNatureOfJob { get; set; }
        public string FatherDepartment { get; set; }
        public string MotherName { get; set; }
        public string MotherQualification { get; set; }
        public string MotherProfession { get; set; }
        public string MissingDocuments { get; set; }
        public string CNIC { get; set; }
        public int RegistrationNo { get; set; }
        public string Sex { get; set; }
        public string ResidanceTelephone { get; set; }
        public string Remarks { get; set; }
        public string GeneralRemarks { get; set; }
        public string DateForSubmission { get; set; }
        public string FatherAlive { get; set; }
        public string LiveWith { get; set; }
        public string Domicile { get; set; }
        public System.DateTime AdmissionDate { get; set; }
        public string AdmittedClass { get; set; }
        public Nullable<int> ApplyLateFee { get; set; }
        public string StudentCNIC { get; set; }

        public List<StudentMobileResponse> Mobiles { get; set; }

    }

    public partial class StudentMobileResponse
    {
        public int ID { get; set; }
        public bool IsDefault { get; set; }
        public string MobileHolder { get; set; }
        public string MobileNo { get; set; }
        public string Relation { get; set; }
        public int StudentID { get; set; }

    }
    public class AttendanceResponse
    {
        public bool IsTodayHoliday { get; set; }

        public string TodayStatus { get; set; }
        public decimal MonthPresent { get; set; }
        public string MonthAbsent { get; set; }
        public string MonthLeave { get; set; }
        public decimal MonthPercnet
        {
            get
            {
                return (MonthPresent / TotalWorkingDaysMonth) * 100;

            }
        }

        public decimal YearPresent { get; set; }
        public string YearAbsent { get; set; }
        public string YearLeave { get; set; }
        public decimal YearPercnet
        {
            get
            {
                return (YearPresent / TotalWorkingDaysYear) * 100;

            }
        }

        public decimal TotalWorkingDaysMonth { get; set; }
        public decimal TotalWorkingDaysYear { get; set; }

    }


    public class ResultResponse
    {
        public string ExamName2 { get; set; }
        public double GrantTotal { get; set; }
        public double TotalObtained { get; set; }
        public double TotalPercentage { get; set; }
        public string TotalStatus { get; set; }
        public IEnumerable<SubjectResult> Results { get; set; }

    }
    public class SubjectResult
    {
        public string SubjectName { get; set; }
        public string ObtainedMarks { get; set; }
        public double Percentage { get; set; }
        public double TotalMarks { get; set; }
        public string Status { get; set; }
    }
    public class FeeResponse
    {
        public string ForMonth { get; set; }
        public decimal Total { get; set; }
        public decimal Received { get; set; }
        public string ReceivedDate { get; set; }
        public decimal Balance { get; set; }
    }
    public class FeeInfo
    {
        public decimal Receiveable { get; set; }
        public decimal LasPayment { get; set; }
        public string LastPaymentDate { get; set; }

        public decimal CurrentMonthAmount { get; set; }
        public string CurrentMonthStatus { get; set; }
    }

    public class MessageSubscription
    {
        public int ReceiveFee { get; set; }
        public int IssuedFee { get; set; }
        public int Result { get; set; }

        public int Attendance { get; set; }
        public bool IsMessageActive { get; set; }

        public string ReceiveFeeCharges { get; set; }
        public string IssuedFeeCharges { get; set; }
        public string ResultCharges { get; set; }
        public string AttendanceCharges { get; set; }
    }
    public class BaseQueryModel<TModel>
    {
        public BaseQueryModel()
        {
        }

        public TModel Data { get; set; }
        public List<KeyValueApi> FormValues { get; set; }
        public PictureQueryModel UploadPicture { get; set; }
    }

    public class KeyValueApi
    {
        public KeyValueApi() { }

        public string Key { get; set; }
        public string Value { get; set; }
    }

    public class PictureQueryModel
    {
        public PictureQueryModel() { }

        public string Base64Image { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public int LengthInBytes { get; set; }
    }

    public class LoginModel
    {

        public string Email { get; set; }
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
    public class GenericResponseModel<TResult>
    {
        public GenericResponseModel()
        {
            ErrorList = new List<string>();
        }
        public string Message { get; set; }

        public List<string> ErrorList { get; set; }
        public TResult Data { get; set; }
    }
    public class UserViewModel
    {
        public string FK { get; set; }
        public string UserType { get; set; }
        public String GroupId { get; set; }
        public string UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string Avatar { get; set; }
        public string CurrentRoom { get; set; }
        public string Device { get; set; }

        public bool IsAdmin { get; set; }
        public string Status { get; set; }
        public string Email { get; set; }
        public string CNIC { get; set; }
    }
    public class RoomViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
    public class MessageViewModel
    {
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
        public string From { get; set; }
        public string FromUserId { get; set; }
        public string To { get; set; }
        public string Avatar { get; set; }
        public bool IsMine { get; set; }
        public bool IsRoom { get; set; }
        public string type { get; set; }
    }
    public class AppConfigurationModel
    {
        public AppConfigurationModel()
        {
        }
        public string SchoolName { get; set; }
        public string Logo { get; set; }
        public bool ShowHomepageSlider { get; set; }
        public bool SliderAutoPlay { get; set; }
        public int SliderAutoPlayTimeout { get; set; }
        public string AndroidVersion { get; set; }
        public bool AndriodForceUpdate { get; set; }
        public string PlayStoreUrl { get; set; }
        public string IOSVersion { get; set; }
        public bool IOSForceUpdate { get; set; }
        public string AppStoreUrl { get; set; }
        public string LogoUrl { get; set; }
        public string PrimaryThemeColor { get; set; }
        public string TopBarBackgroundColor { get; set; }
        public string TopBarTextColor { get; set; }
        public string BottomBarBackgroundColor { get; set; }
        public string BottomBarActiveColor { get; set; }
        public string BottomBarInactiveColor { get; set; }
        public string GradientStartingColor { get; set; }
        public string GradientMiddleColor { get; set; }
        public string GradientEndingColor { get; set; }
        public bool GradientEnabled { get; set; }


    }
    public class AuthenticationResult
    {
        public string Token { get; set; }
        public UserInfo UserInfo { get; set; }
        public List<StudentInfo> Students { get; set; }
    }
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserType { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string ProfilePicture { get; set; }
        public string[] Roles { get; set; }
    }

    public class StudentInfo
    {
        public int RegNo { get; set; }
        public int Id { get; set; }
        public string StudentName { get; set; }
        public string Picture { get; set; }
        public bool IsCurrent { get; set; }
        public int ClassId { get; set; }
        public string ClassName { get; set; }
        public int SectionId { get; set; }
        public string SectionName { get; set; }
        public int CampusId { get; set; }
        public string CampusName { get; set; }
        public int ClassSectionId { get; set; }
        public string ClassTeacher { get; set; }
    }
    public class DistributorResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ChartData
    {
        public int SortOrder { get; set; }
        public string XValue { get; set; }

        public decimal YValue { get; set; }

        public int shops { get; set; }

        public int productive { get; set; }

        public decimal LTR { get; set; }
        public decimal CTN { get; set; }
        public decimal SaleValue { get; set; }

    }

    public class AppStartModel
    {
        public int DeviceTypeId { get; set; }
        public string SubscriptionId { get; set; }
    }

    public class MessageModel
    {
        public int ID { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        public bool IsMine { get; set; }
        public string Message { get; set; }
        public DateTime EntryDate { get; set; }
        public DateTime ReadDate { get; set; }
        public string MessageType { get; set; }
        public bool IsDeleted { get; set; }
        public string AuthorName { get; set; }
        public string AuthorPic { get; set; }
    }
}

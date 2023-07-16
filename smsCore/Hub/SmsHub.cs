using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Models;

namespace sms.Hubs
{
    public enum HubSubscriptions { FeeEntry }

    public class SmsHub : Hub
    {

        private UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SchoolEntities db;
        public static readonly List<Room> Rooms = new List<Room>();
        public static readonly List<ConnectedUser> connectedUsers = new List<ConnectedUser>();


        public SmsHub(UserManager<ApplicationUser> userManager, SchoolEntities _db, RoleManager<IdentityRole> roleManager)
        {
            userManager = _userManager;
            _roleManager = roleManager;
            db = _db;
            if (Rooms.Count == 0)
            {
                Rooms.Add(new Room { Id = "-1", Name = "All Students" });
                Rooms.Add(new Room { Id = "-2", Name = "Teachers" });
                Rooms.Add(new Room { Id = "-3", Name = "Admininstration" });
                var cls = db.ClassSections.Select(s => new Room { Id = s.ID.ToString(), Name = s.Class.ClassName + "(" + s.Section.SectionName + ")" }).ToList();
                if (cls.Count > 0)
                    Rooms.AddRange(cls);
                
                
            }
            //Log.Info($"sms hub constructed with Rooms: {Rooms.Count}");
            
        }
        private string GetUserId()
        {
            string userid = "";
            if (Context.User != null && Context.User.Identity != null && Context.User.Identity.IsAuthenticated)
            {
                userid = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            }
            else
            {
                userid = Context.GetHttpContext().Request.Query["userid"];
                //userid = Context.Request.Headers["userid"];
            }
            return userid;
        }

        private bool IsMobile()
        {
            return ((Context.User == null && Context.User.Identity == null || !Context.User.Identity.IsAuthenticated) && !string.IsNullOrEmpty(Context.GetHttpContext().Request.Query["userid"]));
        }
        //private readonly RoleManager<IdentityRole> _roleManager;
        
        //public override Task OnConnected()
        //{
        //    //Log.Info($"sms hub connected: {GetUserId()}");

        //    var userid = GetUserId();
        //    var connected = connectedUsers.FirstOrDefault(w => w.UserId == userid);
        //    if (connected == null)
        //    {
        //        connected = new ConnectedUser { UserId = userid, MachineId = new List<string>() };
        //        connectedUsers.Add(connected);
        //    }
        //    connected.MachineId.Add(Context.ConnectionId);
        //    //return base.OnConnected();
        //}
        //public override Task OnReconnected()
        //{
        //    var userid = GetUserId();
        //    var connected = connectedUsers.FirstOrDefault(w => w.UserId == userid);
        //    if (connected == null)
        //    {
        //        connected = new ConnectedUser { UserId = userid, MachineId = new List<string>() };
        //        connectedUsers.Add(connected);
        //    }
        //    if (!connected.MachineId.Contains(Context.ConnectionId))
        //        connected.MachineId.Add(Context.ConnectionId);

        //    connectedUsers.RemoveAll(w => w.MachineId.Count == 0);
        //    return base.OnReconnected();
        //}
        //public override Task OnDisconnected(bool stopCalled)
        //{
        //    Log.Info($"sms hub Disconnected: {GetUserId()}");
        //    var userid = GetUserId();
        //    var connected = connectedUsers.FirstOrDefault(w => w.UserId == userid);
        //    if (connected != null)
        //    {
        //        connected.MachineId.Remove(Context.ConnectionId);
        //        if (connected.MachineId.Count == 0)
        //            connectedUsers.Remove(connected);
        //    }
        //    connectedUsers.RemoveAll(w => w.MachineId.Count == 0);
        //    foreach (var r in Rooms)
        //    {
        //        r.Users.RemoveAll(v => v.UserId == userid);
        //    }
        //    return base.OnDisconnected(stopCalled);
        //}
        public async Task SubscribeFeeEntry()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, HubSubscriptions.FeeEntry.ToString());
        }

        public async Task JoinRoom(string roomId, string fromroom = "")
        {
            var room = Rooms.FirstOrDefault(w => w.Id == roomId);
            var userid = GetUserId();
            foreach (var r in Rooms)
            {
                r.Users.RemoveAll(v=>v.UserId==userid);
            }
            if (room != null)
            {
                var existUser = room.Users.FirstOrDefault(w => w.UserId == userid);
                if (existUser == null)
                {
                    existUser = new ConnectedUser { UserId = userid };
                    room.Users.Add(existUser);
                }

                existUser.MachineId.Add(Context.ConnectionId);
            }
            //if (!string.IsNullOrEmpty(fromroom))
            //{
            //    var lef = Rooms.FirstOrDefault(w => w.Id == fromroom);
            //    if (lef != null)
            //    {
            //        var existUser = room.Users.FirstOrDefault(w => w.UserId == userid);
            //        if (existUser != null)
            //        {
            //            existUser.MachineId.Remove(Context.ConnectionId);
            //            if (existUser.MachineId.Count == 0)
            //                lef.Users.Remove(existUser);

            //        }
            //    }
            //}
        }
        public async Task<string> SaveMessage(string ToId, string Message, string MessageType)
        {

            //Log.Info("SaveMessage");
            //Log.Info(ToId);
            //Log.Info(Message);
            //Log.Info(MessageType);
            var user = GetUserId();
            //Log.Info("from user "+user);

            return await SaveMessage(new MessageModel { ToId=ToId, Message=Message, MessageType=MessageType });
        }
        public async Task<string> SaveMessage(MessageModel model)
        {
            try
            {

                string toId = model.ToId?.Trim();
                if (string.IsNullOrEmpty(toId))
                {
                    var adminRole = await _roleManager.FindByNameAsync("Admin");
                    var userAdmin = (await _userManager.GetUsersInRoleAsync(adminRole.Name)).FirstOrDefault();

                    if (userAdmin != null)
                        toId = userAdmin.Id;
                }
                else
                {
                    if (toId.Contains(":"))
                        toId = model.ToId?.Trim().Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)[1];
                }
                bool isroom = int.TryParse(toId, out int intToId);
                var user = GetUserId();
                //GetUserId()

                MobileMessage message = new MobileMessage();
                message.EntryDate = DateTime.UtcNow;
                message.FromId = user;
                message.ToId = toId;
                message.MessageType = model.MessageType;
                message.Message = model.Message;
                message.IsDeleted = false;
                message.ReadDate = DateTime.UtcNow;
                db.MobileMessages.Add(message);
                await db.SaveChangesAsync();

                var data = (await db.MobileMessages.Where(w => w.ID == message.ID).Include(i=>i.Author).ToListAsync()).
                    Select(s => new MessageViewModel
                    {
                        //i = s.ID,
                        Timestamp = s.EntryDate.ToLocalTime(),
                        FromUserId = s.FromId,
                        To = s.ToId,
                        Content = s.Message,
                        type = s.MessageType,
                        // = s.ReadDate,
                        //IsDeleted = s.IsDeleted,
                        IsRoom = int.TryParse(s.ToId, out int r),
                        IsMine = false,
                        From = s.Author.UserName,
                    }).FirstOrDefault();

                var participants = connectedUsers.Where(w => w.UserId == toId || w.UserId == user).Select(s => s.MachineId).ToList();
                List<string> ids = new List<string>();
                //Log.Info($"all users: {Newtonsoft.Json.JsonConvert.SerializeObject(connectedUsers)}");
                //Log.Info($"Participants: {Newtonsoft.Json.JsonConvert.SerializeObject(participants)}");

                if (isroom)
                {
                    var rooms = Rooms.Where(w => w.Id == toId);
                    if (rooms != null)
                    {
                        foreach (var r in rooms)
                        {
                            foreach (var p in r.Users)
                            {
                                foreach (var i in p.MachineId)
                                {
                                    ids.Add(i);
                                }
                            }
                        }
                    }
                }

                foreach (var p in participants)
                {
                    ids.AddRange(p);
                }
                ids = ids.Distinct().ToList();
                //Log.Info($"send new messge to {string.Join(",", ids)}");
               await Clients.Clients(ids).SendCoreAsync("newMessage",new object[] { Newtonsoft.Json.JsonConvert.SerializeObject(data) });
                if (isroom)
                {

                    //Log.Info($"send new messge notify  to room");
                        var d = new {data.To, IsRoom=true  };
                        await Clients.Others.SendCoreAsync("notifyNewMessge",new object[] { Newtonsoft.Json.JsonConvert.SerializeObject(d) });


                }
                else
                {
                    var participant = connectedUsers.FirstOrDefault(w => w.UserId == toId);
                    //Log.Info($"send new messge notify  to user {participant}");

                    if (participant != null)
                    {
                        var mids = participant.MachineId.ToList();
                        //Log.Info($"send new messge notify  to ids {string.Join(",", mids)}");
                        mids = mids.Distinct().ToList();
                            var d = new {To= data.FromUserId, IsRoom = false };
                           await Clients.Users(mids).SendCoreAsync("notifyNewMessge",new object[] { Newtonsoft.Json.JsonConvert.SerializeObject(d) });
                    }
                }
        return "Save completed";

            }
            catch (Exception ex)
            {

                //Log.Info($"sending message exeption {ex}");
                //response.ErrorList = new List<string>();
                //response.ErrorList.Add(ex.Message);
                //if (ex.InnerException != null)
                //    response.ErrorList.Add(ex.InnerException.Message);
                //response.Data = false;
                return ex.Message;
            }

        }
    }
    public class ConnectedUser
    {
        public string UserId { get; set; }
        public List<string> MachineId { get; set; } = new List<string>();
    }

    public class Room
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<ConnectedUser> Users { get; set; } = new List<ConnectedUser>();
    }
    
}
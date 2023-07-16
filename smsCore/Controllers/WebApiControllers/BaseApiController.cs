using System;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using sms;
using smsCore.Data;

[Route("api/base")]
public class BaseApiController : Controller
{
    //public string UserId => User.Identity.GetUserId();
    private readonly SchoolEntities db;
    private readonly CurrentUser _user;
    private readonly IWebHostEnvironment env;
    private readonly IHttpContextAccessor httpContextAccessor;
    private readonly UserManager<ApplicationUser> _userManger;
    public BaseApiController(SchoolEntities _db,CurrentUser Cuser, IWebHostEnvironment webHostEnvironment, UserManager<ApplicationUser> userManger, IHttpContextAccessor _httpContextAccessor)
    {
        db = _db;
        _user = Cuser;
        env = webHostEnvironment;
        _userManger= userManger;
        httpContextAccessor = _httpContextAccessor;

    }
    [NonAction]
    public async Task<string> GetImageUrl(string id, EnumManager.Imagetype type)
    {
        string imagePath = "";
        id = type== EnumManager.Imagetype.Logo ? "logo" : id;
        var exist = checkFileExist(id, type);
        if (exist!="204")
            return exist;

        bool converted = int.TryParse(id, out int cid);
        byte[] imgBytes = null;
        if (type== EnumManager.Imagetype.Logo) {
           imgBytes=await db.SchoolLogoes.Where(w => w.ID==4).Select(s=>s.LOGO).FirstOrDefaultAsync();
        }
        if (type== EnumManager.Imagetype.Students) {
            if (converted)
            {
                imgBytes = (await db.StudentPhotos.Where(w => w.StudentID==cid).Select(s => s.StudentImage).ToListAsync()).LastOrDefault();
            }

        }
        else if (type== EnumManager.Imagetype.User)
        {
            
        }
        else if (type== EnumManager.Imagetype.Employee)
        {
            imgBytes =await db.tbl_Employee.Where(w => w.Id==cid).Select(s => s.Photo).FirstOrDefaultAsync();
        }
        else if (type== EnumManager.Imagetype.Father)
        {
            imgBytes =await db.Students.Where(w => w.ID==cid).Select(s => s.FatherPhoto).FirstOrDefaultAsync();
        }
        else if (type== EnumManager.Imagetype.FatherSignature)
        {
            imgBytes =await db.SignatureSpecimen.Where(w => w.StudentID==cid).Select(s => s.FatherSign).FirstOrDefaultAsync();

        }
        else if (type== EnumManager.Imagetype.GaurdianSignature)
        {
            imgBytes =await db.SignatureSpecimen.Where(w => w.StudentID==cid).Select(s => s.GaurdianSign).FirstOrDefaultAsync();
        }
        else if (type== EnumManager.Imagetype.Gaurdian)
        {

        }
        return GetFile(id, type, imgBytes);
    }

    [NonAction]
    string checkFileExist(string cid, EnumManager.Imagetype type)
    {
        string folder = type.ToString()+"/";
        string picture = string.Empty;
        //string path = HttpContext.Current.Server.MapPath("~/Uploads/"+folder);
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", folder);

        if (!System.IO.File.Exists(path))
        {
            System.IO.Directory.CreateDirectory(path);
        }
        string file = path+cid+".jpg";
        if (System.IO.File.Exists(file))
        {
            picture=GetBaseUrl()+ "/Uploads/images/"+folder+cid+".jpg";
            return picture;
        }
        return "204";
    }

    [NonAction]
    public string GetBaseUrl()
    {
        //var request = HttpContext.Current.Request;
        //var request = HttpContext.Request;
        var request = httpContextAccessor.HttpContext.Request;

        //var appUrl = HttpRuntime.AppDomainAppVirtualPath;
        //var appUrl = HttpContext.Request.PathBase.ToString();
        var appUrl = httpContextAccessor.HttpContext.Request.PathBase.ToString();

        if (appUrl != "/")
            appUrl = "/" + appUrl;

        //var baseUrl = string.Format("{0}://{1}{2}", request.Url.Scheme, request.Url.Authority, appUrl);
        var baseUrl = $"{request.Scheme}://{request.Host}{request.PathBase}";
        return baseUrl;
    }

    [NonAction]
    string GetFile(string cid, EnumManager.Imagetype type, byte[] imgByes=null)
    {
        string folder = type.ToString();
        string picture = string.Empty;
        //string path =HttpContext.Current.Server.MapPath("~/Uploads/images/"+folder);
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Uploads", "images", folder);

        if (!System.IO.File.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        string file = path+cid+".jpg";
        if (!System.IO.File.Exists(file))
        {
            FileStream stream = new FileStream(file, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            stream.Write(imgByes, 0, imgByes.Length);
            return picture;
        }
        else return picture;

    }

    //protected ApplicationUserManager AppUserManager
    //{
    //    get
    //    {
    //        return _AppUserManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
    //    }
    //}


    //protected ApplicationRoleManager AppRoleManager
    //{
    //    get
    //    {
    //        return _AppRoleManager ?? Request.GetOwinContext().GetUserManager<ApplicationRoleManager>();
    //    }
    //}
    //protected ModelFactory TheModelFactory
    //{
    //    get
    //    {
    //        if (_modelFactory == null)
    //        {
    //            _modelFactory = new ModelFactory(this.Request, this.AppUserManager);
    //        }
    //        return _modelFactory;
    //    }
    //}
    [HttpPost]
    [Route("login")]
    public string VerifyUser(string userName, string password)
    {
        try
        {
            //var userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var userManager =httpContextAccessor.HttpContext.RequestServices.GetService<UserManager<ApplicationUser>>();

            var user = _userManger.FindByNameAsync(userName);
            //if (userManager.CheckPassword(user, password))
            //{
            //    var campus = db.UserDefinitions.AsNoTracking().Where(w => w.UserId == user.Id).FirstOrDefault();
            //    if (campus == null || campus.UserType.Trim().ToLower() != "c")
            //        return "Error: Only Campus user may logged in";
            //    var data = new {user.Id, CampusId = campus.FK};
            //    return JsonConvert.SerializeObject(data);
            //}
            //if (_userManger.CheckPasswordAsync(user, password))
            //{
            //    var campus = db.UserDefinitions.AsNoTracking().Where(w => w.UserId == user.Id).FirstOrDefault();
            //    if (campus == null || campus.UserType.Trim().ToLower() != "c")
            //        return "Error: Only Campus user may logged in";
            //    var data = new { user.Id, CampusId = campus.FK };
            //    return JsonConvert.SerializeObject(data);
            //}


            return "Error: Invalid Authentication Result";
        }
        catch (Exception aa)
        {
            return "Error: " + aa;
        }
    }

    [NonAction]
    protected IActionResult GetErrorResult(IdentityResult result)
    {
        if (result == null) return NotFound();

        if (!result.Succeeded)
        {
            var error_description = string.Empty;
            if (result.Errors != null)
                foreach (var error in result.Errors)
                {
                    //ModelState.AddModelError("", error);

                    error_description += $"{error} <br />";
                }

            if (ModelState.IsValid)
                // No ModelState errors are available to send, so just return an empty BadRequest.
                return BadRequest();

            return BadRequest(error_description);
        }

        return null;
    }

    [NonAction]
    protected IActionResult GetErrorResult(Exception result)
    {
        if (result == null) return NotFound();

        if (result is DbEntityValidationException)
        {
            var error_description = string.Empty;
            var exc = (DbEntityValidationException) result;
            if (exc != null)
            {
                foreach (var error in exc.EntityValidationErrors)
                foreach (var r in error.ValidationErrors)
                {
                    ModelState.AddModelError(r.PropertyName, r.ErrorMessage);
                    error_description += $"{r.ErrorMessage} <br />";
                }
            }
            else
            {
                ModelState.AddModelError("", result.Message);
                error_description += $"{result.Message} <br />";
            }

            if (ModelState.IsValid)
                // No ModelState errors are available to send, so just return an empty BadRequest.
                return BadRequest();

            return BadRequest(error_description);
        }

        return null;
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Models;
using smsCore.Data;
using smsCore.Data.Models.PublicSite;
using System.Security.Claims;

namespace smsCore.Controllers
{
    public class PostController : BaseController
    {
        private readonly SchoolEntities context;
        private readonly StaticResources _resource;
        private readonly CurrentUser _user;
        private readonly IWebHostEnvironment _hostingEnvironment;


        public PostController(SchoolEntities _context , StaticResources resources , CurrentUser user, IWebHostEnvironment hostingEnvironment)
        {
            context = _context;
            _resource = resources;
            _user = user;
            _hostingEnvironment=hostingEnvironment;


        }


        private const string UploadFolderPath = "~/Uploads/images";
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreatePost()
        {
            if (!context.PublicPostCategories.Any())
            {

            }
            ViewBag.posttype = new SelectList(context.PublicPostCategories.Select(s => new { s.Id, s.Name }), "Id", "Name");
            return View();
        }
        //   [ValidateInput(false)]
        [HttpPost]
        public async Task<string> CreatePost(PublicContentPost model, IFormFile PreviewImage, IFormFile DetailImage)
        {
            try
            {
                var NewPost = new PublicContentPost();

                NewPost.PreviewImage = string.Empty;
                NewPost.DetailImage = string.Empty;
                NewPost.PostCategoryId = model.PostCategoryId;
                NewPost.PostTitle = model.PostTitle;
                NewPost.PostContent = model.PostContent;
                NewPost.CreatedBy = _user.UserID;
                NewPost.CreatedOn = DateTime.Now;
                NewPost.ModifiedBy = _user.UserID;
                NewPost.ModifiedOn = DateTime.Now;
                NewPost.IsActive = true;
                NewPost.IsPublished = true;
                string slug = model.PostTitle.Replace(" ", "-").Replace(".", "").Trim();
                NewPost.Slug = slug;

                if (!System.IO.Directory.Exists(Path.Combine(_hostingEnvironment.WebRootPath, UploadFolderPath)))
                {
                    System.IO.Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, UploadFolderPath));
                }

                if (DetailImage != null)
                {
                    string imageName = UploadFolderPath + "/" + slug + Path.GetExtension(DetailImage.FileName);
                    var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, imageName);
                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await DetailImage.CopyToAsync(fileStream);
                    }
                    NewPost.DetailImage = imageName;
                }

                if (PreviewImage != null)
                {
                    string imageName = UploadFolderPath + "/" + slug + "-preview" + Path.GetExtension(PreviewImage.FileName);
                    var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, imageName);
                    using (var fileStream = new FileStream(imagePath, FileMode.Create))
                    {
                        await PreviewImage.CopyToAsync(fileStream);
                    }
                    NewPost.PreviewImage = imageName;
                }

                context.PublicPosts.Add(NewPost);
                await context.SaveChangesAsync();
                return "success";
                //return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                //ViewBag.posttype = new SelectList(context.PostTypes, "Id", "Name");
                //ModelState.AddModelError("",ex.Message.ToString());
                //return View(model);
                return ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
        }

        public JsonResult GetPostList()
        {
            var list = context.PublicPosts.Where(w => w.IsActive).Select(s => new
            {
                s.Id,
                s.PostTitle,
                PostName = s.PostCategory.Name,
                s.PreviewImage,
                s.PostContent
            });
 
            var listg = Json(new { Data = list }); 
            return listg;
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            var del = context.PublicPosts.Find(id);
            del.IsActive = false;
            context.SaveChanges();
            // return Json(new { status = true, message = "Data has been deleted" }, JsonRequestBehavior.AllowGet);
            return Json(new
            {
                status = true,
                message = "Data has been deleted"
            });
        }

        [HttpGet]
        public IActionResult PostEdit(int id)
        {
            var Edit = context.PublicPosts.Find(id);
            if (Edit == null)
                return NotFound();
            ViewBag.posttype = new SelectList(context.PublicPostCategories.AsNoTracking().Select(s => new { Id = s.Id, Name = s.Description }).ToList(), "Id", "Name", Edit.PostCategoryId);
            return View(Edit);
        }
        [HttpPost]
        public IActionResult PostEdit(PublicContentPost model, IFormFile DetailImage, IFormFile PreviewImage)
        {
            var NewData = context.PublicPosts.Find(model.Id);
            if (NewData == null)
            {
                return NotFound();
            }

            NewData.PostTitle = model.PostTitle;
            NewData.PostCategoryId = model.PostCategoryId;
            NewData.PostContent = model.PostContent;
            NewData.ModifiedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
            NewData.ModifiedOn = DateTime.Now;

            string slug = model.PostTitle.Replace(" ", "-").Replace(".", "").Trim();
            NewData.Slug = slug;

            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), UploadFolderPath)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), UploadFolderPath));
            }

            if (DetailImage != null && DetailImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(NewData.DetailImage))
                {
                    if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), NewData.DetailImage)))
                    {
                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), NewData.DetailImage));
                    }
                }

                string imageName = UploadFolderPath + "/" + slug + Path.GetExtension(DetailImage.FileName);
                using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), imageName), FileMode.Create))
                {
                    DetailImage.CopyTo(stream);
                }
                NewData.DetailImage = imageName;
            }

            if (PreviewImage != null && PreviewImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(NewData.PreviewImage))
                {
                    if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), NewData.PreviewImage)))
                    {
                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), NewData.PreviewImage));
                    }
                }

                string imageName = UploadFolderPath + "/" + slug + "-preview" + Path.GetExtension(PreviewImage.FileName);
                using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), imageName), FileMode.Create))
                {
                    PreviewImage.CopyTo(stream);
                }
                NewData.PreviewImage = imageName;
            }

            try
            {
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

            return RedirectToAction("Index");
        }


        public IActionResult PageList()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreatePage(int id = 0)
        {
            var t = context.PublicPages.Find(id);
            if (t == null)

                return View();
            else
            {
                var vm = new PublicPageViewModel { Description = t.Description, PageContent = t.PageContent, Id = t.Id, PageName = t.PageName, PageTitle = t.PageTitle, Slug = t.Slug, Tags = t.Tags };
                return View(vm);
            }
        }
        //[ValidateInput(false)]
        [HttpPost]
        public string CreatePage(PublicPage model)
        {
            try
            {
                var NewPost = context.PublicPages.Find(model.Id);
                if (NewPost == null)
                {
                    var exist = context.PublicPages.Where(w => w.PageName == model.PageName.Trim()).FirstOrDefault();
                    if (exist != null)
                    {
                        return "Page name already exist.";
                    }
                    NewPost = new PublicPage()
                    {
                        CreatedBy = _user.UserID,
                        CreatedOn = DateTime.Now,
                        IsActive = true,
                        IsPublished = true,
                        ModifiedBy = _user.UserID,
                        ModifiedOn = DateTime.Now
                    };
                    context.PublicPages.Add(NewPost);
                }

                NewPost.PageTitle = model.PageTitle;
                NewPost.PageContent = model.PageContent;
                NewPost.PageName = model.PageName;
                NewPost.Description = model.Description;
                string slug = model.PageTitle.Replace(" ", "-").Replace(".", "").Trim();
                NewPost.Slug = slug;
                NewPost.Tags = model.Tags ?? "";
                context.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.InnerException == null ? ex.Message : ex.InnerException.Message;

            }
        }
        public JsonResult GetPageList()
        {
            var list = context.PublicPages.Where(w => w.IsActive).Select(s => new
            {
                s.Id,
                s.PageName,
                s.PageTitle
            });
             
            var listg = Json(new
            {
                Data = list
            });
            return listg;
        }

        [HttpGet]
        public IActionResult DeletePage(int id)
        {
            var del = context.PublicPages.Find(id);
            del.IsActive = false;
            context.SaveChanges();

            return Json(new { status = true, message = "Data has been deleted" } );
        }


        ///////////////////////////////////////////////////////////////
        public IActionResult SliderList()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CreateSlider()
        {
            return View();
        }
       // [ValidateInput(false)]
        
        [HttpPost]
        public IActionResult Createslider(PublicSlider model, IFormFile PreviewImage)
        {
            try
            {
                var slider = new PublicSlider();
                slider.ButtonText = model.ButtonText;
                slider.ButtonUrl = model.ButtonUrl;
                slider.FirstLine = model.FirstLine;
                slider.SecondLine = model.SecondLine;
                slider.ThirdLine = model.ThirdLine;
                slider.SliderGroup = model.SliderGroup;
                slider.CreatedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                slider.CreatedOn = DateTime.Now;
                slider.ModifiedBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                slider.ModifiedOn = DateTime.Now;
                slider.IsActive = true;
                slider.IsPublished = true;

                if (!Directory.Exists(Path.Combine(_hostingEnvironment.WebRootPath, UploadFolderPath)))
                {
                    Directory.CreateDirectory(Path.Combine(_hostingEnvironment.WebRootPath, UploadFolderPath));
                }

                if (PreviewImage != null && PreviewImage.Length > 0)
                {
                    string imageName = UploadFolderPath + "/" + DateTime.Now.ToString("dd_MM_yyyy_hh_mm_ss") + "-preview" + Path.GetExtension(PreviewImage.FileName);
                    var imagePath = Path.Combine(_hostingEnvironment.WebRootPath, imageName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        PreviewImage.CopyTo(stream);
                    }
                    slider.ImagePath = imageName.Remove(0, 1);
                }

                context.PublicSliders.Add(slider);
                context.SaveChanges();
                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                ex.Source = "Error";
                //      return ex.InnerException == null ? ex.Message : ex.InnerException.Message;
            }
            return RedirectToAction("Index");
        }

        public JsonResult GetSliderList()
        {
            var list = context.PublicSliders.Where(w => w.IsActive).Select(s => new
            {
                s.Id,
                s.SliderGroup,
                s.FirstLine,
                s.SecondLine,
                s.ThirdLine,
                s.ImagePath
            });
            var listg = Json(new
            {
                Data = list
            });
            return listg;
        }

        [HttpGet]
        public IActionResult DeleteSlider(int id)
        {
            var del = context.PublicSliders.Find(id);
            del.IsActive = false;
            context.SaveChanges();
            return Json(new { status = true, message = "Data has been deleted" } );
        }

        [HttpGet]
        public IActionResult SliderEdit(int id)
        {
            var Edit = context.PublicPosts.Find(id);
            if (Edit == null)
                return NotFound();
            ViewBag.posttype = new SelectList(context.PublicPostCategories.AsNoTracking().Select(s => new { Id = s.Id, Name = s.Description }).ToList(), "Id", "Name", Edit.PostCategoryId);
            return View(Edit);
        }
        // [ValidateInput(false)]
        [HttpPost]
        public IActionResult SliderEdit(PublicContentPost model, IFormFile DetailImage, IFormFile PreviewImage)
        {
            var NewData = context.PublicPosts.Find(model.Id);
            if (NewData == null)
            {
                return NotFound();
            }
            NewData.PostTitle = model.PostTitle;
            NewData.PostCategoryId = model.PostCategoryId;
            NewData.PostContent = model.PostContent;

            string slug = model.PostTitle.Replace(" ", "-").Replace(".", "").Trim();
            NewData.Slug = slug;
            if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), UploadFolderPath)))
            {
                Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), UploadFolderPath));
            }
            if (DetailImage != null && DetailImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(NewData.DetailImage))
                {
                    if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), NewData.DetailImage)))
                    {
                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), NewData.DetailImage));
                    }
                }

                string imageName = Path.Combine(UploadFolderPath, slug + Path.GetExtension(DetailImage.FileName));
                using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), imageName), FileMode.Create))
                {
                    DetailImage.CopyTo(stream);
                }
                NewData.DetailImage = imageName;
            }

            if (PreviewImage != null && PreviewImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(NewData.PreviewImage))
                {
                    if (System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), NewData.PreviewImage)))
                    {
                        System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), NewData.PreviewImage));
                    }
                }
                string imageName = Path.Combine(UploadFolderPath, slug + "-preview" + Path.GetExtension(DetailImage.FileName));
                using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), imageName), FileMode.Create))
                {
                    PreviewImage.CopyTo(stream);
                }
                NewData.PreviewImage = imageName;
            }
            try
            {
                context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ex.Message.ToString();
            }

            return RedirectToAction("Index");
        }


        public IActionResult SendEmail(string name, string email, string phone, string message)
        {
            //   string to_email = context.Campuses.FirstOrDefault().emailId;
            string to_email = context.Campuses.FirstOrDefault().emailId;

            message = message + "<br /><b>Name:</b>" + name+"<br /><br /> <b>Contact:</b> " + phone;
            _resource.SendEmail(email, to_email, "New Inquiry message from site", message);
            return Json(new { success = true, message = "" });
        }
    }
}
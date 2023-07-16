using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Models;
using smsCore.Data.Helpers;
using Syncfusion.EJ2.Base;

namespace smsCore.Controllers
{
   
    public class LibraryController : BaseController
    {
        // GET: Library
        private readonly SchoolEntities db;
        
        public LibraryController(SchoolEntities db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public string Create(Library library)
        {
            try
            {                
                db.Libraries.Add(library);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Edit: Library
        public IActionResult EditLibrary(int id)
        {
            var editLib = db.Libraries.Where(n => n.ID == id).FirstOrDefault();
            return View(editLib);
        }

        [HttpPost]
        public string EditLibrary(Library library)
        {
            var editLib = db.Libraries.Find(library.ID);
            if (editLib != null)
            {
                editLib.LibraryName = library.LibraryName;
                editLib.Code = library.Code;
                editLib.OpeningTime = library.OpeningTime;
                editLib.ClosingTime = library.ClosingTime;
                db.SaveChanges();
            }
            else
            {
                return "failed";
            }

            return "success";
        }

        // Delete: Library
        public IActionResult DeleteLibrary(int id)
        {
            var dlt = db.Libraries.Where(n => n.ID == id).FirstOrDefault();
            return View(dlt);
        }

        [HttpPost]
        public string DeleteLibrary(Library library)
        {
            
            var message = string.Empty;
            try
            {
                var libDel = db.Libraries.Find(library.ID);
                db.Libraries.Remove(libDel);
                db.SaveChanges();
                message = "success";
            }
            catch (Exception aa)
            {
                message = aa.Message;
            }
            return message;
        }
        [HttpPost]
        public JsonResult GetLibraries(DataManagerRequest dm)
        {
            var library = db.Libraries.AsNoTracking().Select(s => new
            {    s.ID,
                 s.LibraryName,
                 s.Campus.CampusName,
                 s.Code,
                 OpeningTime = s.OpeningTime.ToString(),
                 ClosingTime = s.ClosingTime.ToString()
            });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                library = operation.PerformSearching(library, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                library = operation.PerformSorting(library, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                library = operation.PerformFiltering(library, dm.Where, dm.Where[0].Operator);
            }
            int count = library.Count();
            if (dm.Skip != 0)
            {
                library = operation.PerformSkip(library, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                library = operation.PerformTake(library, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = library, count = library.Count() });
            }
            else
            {
                return Json(new { result = library, count = library.Count() });
            }
        }


        // GET: BookList
        public IActionResult BookList()
        {
            return View();
        }

        [HttpPost]
        public string BookList(LibraryBookList bookList)
        {
            bookList.IsIssuable = true;
            try
            {
                bookList.EntryDate = DateTime.Now;
                db.LibraryBookLists.Add(bookList);
                db.Entry(bookList).State = EntityState.Added;
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        [HttpPost]
        public JsonResult GetBookList(DataManagerRequest request)
        {
            var booklist = db.LibraryBookLists.Select(s => new {
                s.ID,
                s.AccessionNo,
                s.ClassficationNo,
                s.Title,
                s.Author,
                s.Publisher,
                s.Edition,
                s.Subject,
                s.YearOfPublishing,
                s.VolumeNo,
                s.Language,
                s.Pages,
                s.Price,
                s.Source,
                s.PresentPosition,
                s.Remarks,
                s.Translator,
                EntryDate = s.EntryDate.ToString(),
                s.Cornor,
            });
            DataOperations operations = new DataOperations();
            if(request.Search !=null && request.Search.Count > 0)
            {
                booklist = operations.PerformSearching(booklist, request.Search);
            }
            if(request.Sorted !=null && request.Sorted.Count> 0)
            {
                booklist = operations.PerformSorting(booklist, request.Sorted);
            }
            if(request.Where !=null && request.Where.Count>0)
            {
                booklist = operations.PerformFiltering(booklist, request.Where, request.Where[0].Operator);
            }
            int count = booklist.Count();
            if (request.Skip !=0)
            {
                booklist = operations.PerformSkip(booklist, request.Skip);
            }
            if(request.Take != 0)
            {
                booklist = operations.PerformTake(booklist, request.Take);
            }
            if (request.RequiresCounts)
            {
                return Json(new { result = booklist, count = booklist.Count() });
            }
            else
            {
                return Json(new { result = booklist, count = booklist.Count() });
            }
            
        }

        // Edit: BookList
        public ActionResult EditBookList(int id)
        {
            var list = db.LibraryBookLists.Where(n => n.ID == id).FirstOrDefault();
            return View(list);
        }

        [HttpPost]
        public string EditBookList(LibraryBookList list)
        {
            var staffEdit = db.LibraryBookLists.Find(list.ID);
            if (staffEdit != null)
            {
                staffEdit.AccessionNo = list.AccessionNo;
                staffEdit.ClassficationNo = list.ClassficationNo;
                staffEdit.Title = list.Title;
                staffEdit.Author = list.Author;
                staffEdit.Publisher = list.Publisher;
                staffEdit.Edition = list.Edition;
                staffEdit.Subject = list.Subject;
                staffEdit.YearOfPublishing = list.YearOfPublishing;
                staffEdit.VolumeNo = list.VolumeNo;
                staffEdit.Language = list.Language;
                staffEdit.Pages = list.Pages;
                staffEdit.Price = list.Price;
                staffEdit.Source = list.Source;
                staffEdit.PresentPosition = list.PresentPosition;
                staffEdit.Remarks = list.Remarks;
                staffEdit.Translator = list.Translator;
                staffEdit.Cornor = list.Cornor;
                db.SaveChanges();
            }
            else
            {
                return "failed";
            }

            return "success";
        }

        // Delete: BookList
        public ActionResult DeleteBookList(int id)
        {
            var ff = db.LibraryBookLists.Where(w => w.ID == id).FirstOrDefault();
            return View(ff);
        }

        [HttpPost]
        public async Task<string> DeleteBookList(LibraryBookList bookList)
        {
            var bookListDlt = db.LibraryBookLists.Find(bookList.ID);
            if (bookListDlt != null)
                try
                {
                    var librBookRecords = db.LibraryBookRecords.Where(w => w.LibraryBookListId == bookListDlt.ID)
                        .ToList();
                    var libraryIssuedBooks = db.LibraryIssuedBooks.Where(w => w.LibraryBookRecordId == bookListDlt.ID)
                        .ToList();
                    foreach (var item in librBookRecords)
                    {
                        if (item.LibraryIssuedBooksToStaffs != null)
                            foreach (var issuedtostaff in item.LibraryIssuedBooksToStaffs)
                                db.LibraryIssuedBooksToStaffs.Remove(issuedtostaff);
                        db.LibraryBookRecords.Remove(item);
                    }

                    foreach (var item in libraryIssuedBooks) db.LibraryIssuedBooks.Remove(item);
                    db.LibraryBookLists.Remove(bookListDlt);

                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return ex.Message;
                }
            else
                return "failed";

            return "delete";
        }

        // GET: BookRecord
        public ActionResult BooksRecord()
        {
            ViewBag.LibraryId = new SelectList(db.Libraries, "ID", "LibraryName");
            ViewBag.LibraryBookListId = new SelectList(db.LibraryBookLists, "ID", "Title");
            return View();
        }

        [HttpPost]
        public string BooksRecord(LibraryBookRecord bookRecord)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            bookRecord.isDeleted = false;
            bookRecord.isActive = true;
            bookRecord.UserId = userId;
            try
            {
                bookRecord.EntryDate = DateTime.Now;
                db.LibraryBookRecords.Add(bookRecord);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Edit: BookRecord
        public ActionResult EditBookRecord(int id)
        {
            ViewBag.LibraryId = new SelectList(db.Libraries, "ID", "LibraryName");
            ViewBag.LibraryBookListId = new SelectList(db.LibraryBookLists, "ID", "Title");
            var editRec = db.LibraryBookRecords.Where(w => w.ID == id).FirstOrDefault();
            return View(editRec);
        }

        [HttpPost]
        public string EditBookRecord(LibraryBookRecord bookRecord)
        {
            var editRecord = db.LibraryBookRecords.Find(bookRecord.ID);
            if (editRecord != null)
            {
                editRecord.ShelfNo = bookRecord.ShelfNo;
                db.SaveChanges();
            }
            else
            {
                return "failed";
            }

            return "success";
        }

        // Delete: BookRecord
        public ActionResult DeleteBookRecord(int id)
        {
            var dltRec = db.LibraryBookRecords.Where(w => w.ID == id).FirstOrDefault();
            return View(dltRec);
        }

        [HttpPost]
        public string DeleteBookRecord(LibraryBookRecord library)
        {
            var dltRecord = db.LibraryBookRecords.Find(library.ID);
            if (dltRecord != null)
            {
                db.LibraryBookRecords.Remove(dltRecord);
                db.SaveChanges();
            }
            else
            {
                return "failed";
            }

            return "delete";
        }

        [HttpPost]
        public ActionResult GetBooksRecord(DataManagerRequest dm)
        {
            var library = db.LibraryBookRecords.AsNoTracking().Select(s => new
            {
                s.ID,
                Library = s.Library.LibraryName,
                Book = s.LibraryBookList.Title,
                s.ShelfNo,
                EntryDate = s.EntryDate.ToString()
            });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                library = operation.PerformSearching(library, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                library = operation.PerformSorting(library, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                library = operation.PerformFiltering(library, dm.Where, dm.Where[0].Operator);
            }
            int count = library.Count();
            if (dm.Skip != 0)
            {
                library = operation.PerformSkip(library, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                library = operation.PerformTake(library, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = library, count = library.Count() });
            }
            else
            {
                return Json(new { result = library, count = library.Count() });
            }            
        }

        // GET: BookIssued
        public ActionResult BooksIssued()
        {
            ViewBag.StudentID = new SelectList(db.Students, "ID", "StudentName");
            ViewBag.librarybookList = new SelectList(db.LibraryBookLists, "ID", "Title");
            return View();
        }

        [HttpPost]
        public string BooksIssued(LibraryIssuedBook issuedBook)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            

            issuedBook.UserID = userId;
            try
            {
                issuedBook.IssuedDate = DateTimeHelper.ConvertDate(Request.Form["IssuedDate"]);
                issuedBook.ReturnDate = DateTimeHelper.ConvertDate(Request.Form["ReturnDate"]);
                issuedBook.DueDate = DateTimeHelper.ConvertDate(Request.Form["DueDate"]);
                //var issued = db.LibraryIssuedBooks.Count();
                //if (issued == 0)
                //    issuedBook.ID = 1;
                //else
                //    issuedBook.ID = db.LibraryIssuedBooks.Max(m => m.ID) + 1;
                db.LibraryIssuedBooks.Add(issuedBook);
                db.SaveChanges();
                return "success";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        // Edit: BookIssued
        public ActionResult EditBookIssue(int id)
        {
            ViewBag.StudentID = new SelectList(db.Students, "ID", "StudentName");
            ViewBag.LibraryBookRecordId = new SelectList(db.LibraryBookLists, "ID", "Title");
            var editIssued = db.LibraryIssuedBooks.Where(f => f.ID == id).FirstOrDefault();
            return View(editIssued);
        }

        [HttpPost]
        public string EditBookIssue(LibraryIssuedBook book)
        {
            var editIss = db.LibraryIssuedBooks.Find(book.ID);
            if (editIss != null)
            {
                editIss.BookCondition = book.BookCondition;
                editIss.ReturnDate = DateTimeHelper.ConvertDate(Request.Form["ReturnDate"].ToString());
                editIss.DueDate = DateTimeHelper.ConvertDate(Request.Form["DueDate"].ToString());
                editIss.IssuedDate = DateTimeHelper.ConvertDate(Request.Form["IssuedDate"].ToString());
                editIss.Remarks = book.Remarks;
                db.SaveChanges();
            }
            else
            {
                return "failed";
            }

            return "success";
        } 

        // Delete: BookIssued
        public ActionResult DeleteBookIssue(int id)
        {
            var dlt = db.LibraryIssuedBooks.Where(f => f.ID == id).FirstOrDefault();
            return View(dlt);
        }

        [HttpPost]
        public string DeleteBookIssue(LibraryIssuedBook libraryIssued)
        {
            var dltbk = db.LibraryIssuedBooks.Find(libraryIssued.ID);
            if (dltbk != null)
            {
                db.LibraryIssuedBooks.Remove(dltbk);
                db.SaveChanges();
            }
            else
            {
                return "failed";
            }

            return "delete";
        }

        [HttpPost]
        public JsonResult GetIssuedBooks(DataManagerRequest dm)
        {
            var issued = db.LibraryIssuedBooks.Include(w=> w.LibraryBookList).Select(s => new
            {
                s.ID,
                Bookname = s.LibraryBookList.Title,
                StudentID= s.StudentID,
                s.BookCondition,
                IssuedDate = s.IssuedDate.ToString(),
                s.Remarks,
                DueDate = s.DueDate.ToString(),
                ReturnDate = s.ReturnDate.ToString()
            });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                issued = operation.PerformSearching(issued, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                issued = operation.PerformSorting(issued, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                issued = operation.PerformFiltering(issued, dm.Where, dm.Where[0].Operator);
            }
            int count = issued.Count();
            if (dm.Skip != 0)
            {
                issued = operation.PerformSkip(issued, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                issued = operation.PerformTake(issued, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = issued, count = issued.Count() });
            }
            else
            {
                return Json(new { result = issued, count = issued.Count() });
            }
        }
        

        // GET: BookIssuedToStaff
        public ActionResult BooksIssuedToStaff()
        {
            ViewBag.StaffID = new SelectList(db.tbl_Employee, "Id", "employeeName");
            ViewBag.LibraryBookListId = new SelectList(db.LibraryBookLists, "ID", "Title");
            return View();
        }

        [HttpPost]
        public IActionResult BooksIssuedToStaff(LibraryIssuedBooksToStaff staff)
        { 

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            staff.UserID = userId;

            try
            {
               

                staff.IssuedDate = DateTimeHelper.ConvertDate(Request.Form["IssuedDate"].ToString());
                staff.ReturnDate = DateTimeHelper.ConvertDate(Request.Form["ReturnDate"].ToString());
                staff.DueDate = DateTimeHelper.ConvertDate(Request.Form["DueDate"].ToString());
                db.LibraryIssuedBooksToStaffs.Add(staff);
                db.SaveChanges();
                return Ok("success");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        // Edit: BookIssuedToStaff
        public ActionResult EditStaff(int id)
        {
            ViewBag.StaffID = new SelectList(db.tbl_Employee, "Id", "employeeName");
            ViewBag.LibraryBookRecordId = new SelectList(db.LibraryBookLists, "ID", "Title");

           // ViewBag.LibraryBookRecordId = new SelectList(db.LibraryBookRecords, "ID", "ShelfNo");
            var ff = db.LibraryIssuedBooksToStaffs.Where(w => w.ID == id).FirstOrDefault();
            return View(ff);
        }

        [HttpPost]
        public string EditStaff(LibraryIssuedBooksToStaff toStaff)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            toStaff.UserID = userId;
            var staffEdit = db.LibraryIssuedBooksToStaffs.Find(toStaff.ID);
            if (staffEdit != null)
            {
                staffEdit.IssuedDate = toStaff.IssuedDate;
                staffEdit.DueDate = toStaff.DueDate;
                staffEdit.ReturnDate = toStaff.ReturnDate;
                staffEdit.Remarks = toStaff.Remarks;
                staffEdit.BookCondition = toStaff.BookCondition;
                staffEdit.StaffID = toStaff.StaffID;
                db.SaveChanges();
            }
            else
            {
                return "failed";
            }

            return "success";
        }

        // Delete: BookIssuedToStaff
        public ActionResult DeleteStaff(int id)
        {
            var ff = db.LibraryIssuedBooksToStaffs.Where(w => w.ID == id).FirstOrDefault();
            return View(ff);
        }

        [HttpPost]
        public string DeleteStaff(LibraryIssuedBooksToStaff toStaff)
        {
            var staffDlt = db.LibraryIssuedBooksToStaffs.Find(toStaff.ID);
            if (staffDlt != null)
            {
                db.LibraryIssuedBooksToStaffs.Remove(staffDlt);
                db.SaveChanges();
            }
            else
            {
                return "failed";
            }

            return "delete";
        }


        public ActionResult GetIssuedBooksToStaff(DataManagerRequest dm)
        {
            var bookIssued = db.LibraryIssuedBooksToStaffs.ToList();
            var totalRecords = bookIssued.Count();
            //Prepare JqGridData instance
            var issued = db.LibraryIssuedBooksToStaffs.Include(w => w.LibraryBookRecord).Select(s => new
            {
                s.ID,
                bookName = s.LibraryBookRecord.LibraryBookList.Title,
                s.BookCondition,
                IssuedDate = s.IssuedDate.ToString(),
                s.Remarks,
                DueDate = s.DueDate.ToString(),
                ReturnDate = s.ReturnDate.ToString()                
            });
            DataOperations operation = new DataOperations();
            if (dm.Search != null && dm.Search.Count > 0)
            {
                issued = operation.PerformSearching(issued, dm.Search);  //Search
            }
            if (dm.Sorted != null && dm.Sorted.Count > 0) //Sorting
            {
                issued = operation.PerformSorting(issued, dm.Sorted);
            }
            if (dm.Where != null && dm.Where.Count > 0) //Filtering
            {
                issued = operation.PerformFiltering(issued, dm.Where, dm.Where[0].Operator);
            }
            int count = issued.Count();
            if (dm.Skip != 0)
            {
                issued = operation.PerformSkip(issued, dm.Skip);   //Paging
            }
            if (dm.Take != 0)
            {
                issued = operation.PerformTake(issued, dm.Take);
            }
            if (dm.RequiresCounts)
            {
                return Json(new { result = issued, count = issued.Count() });
            }
            else
            {
                return Json(new { result = issued, count = issued.Count() });
            }
            
        }
    }
}
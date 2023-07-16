using Microsoft.AspNetCore.Mvc;
using Models;
using smsCore.Data.Helpers;

namespace smsCore.Controllers
{
    public class MyMessagesController : BaseController
    {
        private readonly SchoolEntities database;
        private readonly MessagingSystem helper;

        public MyMessagesController(SchoolEntities _database, MessagingSystem _helper)
        {
            database =_database ;

            helper = _helper;
        }

        // GET: MyMessages
        public IActionResult Index()
        {
            return View();
        }

        //public async Task<ActionResult> MessagePakages()
        //{
        //    var package =await helper.GetAllPackages();
        //    return View(package);
        //}

        //[HttpPost]
        //public async Task<string> MessagePakages(int PackageId)
        //{
        //    var result =await helper.PurchasedPackages(PackageId);
        //    if(result.IsSuccess)
        //    {
        //        return "Your order has been placed successfully. Please make the payment to activate the package.";
        //    }
        //    else
        //    {
        //        return "An error occured while trying to place pakcage order.";
        //    }

        //}


        //public async Task<ActionResult> MyPakages()
        //{
        //    List<PackageModel> models = new List<PackageModel>();
        //    try
        //    {
        //        models = await helper.PurchasedGetAll();
        //    }
        //    catch { }
        //    return View(models);
        //}

        //[HttpGet]
        //public ActionResult MessageLogs()
        //{
        //    return View();
        //}

        //[HttpPost]
        //public async Task<ActionResult> MessageLogs(DateTime StartDate, DateTime EndDate)
        //{
        //    var logs =await helper.Logs(StartDate, EndDate);

        //    return View(logs);
        //}
    }
}
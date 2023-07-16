//using system;
//using system.collections.generic;
//using system.data;
//using system.data.sqlclient;
//using system.globalization;
//using system.linq;
//using system.web.http;
//using system.web.http.results;
//using microsoft.aspnetcore.mvc;
//using microsoft.entityframeworkcore;
//using models;

//using newtonsoft.json;
//using sms.helpers;
//using smscore.data;
//using smscore.data.helpers;
//using syncfusion.ej2;
//using syncfusion.ej2.base;
//using utilities;

//namespace sms.webapicontrollers
//{
//    [system.web.http.authorize]
//    [system.web.http.routeprefix("api/payroll")]
//    public class payrollapicontroller : apicontroller
//    {
//        private readonly schoolentities db;
//        private readonly currentuser _user;
//        private readonly utilityfunctions utilityfunctions;
//        private readonly extensions1 _methods;
//        public payrollapicontroller(schoolentities _db, currentuser cuser, utilityfunctions utilityfunctions, extensions1 extensions1)
//        {
//            db = _db;
//            _user = cuser;
//            this.utilityfunctions = utilityfunctions;
//            _methods = extensions1;
//        }

//        //[system.web.http.route("getpackages")]
//        //public string getpackage()
//        //{
//        //    var dtblsalarypackage = new datatable();
//        //    var spsalarypackage = new salarypackagesp();
//        //    var data = spsalarypackage.salarypackageviewallformonthlysalarysettings().asenumerable().select(s => new
//        //    {
//        //        salarypackageid = (decimal) s["salarypackageid"],
//        //        salarypackagename = s["salarypackagename"].tostring()
//        //    });
//        //    var package = new selectlist(data, "salarypackageid", "salarypackagename");
//        //    return jsonconvert.serializeobject(package);
//        //}

//        [system.web.http.route("get-employeesalarymonth/{code}")]
//        public string getemployeesalarymonth(string code)
//        {
//            var campusid = _user.getcampusids();
//            var id = utilityfunctions.parsestaffcodes(code, campusid);
//            var months = db.monthlysalaries.asnotracking().where(w => id.contains(w.employeeid)).
//                orderby(o => o.formonth).select(s => new { s.id, s.formonth }).tolist()
//                .select(s => new { id = s.id, text = s.formonth.tostring("mmmm, yyyy") }).tolist();
//            return jsonconvert.serializeobject(months);
//        }

//        [system.web.http.route("get-employee-info-bycode/{code}")]
//        public jsonresult getsemployeeinfobycode(string code)
//        {
//            var campusid = _user.getcampusids();
//            //var id = utilityfunctions.parsestaffcodes(code, campusid);

//            var photo = _methods.getphoto(code);
//            var emp = db.tbl_employee.
//                asnotracking().where(w => w.employeecode == code && campusid.contains(w.campusid)).select(s => new
//                {
//                    employeephoto = photo,
//                    s.employeecode,
//                    s.employeename,
//                    s.tbl_designation.designationname,
//                    s.salarytype,
//                    s.fathername,
//                    s.address,
//                    s.cnic
//                }).firstordefault();
//            return new jsonresult { data = emp, jsonrequestbehavior = jsonrequestbehavior.allowget };

//        }

//        [system.web.http.route("get-employee-info")]
//        public jsonresult getsemployeeinfo(string code)
//        {
//            var photo = _methods.getphoto(code);
//            var emp = db.tbl_employee.
//                asnotracking().where(w => w.employeecode.contains(code) || w.employeename.contains(code)).select(s => new
//                {
//                    employeephoto = photo,
//                    s.employeecode,
//                    s.employeename,
//                    s.tbl_designation.designationname,
//                    s.salarytype,
//                    s.fathername,
//                    s.address,
//                    s.cnic
//                });
//            return new jsonresult { data = emp, jsonrequestbehavior = jsonrequestbehavior.allowget };
//        }

//        [system.web.http.httppost]
//        [system.web.http.route("get-monthly-salary-pkg_details/{monthid}")]
//        public system.web.http.results.jsonresult<object> get_monthly_salary_pkg_detail(datamanagerrequest dm, int monthid)
//        {
//            //var data = db.monthlysalaries.include(s => s.tbl_monthlysalarydetails).include(e => e.employee).asnotracking().where(w => w.id == monthid).select(s => new
//            //{
//            //    s.id,
//            //    //packagename = s.tbl_monthlysalarydetails.firstordefault().monthlysalary.salarypackage.salarypackagename,
//            //    amount = s.tbl_monthlysalarydetails.firstordefault().amount,
//            //    employeename = s.employee.employeename,
//            //    salarytype = s.employee.salarytype,
//            //});

//            var data = db.monthlysalarydetails.where(w => w.monthlysalaryid == monthid).include(i => i.payhead).select(s => new
//            {
//                s.id,
//                s.payhead.payheadname,
//                s.amount
//            });

//            return json<object>(new { result = data, count = data.count() });
//        }
//        [system.web.http.httppost]
//        [system.web.http.route("get-employee-salaryinfo-for-month/{monthid}")]
//        public system.web.http.results.jsonresult<object> getpaymenthi(datamanagerrequest dm, int monthid)
//        {

//            var data = db.monthlysalaries.include(s => s.tbl_monthlysalarydetails).include(e => e.employee).asnotracking().where(w => w.id == monthid).select(s => new
//            {
//                s.id,
//                // amount=s.tbl_monthlysalarydetails.select(t => new { t.amount, t.type }).defaultifempty().sum(t => t.amount * t.type),
//                amount = s.tbl_monthlysalarydetails.firstordefault().amount,
//                entrydate = s.entrydate,
//                salarytype = s.employee.salarytype,
//            });
//            return json<object>(new { result = data, count = data.count() });
//        }
//    }
//}
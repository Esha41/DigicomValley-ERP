using ERP_Project.Data;
using ERP_Project.Models;
using ERP_Project.Models.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Web;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json.Converters;
using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace ERP_Project.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _context;
        public string  getIPaddress;
        public UserController(ApplicationDbContext db, IHttpContextAccessor context, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _context = context;
            _userManager = userManager;
            getIPaddress = getip(_context);
        }
        public static string getip(IHttpContextAccessor context)
        {
            string IPaddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
          
            return IPaddress;
        }
        [Authorize(Roles = "Employee,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
       
        public async Task<IActionResult> EmployeeAttendance(string? smonth,int? syear)
        {
            //sundays,leaves , time record

            //  DateTime date1 = DateTime.UtcNow;
            string timetype = "";
            if (TempData["timetyp"] != null)
            {
                 timetype = Convert.ToString(TempData["timetyp"]);
            }
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            var date = DateTime.Now;
            var date2 = TimeZoneInfo.ConvertTime(date, tz);

            //getting sundays
            var monthNumber = 0;
            int? yearr = null;
            if (smonth != null)
            {
                monthNumber = DateTime.ParseExact(smonth, "MMMM", CultureInfo.CurrentCulture).Month;
                yearr = syear;
            }
            List<int> SundayList = new List<int>();
            if (smonth != null)
            {
              
                var month = monthNumber;
                var year =Convert.ToInt32(yearr);
                var dayName = DayOfWeek.Sunday;
                CultureInfo ci = new CultureInfo("en-US");
                for (int i = 1; i <= ci.Calendar.GetDaysInMonth(year, month); i++)
                {
                    if (new DateTime(year, month, i).DayOfWeek == dayName)
                    {
                        SundayList.Add(i);
                    }
                }
            }
            else
            {
                var month = DateTime.Now.Month;
                var year = DateTime.Now.Year;
                var dayName = DayOfWeek.Sunday;
                CultureInfo ci = new CultureInfo("en-US");
                for (int i = 1; i <= ci.Calendar.GetDaysInMonth(year, month); i++)
                {
                    if (new DateTime(year, month, i).DayOfWeek == dayName)
                    {
                        SundayList.Add(i);
                    }
                }
            }
            //
            //if (_db.IPAddresse.Any(a=>a.IpAddreess==getIPaddress))
            //{
            //    ViewBag.isIPtrue = true;
            //}
            //else
            //{
            //    ViewBag.isIPtrue = false;   //correct ths
            //}
            ViewBag.isIPtrue = true;
            ViewBag.timeTyp = timetype;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
                if ((role.ElementAt(0) == "Employee")|| (role.ElementAt(0) == "DepartmentHead")|| (role.ElementAt(0) == "HRManager") || (role.ElementAt(0) == "TeamHead"))
                {
                    var isAttendanceIpBased = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault().IsAttendanceIPbased;
                    if(isAttendanceIpBased==true)
                    {
                        ViewBag.officeBased = true;
                    }
                    else
                    {
                        ViewBag.officeBased = false;
                    }
                    if ((role.ElementAt(0) == "DepartmentHead") || (role.ElementAt(0) == "TeamHead") || (role.ElementAt(0) == "Admin"))
                    {
                        ViewBag.isIPtrue = true;
                    }
                        //var dateTimeConverter = DateTime.Now.Date;

                        var id = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault().EmployeeId;
                    var empData = new List<EmployeeTimeRecord>();
                    if (smonth != null)
                    {
                         empData = _db.EmployeeTimeRecord.Where(f => f.EmployeeId == id && f.Date.Month == monthNumber && f.Date.Year==syear).OrderBy(f => f.Date).ToList(); //.OrderBy(f => f.Date)
                    }
                    else
                    {
                         empData = _db.EmployeeTimeRecord.Where(f => f.EmployeeId == id && f.Date.Month == date2.Month && f.Date.Year==date2.Year).OrderBy(f => f.Date).ToList(); //.OrderBy(f => f.Date)

                    }
                    var groupByempData = empData.GroupBy(a => a.Date.Date);
                    // var a =date2.Date;
                    //checking if emp has submited its report
                    if (_db.EmployeeReports.Any(a=>a.EmployeeId==id && a.Status && a.Date.Date== date2.Date)) /* a.Date.Date==DateTime.Now.Date*/
                    {
                        ViewBag.isReportSubmitted = true;
                    }
                    else
                    {
                        ViewBag.isReportSubmitted = false;
                    }
               
                    //official report and break time
                    AttendanceVM avm = new AttendanceVM();
                    var PairempDataToday = new List<KeyValuePair<string, string>>();
                    var empDataToday = _db.EmployeeTimeRecord.Where(a => a.EmployeeId == id && a.Date.Date == date2.Date).ToList();
                    //var x = date2.Day;
                    if (_db.AssignShifts.Any(a => a.EmployeeId == id))
                    {

                        var getShifttype = _db.AssignShifts.Where(a => a.EmployeeId == id).FirstOrDefault();
                        var shiftid = getShifttype.ShiftId;
                      
                        var stringDay = date2.ToString("dddd");
                        if (getShifttype != null && getShifttype.ShiftType == "official")
                        {
                            avm.shiftType = "official";
                            var z = _db.OfficialShifts.Where(f => f.Day.Equals(stringDay) && f.ShiftId == shiftid).Select(s => new
                            {
                                s.StartTime,
                                s.BreakTime,
                                // s.MinEndTime,
                                MinEndTime = s.MinEndTime,
                                s.EndTime

                            }).FirstOrDefault();

                            var obj1 = z;// == null ? null : z;
                            if (obj1 == null)
                            {
                                avm.emporgReportTime = null;
                                avm.emporgBreakTime = 0;
                            }

                            else
                            {
                                avm.emporgReportTime = Convert.ToString(string.Format("{0:hh:mm tt}", z.StartTime));
                                // avm.emporgBreakTime = Convert.ToString(string.Format("{0:hh:mm tt}", z.BreakTime));
                                avm.emporgBreakTime = z.BreakTime;
                                avm.emporgEndTime = z.MinEndTime;
                                avm.empExactEndTime = z.EndTime;
                            }
                            foreach (var item in empDataToday)
                            {
                                //when request go to admin, and admin approve it after employee original time, then checkout time will be employee original time
                                if (item.RecordTypeName == "checkout" && item.UserRemarks != null && item.RecordTime.TimeOfDay > avm.empExactEndTime.TimeOfDay)
                                {
                                    item.RecordTime = avm.empExactEndTime;
                                }
                                PairempDataToday.Add(new KeyValuePair<string, string>(item.RecordTypeName, Convert.ToString(string.Format("{0:hh:mm tt}", item.RecordTime))));
                            }
                        }
                        else if (getShifttype != null && getShifttype.ShiftType == "custom")
                        {
                            avm.shiftType = "custom";
                             var z1 = _db.EmployeeTimmings.Where(f => f.Day == stringDay && f.ShiftAssignId == getShifttype.AssignShiftsId).FirstOrDefault();
                            var z = _db.EmployeeTimmings.Where(f => f.ShiftAssignId == getShifttype.AssignShiftsId).ToList();
                            var obj1 = z == null ? null : z;
                            if (obj1 == null)
                            {
                                avm.emporgReportTime2 = null;
                                avm.emporgBreakTime = 0;
                            }
                            var obj2 = z1 == null ? null : z1;
                            if (obj2 == null)
                            {
                                avm.emporgReportTime = null;
                                avm.emporgBreakTime = null;
                                avm.emporgEndTime = null;
                                foreach (var custom in z)
                                {
                                    avm.empExactEndTime2.Add(new KeyValuePair<string, DateTime>(custom.Day, custom.EndTime));
                                    avm.emporgReportTime2.Add(new KeyValuePair<string, string>(custom.Day, Convert.ToString(string.Format("{0:hh:mm tt}", custom.StartTime))));

                                }
                            }
                            else
                            {
                                avm.emporgReportTime = Convert.ToString(string.Format("{0:hh:mm tt}", z1.StartTime));
                                avm.emporgBreakTime = z1.BreakTime;
                                avm.emporgEndTime = z1.MinEndTime;

                                foreach (var custom in z)
                                {
                                    avm.empExactEndTime2.Add(new KeyValuePair<string, DateTime>(custom.Day, custom.EndTime));
                                    avm.emporgReportTime2.Add(new KeyValuePair<string, string>(custom.Day, Convert.ToString(string.Format("{0:hh:mm tt}", custom.StartTime))));
                                    
                                }
                                
                            }
                            var tempdata = avm.empExactEndTime2.Where(s => s.Key == Convert.ToString(date2.DayOfWeek)).FirstOrDefault().Value.Date.TimeOfDay;
                           
                            foreach (var item in empDataToday)
                            {
                                //when request go to admin, and admin approve it after employee original time, then checkout time will be employee original time
                                if (item.RecordTypeName == "checkout" && item.UserRemarks != null && item.RecordTime.TimeOfDay > tempdata)// avm.empExactEndTime.TimeOfDay)
                                {
                                    item.RecordTime = avm.empExactEndTime;
                                }
                                PairempDataToday.Add(new KeyValuePair<string, string>(item.RecordTypeName, Convert.ToString(string.Format("{0:hh:mm tt}", item.RecordTime))));
                            }
                        }
                        else
                        {
                            
                            foreach (var item in empDataToday)
                            {
                               
                                PairempDataToday.Add(new KeyValuePair<string, string>(item.RecordTypeName, Convert.ToString(string.Format("{0:hh:mm tt}", item.RecordTime))));
                            }
                        }
                    }
                    else
                    {

                        foreach (var item in empDataToday)
                        {

                            PairempDataToday.Add(new KeyValuePair<string, string>(item.RecordTypeName, Convert.ToString(string.Format("{0:hh:mm tt}", item.RecordTime))));
                        }
                    }

                    var unappReqToday = _db.CheckoutApprovalRequests.Where(a => a.Date.Date == date2.Date).ToList();
                   
                    if (smonth != null)
                    {
                        avm.getMonthName = smonth;
                       avm.getMonth = smonth;
                        avm.monthBit = 1;
                        avm.getYear =Convert.ToInt32(syear);
                        avm.selectedMonth = monthNumber;
                        avm.employeeLeaves = _db.Leaves.Where(a => a.Status.Contains("Approved") && a.EmployeeId == id && ((a.To.Month == monthNumber && a.Date.Year == syear) || (a.From.Month == monthNumber && a.Date.Year == syear))).ToList();
                    }
                    else
                    {
                        avm.getMonthName = DateTime.Now.ToString("MMMM");
                        var monthNumber2 = DateTime.ParseExact(avm.getMonthName, "MMMM", CultureInfo.CurrentCulture).Month;
                       
                        avm.getMonth = avm.getMonthName;
                        avm.getYear = DateTime.Now.Year;
                        avm.monthBit = 0;
                        avm.selectedMonth = monthNumber2;
                        avm.employeeLeaves = _db.Leaves.Where(a => a.Status.Contains("Approved") && a.EmployeeId == id && ((a.To.Month == DateTime.Now.Month && a.To.Year == DateTime.Now.Year) || (a.From.Month == DateTime.Now.Month && a.From.Year == DateTime.Now.Year))).ToList();

                    }
                 //   avm.announcement = _db.announcements.Where(a => a.Status == true && a.StartDate.Date <= date2.Date && a.EndDate.Date >= date2.Date).ToList(); //date to be cleared
                    avm.empTimingsList = _db.EmployeeTimmings.Include(a=>a.AssignShifts).ThenInclude(a=>a.Employee).Where(s=>s.AssignShifts.EmployeeId==id).ToList();
                    avm.unapprovedcheckouts = unappReqToday;
                    avm.sundays = SundayList;
                    avm.empTimeRecordCred = groupByempData;
                    avm.empTimeRecordToday = PairempDataToday;
                    avm.AllemployeeTimeRecords = empDataToday;  //STATUS of employees
                   // avm.employeeLeaves = approvedLeaves;
                    avm.empId = id;

                    return View(avm);
                }
            }
            return Ok();          
        }
        [HttpPost]
        public async Task<IActionResult> showPrevAttendance(AttendanceVM atvm)
        {
            /*  var monthNumber = DateTime.ParseExact(atvm.getMonth, "MMMM", CultureInfo.CurrentCulture).Month;
              atvm.attendance = _db.EmployeeAttendanceRecord.Where(a => a.EmployeeId == atvm.empId && a.Date.Month == monthNumber).ToList();
              atvm.AllemployeeTimeRecords=_db.EmployeeTimeRecord.Where(a => a.EmployeeId == atvm.empId && a.Date.Month == monthNumber).ToList();
  */
            var monthNumber = DateTime.ParseExact(atvm.getMonth, "MMMM", CultureInfo.CurrentCulture).Month;
            if (/*monthNumber > DateTime.Now.Month || */atvm.getYear >=DateTime.Now.Year)
            {
                return RedirectToAction(nameof(EmployeeAttendance));
            }
            return RedirectToAction(nameof(EmployeeAttendance),new { smonth=atvm.getMonth,syear=atvm.getYear });
        }
            [HttpPost]
        public ActionResult EmployeeAttendance(AttendanceVM avm) 
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            DateTime date2 = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
           // DateTime date2 = DateTime.Now;
             var list = _db.EmployeeTimeRecord.Where(a => a.EmployeeId == avm.empId && a.Date.Date.Equals(date2.Date)).ToList();
            if (list.Any(a => a.RecordTypeName == "checkout"))
            {
                return RedirectToAction("EmployeeAttendance", new { id = avm.empId });
            }

            string reason = avm.employeeEarlyLeavingReason;
            EmployeeTimeRecord etr = new EmployeeTimeRecord();
            etr.RecordTypeName = "checkout";
            etr.RecordTime = date2;
            etr.Date = date2;
            etr.EmployeeId = avm.empId;
            etr.IsApproved = false;
            etr.Status = false;
            etr.UserRemarks = avm.employeeEarlyLeavingReason;
          

            _db.EmployeeTimeRecord.Add(etr);
             _db.SaveChanges();
            //apply table here

            CheckoutApprovalRequest rq = new CheckoutApprovalRequest();
            rq.EmployeeTimeRecordId = etr.EmployeeTimeRecordId;
            rq.ApprovalStatus = "pending";
            rq.Date = date2;
            _db.CheckoutApprovalRequests.Add(rq);
            _db.SaveChanges();

            if (etr.IsApproved == false)
            {
                return RedirectToAction("ShowMsg", new { id = avm.empId });
            }

            return RedirectToAction("EmployeeAttendance");
         
        }
            [HttpPost]
        public ActionResult PostCurrentTimeToDb(string type, int id) //temporarily employee id
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            DateTime date2 = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            if (type == "checkout")
            {
                EmployeeAttendanceRecord at = new EmployeeAttendanceRecord();
                at.EmployeeAttendanceRecordId = 0;
                at.Date = date2;
                at.status = "present";
                at.EmployeeId = id;
                _db.EmployeeAttendanceRecord.Add(at);
                _db.SaveChanges();
            }
            //DateTime date3 = TimeZoneInfo.ConvertTime(time, tz);

            var type2 = "";
            if (type == "checkin" || type == "checkout")
            {
                type2 = type;
            }
            var list = _db.EmployeeTimeRecord.Where(a => a.EmployeeId == id && a.Date.Date.Equals(date2.Date)).ToList();
          
            if ((list.Any(a => a.RecordTypeName == "checkout"))&&type=="checkout")
            {
                var emptrid = list.FirstOrDefault(a => a.RecordTypeName == "checkout").EmployeeTimeRecordId;
                var updateExistingChkout = _db.EmployeeTimeRecord.FirstOrDefault(a => a.EmployeeTimeRecordId == emptrid);
                updateExistingChkout.IsApproved = true;
                updateExistingChkout.RecordTime = date2;
                updateExistingChkout.Date = date2;
                //  updateExistingChkout.UserRemarks = null;
                updateExistingChkout.Status = false;
                _db.EmployeeTimeRecord.Update(updateExistingChkout);
                _db.SaveChanges();
            }
            else
            {
                EmployeeTimeRecord etr = new EmployeeTimeRecord();
                etr.RecordTypeName = type;
                etr.RecordTime = date2;
                etr.EmployeeId = id;
                etr.IsApproved = true;
                etr.Status = false;
                etr.Date = date2;

                _db.EmployeeTimeRecord.Add(etr);
                _db.SaveChanges();
            }
            TempData["timetyp"] = type;
            // return RedirectToAction("EmployeeAttendance", new { timetype = type }); 

            return RedirectToAction("EmployeeAttendance");
        }
        [HttpPost]
        public ActionResult postDailyReport(AttendanceVM avm)
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            DateTime date2 = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            EmployeeReports er = new EmployeeReports();

            if (_db.EmployeeReports.Any(a => a.EmployeeId == avm.empId && a.Date == date2))
            {
                var temp = _db.EmployeeReports.Find(avm.empId);
                _db.EmployeeReports.Remove(temp);
                _db.SaveChanges();

                er.Status = true;
                er.IsApproved = true;
                er.EmployeeId = avm.empId;
                er.ReportDescription = avm.employeeReport;
                er.Date = date2;
                _db.EmployeeReports.Add(er);
                _db.SaveChanges();
            }
            else
            {
                er.Status = true;
                er.IsApproved = true; //temporarily
                er.EmployeeId = avm.empId;
                er.ReportDescription = avm.employeeReport;
                er.Date = date2;
                _db.EmployeeReports.Add(er);
                _db.SaveChanges();
            }
            return RedirectToAction("EmployeeAttendance");
        }
            // GET: UserController

            public ActionResult TermsandConditions()
        {
            return View();
        }

        // GET: UserController/Details/5
        public ActionResult ShowMsg(int id)
        {
            ViewBag.id = id;
            return View();
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult testingTime() //temporarily employee id
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            var date2 = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            return Ok(new { pakZone = date2, curZone = DateTime.Now });
        }
    }
}

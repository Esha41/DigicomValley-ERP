using ERP_Project.Data;
using ERP_Project.Models;
using ERP_Project.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_Project.Controllers
{
    public class AttendanceController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public AttendanceController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)

        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }
        [Authorize(Roles = "Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
        public async Task<IActionResult> Index()
        {
           
            List<int> SundayList = new List<int>();
            var month = DateTime.Now.Month;
            var monthName = DateTime.Now.ToString("MMMM");
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
            var approvedLeaves = _db.Leaves.Where(a => a.Status.Contains("Approved") && ((a.To.Month == DateTime.Now.Month && a.To.Year == DateTime.Now.Year) || (a.From.Month == DateTime.Now.Month && a.From.Year == DateTime.Now.Year))).ToList();

            var empList = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).ToList();/*.Where(a => a.Status == true)*/

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
                if (role.ElementAt(0) == "Admin")
                {
                    AttendanceVM avm = new AttendanceVM();
                    var empTimeRecrdList = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && a.Date.Month.Equals(DateTime.Now.Month)).OrderBy(a => a.Date).Distinct().ToList();

                    avm.EmployeePositionsList = _db.EmployeePositions.Include(a => a.Employee).ToList();
                    avm.reportsList = _db.EmployeeReports.Include(a => a.Employee).Where(a => a.ReportDescription != null).ToList();//&& a.Date.Date == DateTime.Now.Date)
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList.ToList();
                    avm.AllemployeeTimeRecords = empTimeRecrdList;
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);

                }
                else if (role.ElementAt(0) == "DepartmentHead")
                {
                    empList = empList.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).ToList();
                    AttendanceVM avm = new AttendanceVM();
                    List<EmployeeTimeRecord> tempEmpTimeRecrd = new List<EmployeeTimeRecord>();
                    foreach (var x in empList)
                    {
                        if (_db.EmployeeTimeRecord.Any(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)))
                        {
                            var tempo = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)).ToList();
                            tempEmpTimeRecrd.AddRange(tempo);
                        }
                    }

                    foreach (var item in empList)
                    {
                        if (_db.EmployeeReports.Any(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null)) //&&a.Date.Date==DateTime.Now.Date
                        {
                            avm.reportsList.Add(_db.EmployeeReports.FirstOrDefault(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null));//&& a.Date.Date == DateTime.Now.Date)
                        }
                    }

                    //getting joiing date of all employees
                    foreach (var pos in empList)
                    {
                        if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                        {
                            avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                        }
                    }
                    //close
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList.Distinct().ToList();
                    //  avm.AllemployeeTimeRecords = avm.AllemployeeTimeRecords.OrderBy(a => a.Date).ToList();
                    avm.AllemployeeTimeRecords = tempEmpTimeRecrd.OrderBy(a => a.Date).Distinct().ToList();
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);
                }
                else if (role.ElementAt(0) == "HRManager")
                {
                    var refIdUser = empList.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                    empList= empList.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).ToList();

                    AttendanceVM avm = new AttendanceVM();
                    List<EmployeeTimeRecord> tempEmpTimeRecrd = new List<EmployeeTimeRecord>();
                    foreach (var x in empList)
                    {
                        if (_db.EmployeeTimeRecord.Any(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)))
                        {
                            var tempo = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)).ToList();
                            tempEmpTimeRecrd.AddRange(tempo);
                        }
                    }

                    foreach (var item in empList)
                    {
                        if (_db.EmployeeReports.Any(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null)) //&&a.Date.Date==DateTime.Now.Date
                        {
                            avm.reportsList.Add(_db.EmployeeReports.FirstOrDefault(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null));//&& a.Date.Date == DateTime.Now.Date)
                        }
                    }

                    //getting joiing date of all employees
                    foreach (var pos in empList)
                    {
                        if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                        {
                            avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                        }
                    }
                    //close
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList.Distinct().ToList();
                    avm.AllemployeeTimeRecords = tempEmpTimeRecrd.OrderBy(a => a.Date).Distinct().ToList();
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);
                }

                else if (role.ElementAt(0) == "TeamHead")
                {
                    var empId = _db.Employees.Where(s => s.Email == user.Email).FirstOrDefault().EmployeeId;
                    var teamHeadId = _db.Department_Teams_Heads.Where(a => a.EmployeeId == empId);
                    var teamEmp = from te in _db.department_Teams_Employees
                                  join th in teamHeadId
                                  on te.DepartmentTeamsId equals th.HeadId
                                  select te;

                    var empList1 = from e in _db.Employees
                                  join id in teamEmp
                                  on e.EmployeeId equals id.EmployeeId
                                  select e;

                    AttendanceVM avm = new AttendanceVM();
                    List<EmployeeTimeRecord> tempEmpTimeRecrd = new List<EmployeeTimeRecord>();
                    foreach (var x in empList1)
                    {
                        if (_db.EmployeeTimeRecord.Any(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)))
                        {
                            var tempo = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)).ToList();
                            tempEmpTimeRecrd.AddRange(tempo);
                        }
                    }

                    foreach (var item in empList1)
                    {
                        if (_db.EmployeeReports.Any(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null)) //&&a.Date.Date==DateTime.Now.Date
                        {
                            avm.reportsList.Add(_db.EmployeeReports.FirstOrDefault(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null));//&& a.Date.Date == DateTime.Now.Date)
                        }
                    }

                    //getting joiing date of all employees
                    foreach (var pos in empList1)
                    {
                        if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                        {
                            avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                        }
                    }
                    //close
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList1.Distinct().ToList();
                    avm.AllemployeeTimeRecords = tempEmpTimeRecrd.OrderBy(a => a.Date).Distinct().ToList();
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);
                }
                else
                {
                    AttendanceVM avm = new AttendanceVM();
                    var empTimeRecrdList = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && a.Date.Month.Equals(DateTime.Now.Month)).OrderBy(a => a.Date).Distinct().ToList();

                    avm.EmployeePositionsList = _db.EmployeePositions.Include(a => a.Employee).ToList();
                    avm.reportsList = _db.EmployeeReports.Include(a => a.Employee).Where(a => a.ReportDescription != null).ToList();//&& a.Date.Date == DateTime.Now.Date)
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList.ToList();
                    avm.AllemployeeTimeRecords = empTimeRecrdList;
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);

                }

            }
            else
            {
                string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                return Redirect(link);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Index(AttendanceVM atvm)
        {
            if (_db.EmployeeTimeRecord.Any(t => t.EmployeeId == atvm.empId && t.Date.Date == atvm.Chdate && t.RecordTypeName == "checkout"&& atvm.ChangeAttendanceStatusReason!=null))
            {
                var chngStatus = _db.EmployeeTimeRecord.Where(t => t.EmployeeId == atvm.empId && t.Date.Date == atvm.Chdate && t.RecordTypeName == "checkout").FirstOrDefault();
                chngStatus.AdminRemarks = atvm.ChangeAttendanceStatusReason;
                chngStatus.IsApproved = atvm.AtToggle;
                _db.EmployeeTimeRecord.Update(chngStatus);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            //getting sundays
            var monthNumber = 0;
            List<int> SundayList = new List<int>();
            if (atvm.getMonth != null)
            {
                monthNumber = DateTime.ParseExact(atvm.getMonth, "MMMM", CultureInfo.CurrentCulture).Month;
             
                var month = monthNumber;
                var year1 = atvm.getYear;
                var dayName = DayOfWeek.Sunday;
                CultureInfo ci = new CultureInfo("en-US");
                for (int i = 1; i <= ci.Calendar.GetDaysInMonth(year1, month); i++)
                {
                    if (new DateTime(year1, month, i).DayOfWeek == dayName)
                    {
                        SundayList.Add(i);
                    }
                }
            }

            AttendanceVM avm = new AttendanceVM();
            avm.reportsList = _db.EmployeeReports.Include(a => a.Employee).Where(a => a.ReportDescription != null).ToList();
            var empList = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(s=>s.Status==true||s.Status==false);
            var empTimeRecrdList = _db.EmployeeTimeRecord.Where(a => a.RecordTypeName == "checkout" && a.Date.Month.Equals(DateTime.Now.Month)).OrderBy(a => a.Date).ToList();
            var approvedLeaves =_db.Leaves.Where(a => a.Status.Contains("Approved") && ((a.To.Month == monthNumber && a.Date.Year == atvm.getYear) || (a.From.Month == monthNumber && a.Date.Year == atvm.getYear))).ToList();

            //getting joiing date of all employees
            foreach (var pos in empList)
            {
                if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                {
                    avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                }
            }
            //close
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (role.ElementAt(0) == "Admin")
            {
                empList = empList.Where(a => a.Status == true);

            }
            else if (role.ElementAt(0) == "DepartmentHead")
            {
                empList = empList.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).Distinct();

            }
            else if (role.ElementAt(0) == "HRManager")
            {
                var refIdUser = empList.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                empList = empList.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).Distinct();

            }
            else if (role.ElementAt(0) == "TeamHead")
            {
                var empId = _db.Employees.Where(s => s.Email == user.Email).FirstOrDefault().EmployeeId;
                var teamHeadId = _db.Department_Teams_Heads.Where(a => a.EmployeeId == empId);
                var teamEmp = from te in _db.department_Teams_Employees
                              join th in teamHeadId
                              on te.DepartmentTeamsId equals th.HeadId
                              select te;

                empList = from e in _db.Employees
                              join id in teamEmp
                              on e.EmployeeId equals id.EmployeeId
                              select e;
            }

           /* else
            {
                empList = empList.Where(a => a.Status == true);
            }*/
            if (atvm.getYear > DateTime.Now.Year || (atvm.getYear==DateTime.Now.Year && monthNumber>DateTime.Now.Month))
            {
                ViewBag.IsRecordExist = false;
                return View(avm);
            }
            else
            {
                ViewBag.IsRecordExist = true;
            }
            if (atvm.departments != null && atvm.departments.DepartmentId != 0 && (atvm.getMonth != null) && (monthNumber != DateTime.Now.Month) && atvm.getYear != 0)
            {
                var year = DateTime.Now.Year;
                var empTimeRecrdListbyMonth = _db.EmployeeTimeRecord.Where(a => (a.RecordTypeName == "checkout") && a.Date.Month == monthNumber && a.Date.Year == atvm.getYear).OrderBy(a => a.Date).ToList();
                var empListbydep = _db.Employees.Where(a => a.DepartmentId == atvm.departments.DepartmentId).ToList();
                avm.AllEmployees = empListbydep;
                avm.selectedMonth = monthNumber;
                avm.getYear = atvm.getYear;
                avm.AllemployeeTimeRecords = empTimeRecrdListbyMonth;
                avm.employeeLeaves = approvedLeaves;
                avm.sundays = SundayList;
                return View(avm);
            }
            else if ((monthNumber== DateTime.Now.Month)&& (atvm.getYear == DateTime.Now.Year))
            {
                return RedirectToAction(nameof(Index));
            }
            else if ((atvm.getMonth != null)/*&&(monthNumber!= DateTime.Now.Month)&& (atvm.getYear != DateTime.Now.Year)*/ && atvm.getYear != 0)
            {

                /////////////////////////////////////////
                var year = DateTime.Now.Year;
                var empTimeRecrdListbyMonth = _db.EmployeeTimeRecord.Where(a => (a.RecordTypeName == "checkout") && a.Date.Year == atvm.getYear && a.Date.Month == monthNumber).OrderBy(a => a.Date).ToList();
                avm.reportsList = _db.EmployeeReports.Include(a => a.Employee).Where(a => a.ReportDescription != null).ToList();
                avm.AllEmployees = empList.Distinct().ToList();
                avm.AllemployeeTimeRecords = empTimeRecrdListbyMonth;
                avm.selectedMonth = monthNumber;
                avm.getYear = atvm.getYear;
                avm.employeeLeaves = approvedLeaves;
                avm.sundays = SundayList;
                return View(avm);
            }
          /*  if (atvm.employees != null)
            {
                var empListbydep = _db.Employees.Where(a => a.EmployeeId == atvm.employees.EmployeeId).ToList();
                avm.AllEmployees = empListbydep;
                avm.AllemployeeTimeRecords = empTimeRecrdList;
                avm.employeeLeaves = approvedLeaves;
                avm.reportsList = _db.EmployeeReports.Include(a => a.Employee).Where(a => a.Employee.Status == true && a.ReportDescription != null).ToList();
                // avm.sundays = SundayList;
                return View(avm);
            }*/
            /*  avm.AllEmployees = empList;
              avm.AllemployeeTimeRecords = empTimeRecrdList;
              avm.employeeLeaves = approvedLeaves;*/

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
        public async Task<ActionResult> ApprovalCheckoutRequest()
        {
           
/*
            var dateTimeConverter = DateTime.Now.Date;
            var universalTime = dateTimeConverter.ToUniversalTime().Date;*/
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            DateTime dateTimeConverter = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            string[] referenceId = new string[2];
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            var empList = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.Status == true).ToList();

            if (role.ElementAt(0)=="Admin") 
            {
                AttendanceVM avm = new AttendanceVM();
                avm.users = _db.Users.ToList();
                avm.AllEmployees = empList;
                if (avm.AllEmployees.Count() > 0)
                {
                    foreach (var obj in avm.AllEmployees)
                    {
                        if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId))
                        {
                            avm.unapprovedcheckouts.AddRange(_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending").ToList());

                        }

                    }
                }
                return View(avm);
            }
            else if (role.ElementAt(0) == "DepartmentHead")
            {
                var applicationDbContext = empList.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).Distinct().ToList();

                AttendanceVM avm = new AttendanceVM();
                avm.users = _db.Users.ToList();
                avm.AllEmployees = empList;
                if (applicationDbContext.Count() > 0)
                {
                    foreach (var obj in applicationDbContext.Distinct())
                    {
                        if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId ))
                        {
                            avm.unapprovedcheckouts.AddRange(_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a =>a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending").ToList());

                        }
                    }
                }
                return View(avm);
            }
            else if (role.ElementAt(0) == "HRManager")
            {
                var refIdUser = empList.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                var applicationDbContext = empList.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).Distinct().ToList();
    
                AttendanceVM avm = new AttendanceVM();
                avm.users = _db.Users.ToList();
                avm.AllEmployees = empList;
                if (applicationDbContext.Count() > 0)
                {
                    foreach (var obj1 in applicationDbContext.Distinct())
                    {
                        if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj1.EmployeeId ))  //&& a.Date.Date == dateTimeConverter.Date
                        {
                            avm.unapprovedcheckouts.AddRange(_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => a.EmployeeTimeRecords.EmployeeId == obj1.EmployeeId&&a.ApprovalStatus=="pending").ToList());
                        }
                    }
                }
                return View(avm);
            }
            else
            {
                AttendanceVM avm = new AttendanceVM();
                avm.users = _db.Users.ToList();
                avm.AllEmployees = _db.Employees.ToList();
                avm.unapprovedcheckouts = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => a.ApprovalStatus == "pending").ToList();

                return View(avm);
            }
        }
        
        public async Task<ActionResult> ApprovalCheckoutRequestPost(int? id,int status, int? redirect)
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            DateTime date2 = TimeZoneInfo.ConvertTime(DateTime.Now, tz);

            //var dateTimeConverter = DateTime.Now.Date; // DateTime.UtcNow.AddDays(1).Date;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);

        
            var emp = 0;
            if (role.ElementAt(0) == "DepartmentHead")
            {
                 emp = _db.Employees.Include(v => v.Department).Where(a => a.Email == user.Email).FirstOrDefault().EmployeeId;
            }

                var updateStatus = _db.EmployeeTimeRecord.Where(a => a.EmployeeTimeRecordId == id).FirstOrDefault();
            //getting employee original end tym
            var getShifttype = _db.AssignShifts.Where(a => a.EmployeeId == updateStatus.EmployeeId).FirstOrDefault();

            var stringDay = DateTime.Now.ToString("dddd");
            DateTime endTym = DateTime.Now;
            if (getShifttype != null && getShifttype.ShiftType == "official")
            {
                var z = _db.OfficialShifts.Where(f => f.Day.Equals(stringDay)).Select(s => new
                {
                    s.EndTime,
                }).FirstOrDefault();
                endTym = z.EndTime;
            }
            if (getShifttype != null && getShifttype.ShiftType == "custom")
            {
                var z = _db.EmployeeTimmings.Where(f => f.Day == stringDay && f.ShiftAssignId == getShifttype.AssignShiftsId).Select(a => new
                {
                    a.EndTime,
                }).FirstOrDefault();
                endTym = z.EndTime;
            }
            //
            if (status == 1)
            {
                var chk = _db.CheckoutApprovalRequests.Include(a=>a.EmployeeTimeRecords).FirstOrDefault(a => a.EmployeeTimeRecordId == id &&/*&& a.Date.Date == date2.Date && */(a.ApprovalStatus == "pending" || a.ApprovalStatus == "declined"));

                EmployeeAttendanceRecord at = new EmployeeAttendanceRecord();
                at.EmployeeAttendanceRecordId = 0;
                at.Date = chk.Date;
                at.status = "present";
                at.EmployeeId = chk.EmployeeTimeRecords.EmployeeId;
                _db.EmployeeAttendanceRecord.Add(at);
                _db.SaveChanges();

              
                chk.ResponseTime = chk.Date;
              //  chk.Date = date2;
                    chk.ApprovalStatus = "approved";
                    chk.EmployeeTimeRecordId = id;
                chk.Department_Teams_HeadsId = null;//_db.Department_Teams_Heads.Where(a => a.EmployeeId == emp).FirstOrDefault().Department_Teams_HeadsId;
                chk.ReferenceUserId = Guid.Parse(userId);
                //admin id? jisny request response ki h?
                _db.CheckoutApprovalRequests.Update(chk);
                    _db.SaveChanges();
              
                updateStatus.IsApproved = true;
                updateStatus.RecordTime = chk.Date;
                updateStatus.Date = chk.Date;
              /*  if (updateStatus.RecordTypeName == "checkout" && updateStatus.UserRemarks != null && updateStatus.RecordTime.TimeOfDay > endTym.TimeOfDay)
                {
                    updateStatus.RecordTime = endTym;
                }*/
                _db.EmployeeTimeRecord.Update(updateStatus);
                _db.SaveChanges();
            }
            if (status == 0)
            {
                var chk = _db.CheckoutApprovalRequests.FirstOrDefault(a => a.EmployeeTimeRecordId == id /*&& a.Date.Date == date2.Date*/ && (a.ApprovalStatus == "declined" ||a.ApprovalStatus == "approved"));
         
                chk.ResponseTime = chk.Date;
                chk.Date = chk.Date;
                chk.ApprovalStatus = "declined";
                chk.EmployeeTimeRecordId = id;
                //admin id? jisny request response ki h?
                chk.ReferenceUserId = Guid.Parse(userId);
                _db.CheckoutApprovalRequests.Update(chk);
                _db.SaveChanges();


                var delReport=_db.EmployeeReports.Where(a => a.EmployeeId == updateStatus.EmployeeId).FirstOrDefault();
                _db.EmployeeReports.Remove(delReport);
                _db.SaveChanges();


                updateStatus.Status = true;
                updateStatus.IsApproved = false;
                updateStatus.RecordTime = chk.Date;
                updateStatus.Date = chk.Date;
                if (updateStatus.RecordTypeName == "checkout" && updateStatus.UserRemarks != null && updateStatus.RecordTime.TimeOfDay > endTym.TimeOfDay)
                {
                    updateStatus.RecordTime = endTym;
                }
                _db.EmployeeTimeRecord.Update(updateStatus);
                _db.SaveChanges();
            }
            if (redirect != 1)
            {
                return RedirectToAction("ApprovalCheckoutRequest");
            }
            else
            {
                string link = Request.Scheme + "://" + Request.Host + "/";

                return Redirect(link);
            }
        }
        [HttpPost]
        public async Task<ActionResult> GetCompanies()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);           
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
                var companies = _db.Companies.ToList();
                List<SelectListItem> CompanyNames = new List<SelectListItem>();
                companies.ForEach(x =>
                {
                    CompanyNames.Add(new SelectListItem { Text = x.CompanyName, Value = x.CompanyId.ToString() });
                });
                ViewData["companyId"] = new SelectList(companies, "CompanyId", "CompanyName"); ;
                return Json(CompanyNames);
         //   }
        }
    
        [HttpPost]
        public async Task<ActionResult> GetDepartmentsByCompanyId(int Companyid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
                var departments = _db.Departments.Where(a => a.CompanyId == Companyid).ToList();
                List<SelectListItem> DepartmentNames = new List<SelectListItem>();
                departments.ForEach(x =>
                {
                    DepartmentNames.Add(new SelectListItem { Text = x.DepartmentName, Value = x.DepartmentId.ToString() });
                });
                ViewData["departmentId"] = new SelectList(departments, "DepartmentId", "DepartmentName");
                return Json(DepartmentNames);
      
        }
        [HttpPost]
        public  async Task<IActionResult> GetEmployeesByDepartmentId(int DepartmentId)
        {
          

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            var employees = _db.Employees.Where(a => a.DepartmentId == DepartmentId && a.Status == true).ToList();
            if (role.ElementAt(0) == "DepartmentHead")
            {
                employees = employees.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).Distinct().ToList();

            }
            else if (role.ElementAt(0) == "HRManager")
            {

                var refIdUser = employees.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                employees = employees.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).Distinct().ToList();

            }
            List<SelectListItem> EmployeeNames = new List<SelectListItem>();
            employees.ForEach(x =>
            {
                EmployeeNames.Add(new SelectListItem { Text = x.FullName, Value = x.EmployeeId.ToString() });
            });
            ViewData["employeeId"] = new SelectList(employees, "EmployeeId", "FullName");
            return Json(EmployeeNames);


        }

        // GET: AttendanceController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: AttendanceController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: AttendanceController/Create
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

        // GET: AttendanceController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: AttendanceController/Edit/5
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

        // GET: AttendanceController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: AttendanceController/Delete/5
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
        /* [Authorize(Roles = "Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
        public async Task<IActionResult> Index()
        {
           var empList = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a=>a.Status==true);

            List<int> SundayList = new List<int>();
            var month = DateTime.Now.Month;
            var monthName = DateTime.Now.ToString("MMMM");
            var year = DateTime.Now.Year;
            var dayName = DayOfWeek.Sunday;
            CultureInfo ci = new CultureInfo("en-US");
            for(int i=1;i<=ci.Calendar.GetDaysInMonth(year,month);i++)
            {
                if(new DateTime(year,month,i).DayOfWeek==dayName)
                {
                    SundayList.Add(i);
                }
            }
            var approvedLeaves = _db.Leaves.Where(a => a.Status.Contains("Approved") &&((a.To.Month == DateTime.Now.Month && a.To.Year == DateTime.Now.Year) || (a.From.Month == DateTime.Now.Month && a.From.Year == DateTime.Now.Year))).ToList();


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
                if (role.ElementAt(0) == "Admin")
                {
                    var empList = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation);
                    AttendanceVM avm = new AttendanceVM();

                  //  var approvedLeaves = _db.Leaves.Include(a => a.Employee).Where(a => a.Status == "Approved").ToList();
                    var empTimeRecrdList = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && a.Date.Month.Equals(DateTime.Now.Month)).OrderBy(a => a.Date).Distinct().ToList();


                    avm.EmployeePositionsList = _db.EmployeePositions.Include(a => a.Employee).ToList();
                    avm.reportsList=_db.EmployeeReports.Include(a=>a.Employee).Where(a => a.ReportDescription != null).ToList();//&& a.Date.Date == DateTime.Now.Date)
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList.ToList();
                    avm.AllemployeeTimeRecords = empTimeRecrdList;
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);

                }
                 else if (role.ElementAt(0) == "DepartmentHead")
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();

                    var empList = _db.Employees.Where(a=>(a.ReferenceUserId == employee.ReferenceUserId || a.Email == employee.Email || a.ReferenceUserId.ToString() == user.Id)).ToList(); //get only current month record
                    //geting emp created by hr manager
                     List<Employees> temp = new List<Employees>();

                    foreach (var abc in empList.ToList())
                    {
                        if (_db.Employees.Include(s => s.Department).Any(a => a.Email == abc.Email && a.Department.DepartmentName == "HR"))
                        {
                            //   hremails.Add(emp.Email);
                            var hruser = _db.Users.Where(a => a.Email == abc.Email).FirstOrDefault().Id;
                            if (_db.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                            {
                                empList.AddRange(_db.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).ToList());
                                temp.AddRange(_db.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).ToList());
                            }
                        }
                    }
                    AttendanceVM avm = new AttendanceVM();
                    //emp time recrd
                   // List<Leaves> tempapprovedLeaves = new List<Leaves>();
                  *//*  var approvedLeaves = _db.Leaves.Include(a => a.Employee).Where(a => a.Status == "Approved" && (a.Employee.ReferenceUserId == employee.ReferenceUserId || a.Employee.ReferenceUserId.ToString() == user.Id)).ToList();
                    tempapprovedLeaves.AddRange(approvedLeaves);*//*

                    List<EmployeeTimeRecord> tempEmpTimeRecrd = new List<EmployeeTimeRecord>();
                    foreach (var x in empList)
                    {
                        if (_db.EmployeeTimeRecord.Any(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)))
                        {
                            var tempo = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)).ToList();
                            tempEmpTimeRecrd.AddRange(tempo);
                        }
                    }

                    if (temp.Count > 0)
                    {
                        foreach (var item in temp.ToList())
                        {
                            var t = item.EmployeeId;
                          *//*  if ((_db.Leaves.Any(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t)) && (_db.Leaves.FirstOrDefault(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t)) != null)
                            {
                                tempapprovedLeaves.Add(_db.Leaves.Include(a => a.Employee).FirstOrDefault(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t));
                            }*//*
                            if ((_db.EmployeeTimeRecord.Include(a => a.Employee).Any(a => a.RecordTypeName == "checkout" && (a.Employee.ReferenceUserId == employee.ReferenceUserId || a.Employee.ReferenceUserId.ToString() == user.Id) && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t)) && (_db.EmployeeTimeRecord.FirstOrDefault(a => a.RecordTypeName == "checkout" && a.Employee.ReferenceUserId.ToString() == user.Id && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t)) != null)
                            {
                                tempEmpTimeRecrd.AddRange(_db.EmployeeTimeRecord.Where(a => a.RecordTypeName == "checkout" && a.Employee.ReferenceUserId.ToString() == user.Id && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t));
                            }
                        }
                    }
                    //close
                    foreach (var item in empList)
                    {
                        if(_db.EmployeeReports.Any(a=>a.EmployeeId==item.EmployeeId&&a.Status==true&&a.ReportDescription!=null)) //&&a.Date.Date==DateTime.Now.Date
                        {
                            avm.reportsList.Add(_db.EmployeeReports.FirstOrDefault(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null));//&& a.Date.Date == DateTime.Now.Date)
                        }
                    }

                    //getting joiing date of all employees
                    foreach (var pos in empList)
                    {
                        if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                        {
                            avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                        }
                    }
                    //close
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList.Distinct().ToList();
                    //  avm.AllemployeeTimeRecords = avm.AllemployeeTimeRecords.OrderBy(a => a.Date).ToList();
                    avm.AllemployeeTimeRecords = tempEmpTimeRecrd.OrderBy(a => a.Date).Distinct().ToList();
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);
                }
                else if (role.ElementAt(0) == "HRManager")
                {
                    List<Employees> temp = new List<Employees>();
                    var hr = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    var applicationDbContext = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.ReferenceUserId == hr.ReferenceUserId || a.ReferenceUserId.ToString() == user.Id).ToList(); //
                    //getting emp created by another hr
                    var email = "";
                    foreach (var emp in applicationDbContext)
                    {
                        if (_db.Employees.Include(s => s.Department).Any(a => a.Email == emp.Email && a.Department.DepartmentName == "HR"))
                        {
                            email = emp.Email;
                        }
                    }
                    if (email != "")
                    {
                        var hruser = _db.Users.Where(a => a.Email == email).FirstOrDefault().Id;
                        if (_db.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                        {
                            applicationDbContext.Add(_db.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).FirstOrDefault());
                        }
                    }
                    //close
                    //getting employyees created by dep head ref id
                    List<IdentityUser> AdmindepHeadList = new List<IdentityUser>();
                    List<Department_Teams_Heads> depHeadList = new List<Department_Teams_Heads>();
                    foreach (var item in applicationDbContext)
                    {
                        if (_db.Department_Teams_Heads.Any(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId))
                        {
                            depHeadList.Add(_db.Department_Teams_Heads.FirstOrDefault(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId));
                        }
                    }

                    var users = _db.Users.ToList();
                    foreach (var item in depHeadList.Distinct())
                    {
                        if (applicationDbContext.Any(a => a.EmployeeId == item.EmployeeId))
                        {
                            AdmindepHeadList.Add(_db.Users.Where(s => s.Email == item.Employee.Email).FirstOrDefault()); //getting dep head
                        }
                    }
                    foreach (var item in AdmindepHeadList.Distinct())
                    {
                        if (_db.Employees.Any(s => s.ReferenceUserId.ToString() == item.Id))
                        {
                            temp.Add(_db.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                            applicationDbContext.Add(_db.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                        }
                    }
                    //close

                    AttendanceVM avm = new AttendanceVM();
                  
                    //emp time recrd
                   *//* List<Leaves> tempapprovedLeaves = new List<Leaves>();
                    var approvedLeaves = _db.Leaves.Include(a => a.Employee).Where(a => a.Status == "Approved" && (a.Employee.ReferenceUserId == hr.ReferenceUserId || a.Employee.ReferenceUserId.ToString() == user.Id)).ToList();
                    tempapprovedLeaves.AddRange(approvedLeaves);
*//*
                    List<EmployeeTimeRecord> tempEmpTimeRecrd = new List<EmployeeTimeRecord>();
                    //  var empTimeRecrdList = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && (a.Employee.ReferenceUserId == hr.ReferenceUserId || a.Employee.ReferenceUserId.ToString() == user.Id) && a.Date.Month.Equals(DateTime.Now.Month)).OrderBy(a => a.Date).ToList();

                    foreach (var x in applicationDbContext)
                    {
                        if (_db.EmployeeTimeRecord.Any(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)))
                         {
                            var tempo = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)).ToList();
                            tempEmpTimeRecrd.AddRange(tempo);
                                }
                            //  tempEmpTimeRecrd.AddRange(empTimeRecrdList);
                    }
                  

                    if (temp.Count > 0)
                    {
                        foreach (var item in temp.ToList())
                        {
                            var t = item.EmployeeId;
                           *//* if ((_db.Leaves.Any(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t)) && (_db.Leaves.FirstOrDefault(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t)) != null)
                            {
                                tempapprovedLeaves.Add(_db.Leaves.Include(a => a.Employee).FirstOrDefault(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t));
                            }*//*
                            if ((_db.EmployeeTimeRecord.Include(a => a.Employee).Any(a => a.RecordTypeName == "checkout" && (a.Employee.ReferenceUserId == hr.ReferenceUserId || a.Employee.ReferenceUserId.ToString() == user.Id) && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t)) && (_db.EmployeeTimeRecord.FirstOrDefault(a => a.RecordTypeName == "checkout" && a.Employee.ReferenceUserId.ToString() == user.Id && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t)) != null)
                            {
                                tempEmpTimeRecrd.AddRange(_db.EmployeeTimeRecord.Where(a => a.RecordTypeName == "checkout" && a.Employee.ReferenceUserId.ToString() == user.Id && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t));
                            }
                        }
                    }
                    //close
                    foreach (var item in applicationDbContext)
                    {
                        if (_db.EmployeeReports.Any(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null)) //&&a.Date.Date==DateTime.Now.Date
                        {
                            avm.reportsList.Add(_db.EmployeeReports.FirstOrDefault(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null));//&& a.Date.Date == DateTime.Now.Date)
                        }
                    }

                    //getting joiing date of all employees
                    foreach (var pos in applicationDbContext)
                    {
                        if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                        {
                            avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                        }
                    }
                    //close
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = applicationDbContext.Distinct().ToList();
                    avm.AllemployeeTimeRecords = tempEmpTimeRecrd.OrderBy(a => a.Date).Distinct().ToList();
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);
                }
                else if (role.ElementAt(0) == "TeamHead")
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();

                    var teamHead = _db.Department_Teams_Heads.Where(a => a.EmployeeId == employee.EmployeeId).FirstOrDefault();
                    var teamEmployees = _db.department_Teams_Employees.Include(a => a.Employee).Include(a => a.GetDepartmentTeam).Where(a => a.DepartmentTeamsId == teamHead.HeadId).ToList();
                    List<Employees> empList = new List<Employees>();
                    List<EmployeeTimeRecord> empTimeRecrdList = new List<EmployeeTimeRecord>();
                  //  List<Leaves> approvedLeaves = new List<Leaves>();
                    foreach (var item in teamEmployees)
                    {
                        empList.Add(_db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).FirstOrDefault(a => a.Email != employee.Email && a.EmployeeId == item.EmployeeId));
                        if (_db.EmployeeTimeRecord.Include(a => a.Employee).Any(a => a.RecordTypeName == "checkout" && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == item.EmployeeId))
                        {
                            empTimeRecrdList.AddRange(_db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == item.EmployeeId));
                        }
                      *//*  if (_db.Leaves.Include(a => a.Employee).Any(a => a.Employee.DepartmentId == employee.DepartmentId && a.Status == "Approved" && a.EmployeeId == item.EmployeeId))
                        {
                            approvedLeaves.Add(_db.Leaves.Include(a => a.Employee).FirstOrDefault(a => a.Employee.DepartmentId == employee.DepartmentId && a.Status == "Approved" && a.EmployeeId == item.EmployeeId));
                        }*//*
                    }

                    AttendanceVM avm = new AttendanceVM();
                    foreach (var item in empList)
                    {
                        if (_db.EmployeeReports.Any(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null)) //&&a.Date.Date==DateTime.Now.Date
                        {
                            if (_db.EmployeeReports.Any(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null))
                            {
                                avm.reportsList.Add(_db.EmployeeReports.FirstOrDefault(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null));//&& a.Date.Date == DateTime.Now.Date)
                            }
                        }
                    }
                    //getting joiing date of all employees
                    foreach (var pos in empList)
                    {
                        if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                        {
                            avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                        }
                    }
                    //close
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList.ToList();
                    avm.AllemployeeTimeRecords = empTimeRecrdList.OrderBy(a => a.Date).Distinct().ToList();
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);
                }
                else if (role.ElementAt(0) == "Admin")
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    var applicationDbContext= _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.ReferenceUserId.ToString() == user.Id).ToList(); //get only current month record

                    List<Employees> temp = new List<Employees>();//getting employees created by admin department head 
                    List<IdentityUser> AdmindepHeadList = new List<IdentityUser>();
                    List<Department_Teams_Heads> depHeadList = new List<Department_Teams_Heads>();
                    foreach (var item in applicationDbContext)
                    {
                        if (_db.Department_Teams_Heads.Any(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId))
                        {
                            depHeadList.Add(_db.Department_Teams_Heads.FirstOrDefault(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId));
                        }
                    }

                    var users = _db.Users.ToList();
                    foreach (var item in depHeadList.Distinct())
                    {
                        if (applicationDbContext.Any(a => a.EmployeeId == item.EmployeeId))
                        {
                            AdmindepHeadList.Add(_db.Users.Where(s => s.Email == item.Employee.Email).FirstOrDefault()); //getting dep head
                        }
                    }
                    foreach (var item in AdmindepHeadList.Distinct())
                    {
                        if (_db.Employees.Any(s => s.ReferenceUserId.ToString() == item.Id))
                        {
                            temp.Add(_db.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                            applicationDbContext.Add(_db.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                        }
                    }
                    //close
                    //getting employees created by hr
                    foreach (var abc in applicationDbContext.ToList())
                    {
                        if (_db.Employees.Include(s => s.Department).Any(a => a.Email == abc.Email && a.Department.DepartmentName == "HR"))
                        {
                            //   hremails.Add(emp.Email);
                            var hruser = _db.Users.Where(a => a.Email == abc.Email).FirstOrDefault().Id;
                            if (_db.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                            {
                                applicationDbContext.AddRange(_db.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).ToList());
                            }
                        }
                    }

                    //getting joiing date of all employees
                    AttendanceVM avm = new AttendanceVM();
                    foreach (var pos in applicationDbContext)
                    {
                        if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                        {
                            avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                        }
                    }
                        //close
                      *//*  List<Leaves> tempapprovedLeaves = new List<Leaves>();
                    var approvedLeaves=_db.Leaves.Include(a => a.Employee).Where(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id).ToList();
                    tempapprovedLeaves.AddRange(approvedLeaves);*//*

                    List<EmployeeTimeRecord> tempEmpTimeRecrd = new List<EmployeeTimeRecord>();
                    *//*  var empTimeRecrdList = _db.EmployeeTimeRecord.Where(a => a.RecordTypeName == "checkout" && a.Employee.ReferenceUserId.ToString() == user.Id && a.Date.Month.Equals(DateTime.Now.Month)).OrderBy(a => a.Date).ToList();
                      tempEmpTimeRecrd.AddRange(empTimeRecrdList);*//*
                    foreach (var x in applicationDbContext)
                    {
                        if (_db.EmployeeTimeRecord.Any(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)))
                        {
                            var tempo = _db.EmployeeTimeRecord.Include(a => a.Employee).Where(a => a.RecordTypeName == "checkout" && (a.EmployeeId == x.EmployeeId) && a.Date.Month.Equals(DateTime.Now.Month)).ToList();
                            tempEmpTimeRecrd.AddRange(tempo);
                        }
                        //  tempEmpTimeRecrd.AddRange(empTimeRecrdList);
                    }
                    if (temp.Count > 0)
                    {
                        foreach (var item in temp.ToList())
                        {
                            var t = item.EmployeeId;
                         *//*   if ((_db.Leaves.Any(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t))&&(_db.Leaves.FirstOrDefault(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t))!=null)
                            {
                                tempapprovedLeaves.Add(_db.Leaves.Include(a => a.Employee).FirstOrDefault(a => a.Status == "Approved" && a.Employee.ReferenceUserId.ToString() == user.Id && a.EmployeeId == t));
                            }*//*
                            if ((_db.EmployeeTimeRecord.Any(a => a.RecordTypeName == "checkout" && a.Employee.ReferenceUserId.ToString() == user.Id && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t))&& (_db.EmployeeTimeRecord.FirstOrDefault(a => a.RecordTypeName == "checkout" && a.Employee.ReferenceUserId.ToString() == user.Id && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t))!=null)
                            {
                                tempEmpTimeRecrd.Add(_db.EmployeeTimeRecord.FirstOrDefault(a => a.RecordTypeName == "checkout" && a.Employee.ReferenceUserId.ToString() == user.Id && a.Date.Month.Equals(DateTime.Now.Month) && a.EmployeeId == t));
                            }
                        }
                    }
                   
                    foreach (var item in applicationDbContext)
                    {
                        if (_db.EmployeeReports.Any(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null)) //&&a.Date.Date==DateTime.Now.Date
                        {
                            avm.reportsList.AddRange(_db.EmployeeReports.Where(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null));//&& a.Date.Date == DateTime.Now.Date)
                        }
                    }
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = applicationDbContext.Distinct().ToList();
                    avm.AllemployeeTimeRecords = tempEmpTimeRecrd.OrderBy(a => a.Date).Distinct().ToList();
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);
                }
                else
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();

                    var empList = _db.Employees.ToList(); //get only current month record
                    var empTimeRecrdList = _db.EmployeeTimeRecord.Where(a => a.RecordTypeName == "checkout" && a.Date.Month.Equals(DateTime.Now.Month)).OrderBy(a => a.Date).ToList();
                   // var approvedLeaves = _db.Leaves.Where(a => a.Status == "Approved").ToList();

                    AttendanceVM avm = new AttendanceVM();
                    foreach (var item in empList)
                    {
                        if (_db.EmployeeReports.Any(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null)) //&&a.Date.Date==DateTime.Now.Date
                        {
                            avm.reportsList.Add(_db.EmployeeReports.FirstOrDefault(a => a.EmployeeId == item.EmployeeId && a.Status == true && a.ReportDescription != null));//&& a.Date.Date == DateTime.Now.Date)
                        }
                    }
                    //getting joiing date of all employees
                    foreach (var pos in empList)
                    {
                        if (_db.EmployeePositions.Any(a => a.EmployeeId == pos.EmployeeId))
                        {
                            avm.EmployeePositionsList.Add(_db.EmployeePositions.FirstOrDefault(a => a.EmployeeId == pos.EmployeeId));
                        }
                    }
                    //close
                    avm.getYear = DateTime.Now.Year;
                    avm.getMonth = monthName;
                    avm.AllEmployees = empList;
                    avm.AllemployeeTimeRecords = empTimeRecrdList;
                    avm.employeeLeaves = approvedLeaves;
                    avm.sundays = SundayList;
                    return View(avm);
                }
            }
            else
            {
                string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                return Redirect(link);
            }
        }
       */
    }
}

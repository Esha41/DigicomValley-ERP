using ERP_Project.Data;
using ERP_Project.Models;
using ERP_Project.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
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
    public class AdminReports : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public AdminReports(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)

        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }


        [Authorize(Roles = "Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
        public async Task<IActionResult> Index()
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            DateTime dateTimeConverter = TimeZoneInfo.ConvertTime(DateTime.Now, tz);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
                var employeeDb = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a=>a.Status==true);
                var DepartmentDb = _db.Departments.ToList();
                var teamDb = _db.DepartmentTeams.ToList();
                var DesignationDb = _db.Department_Designations.ToList();

                ReportsVM RVM = new ReportsVM();
                if (role.ElementAt(0) == "Admin")
                {
                    var empDataToday = _db.EmployeeTimeRecord.Where(a => a.Date.Date == dateTimeConverter.Date).ToList(); //==datetime.now
                    var appdblist = employeeDb.Where(a => a.Email!=user.Email);

                    RVM.AllEmployees = appdblist.ToList();
                    RVM.empTimeRecord = empDataToday;
                    RVM.employeeReports = _db.EmployeeReports.ToList();
                    RVM.AssignShiftList = _db.AssignShifts.ToList();
                    RVM.EmployeeTimmingList = _db.EmployeeTimmings.ToList();
                    RVM.OfficialShiftsList = _db.OfficialShifts.ToList();
                    RVM.currdate = dateTimeConverter.Date;
                    RVM.getDate1 = dateTimeConverter.Date;
                    return View(RVM);
                }
                    if (role.ElementAt(0) == "DepartmentHead")
                {
                    var applicationDbContext = employeeDb.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).Distinct().ToList();

                    //time activities
                    var empDataToday = _db.EmployeeTimeRecord.Where(a => a.Date.Date== dateTimeConverter.Date).ToList(); //==datetime.now
                    var appdblist = applicationDbContext.Distinct().Where(a => a.Status == true).Distinct();
                    RVM.AllEmployees = appdblist.ToList();
                    RVM.empTimeRecord = empDataToday;
                    RVM.employeeReports = _db.EmployeeReports.ToList();
                    RVM.AssignShiftList = _db.AssignShifts.ToList();
                    RVM.EmployeeTimmingList = _db.EmployeeTimmings.ToList();
                    RVM.OfficialShiftsList = _db.OfficialShifts.ToList();
                    RVM.currdate = dateTimeConverter.Date;
                    RVM.getDate1 = dateTimeConverter.Date;
                    return View(RVM);
                }
                else if (role.ElementAt(0) == "HRManager")
                {
                    var refIdUser = employeeDb.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                    var applicationDbContext = employeeDb.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).Distinct().ToList();

                    var empDataToday = _db.EmployeeTimeRecord.Where(a => a.Date.Date == dateTimeConverter.Date).ToList(); //==datetime.now
                    var appdblist = applicationDbContext.Distinct();
                    RVM.empTimeRecord = empDataToday;

                    RVM.AllEmployees = appdblist.Distinct().ToList();
                    RVM.employeeReports = _db.EmployeeReports.ToList();
                    RVM.AssignShiftList = _db.AssignShifts.ToList();
                    RVM.EmployeeTimmingList = _db.EmployeeTimmings.ToList();
                    RVM.OfficialShiftsList = _db.OfficialShifts.ToList();
                    RVM.currdate = dateTimeConverter.Date;
                    RVM.getDate1 = dateTimeConverter.Date;
                    return View(RVM);
                }
            }

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Index(ReportsVM atvm)
        {
            ReportsVM RVM = new ReportsVM();
            if (atvm.departments != null && atvm.employees != null&&atvm.getDate1 != null)
            {
                var date = atvm.getDate1.ToString("MM/dd/yyyy");
                DateTime dateformat = Convert.ToDateTime(date);
                var comp = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.DepartmentId == atvm.departments.DepartmentId && a.EmployeeId == atvm.employees.EmployeeId&&a.Status==true).ToList();
                RVM.AllEmployees = comp;
                RVM.empTimeRecord = _db.EmployeeTimeRecord.Where(a=>(a.Date.Date.Equals(dateformat.Date)) && a.EmployeeId==atvm.employees.EmployeeId).ToList();
                RVM.employeePerReport = _db.EmployeeReports.Where(a=> (a.Date.Date.Equals(dateformat.Date)) && a.EmployeeId == atvm.employees.EmployeeId).ToList();
                RVM.AssignShiftList = _db.AssignShifts.ToList();
                RVM.EmployeeTimmingList = _db.EmployeeTimmings.ToList();
                RVM.OfficialShiftsList = _db.OfficialShifts.ToList();
                RVM.currdate = dateformat;
                return View(RVM);
            }
            if (atvm.departments!=null&& atvm.employees != null)
            {
                var comp = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.DepartmentId == atvm.departments.DepartmentId&&a.EmployeeId==atvm.employees.EmployeeId && a.Status == true).ToList();
                RVM.AllEmployees = comp;
                RVM.empTimeRecord = _db.EmployeeTimeRecord.Where(a => a.EmployeeId == atvm.employees.EmployeeId).ToList();
                RVM.employeePerReport = _db.EmployeeReports.Where(a => a.EmployeeId == atvm.employees.EmployeeId).ToList();
                RVM.AssignShiftList = _db.AssignShifts.ToList();
                RVM.EmployeeTimmingList = _db.EmployeeTimmings.ToList();
                RVM.OfficialShiftsList = _db.OfficialShifts.ToList();
                RVM.currdate = DateTime.Now.Date;
                return View(RVM);
            }
            if (atvm.departments != null)
            {
                var comp = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.DepartmentId == atvm.departments.DepartmentId && a.Status == true).ToList();
                RVM.AllEmployees = comp;
                RVM.empTimeRecord = _db.EmployeeTimeRecord.ToList();
                RVM.employeePerReport = _db.EmployeeReports.ToList();
                RVM.AssignShiftList = _db.AssignShifts.ToList();
                RVM.EmployeeTimmingList = _db.EmployeeTimmings.ToList();
                RVM.OfficialShiftsList = _db.OfficialShifts.ToList();
                RVM.currdate = DateTime.Now.Date;
                return View(RVM);
            }
            if (atvm.getDate1 != null)
            {
                var date = atvm.getDate1.ToString("MM/dd/yyyy");
                DateTime dateformat = Convert.ToDateTime(date);
                var empList = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a =>a.Status == true).ToList();
               
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
         
                if (role.ElementAt(0) == "Admin")
                {
                    empList = empList.Where(a=>a.Status==true).ToList();

                }
                else if (role.ElementAt(0) == "DepartmentHead")
                {
                    empList = empList.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).Distinct().ToList();

                }
                else if (role.ElementAt(0) == "HRManager")
                {
                    var refIdUser = empList.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                    empList = empList.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).Distinct().ToList();

                }
                else
                {
                    empList = empList.Where(a => a.Status == true).ToList();

                }

                RVM.AllEmployees = empList.Where(a=>a.Status==true).Distinct().ToList();
                RVM.empTimeRecord = _db.EmployeeTimeRecord.Where(a => (a.Date.Date.Equals(dateformat.Date))).ToList();
                RVM.employeePerReport = _db.EmployeeReports.Where(a => (a.Date.Date.Equals(dateformat.Date))).ToList();
                RVM.AssignShiftList = _db.AssignShifts.ToList();
                RVM.EmployeeTimmingList = _db.EmployeeTimmings.ToList();
                RVM.OfficialShiftsList = _db.OfficialShifts.ToList();
                RVM.currdate = dateformat;
                return View(RVM);
            }
            /*    RVM.AllEmployees = comp;
                RVM.empTimeRecord = _db.EmployeeTimeRecord.ToList();
                RVM.employeeReports = _db.EmployeeReports.ToList();*/

            return View(RVM);
        }

        [Authorize(Roles = "Admin,DepartmentHead,HRManager")]
        public async Task<IActionResult> monthlyReports()
        {
            ReportsVM RVM = new ReportsVM();
            RVM.IsRecordExist = true;
            return View(RVM);
        }
        [HttpPost]
        public async Task<IActionResult> monthlyReports(ReportsVM atvm)
        {
            ReportsVM RVM = new ReportsVM();
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
             DateTime dateTimeConverter = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            if (atvm.departments == null || atvm.getYear == 0)
            {
                ViewBag.noCompSelect = true;
                return View(RVM);
            }
            /* 
            //DateTime fdate = DateTime.Now.AddDays(10);
            DateTime fdate = dateTimeConverter.Date;
            //DateTime sdate = DateTime.Now.AddDays(-1);
            DateTime sdate = fdate.AddMonths(-1).Date;
            TimeSpan ts = fdate - sdate;
            var sundays = ((ts.TotalDays / 7) + (sdate.DayOfWeek == DayOfWeek.Sunday || fdate.DayOfWeek == DayOfWeek.Sunday || fdate.DayOfWeek > sdate.DayOfWeek ? 1 : 0));

            sundays = Math.Round(sundays - .5, MidpointRounding.AwayFromZero);*/
            //getting sundays
            var monthNumber = DateTime.ParseExact(atvm.getMonth, "MMMM", CultureInfo.CurrentCulture).Month;
            List<int> SundayList = new List<int>();
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
            var DaysInmonth =  DateTime.DaysInMonth(atvm.getYear,monthNumber);
            //
            if (atvm.departments != null && atvm.employees != null && atvm.getMonth!=null && atvm.getYear!=0)
            {
                var id = atvm.employees.EmployeeId;
                var empData = new List<EmployeeTimeRecord>();
                    empData = _db.EmployeeTimeRecord.Where(f => f.EmployeeId == atvm.employees.EmployeeId && f.Date.Month ==monthNumber&& f.Date.Year == atvm.getYear).OrderBy(f => f.Date).ToList();
                //getting shifts
                if (_db.AssignShifts.Any(a => a.EmployeeId == id))
                {

                    var getShifttype = _db.AssignShifts.Where(a => a.EmployeeId == id).FirstOrDefault();
                    var shiftid = getShifttype.ShiftId;

                    var stringDay = DateTime.Now.ToString("dddd");
                    if (getShifttype != null && getShifttype.ShiftType == "official")
                    {
                        RVM.shiftType = "official";
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
                            RVM.emporgReportTime = null;
                            RVM.emporgBreakTime = 0;
                        }

                        else
                        {
                            RVM.emporgReportTime = Convert.ToString(string.Format("{0:hh:mm tt}", z.StartTime));
                            // RVM.emporgBreakTime = Convert.ToString(string.Format("{0:hh:mm tt}", z.BreakTime));
                            RVM.emporgBreakTime = z.BreakTime;
                            RVM.emporgEndTime = z.MinEndTime;
                            RVM.empExactEndTime = z.EndTime;
                        }
                    }
                    else if (getShifttype != null && getShifttype.ShiftType == "custom")
                    {
                        RVM.shiftType = "custom";
                        var z1 = _db.EmployeeTimmings.Where(f => f.Day == stringDay && f.ShiftAssignId == getShifttype.AssignShiftsId).FirstOrDefault();
                        var z = _db.EmployeeTimmings.Where(f => f.ShiftAssignId == getShifttype.AssignShiftsId).ToList();
                        var obj1 = z == null ? null : z;
                        if (obj1 == null)
                        {
                            RVM.emporgReportTime2 = null;
                            RVM.emporgBreakTime = 0;
                        }
                        var obj2 = z1 == null ? null : z1;
                        if (obj2 == null)
                        {
                            RVM.emporgReportTime = null;
                            RVM.emporgBreakTime = null;
                            RVM.emporgEndTime = null;
                            foreach (var custom in z)
                            {
                                RVM.empExactEndTime3.Add(new KeyValuePair<string, DateTime>(custom.Day, custom.EndTime));
                                RVM.emporgReportTime2.Add(new KeyValuePair<string, string>(custom.Day, Convert.ToString(string.Format("{0:hh:mm tt}", custom.StartTime))));

                            }
                        }
                        else
                        {
                            RVM.emporgReportTime = Convert.ToString(string.Format("{0:hh:mm tt}", z1.StartTime));
                            RVM.emporgBreakTime = z1.BreakTime;
                            RVM.emporgEndTime = z1.MinEndTime;

                            foreach (var custom in z)
                            {
                                RVM.empExactEndTime3.Add(new KeyValuePair<string, DateTime>(custom.Day, custom.EndTime));
                                RVM.emporgReportTime2.Add(new KeyValuePair<string, string>(custom.Day, Convert.ToString(string.Format("{0:hh:mm tt}", custom.StartTime))));

                            }

                        }
                    }
                }

                //
                var groupByempData = empData.GroupBy(a => a.Date.Date);
                RVM.empTimeRecordCred = groupByempData;
                RVM.empTimingsList = _db.EmployeeTimmings.Include(a => a.AssignShifts).ThenInclude(a => a.Employee).Where(s => s.AssignShifts.EmployeeId == id).ToList();
              

                var data = _db.EmployeeAttendanceRecord.Include(a=>a.Employee).ThenInclude(a=>a.Department_Designation).Where(a => a.EmployeeId == atvm.employees.EmployeeId && a.Date.Month == monthNumber && a.Date.Year == atvm.getYear).OrderBy(a=>a.Date).ToList();  //a.Date >= dateTimeConverter.AddMonths(-1)
                RVM.getYear = atvm.getYear;
                RVM.employees = _db.Employees.Include(a => a.Department_Designation).Where(a => a.DepartmentId == atvm.departments.DepartmentId && a.EmployeeId == atvm.employees.EmployeeId).FirstOrDefault();
                var empposition = _db.EmployeePositions.Where(a => a.EmployeeId == RVM.employees.EmployeeId).FirstOrDefault();
                RVM.employeePerReport = _db.EmployeeReports.Where(a => a.Date.Month == monthNumber && a.Date.Year == atvm.getYear && a.EmployeeId == atvm.employees.EmployeeId).OrderBy(a => a.Date).ToList();
                RVM.leavesCount= data.Where(a => a.status == "leave").Count();
                RVM.presentsCount = data.Where(a => a.status == "present").Count();
                RVM.absentsCount = DaysInmonth - RVM.leavesCount - RVM.presentsCount -SundayList.Count();
                /* if((monthNumber>dateTimeConverter.Month || atvm.getYear>dateTimeConverter.Year) || (empposition.StartDate.Month>monthNumber && empposition.StartDate.Year>=atvm.getYear) || (empposition.EndDate.Month < monthNumber && empposition.EndDate.Year <= atvm.getYear))
                 {*/
                if (atvm.getYear > DateTime.Now.Year || (atvm.getYear == DateTime.Now.Year && monthNumber > DateTime.Now.Month) || (empposition.StartDate.Month > monthNumber && empposition.StartDate.Year >= atvm.getYear) || (empposition.EndDate.Month < monthNumber && empposition.EndDate.Year <= atvm.getYear))
                {
                    RVM.IsRecordExist = false;
                }
                else
                {
                    RVM.IsRecordExist = true;
                }
                return View(RVM);
            }
           

            return RedirectToAction(nameof(monthlyReports));
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
           // }
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
           // }
        }
        [HttpPost]
        public async Task<IActionResult> GetEmployeesByDepartmentId(int DepartmentId)
        {


            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
        
            var employees = _db.Employees.Where(a => a.DepartmentId == DepartmentId && a.Status == true).ToList();
           if (role.ElementAt(0) == "DepartmentHead")
            {
                employees= employees.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).Distinct().ToList();
            
            }
            else if (role.ElementAt(0) == "HRManager")
            {

                var refIdUser = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
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
    }
}

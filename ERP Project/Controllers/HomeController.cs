using ERP_Project.Data;
using ERP_Project.Models;
using ERP_Project.Models.View_Models;
using ERP_Project.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_Project.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        [Authorize(Roles = "Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
        public async Task<IActionResult> Index()
        {
            try
            {
                TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                DateTime dateTimeConverter = TimeZoneInfo.ConvertTime(DateTime.Now, tz);

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != null)
                {
                    var user = await _userManager.FindByIdAsync(userId);
                    List<string> hremails = new List<string>();
                    var role = await _userManager.GetRolesAsync(user);
                    var employeeDb = _db.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.Status == true);
                    var DepartmentDb = _db.Departments.ToList();
                    var teamDb = _db.DepartmentTeams.ToList();
                    var DesignationDb = _db.Department_Designations.ToList();
                    var teamEmployees = _db.department_Teams_Employees.Include(a => a.Employee).Include(a => a.GetDepartmentTeam).Where(a => a.Status == true).ToList();
                    // var chkoutApp = _db.CheckoutApprovalRequests.ToList();
                    var empTimeRecord = _db.EmployeeTimeRecord.Include(a => a.Employee).ThenInclude(a => a.Department_Designation).Where(a => a.Date.Date == dateTimeConverter.Date).ToList();
                    DashboardVM dashboad = new DashboardVM();
                    dashboad.users = _db.Users.ToList();
                    dashboad.AllEmployees = _db.Employees.ToList();
                    if (role.ElementAt(0) == "Admin")
                    {
                        //ative employees count strt
                        List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                        List<int> activeEmployeesId = new List<int>();
                        foreach (var em in employeeDb)
                        {
                            empTymData.AddRange(empTimeRecord.Where(f => f.EmployeeId == em.EmployeeId));
                        }
                        var groupByempData = empTymData.GroupBy(a => a.Employee.EmployeeId);

                        bool status = true;
                        int countItm = groupByempData.Count();
                        foreach (var calTym in groupByempData)
                        {
                            var rem1 = calTym.Count() % 2;
                            if (calTym.Count() == 1)
                            {
                                //means employee has checked in
                                activeEmployeesId.Add(calTym.Key);   //key has emp id
                            }
                            else if (calTym.Count() == 2)
                            {
                                foreach (var tym in calTym)
                                {
                                    //check in-------time out..emp seat pe ni h
                                    status = false;
                                }
                            }
                            else if (calTym.Count() == 3)
                            {
                                //check in-------time out-------time in....emp seat pe ni h
                                activeEmployeesId.Add(calTym.Key);
                            }
                            else if (rem1 == 0)  //even
                            {
                                status = false;
                            }
                            else  //odd
                            {
                                activeEmployeesId.Add(calTym.Key);
                            }

                        }
                        dashboad.presentEmployees = empTimeRecord.Where(a => a.RecordTypeName == "checkin").ToList();
                        var leaveList = _db.EmployeeAttendanceRecord.Where(a => a.Date.Date == dateTimeConverter.Date && a.status == "leave").ToList();
                        foreach (var o in leaveList)
                        {
                            var ed = _db.Employees.Where(a => a.EmployeeId == o.EmployeeId).FirstOrDefault();
                            dashboad.EmployeesOnLeaves.Add(ed);
                        }

                        foreach (var item in employeeDb)
                        {
                            if ((!dashboad.presentEmployees.Any(a => a.EmployeeId == item.EmployeeId)) && (!dashboad.leavesEmployees.Any(a => a.EmployeeId == item.EmployeeId)))
                            {
                                dashboad.absentEmployees.Add(item);
                            }
                        }
                        //ative employees count end
                        dashboad.leavesEmployees = _db.Leaves.Where(a => a.Status == "pending").ToList();
                        dashboad.employeeCount = employeeDb.Count();
                        dashboad.deptCount = DepartmentDb.Count();
                        dashboad.activeEmployeesCount = activeEmployeesId.Count();
                        dashboad.teamCount = teamDb.Where(a => a.Status == true).Count();
                        dashboad.companyCount = DesignationDb.Count();
                        dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Month == DateTime.Now.Month).ToList();
                        if (employeeDb.Count() > 0)
                        {
                            foreach (var obj in employeeDb)
                            {
                                if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending"))
                                {
                                    var recrd = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => /*a.Date.Date == dateTimeConverter.Date*/a.ApprovalStatus == "pending" && a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId).ToList();
                                    dashboad.newCheckoutRequest.AddRange(recrd);
                                }
                            }
                        }
                    }
                    else if (role.ElementAt(0) == "DepartmentHead")  // a.DepartmentId == employee.DepartmentId &&
                    {
                        var empData = employeeDb.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).Distinct().ToList();

                        //ative employees count strt
                        List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                        List<int> activeEmployeesId = new List<int>();
                        foreach (var em in empData)
                        {
                            empTymData.AddRange(_db.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
                        }
                        var groupByempData = empTymData.GroupBy(a => a.Employee.EmployeeId);

                        bool status = true;
                        int countItm = groupByempData.Count();
                        foreach (var calTym in groupByempData)
                        {
                            var rem1 = calTym.Count() % 2;
                            if (calTym.Count() == 1)
                            {
                                //means employee has checked in
                                activeEmployeesId.Add(calTym.Key);   //key has emp id
                            }
                            else if (calTym.Count() == 2)
                            {
                                foreach (var tym in calTym)
                                {
                                    //check in-------time out..emp seat pe ni h
                                    status = false;
                                }
                            }
                            else if (calTym.Count() == 3)
                            {
                                //check in-------time out-------time in....emp seat pe ni h
                                activeEmployeesId.Add(calTym.Key);
                            }
                            else if (rem1 == 0)  //even
                            {
                                status = false;
                            }
                            else  //odd
                            {
                                activeEmployeesId.Add(calTym.Key);
                            }

                        }
                        //
                        dashboad.presentEmployees = empTymData.Where(a => a.RecordTypeName == "checkin").ToList();
                        foreach (var item in empData.Where(a=>a.Status==true))
                        {

                            dashboad.leavesEmployees = _db.Leaves.Include(a => a.Employee).ThenInclude(a => a.Department_Designation).Where(a => a.Date.Date == dateTimeConverter.Date && a.EmployeeId == item.EmployeeId).ToList();

                            if ((!dashboad.presentEmployees.Any(a => a.EmployeeId == item.EmployeeId)) && (!dashboad.leavesEmployees.Any(a => a.EmployeeId == item.EmployeeId)))
                            {
                                dashboad.absentEmployees.Add(item);
                            }
                        }
                        //ative employees count end

                        dashboad.employeeCount = empData.Count();
                        dashboad.deptCount = DepartmentDb.Count();
                        dashboad.activeEmployeesCount = activeEmployeesId.Count();
                        dashboad.companyCount = DesignationDb.Count();
                        dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Month == DateTime.Now.Month).ToList();
                        if (empData.Count() > 0)
                        {
                            foreach (var obj in empData)
                            {
                                if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending"))
                                {
                                    var recrd = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => /*a.Date.Date == dateTimeConverter.Date*/a.ApprovalStatus == "pending" && a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId).ToList();
                                    dashboad.newCheckoutRequest.AddRange(recrd);
                                }
                            }
                        }
                    }
                    else if (role.ElementAt(0) == "HRManager")
                    {

                        var refIdUser = employeeDb.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                        var applicationDbContext = employeeDb.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).Distinct().ToList();

                        //close
                        dashboad.employeeCount = applicationDbContext.Count();
                        var uid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                        // dashboad.employeeCount = employeeDb.Count();
                        dashboad.deptCount = DepartmentDb.Count();
                        dashboad.companyCount = DesignationDb.Count();
                        dashboad.teamCount = teamDb.Count();
                        dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Date == DateTime.Now.Date && a.ReferenceUserId == uid).ToList();
                        //dashboad.newCheckoutRequest = _db.CheckoutApprovalRequests.Include(a => a.Department_Teams_Head).Include(a => a.Department_Teams_Head.Employee).Include(a => a.EmployeeTimeRecords).Include(a => a.EmployeeTimeRecords.Employee).Include(a => a.Department_Teams_Head.Employee).Where(a => a.Date.Date == DateTime.Now.Date).ToList();

                        if (applicationDbContext.Count() > 0)
                        {
                            foreach (var obj in applicationDbContext)
                            {
                                if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending"))
                                {
                                    var recrd = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => /*a.Date.Date == dateTimeConverter.Date*/a.ApprovalStatus == "pending" && a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId).ToList();
                                    dashboad.newCheckoutRequest.AddRange(recrd);
                                }
                            }
                        }
                    }
                    else if (role.ElementAt(0) == "TeamHead")  // a.DepartmentId == employee.DepartmentId &&
                    {
                        var empId = _db.Employees.Where(s => s.Email == user.Email).FirstOrDefault().EmployeeId;
                        var teamHeadId = _db.Department_Teams_Heads.Where(a => a.EmployeeId == empId);
                        var teamEmp = from te in _db.department_Teams_Employees
                                      join th in teamHeadId
                                      on te.DepartmentTeamsId equals th.HeadId
                                      select te;

                        var empData = from e in _db.Employees.Where(a=>a.Status==true)
                                      join id in teamEmp
                                      on e.EmployeeId equals id.EmployeeId
                                      select e;

                        //ative employees count strt
                        List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                        List<int> activeEmployeesId = new List<int>();
                        foreach (var em in empData)
                        {
                            empTymData.AddRange(_db.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
                        }
                        var groupByempData = empTymData.GroupBy(a => a.Employee.EmployeeId);

                        bool status = true;
                        int countItm = groupByempData.Count();
                        foreach (var calTym in groupByempData)
                        {
                            var rem1 = calTym.Count() % 2;
                            if (calTym.Count() == 1)
                            {
                                //means employee has checked in
                                activeEmployeesId.Add(calTym.Key);   //key has emp id
                            }
                            else if (calTym.Count() == 2)
                            {
                                foreach (var tym in calTym)
                                {
                                    //check in-------time out..emp seat pe ni h
                                    status = false;
                                }
                            }
                            else if (calTym.Count() == 3)
                            {
                                //check in-------time out-------time in....emp seat pe ni h
                                activeEmployeesId.Add(calTym.Key);
                            }
                            else if (rem1 == 0)  //even
                            {
                                status = false;
                            }
                            else  //odd
                            {
                                activeEmployeesId.Add(calTym.Key);
                            }

                        }
                        //
                        dashboad.presentEmployees = empTymData.Where(a => a.RecordTypeName == "checkin").ToList();
                        foreach (var item in empData)
                        {

                            dashboad.leavesEmployees = _db.Leaves.Include(a => a.Employee).ThenInclude(a => a.Department_Designation).Where(a => a.Date.Date == dateTimeConverter.Date && a.EmployeeId == item.EmployeeId).ToList();

                            if ((!dashboad.presentEmployees.Any(a => a.EmployeeId == item.EmployeeId)) && (!dashboad.leavesEmployees.Any(a => a.EmployeeId == item.EmployeeId)))
                            {
                                dashboad.absentEmployees.Add(item);
                            }
                        }
                        //ative employees count end

                        dashboad.employeeCount = empData.Count();
                        dashboad.deptCount = DepartmentDb.Count();
                        dashboad.activeEmployeesCount = activeEmployeesId.Count();
                        dashboad.companyCount = DesignationDb.Count();
                        dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Month == DateTime.Now.Month).ToList();
                        if (empData.Count() > 0)
                        {
                            foreach (var obj in empData)
                            {
                                if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending"))
                                {
                                    var recrd = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => /*a.Date.Date == dateTimeConverter.Date*/a.ApprovalStatus == "pending" && a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId).ToList();
                                    dashboad.newCheckoutRequest.AddRange(recrd);
                                }
                            }
                        }
                    }

                    else
                    {

                        dashboad.employeeCount = employeeDb.Count();
                        dashboad.deptCount = DepartmentDb.Count();
                        dashboad.companyCount = DesignationDb.Count();
                        dashboad.teamCount = teamDb.Count();
                        dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Date == dateTimeConverter.Date).ToList();

                        dashboad.newCheckoutRequest = _db.CheckoutApprovalRequests.Include(a => a.Department_Teams_Head).Include(a => a.Department_Teams_Head.Employee).Include(a => a.EmployeeTimeRecords).Include(a => a.EmployeeTimeRecords.Employee).Include(a => a.Department_Teams_Head.Employee).Where(a =>/* a.Date.Date == DateTime.Now.Date*/a.ApprovalStatus == "pending").ToList();
                    }
                    dashboad.userRole = role.ElementAt(0);
                    return View(dashboad);
                }
                else
                {
                    string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Logout";
                    return Redirect(link);
                }
            }
            catch (Exception e)
            {
                string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Logout";
                return Redirect(link);
            }
        }

        /*  [Authorize(Roles = "Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
          public async Task<IActionResult> Index()
          {
              TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
              DateTime dateTimeConverter = TimeZoneInfo.ConvertTime(DateTime.Now, tz);

              var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
              if (userId != null)
              {
                  var user = await _userManager.FindByIdAsync(userId);
                  List<string> hremails = new List<string>();
                  var role = await _userManager.GetRolesAsync(user);
                  var employeeDb = _db.Employees.ToList();
                  var DepartmentDb = _db.Departments.ToList();
                  var teamDb = _db.DepartmentTeams.ToList();
                  var DesignationDb = _db.Department_Designations.ToList();
                  var teamEmployees = _db.department_Teams_Employees.Include(a=>a.Employee).Include(a=>a.GetDepartmentTeam).Where(a=>a.Status==true).ToList();
                  var chkoutApp = _db.CheckoutApprovalRequests.ToList();
                  var empTimeRecord = _db.EmployeeTimeRecord.Include(a=>a.Employee).Where(a => a.Date.Date == dateTimeConverter.Date).ToList();
              DashboardVM dashboad = new DashboardVM();
                  dashboad.users = _db.Users.ToList();
                  dashboad.AllEmployees = _db.Employees.ToList();
                  if (role.ElementAt(0) == "Admin"&&user.Id== "d7241547-861b-4b0a-bc8f-8e26cef589c3") 
                  {
                      var applicationDbContext= employeeDb.Where(a => a.Status == true && a.Email != user.Email).ToList();
                      //ative employees count strt
                      List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                      List<int> activeEmployeesId = new List<int>();
                      foreach (var em in applicationDbContext)
                      {
                          empTymData.AddRange(_db.EmployeeTimeRecord.Include(s => s.Employee).ThenInclude(a=>a.Department_Designation).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
                      }
                      var groupByempData = empTymData.GroupBy(a => a.Employee.EmployeeId);

                      bool status = true;
                      int countItm = groupByempData.Count();
                      foreach (var calTym in groupByempData)
                      {
                          var rem1 = calTym.Count() % 2;
                          if (calTym.Count() == 1)
                          {
                              //means employee has checked in
                              activeEmployeesId.Add(calTym.Key);   //key has emp id
                          }
                          else if (calTym.Count() == 2)
                          {
                              foreach (var tym in calTym)
                              {
                                  //check in-------time out..emp seat pe ni h
                                  status = false;
                              }
                          }
                          else if (calTym.Count() == 3)
                          {
                              //check in-------time out-------time in....emp seat pe ni h
                              activeEmployeesId.Add(calTym.Key);
                          }
                          else if (rem1 == 0)  //even
                          {
                              status = false;
                          }
                          else  //odd
                          {
                              activeEmployeesId.Add(calTym.Key);
                          }

                      }
                      dashboad.presentEmployees = empTimeRecord.Where(a => a.RecordTypeName == "checkin").ToList();
                      var leaveList = _db.EmployeeAttendanceRecord.Where(a => a.Date.Date == dateTimeConverter.Date && a.status == "leave").ToList();
                      foreach(var o in leaveList)
                      {
                          var ed = _db.Employees.Where(a => a.EmployeeId == o.EmployeeId).FirstOrDefault();
                          dashboad.EmployeesOnLeaves.Add(ed); 
                      }

                      foreach (var item in employeeDb.Where(a=>a.Status==true))
                      {
                          if ((!dashboad.presentEmployees.Any(a => a.EmployeeId == item.EmployeeId)) && (!dashboad.leavesEmployees.Any(a => a.EmployeeId == item.EmployeeId)))
                          {
                              dashboad.absentEmployees.Add(item);
                           }
                      }
                      //ative employees count end
                      dashboad.leavesEmployees = _db.Leaves.Where(a => a.Status == "pending").ToList();
                      dashboad.employeeCount = applicationDbContext.Count();
                      dashboad.deptCount = DepartmentDb.Count();
                      dashboad.activeEmployeesCount = activeEmployeesId.Count();
                      dashboad.teamCount = teamDb.Where(a=>a.Status==true).Count();
                      dashboad.companyCount = DesignationDb.Count();
                      dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Month == DateTime.Now.Month).ToList();
                      if (employeeDb.Count() > 0)
                      {
                          foreach (var obj in employeeDb)
                          {
                              if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending"))
                              {
                                  var recrd = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => *//*a.Date.Date == dateTimeConverter.Date*//*a.ApprovalStatus=="pending" && a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId).ToList();
                                  dashboad.newCheckoutRequest.AddRange(recrd);
                              }
                          }
                      }
                  }
                      else if (role.ElementAt(0) == "DepartmentHead")  // a.DepartmentId == employee.DepartmentId &&
                  {
                      var employee = employeeDb.Where(a => a.Email == user.Email).FirstOrDefault();
                      var departmentHead = _db.Department_Teams_Heads.Where(a => a.EmployeeId == employee.EmployeeId).FirstOrDefault();
                      var empData = employeeDb.Where(a => (a.ReferenceUserId == employee.ReferenceUserId || a.Email == employee.Email || a.ReferenceUserId.ToString() == user.Id)).ToList(); //

                      //geting emp created by hr manager

                      foreach (var emp in empData.ToList())
                      {
                          if (_db.Employees.Include(s => s.Department).Any(a => a.Email == emp.Email && a.Department.DepartmentName == "HR"))
                          {
                           //   hremails.Add(emp.Email);
                              var hruser = _db.Users.Where(a => a.Email == emp.Email).FirstOrDefault().Id;
                              if (_db.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                              {
                                  empData.AddRange(_db.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).ToList());
                              }
                          }
                      }
                      //ative employees count strt
                      List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                      List<int> activeEmployeesId = new List<int>();
                      foreach (var em in empData)
                      {
                          empTymData.AddRange(_db.EmployeeTimeRecord.Include(s=>s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
                      }
                      var groupByempData = empTymData.GroupBy(a => a.Employee.EmployeeId);

                      bool status = true;
                      int countItm = groupByempData.Count();
                      foreach (var calTym in groupByempData)
                      {
                         var rem1 = calTym.Count() % 2;
                          if (calTym.Count() == 1)
                          {
                              //means employee has checked in
                              activeEmployeesId.Add(calTym.Key);   //key has emp id
                          }
                          else if (calTym.Count() == 2)
                          {
                              foreach (var tym in calTym)
                              {
                                  //check in-------time out..emp seat pe ni h
                                  status = false;
                              }
                          }
                          else if (calTym.Count() == 3)
                          {
                              //check in-------time out-------time in....emp seat pe ni h
                              activeEmployeesId.Add(calTym.Key);
                          }
                        else if(rem1 == 0)  //even
                          {
                              status = false;
                          }
                          else  //odd
                          {
                              activeEmployeesId.Add(calTym.Key);
                          }

                      }
                      //
                      dashboad.presentEmployees = empTymData.Where(a => a.RecordTypeName == "checkin").ToList();
                      foreach (var item in empData.Where(a => a.Status == true))
                      {

                          dashboad.leavesEmployees = _db.Leaves.Include(a => a.Employee).ThenInclude(a => a.Department_Designation).Where(a => a.Date.Date == dateTimeConverter.Date && a.EmployeeId==item.EmployeeId).ToList();

                          if ((!dashboad.presentEmployees.Any(a => a.EmployeeId == item.EmployeeId)) && (!dashboad.leavesEmployees.Any(a => a.EmployeeId == item.EmployeeId)))
                          {
                              dashboad.absentEmployees.Add(item);
                          }
                      }
                      //ative employees count end

                      dashboad.employeeCount = empData.Where(a => a.Status == true).Count();
                      dashboad.deptCount= DepartmentDb.Count();
                      dashboad.activeEmployeesCount = activeEmployeesId.Count();
                      dashboad.companyCount = DesignationDb.Count();
                      dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Month == DateTime.Now.Month).ToList();
                     if (empData.Count() > 0)
                      {
                          foreach (var obj in empData)
                          {
                              if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending"))
                              {
                                  var recrd = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => *//*a.Date.Date == dateTimeConverter.Date*//*a.ApprovalStatus == "pending" && a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId).ToList();
                                  dashboad.newCheckoutRequest.AddRange(recrd);
                              }
                          }
                      }
                  }
                  else if (role.ElementAt(0) == "TeamHead")
                  {
                      var employee = employeeDb.Where(a => a.Email == user.Email).FirstOrDefault();

                      var teamHead = _db.Department_Teams_Heads.Where(a => a.EmployeeId == employee.EmployeeId).FirstOrDefault();
                      var empData=teamEmployees.Where(a => a.DepartmentTeamsId == teamHead.HeadId).ToList();
                      //ative employees count strt
                      List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                      List<int> activeEmployeesId = new List<int>();
                      foreach (var em in empData)
                      {
                          empTymData.AddRange(_db.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
                      }
                      var groupByempData = empTymData.GroupBy(a => a.Employee.EmployeeId);

                      bool status = true;
                      int countItm = groupByempData.Count();
                      foreach (var calTym in groupByempData)
                      {
                          var rem1 = calTym.Count() % 2;
                          if (calTym.Count() == 1)
                          {
                              //means employee has checked in
                              activeEmployeesId.Add(calTym.Key);   //key has emp id
                          }
                          else if (calTym.Count() == 2)
                          {
                              foreach (var tym in calTym)
                              {
                                  //check in-------time out..emp seat pe ni h
                                  status = false;
                              }
                          }
                          else if (calTym.Count() == 3)
                          {
                              //check in-------time out-------time in....emp seat pe ni h
                              activeEmployeesId.Add(calTym.Key);
                          }
                          else if (rem1 == 0)  //even
                          {
                              status = false;
                          }
                          else  //odd
                          {
                              activeEmployeesId.Add(calTym.Key);
                          }

                      }
                      //ative employees count end
                      // var depTeams = teamEmployees.Where(a => a.DepartmentTeamsId == teamHead.HeadId).ToList();
                      dashboad.deptCount = DepartmentDb.Where(a => a.Status == true).Count();
                      dashboad.employeeCount = empData.Count();//teamEmployees.Where(a => a.DepartmentTeamsId == teamHead.HeadId).Count();//
                      dashboad.companyCount = DesignationDb.Where(a => a.DepartmentId == employee.DepartmentId).Count();
                      dashboad.activeEmployeesCount = activeEmployeesId.Count();
                      dashboad.deptCount = DepartmentDb.Count();
                    //  dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Month == DateTime.Now.Month).ToList();
                     dashboad.newCheckoutRequest = _db.CheckoutApprovalRequests.Include(a => a.Department_Teams_Head).Include(a => a.EmployeeTimeRecords).Include(a => a.EmployeeTimeRecords.Employee).Include(a => a.Department_Teams_Head.Employee).Where(a => *//*a.Date.Date == DateTime.Now.Date*//*a.ApprovalStatus == "pending" && a.Department_Teams_HeadsId == teamHead.Department_Teams_HeadsId).ToList();



                  }
                  else if (role.ElementAt(0) == "HRManager")
                  {

                      // var applicationDbContext = employeeDb.Where(a => a.ReferenceUserId.ToString() == user.Id).ToList();
                      //getting employees created by admin
                       var hr = employeeDb.Where(a => a.Email == user.Email).FirstOrDefault();// a.Email != employee.Email && (
                      var applicationDbContext = employeeDb.Where(a => a.ReferenceUserId == hr.ReferenceUserId || a.ReferenceUserId.ToString() == user.Id).ToList(); //
                                                                                                                                                                                                         //getting employyees created byanother hr
                   //getting emp created by another hr
                      var email = "";
                      foreach (var emp in applicationDbContext)
                      {
                          if (_db.Employees.Include(s => s.Department).Any(a => a.Email == emp.Email && (a.Department.DepartmentName == "HR" || a.Department.DepartmentName == "Management")))
                          {
                              email = emp.Email;
                          }
                      }
                      var hruser = _db.Users.Where(a => a.Email == email).FirstOrDefault().Id;
                      if (_db.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                      {
                          applicationDbContext.Add(_db.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).FirstOrDefault());
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
                              applicationDbContext.Add(_db.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                          }
                      }
                      //close
                      dashboad.employeeCount = applicationDbContext.Where(a => a.Status == true).Count();
                      var uid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
                     // dashboad.employeeCount = employeeDb.Count();
                      dashboad.deptCount = DepartmentDb.Count();
                      dashboad.companyCount = DesignationDb.Count();
                      dashboad.teamCount = teamDb.Count();
                      dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Date == DateTime.Now.Date && a.ReferenceUserId == uid).ToList();
                      //dashboad.newCheckoutRequest = _db.CheckoutApprovalRequests.Include(a => a.Department_Teams_Head).Include(a => a.Department_Teams_Head.Employee).Include(a => a.EmployeeTimeRecords).Include(a => a.EmployeeTimeRecords.Employee).Include(a => a.Department_Teams_Head.Employee).Where(a => a.Date.Date == DateTime.Now.Date).ToList();

                      if (applicationDbContext.Count() > 0)
                      {
                          foreach (var obj in applicationDbContext)
                          {
                              if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending"))
                              {
                                  var recrd = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a => *//*a.Date.Date == dateTimeConverter.Date*//*a.ApprovalStatus == "pending" && a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId).ToList();
                                  dashboad.newCheckoutRequest.AddRange(recrd);
                              }
                          }
                      }
                  }
                  else if(role.ElementAt(0) == "Admin")
                  {

                     //
                      var applicationDbContext=employeeDb.Where(a => a.ReferenceUserId.ToString() == user.Id).ToList();
                      //getting employees created by department head 
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
                              applicationDbContext.Add(_db.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                          }
                      }
                      //close
                      //geting emp created by hr manager

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
                      //ative employees count strt
                      List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                      List<int> activeEmployeesId = new List<int>();
                      foreach (var em in applicationDbContext)
                      {
                          empTymData.AddRange(_db.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
                      }
                      var groupByempData = empTymData.GroupBy(a => a.Employee.EmployeeId);

                      bool status = true;
                      int countItm = groupByempData.Count();
                      foreach (var calTym in groupByempData)
                      {
                          var rem1 = calTym.Count() % 2;
                          if (calTym.Count() == 1)
                          {
                              //means employee has checked in
                              activeEmployeesId.Add(calTym.Key);   //key has emp id
                          }
                          else if (calTym.Count() == 2)
                          {
                              foreach (var tym in calTym)
                              {
                                  //check in-------time out..emp seat pe ni h
                                  status = false;
                              }
                          }
                          else if (calTym.Count() == 3)
                          {
                              //check in-------time out-------time in....emp seat pe ni h
                              activeEmployeesId.Add(calTym.Key);
                          }
                          else if (rem1 == 0)  //even
                          {
                              status = false;
                          }
                          else  //odd
                          {
                              activeEmployeesId.Add(calTym.Key);
                          }

                      }
                      //ative employees count end
                      dashboad.employeeCount = applicationDbContext.Where(a => a.Status == true).Count();
                      dashboad.deptCount = DepartmentDb.Count();
                      dashboad.activeEmployeesCount = activeEmployeesId.Count();
                      dashboad.companyCount = DesignationDb.Count();
                      dashboad.teamCount = teamDb.Where(a=>a.ReferenceUserId.ToString()==user.Id).Count();
                      dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Date == DateTime.Now.Date).ToList();
                      //getting check out approval request

                      if (applicationDbContext.Count() > 0)
                      {
                          foreach (var obj in applicationDbContext)
                          {
                              if (_db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).Any(a => a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId && a.ApprovalStatus == "pending"))
                              {
                                  var recrd = _db.CheckoutApprovalRequests.Include(a => a.EmployeeTimeRecords).ThenInclude(a => a.Employee).ThenInclude(a => a.Department_Designation).ThenInclude(a => a.Department).Where(a =>*//* a.Date.Date == dateTimeConverter.Date*//*a.ApprovalStatus == "pending" && a.EmployeeTimeRecords.EmployeeId == obj.EmployeeId).ToList();
                                     dashboad.newCheckoutRequest.AddRange(recrd);
                              }
                          }
                      }
                      // dashboad.newCheckoutRequest = _db.CheckoutApprovalRequests.Include(a => a.Department_Teams_Head).Include(a => a.Department_Teams_Head.Employee).Include(a => a.EmployeeTimeRecords).Include(a => a.EmployeeTimeRecords.Employee).Include(a => a.Department_Teams_Head.Employee).Where(a => a.Date.Date == DateTime.Now.Date).ToList();
                  }
                  else
                  {

                      dashboad.employeeCount = employeeDb.Count();
                      dashboad.deptCount = DepartmentDb.Count();
                      dashboad.companyCount = DesignationDb.Count();
                      dashboad.teamCount = teamDb.Count();
                      dashboad.newApplicants = _db.Applicants.Include(a => a.Application).Include(a => a.Application.Designation).Where(a => a.Date.Date == dateTimeConverter.Date).ToList();

                      dashboad.newCheckoutRequest = _db.CheckoutApprovalRequests.Include(a => a.Department_Teams_Head).Include(a => a.Department_Teams_Head.Employee).Include(a => a.EmployeeTimeRecords).Include(a => a.EmployeeTimeRecords.Employee).Include(a => a.Department_Teams_Head.Employee).Where(a =>*//* a.Date.Date == DateTime.Now.Date*//*a.ApprovalStatus == "pending").ToList();
                  }
                  dashboad.userRole = role.ElementAt(0);
              return View(dashboad);
              }
              else
              {
                  string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Logout";
                  return Redirect(link);
              }
          }
  */
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

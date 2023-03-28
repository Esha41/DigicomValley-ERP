using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERP_Project.Data;
using ERP_Project.Models;
using ERP_Project.Models.View_Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Dynamic;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;

namespace ERP_Project.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        /* private readonly RelationTypes _rt;*/
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public readonly IWebHostEnvironment _WebHost;
        public EmployeesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, RoleManager<IdentityRole> roleManager, IWebHostEnvironment WebHost)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _WebHost = WebHost;

        }

        public void SelectedId(int id)
        {
            ViewBag.empId = id;
        }

        [Authorize(Roles = "Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
        public async Task<IActionResult> Index()
        {
            /*   dynamic myModel = new ExpandoObject();
               myModel.depHeadList = _context.Department_Teams_Heads.ToList();*/
            string[] referenceId = new string[2];
            EmployeeVM EMP = new EmployeeVM();
            EMP.depHeadsList = _context.Department_Teams_Heads.ToList();
            EMP.department = _context.Departments.ToList();
            EMP.designations = _context.Department_Designations.ToList();
            EMP.departmentTeams = _context.DepartmentTeams.ToList();
            EMP.employeePositions = _context.EmployeePositions.ToList();
            EMP.employees = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (userId != null)
            {
                if (role.ElementAt(0) == "Admin") //employees created by admin will only be shown to admin
                {
                    return View(EMP);
                }
               else if (role.ElementAt(0) == "DepartmentHead")
                {
                    EMP.employees = EMP.employees.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email==user.Email).Distinct().ToList();
                    return View(EMP);
                }
                else if (role.ElementAt(0) == "HRManager")
                {

                    var refIdUser = EMP.employees.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                    EMP.employees = EMP.employees.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).Distinct().ToList();
                    return View(EMP);
                }
                else if (role.ElementAt(0) == "TeamHead")
                {
                    var empId = _context.Employees.Where(s => s.Email == user.Email).FirstOrDefault().EmployeeId;
                    var teamHeadId = _context.Department_Teams_Heads.Where(a => a.EmployeeId == empId);
                    var teamEmp = from te in _context.department_Teams_Employees
                                  join th in teamHeadId
                                  on te.DepartmentTeamsId equals th.HeadId
                                  select te;

                    var empData = from e in _context.Employees.Include(s=>s.Company).Include(s=>s.Department).Include(s=>s.Department_Designation)
                                  join id in teamEmp
                                  on e.EmployeeId equals id.EmployeeId
                                  select e;
                    EMP.employees = empData;
                        return View(EMP);
                }
                else
                {
                    return View(EMP);
                }
            }
            else
            {
                string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                return Redirect(link);
            }

        }
      

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(EmployeeVM evm)
        {
            try
            {
                var Employees = _context.Employees.Find(evm.empId2);
                if (evm.empId2 == null || Employees == null)
                {
                    return NotFound();
                }
                Employees.Status = !(Employees.Status);
                Employees.Password = Employees.Password + DateTime.Now.Month;
                if (evm.RemarksToChngStatus != null)
                {
                    Employees.RemarksForChngStatus = evm.RemarksToChngStatus;
                }
                _context.Employees.Update(Employees);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        [Authorize(Roles = "Admin,DepartmentHead,TeamHead,HRManager")]
        public async Task<IActionResult> getActive()
        {

            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            DateTime dateTimeConverter = TimeZoneInfo.ConvertTime(DateTime.Now, tz);

            string[] referenceId = new string[2];
            EmployeeVM EMP = new EmployeeVM();
            EMP.depHeadsList = _context.Department_Teams_Heads.ToList();
            EMP.department = _context.Departments.ToList();
            EMP.designations = _context.Department_Designations.ToList();
            EMP.departmentTeams = _context.DepartmentTeams.ToList();
            EMP.employees = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (userId != null)
            {
                if (role.ElementAt(0) == "Admin")
                {
                    var applicationDbContext = EMP.employees.Where(a => a.Email != user.Email).ToList();

                    //ative employees count strt
                    List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                    List<int> activeEmployeesId = new List<int>();
                    foreach (var em in applicationDbContext)
                    {
                        empTymData.AddRange(_context.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
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
                    List<Employees> empData = new List<Employees>();
                    foreach (var id in activeEmployeesId)
                    {
                        empData.Add(_context.Employees.FirstOrDefault(e => e.EmployeeId == id));
                    }
                    //getting employes from ids


                    //end
                    EMP.employees = empData;
                    return View(EMP);
                }
                else if (role.ElementAt(0) == "DepartmentHead")
                {
                    var empData = EMP.employees.Where(a => a.ReferenceUserId.ToString() == user.Id || a.Email == user.Email).Distinct().ToList();

                    //ative employees count strt
                    List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                    List<int> activeEmployeesId = new List<int>();
                    foreach (var em in empData)
                    {
                        empTymData.AddRange(_context.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
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
                    List<Employees> empData2 = new List<Employees>();
                    foreach (var id in activeEmployeesId)
                    {
                        empData2.Add(_context.Employees.FirstOrDefault(e => e.EmployeeId == id));
                    }
                    //getting employes from ids


                    //end
                    EMP.employees = empData2;
                    return View(EMP);
                }
                else if (role.ElementAt(0) == "HRManager")
                {
                    var refIdUser = EMP.employees.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                    var empData = EMP.employees.Where(a => a.ReferenceUserId.ToString() == refIdUser.ToString() || a.Email == user.Email).Distinct().ToList();

                    //ative employees count strt
                    List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                    List<int> activeEmployeesId = new List<int>();
                    foreach (var em in empData)
                    {
                        empTymData.AddRange(_context.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
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
                    List<Employees> empData2 = new List<Employees>();
                    foreach (var id in activeEmployeesId)
                    {
                        empData2.Add(_context.Employees.FirstOrDefault(e => e.EmployeeId == id));
                    }
                    //getting employes from ids


                    //end
                    EMP.employees = empData2;
                    return View(EMP);
                }
                else if (role.ElementAt(0) == "TeamHead")
                {
                    var empId = _context.Employees.Where(s => s.Email == user.Email).FirstOrDefault().EmployeeId;
                    var teamHeadId = _context.Department_Teams_Heads.Where(a => a.EmployeeId == empId);
                    var teamEmp = from te in _context.department_Teams_Employees
                                  join th in teamHeadId
                                  on te.DepartmentTeamsId equals th.HeadId
                                  select te;

                    var empData = from e in _context.Employees
                                  join id in teamEmp
                                  on e.EmployeeId equals id.EmployeeId
                                  select e;
                    //ative employees count strt
                    List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                    List<int> activeEmployeesId = new List<int>();
                    foreach (var em in empData)
                    {
                        empTymData.AddRange(_context.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
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
                    List<Employees> empData2 = new List<Employees>();
                    foreach (var id in activeEmployeesId)
                    {
                        empData2.Add(_context.Employees.FirstOrDefault(e => e.EmployeeId == id));
                    }
                    //getting employes from ids


                    //end
                    EMP.employees = empData2;
                    return View(EMP);
                }

                else
                {
                    var applicationDbContext = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation);
                    EMP.employees = await applicationDbContext.ToListAsync();
                    return View(EMP);
                }
            }
            else
            {
                string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                return Redirect(link);
            }

        }

        /*   [Authorize(Roles = "Admin,DepartmentHead,TeamHead")]
           public async Task<IActionResult> getActive()
           {

               TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
               DateTime dateTimeConverter = TimeZoneInfo.ConvertTime(DateTime.Now, tz);

               string[] referenceId = new string[2];
               EmployeeVM EMP = new EmployeeVM();
               EMP.depHeadsList = _context.Department_Teams_Heads.ToList();
               EMP.department = _context.Departments.ToList();
               EMP.designations = _context.Department_Designations.ToList();
               EMP.departmentTeams = _context.DepartmentTeams.ToList();

               var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
               var user = await _userManager.FindByIdAsync(userId);
               var role = await _userManager.GetRolesAsync(user);
               if (userId != null)
               {
                   if (role.ElementAt(0) == "Admin" && user.Id == "d7241547-861b-4b0a-bc8f-8e26cef589c3")
                   {
                       var applicationDbContext = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.Email != user.Email).ToList();

                       //ative employees count strt
                       List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                       List<int> activeEmployeesId = new List<int>();
                       foreach (var em in applicationDbContext)
                       {
                           empTymData.AddRange(_context.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
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
                       List<Employees> empData = new List<Employees>();
                       foreach (var id in activeEmployeesId)
                       {
                           empData.Add(_context.Employees.FirstOrDefault(e => e.EmployeeId == id));
                       }
                       //getting employes from ids


                       //end
                       EMP.employees = empData;
                       return View(EMP);
                   }
                   if (role.ElementAt(0) == "DepartmentHead")
                   {
                       var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                       var depListId = _context.Department_Teams_Heads.Where(a => a.HeadType == "Department" && a.EmployeeId == employee.EmployeeId).ToList();
                       var empData = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => (a.ReferenceUserId == employee.ReferenceUserId || a.Email == employee.Email || a.ReferenceUserId.ToString() == user.Id)).ToList();
                       //geting emp created by hr manager

                       var email = "";
                       foreach (var emp in empData.ToList())
                       {
                           if (_context.Employees.Include(s => s.Department).Any(a => a.Email == emp.Email && a.Department.DepartmentName == "HR"))
                           {
                               //   hremails.Add(emp.Email);
                               var hruser = _context.Users.Where(a => a.Email == emp.Email).FirstOrDefault().Id;
                               if (_context.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                               {
                                   empData.AddRange(_context.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).ToList());
                               }
                           }
                       }

                       //ative employees count strt
                       List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                       List<int> activeEmployeesId = new List<int>();
                       foreach (var em in empData)
                       {
                           empTymData.AddRange(_context.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
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
                       List<Employees> empData2 = new List<Employees>();
                       foreach (var id in activeEmployeesId)
                       {
                           empData2.Add(_context.Employees.FirstOrDefault(e => e.EmployeeId == id));
                       }
                       //getting employes from ids


                       //end
                       EMP.employees = empData2;
                       return View(EMP);
                   }
                   else if (role.ElementAt(0) == "TeamHead")
                   {
                       var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                       var teamHead = _context.Department_Teams_Heads.Where(a => a.EmployeeId == employee.EmployeeId).FirstOrDefault();
                       var teamEmployees = _context.department_Teams_Employees.Include(a => a.Employee).Include(a => a.GetDepartmentTeam).Where(a => a.DepartmentTeamsId == teamHead.HeadId).ToList();
                       List<Employees> applicationDbContext = new List<Employees>();
                       foreach (var item in teamEmployees)
                       {  //a.DepartmentId == employee.DepartmentId && 
                           applicationDbContext.Add(_context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).FirstOrDefault(a => a.EmployeeId == item.EmployeeId));

                       }

                       //ative employees count strt
                       List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                       List<int> activeEmployeesId = new List<int>();
                       foreach (var em in applicationDbContext)
                       {
                           empTymData.AddRange(_context.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
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
                       List<Employees> empData2 = new List<Employees>();
                       foreach (var id in activeEmployeesId)
                       {
                           empData2.Add(_context.Employees.FirstOrDefault(e => e.EmployeeId == id));
                       }
                       //getting employes from ids


                       //end
                       EMP.employees = empData2;

                       return View(EMP);
                   }
                   else if (role.ElementAt(0) == "Admin")
                   {

                       var applicationDbContext = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => (a.ReferenceUserId.ToString() == user.Id)).ToList();
                       //getting employees created by admin department head 
                       List<IdentityUser> AdmindepHeadList = new List<IdentityUser>();
                       List<Department_Teams_Heads> depHeadList = new List<Department_Teams_Heads>();
                       foreach (var item in applicationDbContext)
                       {
                           if (_context.Department_Teams_Heads.Any(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId))
                           {
                               depHeadList.Add(_context.Department_Teams_Heads.FirstOrDefault(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId));
                           }
                       }

                       var users = _context.Users.ToList();
                       foreach (var item in depHeadList.Distinct())
                       {
                           if (applicationDbContext.Any(a => a.EmployeeId == item.EmployeeId))
                           {
                               AdmindepHeadList.Add(_context.Users.Where(s => s.Email == item.Employee.Email).FirstOrDefault()); //getting dep head
                           }
                       }
                       foreach (var item in AdmindepHeadList.Distinct())
                       {
                           if (_context.Employees.Any(s => s.ReferenceUserId.ToString() == item.Id))
                           {
                               applicationDbContext.Add(_context.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                           }
                       }
                       //close
                       //geting emp created by hr manager

                       var email = "";
                       foreach (var emp in applicationDbContext)
                       {
                           if (_context.Employees.Include(s => s.Department).Any(a => a.Email == emp.Email && a.Department.DepartmentName == "HR"))
                           {
                               email = emp.Email;
                           }
                       }
                       if (email != "")
                       {
                           var hruser = _context.Users.Where(a => a.Email == email).FirstOrDefault().Id;
                           if (_context.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                           {
                               applicationDbContext.Add(_context.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).FirstOrDefault());
                           }
                       }
                       //ative employees count strt
                       List<EmployeeTimeRecord> empTymData = new List<EmployeeTimeRecord>();
                       List<int> activeEmployeesId = new List<int>();
                       foreach (var em in applicationDbContext)
                       {
                           empTymData.AddRange(_context.EmployeeTimeRecord.Include(s => s.Employee).Where(f => f.EmployeeId == em.EmployeeId && f.Date.Date == dateTimeConverter.Date));
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
                       List<Employees> empData2 = new List<Employees>();
                       foreach (var id in activeEmployeesId)
                       {
                           empData2.Add(_context.Employees.FirstOrDefault(e => e.EmployeeId == id));
                       }
                       //getting employes from ids


                       //end
                       EMP.employees = empData2;
                       return View(EMP);
                   }

                   else
                   {
                       var applicationDbContext = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation);
                       EMP.employees = await applicationDbContext.ToListAsync();
                       return View(EMP);
                   }
               }
               else
               {
                   string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                   return Redirect(link);
               }

           }
           */// GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            EmployeeVM empVM = new EmployeeVM();
            if (id == null)
            {
                return NotFound();
            }

            var employees = await _context.Employees
                .Include(e => e.Company)
                .Include(e => e.Department)
                .Include(e => e.Department_Designation)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);
            empVM.employee = employees;
            empVM.position = _context.EmployeePositions.Where(a => a.EmployeeId == id && a.Status == true).FirstOrDefault();
            empVM.PrimaryRelations = _context.EmployeeRelations.Where(a => a.EmployeeId == id && a.Type == "primary").FirstOrDefault();
            empVM.SecondaryRelations = _context.EmployeeRelations.Where(a => a.EmployeeId == id && a.Type == "secondary").FirstOrDefault();
            empVM.employeeExperienceList = _context.EmployeeExperience.Where(a => a.EmployeeId == id).ToList();
            empVM.education = _context.EmployeeEducation.Where(a => a.EmployeeId == id).ToList();
            empVM.employeeskillsList = _context.EmployeeSkill.Include(a => a.Skill).Where(a => a.EmployeeId == id).ToList();
            empVM.education = _context.EmployeeEducation.Where(a => a.EmployeeId == empVM.employee.EmployeeId).ToList();
            empVM.employeesReferencesList = _context.EmployeeReferences.Where(a => a.EmployeeId == id).ToList();

            if (_context.AssignShifts.FirstOrDefault(a => a.EmployeeId == id) != null)
            {
                empVM.shift = _context.AssignShifts.Where(a => a.EmployeeId == id).FirstOrDefault().ShiftType;
            }
            else
            {
                empVM.shift = "";
            }
            ViewData["IsCustomShift"] = false;
            ViewData["IsOfficialShift"] = false;
            var assignShift = _context.AssignShifts.Where(a => a.EmployeeId == id).FirstOrDefault();
            //  ShiftAssignVM empVM = new ShiftAssignVM();
            empVM.shiftType = empVM.shift;
            empVM.employee = await _context.Employees.Include(a => a.Department).Include(a => a.Department_Designation).Include(a => a.Company).Where(a => a.EmployeeId == id).FirstOrDefaultAsync();
            if (assignShift != null)
            {

                empVM.assignShift = assignShift;
                if (empVM.assignShift.ShiftType == "custom")
                {
                    empVM.PrevTimming = _context.EmployeeTimmings.Where(a => a.ShiftAssignId == assignShift.AssignShiftsId && a.Status == true).ToList();
                    ViewData["IsCustomShift"] = true;
                }
                else if (empVM.assignShift.ShiftType == "official")
                {
                    empVM.OfficialTimming = _context.OfficialShifts.Where(a => a.ShiftId == assignShift.ShiftId && a.CompanyId == empVM.employee.CompanyId).ToList();
                    ViewData["IsOfficialShift"] = true;
                }
            }



            if (employees == null)
            {
                return NotFound();
            }

            return View(empVM);
        }


        public async Task<IdentityResult> MakeIdentity(string Email, string Password, string Role)
        {
            try
            {

                if (_roleManager != null)
                {
                    if (!await _roleManager.RoleExistsAsync(Role))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(Role));
                    }
                }

                var user = new IdentityUser { UserName = Email, Email = Email, EmailConfirmed = true };

                var result = await _userManager.CreateAsync(user, Password);
                if (result.Succeeded)
                {

                    //_logger.LogInformation("User created a new account with password.");
                    try
                    {
                        await _userManager.AddToRoleAsync(user, Role);
                    }
                    catch (Exception e)
                    {
                        return null;
                    }
                    return result;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                return null;
            }

        }
        // GET: Employees/Create
        [Authorize(Roles = "Admin,HRManager,ProjectManager,ManagingDirector,DepartmentHead")]
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName");
            ViewData["JoiningPosition"] = new SelectList(_context.EmployeePositions.Where(a => a.Status == true).ToList(), "EmployeePositionsId", "PositionType");
            ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.Status == true).ToList(), "DepartmentId", "DepartmentName");
            ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations.Where(a => a.Status == true).ToList(), "Department_DesignationsId", "DesignationName");
            ViewData["SkillsId"] = new SelectList(_context.Skill.Where(a => a.Status == true).ToList(), "SkillsId", "SkillName");

            return View();
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeVM employees)
        {
            try
            {

                string role = "Employee";
                var designation = _context.Department_Designations.Find(employees.employee.Department_DesignationsId);
                if ((designation.DesignationName == "HR Manager")|| (designation.DesignationName == "Junior HR Manager") || (designation.DesignationName == "HR Intern"))
                {
                    role = "HRManager";
                }
                else if (designation.DesignationName == "Project Manager")
                {
                    role = "ProjectManager";
                }
                var result = await MakeIdentity(employees.employee.Email, employees.employee.Password, role);
                if (result.Succeeded == true)
                {


                    IFormFile file = Request.Form.Files["employeephoto"];
                    if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                    {
                        if (!string.IsNullOrEmpty(file.Name) && file.Length > 0)
                        {
                            String type = "EMP_IMG";
                            string fileName = $"{employees.employee.Email}_{type}_{file.FileName}";
                            String uploadfilePath = Path.Combine(_WebHost.WebRootPath, "Images//Employee");
                            if (!System.IO.Directory.Exists(uploadfilePath))
                            {
                                System.IO.Directory.CreateDirectory(uploadfilePath); //Create directory if it doesn't exist
                            }
                            string path = Path.Combine(uploadfilePath, fileName);

                            using (FileStream fs = new FileStream(path, FileMode.Create))
                            {
                                file.CopyTo(fs);
                                employees.employee.Image = fileName;
                            }
                        }
                    }

                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    var user = await _userManager.FindByIdAsync(userId);
                    var refIdUser = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault().ReferenceUserId;
                    var role1 = await _userManager.GetRolesAsync(user);

                    if (role1.ElementAt(0) == "HRManager")
                    {
                        employees.employee.ReferenceUserId = refIdUser;
                    }
                    else if ((role1.ElementAt(0) == "DepartmentHead")|| (role1.ElementAt(0) == "Admin"))
                    {
                        employees.employee.ReferenceUserId = Guid.Parse(userId);
                    }
                    /* var ownerId =Convert.ToInt32(employees.employee.EmployeeOf);
                     employees.employee.EmployeeOf = _context.Employees.FirstOrDefault(s => s.EmployeeId == ownerId).FullName;*/
                    _context.Employees.Add(employees.employee);
                    await _context.SaveChangesAsync();

                   
                    employees.position.EmployeeId = employees.employee.EmployeeId;
                    _context.EmployeePositions.Add(employees.position);
                    await _context.SaveChangesAsync();


                    employees.PrimaryRelations.EmployeeId = employees.employee.EmployeeId;
                    _context.EmployeeRelations.Add(employees.PrimaryRelations);
                    await _context.SaveChangesAsync();

                    employees.SecondaryRelations.EmployeeId = employees.employee.EmployeeId;
                    _context.EmployeeRelations.Add(employees.SecondaryRelations);
                    await _context.SaveChangesAsync();

                    if (employees.employeeExperience.Salary != null && employees.employeeExperience.CompanyName != null && employees.employeeExperience.Position != null && employees.employeeExperience.DateTo != null && employees.employeeExperience.DateFrom != null)
                    {
                        employees.employeeExperience.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeExperience.Add(employees.employeeExperience);
                        await _context.SaveChangesAsync();
                    }


                    if (employees.employeeExperience1.Salary != null && employees.employeeExperience1.CompanyName != null && employees.employeeExperience1.Position != null && employees.employeeExperience1.DateTo != null && employees.employeeExperience1.DateFrom != null)
                    {
                        employees.employeeExperience1.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeExperience.Add(employees.employeeExperience1);
                        await _context.SaveChangesAsync();
                    }

                    if (employees.employeeExperience2.Salary != null && employees.employeeExperience2.CompanyName != null && employees.employeeExperience2.Position != null && employees.employeeExperience2.DateTo != null && employees.employeeExperience2.DateFrom != null)
                    {
                        employees.employeeExperience2.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeExperience.Add(employees.employeeExperience2);
                        await _context.SaveChangesAsync();
                    }


                    if (employees.employeeReferences.FullName != null && employees.employeeReferences.CompanyName != null && employees.employeeReferences.Relationship != null && employees.employeeReferences.ContactNo != null && employees.employeeReferences.Address != null)
                    {
                        employees.employeeReferences.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeReferences.Add(employees.employeeReferences);
                        await _context.SaveChangesAsync();
                    }

                    if (employees.employeeReferences1.FullName != null && employees.employeeReferences1.CompanyName != null && employees.employeeReferences1.Relationship != null && employees.employeeReferences1.ContactNo != null && employees.employeeReferences1.Address != null)
                    {
                        employees.employeeReferences1.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeReferences.Add(employees.employeeReferences1);
                        await _context.SaveChangesAsync();
                    }

                    if (employees.education != null)
                    {
                        if (employees.education.Count() != 0)
                        {
                            foreach (var edu in employees.education)
                            {
                                edu.EmployeeId = employees.employee.EmployeeId;
                                _context.EmployeeEducation.Add(edu);
                            }

                            await _context.SaveChangesAsync();
                        }
                    }

                    if (employees.SkillsId.Count != 0)
                    {

                        foreach (var skill in employees.SkillsId)
                        {
                            EmployeeSkills empSkill = new EmployeeSkills();
                            empSkill.SkillsId = skill;
                            empSkill.EmployeeId = employees.employee.EmployeeId;
                            _context.EmployeeSkill.Add(empSkill);
                        }
                        await _context.SaveChangesAsync();
                    }
                    return RedirectToAction(nameof(Index));
                }
                ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName");
                ViewData["JoiningPosition"] = new SelectList(_context.EmployeePositions.Where(a => a.Status == true).ToList(), "EmployeePositionsId", "PositionType");
                ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.Status == true).ToList(), "DepartmentId", "DepartmentName");
                ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations.Where(a => a.Status == true).ToList(), "Department_DesignationsId", "DesignationName");
                ViewData["SkillsId"] = new SelectList(_context.Skill.Where(a => a.Status == true).ToList(), "SkillsId", "SkillName");
              
                return View(employees);

                /*     else
                     {
                         ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName");
                         ViewData["JoiningPosition"] = new SelectList(_context.EmployeePositions.Where(a => a.Status == true).ToList(), "EmployeePositionsId", "PositionType");
                         ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.Status == true).ToList(), "DepartmentId", "DepartmentName");
                         ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations.Where(a => a.Status == true).ToList(), "Department_DesignationsId", "DesignationName");
                         ViewData["SkillsId"] = new SelectList(_context.Skill.Where(a => a.Status == true).ToList(), "SkillsId", "SkillName");
                         return View(employees);
                     }*/
            }
            catch (Exception e)
            {
                ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName");
                ViewData["JoiningPosition"] = new SelectList(_context.EmployeePositions.Where(a => a.Status == true).ToList(), "EmployeePositionsId", "PositionType");
                ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.Status == true).ToList(), "DepartmentId", "DepartmentName");
                ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations.Where(a => a.Status == true).ToList(), "Department_DesignationsId", "DesignationName");
                ViewData["SkillsId"] = new SelectList(_context.Skill.Where(a => a.Status == true).ToList(), "SkillsId", "SkillName");
              
                return View(employees);
            }
        }




        [HttpPost]
        public ActionResult GetDesignations(int id)
        {
            var Department_Designations = _context.Department_Designations.Where(a => a.DepartmentId == id).ToList();
            List<SelectListItem> DesignationNames = new List<SelectListItem>();
            Department_Designations.ForEach(x =>
            {
                DesignationNames.Add(new SelectListItem { Text = x.DesignationName, Value = x.Department_DesignationsId.ToString() });
            });
            ViewData["Department_DesignationsId"] = new SelectList(Department_Designations, "Department_DesignationsId", "DesignationName"); ;
            return Json(DesignationNames);
        }




        [HttpPost]
        public ActionResult GetDepartments(int id)
        {
            var Departments = _context.Departments.Where(a => a.CompanyId == id).ToList();
            List<SelectListItem> DepartmentNames = new List<SelectListItem>();
            Departments.ForEach(x =>
            {
                DepartmentNames.Add(new SelectListItem { Text = x.DepartmentName, Value = x.DepartmentId.ToString() });
            });
            ViewData["Department_DesignationsId"] = new SelectList(Departments, "DepartmentId", "DepartmentName"); ;
            return Json(DepartmentNames);
        }

     /*   public async Task<IActionResult> getPasswordHash(string password,IdentityUser user3)
        {
            if (user3 == null)
            {
                return NotFound();
            }
            user3.PasswordHash = _userManager.PasswordHasher.HashPassword(user3,password);
            var result = await _userManager.UpdateAsync(user3);
            if (!result.Succeeded)
            {
                //throw exception......
            }

            return Ok();
        }*/
        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin,HRManager,ProjectManager,DepartmentHead,ManagingDirector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            EmployeeVM empVM = new EmployeeVM();
            empVM.employee = await _context.Employees.FindAsync(id);
            if (empVM.employee == null)
            {
                return NotFound();
            }
            empVM.position = _context.EmployeePositions.Where(a => a.EmployeeId == id && a.Status == true).FirstOrDefault();
            empVM.PrimaryRelations = _context.EmployeeRelations.Where(a => a.EmployeeId == id && a.Type == "primary").FirstOrDefault();
            empVM.SecondaryRelations = _context.EmployeeRelations.Where(a => a.EmployeeId == id && a.Type == "secondary").FirstOrDefault();
            empVM.employeeExperience = _context.EmployeeExperience.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (empVM.employeeExperience != null)
            {
                empVM.employeeExperience1 = _context.EmployeeExperience.Where(a => a.EmployeeId == id && a.EmployeeExperienceId != empVM.employeeExperience.EmployeeExperienceId).FirstOrDefault();
                if (empVM.employeeExperience1 != null)
                {
                    empVM.employeeExperience2 = _context.EmployeeExperience.Where(a => a.EmployeeId == id && a.EmployeeExperienceId != empVM.employeeExperience.EmployeeExperienceId && a.EmployeeExperienceId != empVM.employeeExperience1.EmployeeExperienceId).FirstOrDefault();
                }
            }

            empVM.employeeReferences = _context.EmployeeReferences.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (empVM.employeeReferences != null)
            {
                empVM.employeeReferences1 = _context.EmployeeReferences.Where(a => a.EmployeeId == id && a.EmployeeReferenceId != empVM.employeeReferences.EmployeeReferenceId).FirstOrDefault();

            }

            var empSkills = _context.EmployeeSkill.Where(a => a.EmployeeId == empVM.employee.EmployeeId).ToList();
            empVM.SkillsId = new List<int>();
            foreach (var skill in empSkills)
            {
                empVM.SkillsId.Add((int)skill.SkillsId);
            }

            empVM.education = _context.EmployeeEducation.Where(a => a.EmployeeId == empVM.employee.EmployeeId).ToList();
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", empVM.employee.CompanyId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.CompanyId == empVM.employee.CompanyId && a.Status == true).ToList(), "DepartmentId", "DepartmentName", empVM.employee.DepartmentId);
            ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations.Where(a => a.DepartmentId == empVM.employee.DepartmentId && a.Status == true).ToList(), "Department_DesignationsId", "DesignationName", empVM.employee.Department_DesignationsId);
            ViewData["SkillsId"] = new SelectList(_context.Skill.Where(a => a.Status == true).ToList(), "SkillsId", "SkillName", empVM.SkillsId);
       
            return View(empVM);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeVM employees)
        {
            if (id != employees.employee.EmployeeId)
            {
                return NotFound();
            }
            
            ///"Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector"
            string role1 = "Employee";
            var designation1 = _context.Department_Designations.Include(a=>a.Department).Where(s=>s.Department_DesignationsId==employees.employee.Department_DesignationsId).FirstOrDefault();
            if ((designation1.DesignationName == "HR Manager") || (designation1.Department.DepartmentName == "Management")||(designation1.DesignationName == "Junior HR Manager") || (designation1.DesignationName == "HR Intern"))
            {
                var userId1 = _context.Users.FirstOrDefault(a => a.Email == employees.employee.Email).Id;
                var user1 = await _userManager.FindByIdAsync(userId1);
                var role2 = await _userManager.GetRolesAsync(user1);
                var delOldRole = _context.UserRoles.FirstOrDefault(a => a.UserId == userId1);
                _context.UserRoles.Remove(delOldRole);
                role1 = "HRManager";
            }
            else if (designation1.DesignationName == "Project Manager")
            {
                var userId1 = _context.Users.FirstOrDefault(a => a.Email == employees.employee.Email).Id;
                var user1 = await _userManager.FindByIdAsync(userId1);
                var role2 = await _userManager.GetRolesAsync(user1);
                var delOldRole = _context.UserRoles.FirstOrDefault(a => a.UserId == userId1);
                _context.UserRoles.Remove(delOldRole);
                role1 = "ProjectManager";
            }
            else 
            {
                var userId1 = _context.Users.FirstOrDefault(a => a.Email == employees.employee.Email).Id;
                var user1 = await _userManager.FindByIdAsync(userId1);
                var role2 = await _userManager.GetRolesAsync(user1);
                role1 =role2.ElementAt(0);
            }
            var result = await MakeIdentity(employees.employee.Email, employees.employee.Password, role1);
            /*  if (ModelState.IsValid)
              {*/
            try
            {
                //var em = _context.Employees.Find(id);
                try
                {

                    IFormFile file = Request.Form.Files["employeephoto"];
                    if (file != null)
                    {
                        if (Request.Form.Files != null && Request.Form.Files.Count > 0)
                        {
                            var pathExist = "Images/Employee/" + employees.employee.Email;
                            if (System.IO.File.Exists(Path.Combine(_WebHost.WebRootPath, pathExist)))
                            {
                                try
                                {
                                    System.IO.File.Delete(Path.Combine(_WebHost.WebRootPath, pathExist));
                                }
                                catch
                                {
                                }
                            }
                            if (!string.IsNullOrEmpty(file.Name) && file.Length > 0)
                            {
                                String type = "EMP_IMG";
                                string fileName = $"{employees.employee.Email}_{type}_{file.FileName}";
                                String uploadfilePath = Path.Combine(_WebHost.WebRootPath, "Images//Employee");
                                if (!System.IO.Directory.Exists(uploadfilePath))
                                {
                                    System.IO.Directory.CreateDirectory(uploadfilePath); //Create directory if it doesn't exist
                                }
                                string path = Path.Combine(uploadfilePath, fileName);

                                using (FileStream fs = new FileStream(path, FileMode.Create))
                                {
                                    file.CopyTo(fs);
                                    employees.employee.Image = fileName;
                                }
                            }
                        }

                    }
                    // string roleNew = null;
                    string roleNew = "Employee";
                  //  var des = _context.Department_Designations.Find(13);
                    var designation = _context.Department_Designations.Where(a => a.Department_DesignationsId == employees.employee.Department_DesignationsId).FirstOrDefault();
                    if ((designation.DesignationName == "HR Manager") || (designation.DesignationName == "Junior HR Manager") || (designation.DesignationName == "HR Intern"))
                    {
                        roleNew = "HRManager";
                    }
                    else if (designation.DesignationName == "Project Manager")
                    {
                        roleNew = "ProjectManager";
                      
                    }
                    else
                    {
                        var userId1 = _context.Users.FirstOrDefault(a => a.Email == employees.employee.Email).Id;
                        var user1 = await _userManager.FindByIdAsync(userId1);
                        var role2 = await _userManager.GetRolesAsync(user1);
                        roleNew = role2.ElementAt(0);
                    }
                    if (roleNew != null)
                    {
                        var user = await _userManager.FindByEmailAsync(employees.employee.Email);
                        var role = await _userManager.GetRolesAsync(user);
                        IdentityResult deletionResult = await _userManager.RemoveFromRoleAsync(user, role.ElementAt(0));
                        if (_roleManager != null)
                        {
                            if (!await _roleManager.RoleExistsAsync(roleNew))
                            {
                                await _roleManager.CreateAsync(new IdentityRole(roleNew));
                            }

                            await _userManager.AddToRoleAsync(user, roleNew);
                        }
                    }
                    /* var userId5 = _context.Users.FirstOrDefault(a => a.Email == employees.employee.Email).Id;
                     var user5 = await _userManager.FindByIdAsync(userId5);

                     getPasswordHash(employees.employee.Password, user5);*/
                    //   employees.employee.ReferenceUserId = employees.employee.EmployeeId;
                    //  employees.employee.ReferenceUserId = em.ReferenceUserId;

                    _context.Employees.Update(employees.employee);
                    await _context.SaveChangesAsync();

                    employees.position.EmployeeId = employees.employee.EmployeeId;
                    _context.EmployeePositions.Update(employees.position);
                    await _context.SaveChangesAsync();


                    /*  var prevPositions = _context.EmployeePositions.Where(a => a.EmployeeId == id).ToList();
                      foreach (var pos in prevPositions)
                      {

                          pos.EmployeeId = employees.employee.EmployeeId;
                          pos.Status = false;
                          _context.EmployeePositions.Update(pos);
                      }
                      await _context.SaveChangesAsync();
                      var position = employees.position;
                      position.EmployeePositionsId = 0;
                      position.EmployeeId = id;
                      position.Status = true;
                      _context.EmployeePositions.Add(position);
                      _context.SaveChanges();
*/


                    employees.PrimaryRelations.EmployeeId = employees.employee.EmployeeId;
                    _context.EmployeeRelations.Update(employees.PrimaryRelations);
                    await _context.SaveChangesAsync();


                    employees.SecondaryRelations.EmployeeId = employees.employee.EmployeeId;
                    _context.EmployeeRelations.Update(employees.SecondaryRelations);
                    await _context.SaveChangesAsync();

                    if (employees.employeeExperience.Salary != null && employees.employeeExperience.CompanyName != null && employees.employeeExperience.Position != null && employees.employeeExperience.DateTo != null && employees.employeeExperience.DateFrom != null)
                    {
                        employees.employeeExperience.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeExperience.Update(employees.employeeExperience);
                        await _context.SaveChangesAsync();
                    }


                    if (employees.employeeExperience1.Salary != null && employees.employeeExperience1.CompanyName != null && employees.employeeExperience1.Position != null && employees.employeeExperience1.DateTo != null && employees.employeeExperience1.DateFrom != null)
                    {
                        employees.employeeExperience1.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeExperience.Update(employees.employeeExperience1);
                        await _context.SaveChangesAsync();
                    }

                    if (employees.employeeExperience2.Salary != null && employees.employeeExperience2.CompanyName != null && employees.employeeExperience2.Position != null && employees.employeeExperience2.DateTo != null && employees.employeeExperience2.DateFrom != null)
                    {
                        employees.employeeExperience2.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeExperience.Update(employees.employeeExperience2);
                        await _context.SaveChangesAsync();
                    }


                    if (employees.employeeReferences.FullName != null && employees.employeeReferences.CompanyName != null && employees.employeeReferences.Relationship != null && employees.employeeReferences.ContactNo != null && employees.employeeReferences.Address != null)
                    {
                        employees.employeeReferences.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeReferences.Update(employees.employeeReferences);
                        await _context.SaveChangesAsync();
                    }

                    if (employees.employeeReferences1.FullName != null && employees.employeeReferences1.CompanyName != null && employees.employeeReferences1.Relationship != null && employees.employeeReferences1.ContactNo != null && employees.employeeReferences1.Address != null)
                    {
                        employees.employeeReferences1.EmployeeId = employees.employee.EmployeeId;
                        _context.EmployeeReferences.Update(employees.employeeReferences1);
                        await _context.SaveChangesAsync();
                    }



                    var empEdu = _context.EmployeeEducation.Where(a => a.EmployeeId == employees.employee.EmployeeId).ToList();
                    if (empEdu.Count() != 0)
                    {
                        _context.EmployeeEducation.RemoveRange(empEdu);

                        _context.SaveChanges();
                    }




                    if (employees.education != null)
                    {
                        if (employees.education.Count() != 0)
                        {

                            foreach (var edu in employees.education)
                            {

                                edu.EmployeeId = employees.employee.EmployeeId;

                                _context.EmployeeEducation.Add(edu);


                            }

                            await _context.SaveChangesAsync();
                        }
                    }



                    var EmpSkillsList = _context.EmployeeSkill.Where(a => a.EmployeeId == employees.employee.EmployeeId).ToList();
                    _context.EmployeeSkill.RemoveRange(EmpSkillsList);
                    _context.SaveChanges();

                    if (employees.SkillsId.Count != 0)
                    {

                        foreach (var skill in employees.SkillsId)
                        {
                            EmployeeSkills empSkill = new EmployeeSkills();
                            empSkill.SkillsId = skill;
                            empSkill.EmployeeId = employees.employee.EmployeeId;
                            _context.EmployeeSkill.Add(empSkill);
                        }
                        _context.SaveChanges();
                    }
                    return RedirectToAction(nameof(Index));




                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EmployeesExists(employees.employee.EmployeeId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));

            }
            catch (Exception e)
            {
                ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyId", employees.employee.CompanyId);
                ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.Status == true).ToList(), "DepartmentId", "DepartmentId", employees.employee.DepartmentId);
                ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations.Where(a => a.Status == true).ToList(), "Department_DesignationsId", "Department_DesignationsId", employees.employee.Department_DesignationsId);
                ViewData["SkillsId"] = new SelectList(_context.Skill.Where(a => a.Status == true).ToList(), "SkillsId", "SkillName", employees.SkillsId);
               
                return View(employees);
            }
            /* }
             ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyId", employees.employee.CompanyId);
             ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.Status == true).ToList(), "DepartmentId", "DepartmentId", employees.employee.DepartmentId);
             ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations.Where(a => a.Status == true).ToList(), "Department_DesignationsId", "Department_DesignationsId", employees.employee.Department_DesignationsId);
             ViewData["SkillsId"] = new SelectList(_context.Skill.Where(a => a.Status == true).ToList(), "SkillsId", "SkillName", employees.SkillsId);
             return View(employees);*/

        }





        [Authorize(Roles = "Admin,HRManager,DepartmentHead,ProjectManager,ManagingDirector,TeamHead")]
        public ActionResult ChangeStatus(int? id) //RemarksForChngStatus
        {
            try
            {
                var Employees = _context.Employees.Find(id);
                if (id == null || Employees == null)
                {
                    return NotFound();
                }
                if ((Employees.RemarksForChngStatus != null)&& ((User.IsInRole("DepartmentHead")) || (User.IsInRole("Admin"))))
                {
                    Employees.RemarksForChngStatus = null;
                }
                var empPos = _context.EmployeePositions.Where(a => a.EmployeeId == id).FirstOrDefault();
                empPos.EndDate = DateTime.Now;
                _context.EmployeePositions.Update(empPos);
                _context.SaveChanges();

                Employees.Status = !(Employees.Status);
                _context.Employees.Update(Employees);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }



        // GET: Employees/Edit/5
        [Authorize(Roles = "Admin,HRManager,ProjectManager,DepartmentHead,ManagingDirector")]
        public async Task<IActionResult> JobExtend(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            EmployeeVM empVM = new EmployeeVM();
            empVM.employee = await _context.Employees.Include(a => a.Company).Where(a => a.EmployeeId == id).FirstOrDefaultAsync();
            if (empVM.employee == null)
            {
                return NotFound();
            }
            empVM.position = _context.EmployeePositions.Where(a => a.EmployeeId == id && a.Status == true).FirstOrDefault();

            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", empVM.employee.CompanyId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.CompanyId == empVM.employee.CompanyId && a.Status == true).ToList(), "DepartmentId", "DepartmentName", empVM.employee.DepartmentId);
            ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations.Where(a => a.DepartmentId == empVM.employee.DepartmentId && a.Status == true).ToList(), "Department_DesignationsId", "DesignationName", empVM.employee.Department_DesignationsId);
            ViewData["SkillsId"] = new SelectList(_context.Skill.Where(a => a.Status == true).ToList(), "SkillsId", "SkillName", empVM.SkillsId);
            return View(empVM);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JobExtend(int id, EmployeeVM employees)
        {
            if (id != employees.employee.EmployeeId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    try
                    {
                        var employeeDb = _context.Employees.Find(id);
                        employeeDb.DepartmentId = employees.employee.DepartmentId;
                        employeeDb.Department_DesignationsId = employees.employee.Department_DesignationsId;

                        _context.Employees.Update(employeeDb);
                        await _context.SaveChangesAsync();


                        var prevPositions = _context.EmployeePositions.Where(a => a.EmployeeId == employeeDb.EmployeeId).ToList();
                        foreach (var pos in prevPositions)
                        {

                            pos.EmployeeId = employees.employee.EmployeeId;
                            pos.Status = false;
                            _context.EmployeePositions.Update(pos);
                        }
                        await _context.SaveChangesAsync();
                        var position = employees.position;
                        position.EmployeePositionsId = 0;
                        position.EmployeeId = id;
                        position.Status = true;
                        _context.EmployeePositions.Add(position);
                        _context.SaveChanges();




                        return RedirectToAction(nameof(Index));




                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!EmployeesExists(employees.employee.EmployeeId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }


                }
                catch (Exception e)
                {
                    ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyId", employees.employee.CompanyId);
                    ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId", employees.employee.DepartmentId);
                    ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations, "Department_DesignationsId", "Department_DesignationsId", employees.employee.Department_DesignationsId);
                    ViewData["SkillsId"] = new SelectList(_context.Skill, "SkillsId", "SkillName", employees.SkillsId);
                    return View(employees);
                }
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyId", employees.employee.CompanyId);
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentId", employees.employee.DepartmentId);
            ViewData["Department_DesignationsId"] = new SelectList(_context.Department_Designations, "Department_DesignationsId", "Department_DesignationsId", employees.employee.Department_DesignationsId);
            ViewData["SkillsId"] = new SelectList(_context.Skill, "SkillsId", "SkillName", employees.SkillsId);
            return View(employees);

        }




        // GET: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignShifts(string empid, string shiftType)
        {
            int id = Convert.ToInt32(empid);
            if (id == 0)
            {
                return NotFound();
            }
            ViewData["IsCustomShift"] = false;
            ViewData["IsOfficialShift"] = false;
            var assignShift = _context.AssignShifts.Where(a => a.EmployeeId == id).FirstOrDefault();
            ShiftAssignVM empVM = new ShiftAssignVM();
            empVM.shiftType = shiftType;
            empVM.employee = await _context.Employees.Include(a => a.Department).Include(a => a.Department_Designation).Include(a => a.Company).Where(a => a.EmployeeId == id).FirstOrDefaultAsync();
            if (assignShift != null)
            {

                empVM.assignShift = assignShift;
                if (empVM.assignShift.ShiftType == "custom")
                {
                    empVM.PrevTimming = _context.EmployeeTimmings.Where(a => a.ShiftAssignId == assignShift.AssignShiftsId && a.Status == true).ToList();
                    ViewData["IsCustomShift"] = true;
                }
                else if (empVM.assignShift.ShiftType == "official")
                {
                    empVM.OfficialTimming = _context.OfficialShifts.Where(a => a.ShiftId == assignShift.ShiftId && a.CompanyId == empVM.employee.CompanyId).ToList();
                    ViewData["IsOfficialShift"] = true;
                }
            }
            if (empVM.employee == null)
            {
                return NotFound();
            }
            empVM.position = _context.EmployeePositions.FirstOrDefault(a => a.EmployeeId == id).PositionType;
            ViewData["ShiftId"] = new SelectList(_context.Shifts.Where(a => a.CompanyId == empVM.employee.CompanyId && a.Status == true), "ShiftId", "ShiftName");
            return View(empVM);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignShift1(ShiftAssignVM shift)
        {
            if (shift.assignShift.AssignShiftsId != 0) {
                var customShift = _context.EmployeeTimmings.Where(a => a.ShiftAssignId == shift.assignShift.AssignShiftsId).ToList();
                if (customShift.Count() != 0)
                {
                    _context.EmployeeTimmings.RemoveRange(customShift);
                    await _context.SaveChangesAsync();
                }

                await _context.SaveChangesAsync();
                var ashift = _context.AssignShifts.Find(shift.assignShift.AssignShiftsId);
                _context.AssignShifts.Remove(ashift);
                shift.assignShift.AssignShiftsId = 0;
            }
            //    shift.assignShift.EmployeeId = shift.employee.EmployeeId;
            shift.assignShift.ShiftType = shift.shiftType;
            _context.Add(shift.assignShift);
            await _context.SaveChangesAsync();

            if (shift.shiftType == "custom")
            {



                if (shift.Timming.Count != 0)
                {
                    foreach (var timing in shift.Timming)
                    {
                        timing.ShiftAssignId = shift.assignShift.AssignShiftsId;

                        _context.EmployeeTimmings.Add(timing);
                        await _context.SaveChangesAsync();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
            return View();

        }
        // GET: Employees/Delete/5

        [Authorize(Roles = "Admin,HRManager,DepartmentHead,ProjectManager,ManagingDirector")]
        public async Task<IActionResult> ShiftAssign(string empid, string shiftType)
        {
            /*   var id = Convert.ToInt32(empid);
               if (empid == null)
               {
                   return NotFound();
               }
               if(shiftType == "custom")
               {
                   shiftTyp
               }*/

            /*  var employees = await _context.Employees
                  .Include(e => e.Company)
                  .Include(e => e.Department)
                  .Include(e => e.Department_Designation)
                  .FirstOrDefaultAsync(m => m.EmployeeId == empid);*/
            /*            if (employees == null)
                        {
                            return NotFound();
                        }*/

            return View();
        }
        // GET: Employees/Delete/5
        [HttpPost]
        public IActionResult Delete(int? id)
        {
            try
            { 
            if (id == null)
            {
                return NotFound();
            }

            var employees = _context.Employees.Find(id);
            if (employees == null)
            {
                return NotFound();
            }
            var empEdu = _context.EmployeeEducation.Where(a => a.EmployeeId == id);
            var empExp = _context.EmployeeExperience.Where(a => a.EmployeeId == id);
            var empRef = _context.EmployeeReferences.Where(a => a.EmployeeId == id);
            var empRel = _context.EmployeeRelations.Where(a => a.EmployeeId == id);
            var empRep = _context.EmployeeReports.Where(a => a.EmployeeId == id);
            var empSkills = _context.EmployeeSkill.Where(a => a.EmployeeId == id);
            var empPos = _context.EmployeePositions.Where(a => a.EmployeeId == id).FirstOrDefault();
            if (empPos != null)
            {
                var empSalaryHistory = _context.EmployeeSalaryHistory.Where(a => a.EmployeePositionsId == empPos.EmployeePositionsId).ToList();
                if (empSalaryHistory.Count() > 0)
                {
                    _context.EmployeeSalaryHistory.RemoveRange(empSalaryHistory);
                    _context.SaveChanges();
                    _context.EmployeePositions.Remove(empPos);
                    _context.SaveChanges();
                }
            }
            if (empEdu.Count() > 0)
            {
                _context.EmployeeEducation.RemoveRange(empEdu);
                _context.SaveChanges();
            }
            if (empExp.Count() > 0)
            {
                _context.EmployeeExperience.RemoveRange(empExp);
                _context.SaveChanges();
            }
            if (empRef.Count() > 0)
            {
                _context.EmployeeReferences.RemoveRange(empRef);
                _context.SaveChanges();
            }
            if (empRel.Count() > 0)
            {
                _context.EmployeeRelations.RemoveRange(empRel);
                _context.SaveChanges();
            }
            if (empRep.Count() > 0)
            {
                _context.EmployeeReports.RemoveRange(empRep);
                _context.SaveChanges();
            }
            if (empSkills.Count() > 0)
            {
                _context.EmployeeSkill.RemoveRange(empSkills);
                _context.SaveChanges();
            }



            //removing dependant data
            var empTeams = _context.department_Teams_Employees.Where(a => a.EmployeeId == id);
            var empHead = _context.Department_Teams_Heads.Where(a => a.EmployeeId == id); //show on pop up that he is the head
            var empLeaves = _context.Leaves.Where(a => a.EmployeeId == id);
            var empShift = _context.EmployeeIndividualShift.Where(a => a.EmployeeId == id);
            var empShiftsAssign = _context.AssignShifts.Where(a => a.EmployeeId == id).FirstOrDefault();
                if (empShiftsAssign != null)
                {
                    var empTimmings = _context.EmployeeTimmings.Where(a => a.ShiftAssignId == empShiftsAssign.AssignShiftsId);
                    if (empTimmings.Count() > 0)
                    {
                        _context.EmployeeTimmings.RemoveRange(empTimmings);
                        _context.SaveChanges();
                    }
                    _context.AssignShifts.Remove(empShiftsAssign);
                    _context.SaveChanges();
                }
                    
            var empTimeRecord = _context.EmployeeTimeRecord.Where(a => a.EmployeeId == id);
            List<int> chkOutAppList = new List<int>();
            foreach (var items in empTimeRecord.Where(a => a.RecordTypeName == "checkout").ToList())
            {
                if (_context.CheckoutApprovalRequests.FirstOrDefault(a => a.EmployeeTimeRecordId == items.EmployeeTimeRecordId) != null)
                {
                    chkOutAppList.Add(_context.CheckoutApprovalRequests.FirstOrDefault(a => a.EmployeeTimeRecordId == items.EmployeeTimeRecordId).CheckoutApprovalRequestId);
                }
            }
            //
            if (empTeams.Count() > 0)
            {
                _context.department_Teams_Employees.RemoveRange(empTeams);
                _context.SaveChanges();
            }
            if (empHead.Count() > 0)
            {
                _context.Department_Teams_Heads.RemoveRange(empHead);
                _context.SaveChanges();
            }
            if (empLeaves.Count() > 0)
            {
                _context.Leaves.RemoveRange(empLeaves);
                _context.SaveChanges();
            }
            if (empShift.Count() > 0)
            {
                _context.EmployeeIndividualShift.RemoveRange(empShift);
                _context.SaveChanges();
            }
          
            if (chkOutAppList.Count() > 0)
            {
                foreach (var items in chkOutAppList)
                {

                    var delData = _context.CheckoutApprovalRequests.Find(items);
                    _context.CheckoutApprovalRequests.Remove(delData);
                    _context.SaveChanges();
                }
            }
            if (empTimeRecord.Count() > 0)
            {
                _context.EmployeeTimeRecord.RemoveRange(empTimeRecord);
                _context.SaveChanges();
            }
            //identity del
            var aspUser = _context.Users.FirstOrDefault(a => a.Email == employees.Email);
            var aspUserRole = _context.UserRoles.FirstOrDefault(a => a.UserId == aspUser.Id);

            _context.UserRoles.Remove(aspUserRole);
            _context.SaveChanges();

            _context.Users.Remove(aspUser);
            _context.SaveChanges();

            //deleting parent
            _context.Employees.Remove(employees);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        catch (Exception e)
            {
            }
            return RedirectToAction("Index");
        }

        // POST: Employees/Delete/5
       /* [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employees = await _context.Employees.FindAsync(id);
            _context.Employees.Remove(employees);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }*/

        private bool EmployeesExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
        // GET: Employees
        /*   [Authorize(Roles = "Admin,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
           public async Task<IActionResult> Index()
           {
               *//*   dynamic myModel = new ExpandoObject();
                  myModel.depHeadList = _context.Department_Teams_Heads.ToList();*//*
               string[] referenceId = new string[2];
              EmployeeVM EMP = new EmployeeVM();
               EMP.depHeadsList = _context.Department_Teams_Heads.ToList();
               EMP.department = _context.Departments.ToList();
               EMP.designations = _context.Department_Designations.ToList();
               EMP.departmentTeams = _context.DepartmentTeams.ToList();
               EMP.employeePositions = _context.EmployeePositions.ToList();

               var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
               var user = await _userManager.FindByIdAsync(userId);
               var role = await _userManager.GetRolesAsync(user);
               if (userId != null)
               {
                   if (role.ElementAt(0) == "Admin" && user.Id == "d7241547-861b-4b0a-bc8f-8e26cef589c3")
                   {
                       var applicationDbContext = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a=>a.Email!=user.Email);
                       EMP.employees = await applicationDbContext.ToListAsync();
                       return View(EMP);
                   }
                       if (role.ElementAt(0) == "DepartmentHead")
                   {
                       var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                      var depListId = _context.Department_Teams_Heads.Where(a => a.HeadType == "Department" && a.EmployeeId == employee.EmployeeId).ToList();
                       var empData = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => (a.ReferenceUserId == employee.ReferenceUserId || a.Email==employee.Email|| a.ReferenceUserId.ToString()==user.Id)).ToList();
                       //geting emp created by hr manager

                       var email = "";
                       foreach (var emp in empData.ToList())
                       {
                           if (_context.Employees.Include(s => s.Department).Any(a => a.Email == emp.Email && a.Department.DepartmentName == "HR"))
                           {
                               //   hremails.Add(emp.Email);
                               var hruser = _context.Users.Where(a => a.Email == emp.Email).FirstOrDefault().Id;
                               if (_context.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                               {
                                   empData.AddRange(_context.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).ToList());
                               }
                           }
                       }
                       EMP.employees = empData;
                       return View(EMP);
                   }
                   else if (role.ElementAt(0) == "TeamHead")
                   {
                       var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                       var teamHead = _context.Department_Teams_Heads.Where(a => a.EmployeeId == employee.EmployeeId).FirstOrDefault();
                       var teamEmployees =_context.department_Teams_Employees.Include(a => a.Employee).Include(a => a.GetDepartmentTeam).Where(a => a.DepartmentTeamsId == teamHead.HeadId).ToList();
                       List<Employees> applicationDbContext = new List<Employees>();
                       foreach (var item in teamEmployees)
                       {  //a.DepartmentId == employee.DepartmentId && 
                           applicationDbContext.Add(_context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).FirstOrDefault(a => a.EmployeeId == item.EmployeeId));

                       }

                     //  var applicationDbContext = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a => a.DepartmentId == employee.DepartmentId && a.Email != employee.Email && a.ReferenceUserId == employee.ReferenceUserId);
                       EMP.employees =  applicationDbContext.ToList();
                       return View(EMP);
                   }
                   else if(role.ElementAt(0) == "Admin")
                   {

                       var applicationDbContext = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).Where(a =>  (a.ReferenceUserId.ToString() ==user.Id)).ToList();
                     //getting employees created by admin department head 
                       List<IdentityUser> AdmindepHeadList = new List<IdentityUser>();
                       List<Department_Teams_Heads> depHeadList = new List<Department_Teams_Heads>();
                       foreach (var item in applicationDbContext)
                       {
                           if (_context.Department_Teams_Heads.Any(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId))
                           {
                               depHeadList.Add(_context.Department_Teams_Heads.FirstOrDefault(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId));
                           }
                       }

                       var users = _context.Users.ToList();
                       foreach (var item in depHeadList.Distinct())
                       {
                           if (applicationDbContext.Any(a => a.EmployeeId == item.EmployeeId))
                           {
                               AdmindepHeadList.Add(_context.Users.Where(s => s.Email == item.Employee.Email).FirstOrDefault()); //getting dep head
                           }
                       }
                       foreach (var item in AdmindepHeadList.Distinct())
                       {
                           if (_context.Employees.Any(s => s.ReferenceUserId.ToString() == item.Id))
                           {
                               applicationDbContext.Add(_context.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                           }
                       }
                       //close
                       //geting emp created by hr manager

                       var email = "";
                       foreach (var emp in applicationDbContext)
                       {
                           if (_context.Employees.Include(s => s.Department).Any(a => a.Email == emp.Email && a.Department.DepartmentName == "HR"))
                           {
                               email = emp.Email;
                           }
                       }
                       if (email != "")
                       {
                           var hruser = _context.Users.Where(a => a.Email == email).FirstOrDefault().Id;
                           if (_context.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                           {
                               applicationDbContext.Add(_context.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).FirstOrDefault());
                           }
                       }
                       EMP.employees =  applicationDbContext;
                       return View(EMP);
                   }
                   else if (role.ElementAt(0) == "HRManager")
                   {
                       //var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                       referenceId[0] = user.Id;

                           var emps = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation).ToList();
                           referenceId[1] = emps.FirstOrDefault(a => a.Email == user.Email).ReferenceUserId.ToString();
                           emps = emps.Where(a => referenceId.Any(b => b == a.ReferenceUserId.ToString())).ToList();

                       var applicationDbContext = emps;

                       //getting employyees created byanother hr

                       var email = "";
                       foreach (var emp in applicationDbContext)
                       {
                           if (_context.Employees.Include(s => s.Department).Any(a => a.Email == emp.Email && (a.Department.DepartmentName == "HR" || a.Department.DepartmentName == "Management")))
                           {
                               email = emp.Email;
                           }
                       }
                       var hruser = _context.Users.Where(a => a.Email == email).FirstOrDefault().Id;
                       if (_context.Employees.Any(a => a.ReferenceUserId.ToString() == hruser))
                       {
                           applicationDbContext.Add(_context.Employees.Where(a => a.ReferenceUserId.ToString() == hruser).FirstOrDefault());
                       }
                       //close

                       //getting employyees created by dep head ref id
                       List<IdentityUser> AdmindepHeadList = new List<IdentityUser>();
                       List<Department_Teams_Heads> depHeadList = new List<Department_Teams_Heads>();
                       foreach (var item in applicationDbContext)
                       {
                           if (_context.Department_Teams_Heads.Any(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId))
                           {
                               depHeadList.Add(_context.Department_Teams_Heads.FirstOrDefault(a => a.HeadType == "Department" && a.EmployeeId == item.EmployeeId));
                           }
                       }

                       var users = _context.Users.ToList();
                       foreach (var item in depHeadList.Distinct())
                       {
                           if (applicationDbContext.Any(a => a.EmployeeId == item.EmployeeId))
                           {
                               AdmindepHeadList.Add(_context.Users.Where(s => s.Email == item.Employee.Email).FirstOrDefault()); //getting dep head
                           }
                       }
                       foreach (var item in AdmindepHeadList.Distinct())
                       {
                           if (_context.Employees.Any(s => s.ReferenceUserId.ToString() == item.Id))
                           {
                               applicationDbContext.Add(_context.Employees.FirstOrDefault(s => s.ReferenceUserId.ToString() == item.Id));
                           }
                       }
                       //close
                       EMP.employees = applicationDbContext;
                       return View(EMP);
                   }
                   else
                   {
                       var applicationDbContext = _context.Employees.Include(e => e.Company).Include(e => e.Department).Include(e => e.Department_Designation);
                       EMP.employees = await applicationDbContext.ToListAsync();
                       return View(EMP);
                   }
               }
               else
               {
                   string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                   return Redirect(link);
               }

           }*/

    }
}

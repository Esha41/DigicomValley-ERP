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
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_Project.Controllers
{
    public class TeamsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public DepartmentsVM dvm { get; set; }
        public TeamsController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)

        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            dvm = new DepartmentsVM()
            {
                department_Teams_Head_List = new List<Department_Teams_Heads>(),
                EmployeesIds = new List<int>(),
                TeamEmployeesIds = new List<int>(),
                teams=new DepartmentTeams()
            };
        }
        // GET: TeamsController
        [Authorize(Roles = "Admin,HRManager,DepartmentHead,ProjectManager,ManagingDirector")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (userId != null)
            {
                if (role.ElementAt(0) == "Admin" && user.Id == "d7241547-861b-4b0a-bc8f-8e26cef589c3")
                {
                    dvm.DepartmentTeamsList = _db.DepartmentTeams.Include(d => d.Department).ThenInclude(d => d.Company).ToList();
                    dvm.department_Teams_Head_List = _db.Department_Teams_Heads.Include(h => h.Employee).Where(h => h.HeadType == "Team").ToList();
                    dvm.depteamsEmployeesList = _db.department_Teams_Employees.Include(a => a.Employee).ToList();
                }
                   else if (role.ElementAt(0) == "Admin")
                {
                    dvm.DepartmentTeamsList = _db.DepartmentTeams.Include(d => d.Department).ThenInclude(d => d.Company).Where(a => a.ReferenceUserId.ToString() == user.Id).ToList();
                    dvm.department_Teams_Head_List = _db.Department_Teams_Heads.Include(h => h.Employee).Where(h => h.HeadType == "Team").ToList();
                    dvm.depteamsEmployeesList = _db.department_Teams_Employees.Include(a => a.Employee).Where(a => a.Employee.ReferenceUserId.ToString() == user.Id).ToList();
                }
                if (role.ElementAt(0) == "DepartmentHead")
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    dvm.DepartmentTeamsList = _db.DepartmentTeams.Include(d => d.Department).ThenInclude(d => d.Company).Where(a => a.ReferenceUserId.ToString() == user.Id).ToList();
                    dvm.department_Teams_Head_List = _db.Department_Teams_Heads.Include(h => h.Employee).Where(h => h.HeadType == "Team").ToList();
                    dvm.depteamsEmployeesList = _db.department_Teams_Employees.Include(a => a.Employee).ToList();
                }
            }
            else
            {
                dvm.DepartmentTeamsList = _db.DepartmentTeams.Include(d => d.Department).ThenInclude(d => d.Company).ToList();
                dvm.department_Teams_Head_List = _db.Department_Teams_Heads.Include(h => h.Employee).Where(h => h.HeadType == "Team").ToList();
                dvm.depteamsEmployeesList = _db.department_Teams_Employees.ToList();
            }
            return View(dvm);
        }

        // GET: TeamsController/Details/5
        [Authorize(Roles = "Admin,HRManager,DepartmentHead,ProjectManager,ManagingDirector")]
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: TeamsController/Create
        [Authorize(Roles = "Admin,HRManager,ProjectManager,ManagingDirector")]
        public ActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_db.Companies, "CompanyId", "CompanyName");
            ViewData["DepartmentId"] = new SelectList(_db.Departments.Where(a=>a.Status==true), "DepartmentId", "DepartmentName");
            ViewData["EmployeeId"] = new SelectList(_db.Employees.Where(e=>e.Status==true), "EmployeeId", "FullName");
            
            return View();
        }

        // POST: TeamsController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentsVM DVM)
        {
            try
            {
                if (_db.DepartmentTeams.Any(a => (a.TeamName == DVM.teams.TeamName) &&( a.DepartmentId == DVM.teams.DepartmentId )))
                {//( a.Department.CompanyId == DVM.teams.Department.CompanyId)
                    return RedirectToAction(nameof(Create));
                }


                var a = DVM.teams.DepartmentId;
                DVM.teams.Department = null;

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                DVM.teams.ReferenceUserId = Guid.Parse(userId);

                _db.DepartmentTeams.Add(DVM.teams);
                _db.SaveChanges();

                if (DVM.EmployeesIds!=null)
                {
                    Department_Teams_Heads dth = new Department_Teams_Heads();
                    foreach (var item in DVM.EmployeesIds)
                    {
                        dth.Department_Teams_HeadsId = 0;
                        dth.HeadType = "Team";
                        dth.HeadId = DVM.teams.DepartmentTeamsId;
                        dth.EmployeeId = item;
                        dth.Date = DateTime.Now;
                        dth.Status = true;
                        
                        _db.Department_Teams_Heads.Add(dth);
                        await _db.SaveChangesAsync();
                        var employee = _db.Employees.Find(item);
                        var user = await _userManager.FindByEmailAsync(employee.Email);
                        var role = await _userManager.GetRolesAsync(user);
                        IdentityResult deletionResult = await _userManager.RemoveFromRoleAsync(user, role.ElementAt(0));
                        if (_roleManager != null)
                        {
                            if (!await _roleManager.RoleExistsAsync("TeamHead"));
                            {
                                await _roleManager.CreateAsync(new IdentityRole("TeamHead"));
                            }

                            await _userManager.AddToRoleAsync(user, "TeamHead");
                        }
                    }
                }
                if (DVM.TeamEmployeesIds!=null)
                {
                    Department_Teams_Employees dte = new Department_Teams_Employees();
                    foreach (var item in DVM.TeamEmployeesIds)
                    {
                        dte.Department_Teams_EmployeesId = 0;
                        dte.DepartmentTeamsId = DVM.teams.DepartmentTeamsId;
                        dte.EmployeeId = item;
                        dte.Date = DateTime.Now;
                        dte.Status = true;
                        _db.department_Teams_Employees.Add(dte);
                        _db.SaveChanges();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: TeamsController/Edit/5
        [Authorize(Roles = "Admin,HRManager,DepartmentHead,ProjectManager,ManagingDirector")]
        public ActionResult Edit(int id)
        {
          
            if (_db.DepartmentTeams.Find(id) == null)
            {
                return NotFound();
            }
            var teams = _db.DepartmentTeams.Include(t => t.Department).ThenInclude(t => t.Company).FirstOrDefault(t => t.DepartmentTeamsId == id);
            dvm.teams = teams;
            dvm.Companies = _db.Companies.Where(c => c.CompanyId == teams.Department.CompanyId).FirstOrDefault();
            dvm.departments = _db.Departments.Where(c => c.DepartmentId == teams.DepartmentId).FirstOrDefault();

            dvm.department_Teams_Head_List = _db.Department_Teams_Heads.Include(d => d.Employee).Where(d => d.HeadId == teams.DepartmentTeamsId && d.HeadType == "Team").ToList();
            dvm.EmployeeList = dvm.department_Teams_Head_List.Select(s => s.Employee).ToList();
            dvm.EmployeesIds.AddRange(dvm.EmployeeList.Select(a => a.EmployeeId).ToList());

            dvm.depteamsEmployeesList = _db.department_Teams_Employees.Include(d => d.Employee).Where(d => d.DepartmentTeamsId == teams.DepartmentTeamsId).ToList();
            dvm.TeamEmployeesList = dvm.depteamsEmployeesList.Select(s => s.Employee).ToList();
            dvm.TeamEmployeesIds.AddRange(dvm.TeamEmployeesList.Select(a => a.EmployeeId).ToList());

            ViewData["TeamsEmployeeId"] = new SelectList(_db.Employees.Where(e => e.Status == true&&e.CompanyId==teams.Department.CompanyId&&e.DepartmentId==teams.DepartmentId), "EmployeeId", "FullName", dvm.TeamEmployeesIds);
            ViewData["EmployeeId"] = new SelectList(_db.Employees.Where(e=>e.Status==true && e.CompanyId == teams.Department.CompanyId && e.DepartmentId == teams.DepartmentId), "EmployeeId", "FullName", dvm.EmployeesIds);
            ViewData["CompanyId"] = new SelectList(_db.Companies, "CompanyId", "CompanyName", dvm.Companies);
            ViewData["DepartmentId"] = new SelectList(_db.Departments.Where(a => a.Status == true), "DepartmentId", "DepartmentName", dvm.departments);


            return View(dvm);
        }

        // POST: TeamsController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(DepartmentsVM DVM)
        {
            try
            {
                var check = _db.DepartmentTeams.ToList();
                if (_db.DepartmentTeams.Any(a => (a.TeamName == DVM.teams.TeamName) && (a.DepartmentTeamsId != DVM.teams.DepartmentTeamsId) && (a.DepartmentId == DVM.teams.DepartmentId)))
                {
                    return RedirectToAction(nameof(Edit));
                }
                var teams = _db.DepartmentTeams.Find(DVM.teams.DepartmentTeamsId);
                teams.TeamName = DVM.teams.TeamName;
                _db.DepartmentTeams.Update(teams);
                _db.SaveChanges();

                var teamHeadList = _db.Department_Teams_Heads.Include(a=>a.Employee).Where(a => a.HeadId == teams.DepartmentTeamsId&&a.HeadType=="Team").ToList();
                foreach (var head in teamHeadList)
                {
                    var user = await _userManager.FindByEmailAsync(head.Employee.Email);
                    var role = await _userManager.GetRolesAsync(user);
                    IdentityResult deletionResult = await _userManager.RemoveFromRoleAsync(user, role.ElementAt(0));
                    if (_roleManager != null)
                    {
                        if (!await _roleManager.RoleExistsAsync("Employee"))
                        {
                            await _roleManager.CreateAsync(new IdentityRole("Employee"));
                        }

                        await _userManager.AddToRoleAsync(user, "Employee");
                    }
                }
                _db.Department_Teams_Heads.RemoveRange(teamHeadList);
                 _db.SaveChanges();

                if (DVM.EmployeesIds != null)
                {
                    Department_Teams_Heads dth = new Department_Teams_Heads();
                    foreach (var item in DVM.EmployeesIds)
                    {
                        dth.Department_Teams_HeadsId = 0;
                        dth.HeadType = "Team";
                        dth.HeadId = DVM.teams.DepartmentTeamsId;
                        dth.EmployeeId = item;
                        dth.Date = DateTime.Now;
                        dth.Status = true;
                        _db.Department_Teams_Heads.Add(dth);
                        _db.SaveChanges();
                        var employee = _db.Employees.Find(item);
                        var user = await _userManager.FindByEmailAsync(employee.Email);
                        var role = await _userManager.GetRolesAsync(user);
                        IdentityResult deletionResult = await _userManager.RemoveFromRoleAsync(user, role.ElementAt(0));
                        if (_roleManager != null)
                        {
                            if (!await _roleManager.RoleExistsAsync("TeamHead"))
                            {
                                await _roleManager.CreateAsync(new IdentityRole("TeamHead"));
                            }

                            await _userManager.AddToRoleAsync(user, "TeamHead");
                        }
                    }
                }
                var teamEmployeeList = _db.department_Teams_Employees.Where(a => a.DepartmentTeamsId==teams.DepartmentTeamsId).ToList();
                _db.RemoveRange(teamEmployeeList);
                _db.SaveChanges();

                if (DVM.TeamEmployeesIds != null)
                {
                    Department_Teams_Employees dte = new Department_Teams_Employees();

                    foreach (var item in DVM.TeamEmployeesIds)
                    {
                        dte.Department_Teams_EmployeesId = 0;
                        dte.DepartmentTeamsId = DVM.teams.DepartmentTeamsId;
                        dte.EmployeeId = item;
                        dte.Date = DateTime.Now;
                        dte.Status = true;
                        _db.department_Teams_Employees.Add(dte);
                        _db.SaveChanges();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Edit));
            }
        }

        // GET: TeamsController/Delete/5
        [Authorize(Roles = "Admin,HRManager,DepartmentHead,ProjectManager,ManagingDirector")]
        public ActionResult Delete(int id)
        {
            try
            {
                var teams = _db.DepartmentTeams.Find(id);
                if (id == null || teams == null)
                {
                    return NotFound();
                }
                //removing child entities 
               
                //dep heads
                if (_db.Department_Teams_Heads.Any(d => d.HeadId == teams.DepartmentTeamsId && d.HeadType == "Team"))
                {
                    var teamHeadList = _db.Department_Teams_Heads.Where(d => d.HeadId == teams.DepartmentTeamsId).ToList();
                    _db.RemoveRange(teamHeadList);
                    _db.SaveChanges();
                }
                //dep emp
                if (_db.department_Teams_Employees.Any(d => d.DepartmentTeamsId == teams.DepartmentTeamsId))
                {
                    var teamempList = _db.department_Teams_Employees.Where(d => d.DepartmentTeamsId == teams.DepartmentTeamsId).ToList();
                    _db.RemoveRange(teamempList);
                    _db.SaveChanges();
                }

                _db.DepartmentTeams.Remove(teams);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }


        [Authorize(Roles = "Admin,HRManager,DepartmentHead,ProjectManager,ManagingDirector")]
        public ActionResult ChangeStatus(int? id)
        {
            try
            {
                var teams = _db.DepartmentTeams.Find(id);
                if (id == null || teams == null)
                {
                    return NotFound();
                }
                teams.Status = !(teams.Status);
                _db.DepartmentTeams.Update(teams);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }
        [HttpPost]
        public ActionResult GetCompanies()
        {
            var companies = _db.Companies.ToList();
            List<SelectListItem> CompanyNames = new List<SelectListItem>();
            companies.ForEach(x =>
            {
                CompanyNames.Add(new SelectListItem { Text = x.CompanyName, Value = x.CompanyId.ToString() });
            });
            ViewData["companyId"] = new SelectList(companies, "CompanyId", "CompanyName"); ;
            return Json(CompanyNames);
        }
        [HttpPost]
        public ActionResult GetDepartmentsByCompanyId(int Companyid)
        {
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
        public async Task<IActionResult> GetEmployeesByDepartmentId(int DepartmentId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (userId != null)
            {
                if (role.ElementAt(0) == "TeamHead")
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();

                    var employees1 = _db.Employees.Where(a => a.DepartmentId==DepartmentId && a.Status == true && a.DepartmentId == employee.DepartmentId).ToList();
                    List<SelectListItem> EmployeeNames1 = new List<SelectListItem>();
                    employees1.ForEach(x =>
                    {
                        EmployeeNames1.Add(new SelectListItem { Text = x.FullName, Value = x.EmployeeId.ToString() });
                    });
                    ViewData["employeeId"] = new SelectList(employees1, "EmployeeId", "FullName");
                    return Json(EmployeeNames1);
                }
                else if (role.ElementAt(0) == "Admin")
                {
                    var employees1 = _db.Employees.Where(a => a.DepartmentId == DepartmentId && a.Status == true && a.ReferenceUserId.ToString() == user.Id).ToList();
                    List<SelectListItem> EmployeeNames2 = new List<SelectListItem>();
                    employees1.ForEach(x =>
                    {
                        EmployeeNames2.Add(new SelectListItem { Text = x.FullName, Value = x.EmployeeId.ToString() });
                    });
                    ViewData["employeeId"] = new SelectList(employees1, "EmployeeId", "FullName");
                    return Json(EmployeeNames2);
                }
            }

                var employees = _db.Employees.Where(a => a.DepartmentId == DepartmentId).ToList();
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

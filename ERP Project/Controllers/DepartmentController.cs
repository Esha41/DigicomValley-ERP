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
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public DepartmentsVM dvm { get; set; }
        public DepartmentController(ApplicationDbContext db, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)

        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            dvm = new DepartmentsVM()
            {
                departments = new Departments(),
                DepartmentsList = new List<Departments>(),
                department_Teams_Head_List = new List<Department_Teams_Heads>(),
                EmployeesIds = new List<int>(),
            
                
            };
        }
        // GET: DepartmentController
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead,TeamHead")]
        public ActionResult Index()
        {
            dvm.DepartmentsList = _db.Departments.Include(d => d.Company).ToList();
            dvm.department_Teams_Head_List = _db.Department_Teams_Heads.Include(h => h.Employee).Where(h => h.HeadType == "Department").ToList();
           
            return View(dvm);
        }

        // GET: DepartmentController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DepartmentController/Create
        [Authorize(Roles = "Admin,ManagingDirector")]
        public ActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_db.Companies, "CompanyId", "CompanyName");
            ViewData["EmployeeId"] = new SelectList(_db.Employees, "EmployeeId", "FullName");
            return View();
        }

        // POST: DepartmentController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentsVM DVM)
        {
            try
            {
                var check = _db.Departments.ToList();
                if(_db.Departments.Any(a=>a.DepartmentName==DVM.departments.DepartmentName&&a.CompanyId==DVM.departments.CompanyId))
                {
                    return RedirectToAction(nameof(Create));
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                DVM.departments.ReferenceUserId = Guid.Parse(userId);
                _db.Departments.Add(DVM.departments);
                _db.SaveChanges();
                if (DVM.EmployeesIds!=null)
                {
                    Department_Teams_Heads dth = new Department_Teams_Heads();
                    foreach (var item in DVM.EmployeesIds)
                    {
                        dth.Department_Teams_HeadsId = 0;
                        dth.HeadType = "Department";
                        dth.HeadId = DVM.departments.DepartmentId;
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
                            if (!await _roleManager.RoleExistsAsync("DepartmentHead"))
                            {
                                await _roleManager.CreateAsync(new IdentityRole("DepartmentHead"));
                            }

                            await _userManager.AddToRoleAsync(user, "DepartmentHead");
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e) {
                    return RedirectToAction(nameof(Create));
            }
        }

        // GET: DepartmentController/Edit/5
        [Authorize(Roles = "Admin,ManagingDirector")]
        public async Task<IActionResult> Edit(int? id)
        {

            var department = _db.Departments.Find(id);
            if (id == null || department == null)
            {
                return NotFound();
            }
            dvm.departments = department;
            dvm.Companies = _db.Companies.Where(c => c.CompanyId == department.CompanyId).FirstOrDefault();

            dvm.department_Teams_Head_List = _db.Department_Teams_Heads.Include(d=>d.Employee).Where(d => d.HeadId == department.DepartmentId&&d.HeadType=="Department").ToList();
            dvm.EmployeeList = dvm.department_Teams_Head_List.Select(s=>s.Employee).ToList();
            dvm.EmployeesIds.AddRange(dvm.EmployeeList.Select(a => a.EmployeeId).ToList());

            ViewData["CompanyId"] = new SelectList(_db.Companies, "CompanyId", "CompanyName", dvm.Companies.CompanyId);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (userId != null)
            {
                if (role.ElementAt(0) == "DepartmentHead")
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    ViewData["EmployeeId"] = new SelectList(_db.Employees.Where(a => a.CompanyId == department.CompanyId && a.Status == true && a.DepartmentId == employee.DepartmentId), "EmployeeId", "FullName", dvm.EmployeesIds);
                }
                else if (role.ElementAt(0) == "Admin")
                {
                    ViewData["EmployeeId"] = new SelectList(_db.Employees.Where(a => a.CompanyId == department.CompanyId && a.Status == true && a.ReferenceUserId.ToString() == user.Id), "EmployeeId", "FullName", dvm.EmployeesIds);

                }
                else
                {
                    ViewData["EmployeeId"] = new SelectList(_db.Employees.Where(a => a.CompanyId == department.CompanyId && a.Status == true), "EmployeeId", "FullName", dvm.EmployeesIds);

                }
            }
       
            return View(dvm);

        }

        // POST: DepartmentController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DepartmentsVM DVM)
        {
            try
            {
                var check = _db.Departments.ToList();
                if (_db.Departments.Any(a => a.DepartmentName == DVM.departments.DepartmentName && (a.DepartmentId != DVM.departments.DepartmentId) && a.CompanyId == DVM.departments.CompanyId))
                {
                    return RedirectToAction(nameof(Edit));
                }
                var department = _db.Departments.Find(DVM.departments.DepartmentId);
                department.DepartmentName = DVM.departments.DepartmentName;
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                department.ReferenceUserId = Guid.Parse(userId);
                // department.CompanyId = DVM.departments.CompanyId;
                _db.Departments.Update(department);
                _db.SaveChanges();

                var deptHeadList = _db.Department_Teams_Heads.Include(s => s.Employee).Where(a => a.HeadId == department.DepartmentId && a.HeadType == "Department").ToList();
                foreach(var head in deptHeadList)
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
                _db.RemoveRange(deptHeadList);
                await _db.SaveChangesAsync();

                if (DVM.EmployeesIds != null)
                {
                    Department_Teams_Heads dth = new Department_Teams_Heads();
                    foreach (var item in DVM.EmployeesIds)
                    {
                        dth.Department_Teams_HeadsId = 0;
                        dth.HeadType = "Department";
                        dth.HeadId = DVM.departments.DepartmentId;
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
                            if (!await _roleManager.RoleExistsAsync("DepartmentHead"))
                            {
                                await _roleManager.CreateAsync(new IdentityRole("DepartmentHead"));
                            }

                            await _userManager.AddToRoleAsync(user, "DepartmentHead");
                        }
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Edit));
            }
        }

        // GET: DepartmentController/Delete/5
        [Authorize(Roles = "Admin,ManagingDirector")]
        public ActionResult Delete(int? id)
        {
            try
            {
                var department = _db.Departments.Find(id);
                if (id == null || department == null)
                {
                    return NotFound();
                }
                //removing child entities 
                //dep designation
                if (_db.Department_Designations.Any(d => d.DepartmentId == department.DepartmentId))
                {
                    var deptDesigList = _db.Department_Designations.Where(d => d.DepartmentId == department.DepartmentId).ToList();
                    _db.RemoveRange(deptDesigList);
                }
                //dep heads
                if (_db.Department_Teams_Heads.Any(d => d.HeadId == department.DepartmentId&&d.HeadType=="Department"))
                {
                    var deptHeadList = _db.Department_Teams_Heads.Where(d => d.HeadId== department.DepartmentId).ToList();
                    _db.RemoveRange(deptHeadList);
                }
                //dep teams
                if (_db.DepartmentTeams.Any(d => d.DepartmentId == department.DepartmentId))
                {
                    var deptTeamList = _db.DepartmentTeams.Where(d => d.DepartmentId == department.DepartmentId).ToList();
                    _db.RemoveRange(deptTeamList);
                }

                _db.Departments.Remove(department);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
    }

        [Authorize(Roles = "Admin,ManagingDirector")]
        public ActionResult ChangeStatus(int? id)
        {
            try
            {
                var department = _db.Departments.Find(id);
                if (id == null || department == null)
                {
                    return NotFound();
                }
                department.Status = !(department.Status);
                _db.Departments.Update(department);
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
        public async Task<IActionResult> GetEmployeesByCompanyId(int Companyid)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (userId != null)
            {
                if (role.ElementAt(0) == "DepartmentHead")
                {
                    var employee = _db.Employees.Where(a => a.Email == user.Email).FirstOrDefault();

                    var employees1 = _db.Employees.Where(a => a.CompanyId == Companyid && a.Status == true&&a.Email != employee.Email && (a.ReferenceUserId == employee.ReferenceUserId || a.ReferenceUserId.ToString() == user.Id)).ToList();
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
                    var employees1 = _db.Employees.Where(a => a.CompanyId == Companyid && a.Status == true && a.ReferenceUserId.ToString() == user.Id).ToList();
                    List<SelectListItem> EmployeeNames2 = new List<SelectListItem>();
                    employees1.ForEach(x =>
                    {
                        EmployeeNames2.Add(new SelectListItem { Text = x.FullName, Value = x.EmployeeId.ToString() });
                    });
                    ViewData["employeeId"] = new SelectList(employees1, "EmployeeId", "FullName");
                    return Json(EmployeeNames2);
                }
            }
           
                    var employees = _db.Employees.Where(a=>a.CompanyId==Companyid&&a.Status==true).ToList();
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


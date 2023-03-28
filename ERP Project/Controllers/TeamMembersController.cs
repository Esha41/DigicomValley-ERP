using ERP_Project.Data;
using ERP_Project.Models;
using ERP_Project.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Controllers
{
    public class TeamMembersController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DepartmentsVM dvm { get; set; }
        public TeamMembersController(ApplicationDbContext db)

        {
            _db = db;
            dvm = new DepartmentsVM()
            {
                departments = new Departments(),
                DepartmentsList = new List<Departments>(),
                department_Teams_Head_List = new List<Department_Teams_Heads>(),
                EmployeesIds = new List<int>(),
                designations = new Department_Designations(),
                TeamEmployeesList = new List<Employees>()
            };
        }
        // GET: TeamMembersController
        public ActionResult Index(int id)
        {
           // dvm.depteamsEmployeesList = _db.department_Teams_Employees.ToList();
            var team = _db.DepartmentTeams.Find(id);
            var teamEmployees = _db.department_Teams_Employees.Include(a=>a.Employee).ThenInclude(a=>a.Department_Designation).Where(e => e.DepartmentTeamsId == team.DepartmentTeamsId).ToList();
            /*  foreach(var a in teamEmployees)
              {
                  dvm.TeamEmployeesList.Add(_db.Employees.Include(a=>a.Department_Designation).FirstOrDefault(e => e.EmployeeId == a.EmployeeId));
              }*/
            dvm.employeepositions = _db.EmployeePositions.Include(a=>a.Employee).ToList();
            dvm.depteamsEmployeesList = teamEmployees;
            dvm.teams = team;
            return View(dvm);
        }
        [Authorize(Roles = "Admin,HRManager,DepartmentHead,ProjectManager,ManagingDirector")]
        public ActionResult ChangeStatus(int? id)
        {

            var teams = _db.department_Teams_Employees.Where(a => a.Department_Teams_EmployeesId == id).FirstOrDefault();
            try
            {
                if (id == null || teams == null)
                {
                    return NotFound();
                }
                teams.Status = !(teams.Status);
                _db.department_Teams_Employees.Update(teams);
                _db.SaveChanges();
                return RedirectToAction("Index", new { id=teams.DepartmentTeamsId});
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", new { id = teams.DepartmentTeamsId });
            }
        }
        // GET: TeamMembersController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

    /*    // GET: TeamMembersController/Create
        public ActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_db.Companies, "CompanyId", "CompanyName");
            ViewData["DepartmentId"] = new SelectList(_db.Departments, "DepartmentId", "DepartmentName");
            ViewData["DepartmentTeamsId"] = new SelectList(_db.DepartmentTeams, "DepartmentTeamsId", "TeamName");
            ViewData["EmployeeId"] = new SelectList(_db.Employees, "EmployeeId", "FullName");
            return View();
        }
*/
       /* // POST: TeamMembersController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DepartmentsVM DVM)
        {
            try
            {
                var check = _db.department_Teams_Employees.ToList();
                *//*     if (_db.DepartmentTeams.Any(a => a.TeamName == DVM.teams.TeamName && a.DepartmentId == DVM.teams.DepartmentId && a.Department.CompanyId == DVM.teams.Department.CompanyId))
                     {
                         return RedirectToAction(nameof(Create));
                     }*//*

                // var a = DVM.teams.DepartmentId;
                // DVM.teamsEmployees.de.Department = null;
                Department_Teams_Employees dth = new Department_Teams_Employees();
               // _db.department_Teams_Employees.Add(DVM.teamsEmployees);
              
              
                foreach (var item in DVM.EmployeesIds)
                {
                    dth.Department_Teams_EmployeesId = 0;
                    dth.DepartmentTeamsId = DVM.teamsEmployees.GetDepartmentTeam.DepartmentTeamsId;
                    dth.EmployeeId = item;
                    dth.Date = DateTime.Now;
                    dth.Status = true;
                    _db.department_Teams_Employees.Add(dth);
                    _db.SaveChanges();
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Create));
            }
        }
*/
        // GET: TeamMembersController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: TeamMembersController/Edit/5
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

     

        // POST: TeamMembersController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {

            var teams = _db.department_Teams_Employees.Where(a => a.Department_Teams_EmployeesId == id).FirstOrDefault();
            try
            {
                if (id == null || teams == null)
                {
                    return NotFound();
                }
                _db.department_Teams_Employees.Remove(teams);
                _db.SaveChanges();
                return RedirectToAction("Index", new { id = teams.DepartmentTeamsId });
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", new { id = teams.DepartmentTeamsId });
            }
        }
    }
}

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
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_Project.Controllers
{
    public class DesignationController : Controller
    {
        private readonly ApplicationDbContext _db;
        public DepartmentsVM dvm { get; set; }
        public DesignationController(ApplicationDbContext db)

        {
            _db = db;
            dvm = new DepartmentsVM()
            {
                departments = new Departments(),
                DepartmentsList = new List<Departments>(),
                department_Teams_Head_List = new List<Department_Teams_Heads>(),
                EmployeesIds = new List<int>(),
                designations=new Department_Designations()
            };
        }
        // GET: DesignationController
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead,TeamHead")]
        public ActionResult Index()
        {
            dvm.Department_Designations_List = _db.Department_Designations.Include(d => d.Department).Include(d => d.Company).ToList();
         
            return View(dvm);
        }

        // GET: DesignationController/Details/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: DesignationController/Create
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public ActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_db.Companies, "CompanyId", "CompanyName");
            ViewData["DepartmentId"] = new SelectList(_db.Departments.Where(a=>a.Status==true), "DepartmentId", "DepartmentName");
            return View();
        }

        // POST: DesignationController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(DepartmentsVM DVM)
        {
            try
            {
                var check = _db.Department_Designations.ToList();
                if (_db.Department_Designations.Any(a => (a.DesignationName == DVM.designations.DesignationName )&& (a.CompanyId == DVM.designations.CompanyId) &&( a.DepartmentId == DVM.designations.DepartmentId)))
                {
                    return RedirectToAction(nameof(Create));
                }
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                DVM.designations.ReferenceUserId = Guid.Parse(userId);
                _db.Department_Designations.Add(DVM.designations);
                _db.SaveChanges();
               
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: DesignationController/Edit/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public ActionResult Edit(int id)
        {
            var designation = _db.Department_Designations.Find(id);
            if (id == null || designation == null)
            {
                return NotFound();
            }
            dvm.designations = designation;
            dvm.Companies = _db.Companies.Where(c => c.CompanyId == designation.CompanyId).FirstOrDefault();
            dvm.departments = _db.Departments.Where(c => c.DepartmentId == designation.DepartmentId).FirstOrDefault();

            ViewData["CompanyId"] = new SelectList(_db.Companies, "CompanyId", "CompanyName", dvm.Companies);
            ViewData["DepartmentId"] = new SelectList(_db.Departments.Where(a=>a.Status==true&&a.CompanyId==designation.CompanyId), "DepartmentId", "DepartmentName", dvm.departments);


            return View(dvm);
        }

        // POST: DesignationController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(DepartmentsVM DVM)
        {
            try
            {
                var check = _db.Department_Designations.ToList();
                if (_db.Department_Designations.Any(a => (a.DesignationName == DVM.designations.DesignationName) && (a.CompanyId == DVM.designations.CompanyId) && (a.DepartmentId == DVM.designations.DepartmentId)))
                {
                    return RedirectToAction(nameof(Create));
                }
                var designation = _db.Department_Designations.Find(DVM.designations.Department_DesignationsId);
                designation.DesignationName = DVM.designations.DesignationName;
             designation.DepartmentId = DVM.designations.DepartmentId;
                /*       designation.CompanyId = DVM.designations.CompanyId;*/
                _db.Department_Designations.Update(designation);
                _db.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Create));
            }
        }


        // POST: DesignationController/Delete/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public ActionResult Delete(int id)
        {
            try
            {
                var designations = _db.Department_Designations.Find(id);
                if (id == null || designations == null)
                {
                    return NotFound();
                }
                _db.Department_Designations.Remove(designations);
                _db.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public ActionResult ChangeStatus(int? id)
        {
            try
            {
                var designation = _db.Department_Designations.Find(id);
                if (id == null || designation == null)
                {
                    return NotFound();
                }
                designation.Status = !(designation.Status);
                _db.Department_Designations.Update(designation);
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
            var departments = _db.Departments.Where(a => a.CompanyId == Companyid&&a.Status==true).ToList();
            List<SelectListItem> DepartmentNames = new List<SelectListItem>();
            departments.ForEach(x =>
            {
                DepartmentNames.Add(new SelectListItem { Text = x.DepartmentName, Value = x.DepartmentId.ToString() });
            });
            ViewData["departmentId"] = new SelectList(departments, "DepartmentId", "DepartmentName");
            return Json(DepartmentNames);
        }
    }
}

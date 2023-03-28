using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERP_Project.Data;
using ERP_Project.Models;
using ERP_Project.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ERP_Project.Controllers
{
    public class ApplicationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IToggleServices _toggleService;
        private readonly UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public ApplicationsController(ApplicationDbContext context, IToggleServices toggleServices, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _toggleService = toggleServices;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Applications
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
         var uid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);

            if (role.ElementAt(0) == "HRManager")
            {
                var applicationDbContext = _context.Applications.Include(a => a.Company).Include(a => a.Designation).Where(a => a.ReferenceUserId == uid);
                return View(await applicationDbContext.ToListAsync());
            }
            else
            {
                var applicationDbContext = _context.Applications.Include(a => a.Company).Include(a => a.Designation);
                return View(await applicationDbContext.ToListAsync());
            }
        }
        [HttpPost, ActionName("Delete1")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete1(int id)
        {
            var applicantRemarks = _context.applicantRemarks.Where(a => a.ApplicantsId == id).ToList();
            _context.RemoveRange(applicantRemarks);
            _context.SaveChanges();

            var interviews = _context.Interviews.Where(a => a.ApplicantId == id).ToList();
            _context.Interviews.RemoveRange(interviews);
            _context.SaveChanges();

            var applicants = await _context.Applicants.FindAsync(id);
            var appid = applicants.ApplicationId;
            _context.Applicants.Remove(applicants);
            _context.SaveChanges();

           

            return RedirectToAction(nameof(GetApplicants), new { id = appid });
        }
        public async Task<IActionResult> smsSend(int? id)
        {
            var applicant = await _context.Applicants.FindAsync(id);
            applicant.SMSsend = !(applicant.SMSsend);
            _context.Applicants.Update(applicant);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(GetApplicants), new { id = applicant.ApplicationId });
        }
        [HttpPost]
        public async Task<IActionResult> CreateA(Applicants applicants)
        {
            if (ModelState.IsValid)
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                applicants.ReferenceUserId = Guid.Parse(userId);
                _context.Add(applicants);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(GetApplicants), new { id = applicants.ApplicationId });

            }
            ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title", applicants.ApplicationId);
            return View(applicants);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit1(Applicants applicants)
        {
          /*  if (id != applicants.ApplicantsId)
            {
                return NotFound();
            }*/

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicants);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(GetApplicants), new { id = applicants.ApplicationId });
                }
                catch (DbUpdateConcurrencyException)
                {
                   
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", applicants.ApplicationId);
            ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title", applicants.ApplicationId);
            return View(applicants);
        }
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> GetApplicants(int? id,string? status,DateTime? date)
        {
            if (id == null)
            {
                return NotFound();
            }
            if(status== "--Select ApplicationStatus--")
            {
                status = null;
            }
            var applicationDbContext = _context.Applicants.Where(m => m.ApplicationId == id);
            ViewBag.selctedVal = "--Select ApplicationStatus--";
            if (status!=null)
            {
                if (date != null)
                {
                    applicationDbContext = applicationDbContext.Where(m => m.AplicantStatus.ToLower().Equals(status) && m.InterViewDate.Date==date);
                    ViewBag.selectedDate = date;
                }
                else
                {
                    applicationDbContext = applicationDbContext.Where(m => m.AplicantStatus.ToLower().Equals(status));
                }
                ViewBag.selctedVal = status;
              
            }
           
               else if (date != null)
                {
                    applicationDbContext = applicationDbContext.Where(m => m.InterViewDate.Date == date);
                ViewBag.selctedVal = "--Select ApplicationStatus--";
                ViewBag.selectedDate = date;
            }
                
            ViewBag.applicationName = _context.Applications.Where(m => m.ApplicationId == id).FirstOrDefault().Title;
          
            return View(await applicationDbContext.ToListAsync());
        
        }

        // GET: Applications/Details/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applications = await _context.Applications
                .Include(a => a.Company)
                .Include(a => a.Designation)
                .FirstOrDefaultAsync(m => m.ApplicationId == id);
            if (applications == null)
            {
                return NotFound();
            }

            return View(applications);
        }

        // GET: Applications/Create
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public IActionResult Create()
        {
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName");
            ViewData["DesignationId"] = new SelectList(_context.Department_Designations, "Department_DesignationsId", "DesignationName");
            return View();
        }

        // POST: Applications/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Applications applications)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                applications.ReferenceUserId = Guid.Parse(userId);
                _context.Add(applications);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", applications.CompanyId);
            ViewData["DesignationId"] = new SelectList(_context.Department_Designations.Where(a => a.Status == true).ToList(), "Department_DesignationsId", "DesignationName", applications.DesignationId);
            return View(applications);
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "DepartmentId", "DepartmentName");
            return Json(DesignationNames);
        }

        
             [HttpPost]
        public ActionResult GetDepartmentsDesignations(int id)
        {
            var Department_Designations = _context.Department_Designations.Where(a => a.CompanyId == id).ToList();
            List<SelectListItem> DesignationNames = new List<SelectListItem>();
            Department_Designations.ForEach(x =>
            {
                DesignationNames.Add(new SelectListItem { Text = x.DesignationName, Value = x.Department_DesignationsId.ToString() });
            });
            ViewData["DesignationId"] = new SelectList(_context.Department_Designations, "Department_DesignationsId", "DesignationName");
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
        // GET: Applications/Edit/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applications = await _context.Applications.FindAsync(id);
            if (applications == null)
            {
                return NotFound();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments.Where(a => a.Status == true).ToList(), "DepartmentId", "DepartmentName");
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", applications.CompanyId);
            ViewData["DesignationId"] = new SelectList(_context.Department_Designations.Where(a => a.CompanyId == applications.CompanyId && a.Status == true).ToList(), "Department_DesignationsId", "DesignationName", applications.DesignationId);
            return View(applications);
        }

        // POST: Applications/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Applications applications)
        {
            if (id != applications.ApplicationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applications);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicationsExists(applications.ApplicationId))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", applications.CompanyId);
            ViewData["DesignationId"] = new SelectList(_context.Department_Designations, "Department_DesignationsId", "DesignationName", applications.DesignationId);
            return View(applications);
        }

        // GET: Applications/Delete/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applications = await _context.Applications
                .Include(a => a.Company)
                .Include(a => a.Designation)
                .FirstOrDefaultAsync(m => m.ApplicationId == id);
            if (applications == null)
            {
                return NotFound();
            }

            return View(applications);
        }

        // POST: Applications/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var applications = await _context.Applications.FindAsync(id);
            var applicants = _context.Applicants.Where(a => a.ApplicationId == id).ToList();
            _context.RemoveRange(applicants);
            _context.SaveChanges();

            _context.Applications.Remove(applications);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<JsonResult> ChangeApplicationsStatus(int id)
        {
            await _toggleService.ChangeApplicationsStatus(id);
            return Json("Success");
        }

        private bool ApplicationsExists(int id)
        {
            return _context.Applications.Any(e => e.ApplicationId == id);
        }
    }
}

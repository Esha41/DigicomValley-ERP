using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERP_Project.Data;
using ERP_Project.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace ERP_Project.Controllers
{
    public class ApplicantsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        public ApplicantsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Applicants
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Applicants.Include(a => a.Application);
            return View(await applicationDbContext.ToListAsync());
        }


     /*   // GET: Applicants/Details/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicants = await _context.Applicants.FindAsync(id);
            if (applicants == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName");
            ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title", applicants.ApplicationId);
            return View(applicants);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, Applicants applicants)
        {
            if (id != applicants.ApplicantsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicants);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicantsExists(applicants.ApplicantsId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", applicants.ApplicationId);
            ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title", applicants.ApplicationId);
            return View(applicants);
        }
*/
        // GET: Applicants/Create
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Create(string? num)
        {
           
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (num !=null && _context.Applicants.Any(s => s.Phone==num))
            {
                ViewBag.myList = _context.Applicants.Include(s=>s.Application).Where(s => s.Phone == num).ToList();
                if (_context.Applicants.Any(s => s.Phone == num))
                {
                    var applicant = _context.Applicants.Where(s => s.Phone == num).FirstOrDefault();
                    ViewBag.myRemarksList = _context.applicantRemarks.Include(s => s.Applicant).ToList();
                }
                ViewBag.isApplicantExist = true;
                    ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title");
                  
            }
            else
            {
                ViewData["isApplicantExist"] = false;
            }
            if (role.ElementAt(0) == "HRManager")
            {
                ViewData["ApplicationId"] = new SelectList(_context.Applications.Where(a => a.ReferenceUserId == uid), "ApplicationId", "Title");
            }
            else
            {
                ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title");
            }
            return View();
        }

        /* [HttpPost]
         public async Task<IActionResult> Create(Applicants applicants)
         {
             if (ModelState.IsValid)
             {

                 var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                     applicants.ReferenceUserId = Guid.Parse(userId);
                     _context.Add(applicants);
                     await _context.SaveChangesAsync();
                     return RedirectToAction(nameof(Index));

             }
             ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title", applicants.ApplicationId);
             return View(applicants);
         }
 */
       
        // GET: Applicants/Edit/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Edit(int? id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var uid = new Guid(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (id == null)
            {
                return NotFound();
            }

            var applicants = await _context.Applicants.FindAsync(id);
            if (applicants == null)
            {
                return NotFound();
            }
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", applicants.ApplicationId);
            if (role.ElementAt(0) == "HRManager")
            {
                ViewData["ApplicationId"] = new SelectList(_context.Applications.Where(a => a.ReferenceUserId == uid), "ApplicationId", "Title", applicants.ApplicationId);
            }
            else
            {
                ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title", applicants.ApplicationId);
            }
         
            return View(applicants);
        }

        // POST: Applicants/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    /*    [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Applicants applicants)
        {
            if (id != applicants.ApplicantsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(applicants);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ApplicantsExists(applicants.ApplicantsId))
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
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", applicants.ApplicationId);
            ViewData["ApplicationId"] = new SelectList(_context.Applications, "ApplicationId", "Title", applicants.ApplicationId);
            return View(applicants);
        }*/

        // GET: Applicants/Delete/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var applicants = await _context.Applicants
                .Include(a => a.Application)
                .FirstOrDefaultAsync(m => m.ApplicantsId == id);
            if (applicants == null)
            {
                return NotFound();
            }

            return View(applicants);
        }

        // POST: Applicants/Delete/5
      /*  [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var applicants = await _context.Applicants.FindAsync(id);
            _context.Applicants.Remove(applicants);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }*/
        public async Task<IActionResult> AddRemarks(int id, string remarks)
        {
            ApplicantRemarks pr = new ApplicantRemarks();
            pr.Date = DateTime.Now;
            pr.ApplicantsId = id;
            pr.Remarks = remarks;
            _context.applicantRemarks.Add(pr);
            _context.SaveChanges();
            return RedirectToAction(nameof(Details),new { id=id});
        }

        public async Task<IActionResult> Details(int? id)
        { 
            ViewBag.myList = _context.Applicants.Include(s => s.Application).Where(s => s.ApplicantsId == id).ToList();
            ViewBag.myRemarksList = _context.applicantRemarks.Include(s => s.Applicant).Where(s => s.ApplicantsId == id).ToList();
            ViewBag.id = id;
            if (id == null)
            {
                return NotFound();
            }

            var applicants = await _context.Applicants
                .Include(a => a.Application)
                .FirstOrDefaultAsync(m => m.ApplicantsId == id);
            if (applicants == null)
            {
                return NotFound();
            }

            return View(applicants);
        }

        private bool ApplicantsExists(int id)
        {
            return _context.Applicants.Any(e => e.ApplicantsId == id);
        }
    }
}

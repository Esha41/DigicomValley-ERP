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
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ERP_Project.Controllers
{
    public class InterviewsController : Controller
    {
        private readonly ApplicationDbContext _context;
        [BindProperty]
        public InterviewViewModel InterviewViewModel { get; set; }

        public InterviewsController(ApplicationDbContext context)
        {
            _context = context;
            InterviewViewModel = new InterviewViewModel()
            {
                Interview = new Interview(),
                Interviews = new List<Interview>(),
            };
        }

        // GET: Interviews
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Interviews.Include(i => i.Applicant).Include(i => i.Interviewer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Interviews/Details/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interview = await _context.Interviews
                .Include(i => i.Applicant)
                .Include(i => i.Interviewer)
                .FirstOrDefaultAsync(m => m.InterviewId == id);
            if (interview == null)
            {
                return NotFound();
            }

            return View(interview);
        }
        public async Task<IActionResult> AddRemarks(int id, string remarks)
        {
            ApplicantRemarks pr = new ApplicantRemarks();
            pr.Date = DateTime.Now;
            pr.ApplicantsId = id;
            pr.Remarks = remarks;
            _context.applicantRemarks.Add(pr);
            _context.SaveChanges();
            return RedirectToAction(nameof(Create), new { id = id });
        }

        // GET: Interviews/Create
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Create(int? id, InterviewViewModel IntervieVM)
        {
            if (id == null)
            {
                return NotFound();
            }
            var applicant = await _context.Applicants.Include(i => i.Application).FirstOrDefaultAsync(m => m.ApplicantsId == id);
            ViewBag.myRemarksList = _context.applicantRemarks.Include(s => s.Applicant).Where(s => s.ApplicantsId == id).ToList();
            ViewBag.Applicant = applicant;
            ViewBag.id = id;
            InterviewViewModel InterviewVM = new InterviewViewModel();

            InterviewVM.Interviews = await _context.Interviews.Where(a => a.ApplicantId == id).Include(i => i.Interviewer).ToListAsync();

            ViewData["ApplicantId"] = new SelectList(_context.Applicants, "ApplicantsId", "Name");
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName");
            return View(InterviewVM);
        }

        // POST: Interviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Interview interview)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                interview.ReferenceUserId = Guid.Parse(userId);
                _context.Add(interview);
                await _context.SaveChangesAsync();
                var applicant = _context.Applicants.Where(a => a.ApplicantsId == interview.ApplicantId).FirstOrDefault();
                if (interview.InterviewStatus == "Reschedule")
                {
                  
                    applicant.InterViewDate = Convert.ToDateTime(interview.ReScheduleDate);
                }
                applicant.AplicantStatus = interview.InterviewStatus;
                _context.Applicants.Update(applicant);
                _context.SaveChanges();
                return RedirectToAction("Create", "Interviews");
            }
            ViewData["ApplicantId"] = new SelectList(_context.Applicants, "ApplicantsId", "Name", interview.ApplicantId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", interview.EmployeeId);
            return View(interview);
        }

        // GET: Interviews/Edit/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interview = await _context.Interviews.FindAsync(id);
            if (interview == null)
            {
                return NotFound();
            }
            ViewData["ApplicantId"] = new SelectList(_context.Applicants, "ApplicantsId", "Name", interview.ApplicantId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", interview.EmployeeId);
            return View(interview);
        }

        // POST: Interviews/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Interview interview)
        {
            if (id != interview.InterviewId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(interview);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InterviewExists(interview.InterviewId))
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
            ViewData["ApplicantId"] = new SelectList(_context.Applicants, "ApplicantsId", "Name", interview.ApplicantId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", interview.EmployeeId);
            return View(interview);
        }

        // GET: Interviews/Delete/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var interview = await _context.Interviews
                .Include(i => i.Applicant)
                .Include(i => i.Interviewer)
                .FirstOrDefaultAsync(m => m.InterviewId == id);
            if (interview == null)
            {
                return NotFound();
            }

            return View(interview);
        }

        // POST: Interviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var interview = await _context.Interviews.FindAsync(id);
            _context.Interviews.Remove(interview);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InterviewExists(int id)
        {
            return _context.Interviews.Any(e => e.InterviewId == id);
        }
    }
}

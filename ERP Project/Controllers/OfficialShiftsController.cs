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
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ERP_Project.Controllers
{
    public class OfficialShiftsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IToggleServices _toggleService;
        public OfficialShiftsController(ApplicationDbContext context, IToggleServices toggleServices)
        {
            _context = context;
            _toggleService = toggleServices;
        }

        // GET: OfficialShifts
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.OfficialShifts.Include(o => o.Company).Include(o => o.Shifts);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: OfficialShifts/Details/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officialShifts = await _context.OfficialShifts
                .Include(o => o.Company)
                .Include(o => o.Shifts)
                .FirstOrDefaultAsync(m => m.OfficialShiftsId == id);
            if (officialShifts == null)
            {
                return NotFound();
            }

            return View(officialShifts);
        }

        // GET: OfficialShifts/Create
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public IActionResult Create()
        {

            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName");
            ViewData["ShiftId"] = new SelectList(_context.Shifts, "ShiftId", "ShiftName");
            return View();
        }

        // POST: OfficialShifts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OfficialShifts officialShifts, string day, string shift)
        {
            var checkday = _context.OfficialShifts.Where(m => m.Day == officialShifts.Day && m.ShiftId == officialShifts.ShiftId).FirstOrDefault();
            if (checkday != null)
            {
                ViewBag.Message = "Exist";
                ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", officialShifts.CompanyId);
                ViewData["ShiftId"] = new SelectList(_context.Shifts, "ShiftId", "ShiftName", officialShifts.ShiftId);
                return View();
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    officialShifts.ReferenceUserId = Guid.Parse(userId);
                    _context.OfficialShifts.Add(officialShifts);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", officialShifts.CompanyId);
            ViewData["ShiftId"] = new SelectList(_context.Shifts, "ShiftId", "ShiftName", officialShifts.ShiftId);
            return View(officialShifts);
        }

        // GET: OfficialShifts/Edit/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officialShifts = await _context.OfficialShifts.FindAsync(id);
            if (officialShifts == null)
            {
                return NotFound();
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", officialShifts.CompanyId);
            ViewData["ShiftId"] = new SelectList(_context.Shifts, "ShiftId", "ShiftName", officialShifts.ShiftId);
            return View(officialShifts);
        }

        // POST: OfficialShifts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OfficialShifts officialShifts)
        {
            if (id != officialShifts.OfficialShiftsId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(officialShifts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OfficialShiftsExists(officialShifts.OfficialShiftsId))
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
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", officialShifts.CompanyId);
            ViewData["ShiftId"] = new SelectList(_context.Shifts, "ShiftId", "ShiftName", officialShifts.ShiftId);
            return View(officialShifts);
        }

        // GET: OfficialShifts/Delete/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var officialShifts = await _context.OfficialShifts
                .Include(o => o.Company)
                .Include(o => o.Shifts)
                .FirstOrDefaultAsync(m => m.OfficialShiftsId == id);
            if (officialShifts == null)
            {
                return NotFound();
            }

            return View(officialShifts);
        }

        // POST: OfficialShifts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var officialShifts = await _context.OfficialShifts.FindAsync(id);
            _context.OfficialShifts.Remove(officialShifts);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<JsonResult> ChangeShiftTimmingStatus(int id)
        {
            await _toggleService.ChangeShiftTimmingStatus(id);
            return Json("Success");
        }


        private bool OfficialShiftsExists(int id)
        {
            return _context.OfficialShifts.Any(e => e.OfficialShiftsId == id);
        }
    }
}

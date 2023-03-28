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
    public class ShiftsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IToggleServices _toggleService;
        public ShiftsController(ApplicationDbContext context, IToggleServices toggleServices)
        {
            _context = context;
            _toggleService = toggleServices;
        }

        // GET: Shifts
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Shifts.Include(a => a.Company).ToListAsync());
        }

        // GET: Shifts/Details/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shifts = await _context.Shifts
                .FirstOrDefaultAsync(m => m.ShiftId == id);
            if (shifts == null)
            {
                return NotFound();
            }

            return View(shifts);
        }

        // GET: Shifts/Create
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public IActionResult Create()
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName");

            return View();
        }

        // POST: Shifts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Shifts shifts)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                shifts.ReferenceUserId = Guid.Parse(userId);
                _context.Shifts.Add(shifts);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(shifts);
        }

        // GET: Shifts/Edit/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shifts = await _context.Shifts.FindAsync(id);
            ViewData["CompanyId"] = new SelectList(_context.Companies, "CompanyId", "CompanyName", shifts.CompanyId);

            if (shifts == null)
            {
                return NotFound();
            }
            return View(shifts);
        }

        // POST: Shifts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,Shifts shifts)
        {
            if (id != shifts.ShiftId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(shifts);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ShiftsExists(shifts.ShiftId))
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
            return View(shifts);
        }

        // GET: Shifts/Delete/5
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var shifts = await _context.Shifts
                .FirstOrDefaultAsync(m => m.ShiftId == id);
            if (shifts == null)
            {
                return NotFound();
            }

            return View(shifts);
        }

        // POST: Shifts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var shifts = await _context.Shifts.FindAsync(id);
            _context.Shifts.Remove(shifts);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead")]
        public async Task<JsonResult> ChangeShiftStatus(int id)
        {
            await _toggleService.ChangeShiftStatus(id);
            return Json("Success");
        }

        private bool ShiftsExists(int id)
        {
            return _context.Shifts.Any(e => e.ShiftId == id);
        }
    }
}

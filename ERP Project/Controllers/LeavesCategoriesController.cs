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
    public class LeavesCategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private IToggleServices _toggleService;

        public LeavesCategoriesController(ApplicationDbContext context, IToggleServices toggleServices)
        {
            _context = context;
            _toggleService = toggleServices;
        }

        // GET: LeavesCategories
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.LeavesCategories.ToListAsync());
        }

        // GET: LeavesCategories/Details/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leavesCategory = await _context.LeavesCategories
                .FirstOrDefaultAsync(m => m.LeavesCategoryId == id);
            if (leavesCategory == null)
            {
                return NotFound();
            }

            return View(leavesCategory);
        }

        // GET: LeavesCategories/Create
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: LeavesCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LeavesCategoryId,Name,Status,Date")] LeavesCategory leavesCategory)
        {
            if (ModelState.IsValid)
            {

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                leavesCategory.ReferenceUserId = Guid.Parse(userId);
                _context.LeavesCategories.Add(leavesCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(leavesCategory);
        }

        // GET: LeavesCategories/Edit/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leavesCategory = await _context.LeavesCategories.FindAsync(id);
            if (leavesCategory == null)
            {
                return NotFound();
            }
            return View(leavesCategory);
        }

        // POST: LeavesCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("LeavesCategoryId,Name,Status,Date")] LeavesCategory leavesCategory)
        {
            if (id != leavesCategory.LeavesCategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leavesCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeavesCategoryExists(leavesCategory.LeavesCategoryId))
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
            return View(leavesCategory);
        }

        // GET: LeavesCategories/Delete/5
        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leavesCategory = await _context.LeavesCategories
                .FirstOrDefaultAsync(m => m.LeavesCategoryId == id);
            if (leavesCategory == null)
            {
                return NotFound();
            }

            return View(leavesCategory);
        }

        // POST: LeavesCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leavesCategory = await _context.LeavesCategories.FindAsync(id);
            _context.LeavesCategories.Remove(leavesCategory);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,HRManager,ManagingDirector")]
        public async Task<JsonResult> ChangeLeavesCategoriesStatus(int id)
        {
            await _toggleService.ChangeLeavesCategoriesStatus(id);
            return Json("Success");
        }

        private bool LeavesCategoryExists(int id)
        {
            return _context.LeavesCategories.Any(e => e.LeavesCategoryId == id);
        }
    }
}

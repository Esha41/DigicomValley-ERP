using ERP_Project.Data;
using ERP_Project.Models;
using ERP_Project.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_Project.Controllers
{
    public class AnnouncementController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementController(ApplicationDbContext context)
        {
            _context = context;
        }
        // GET: DeliveryCharges
        [Authorize(Roles = ",Admin,Employee,DepartmentHead,HRManager,ProjectManager,ManagingDirector,TeamHead")]
        public async Task<IActionResult> Index()
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");

            DateTime date2 = TimeZoneInfo.ConvertTime(DateTime.Now, tz);
            AttendanceVM avm = new AttendanceVM();
            avm.users = _context.Users.ToList();
            if (User.IsInRole("Employee"))
            {
                avm.announcement = _context.announcements.Where(a => a.Status == true && (a.StartDate.Date <= date2.Date || a.StartDate.Date < date2.Date) && (a.EndDate.Date >= date2.Date || a.EndDate<date2.Date)).ToList();
            }
            else
            {
                avm.announcement = _context.announcements.ToList();
            }
            avm.AllEmployees = _context.Employees.Include(A=>A.Department).Where(a => a.Status == true && a.Department.DepartmentName == "HR").ToList();
            return View(avm);
        }
        [Authorize(Roles = "HRManager,Admin")]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Announcement obj)
        {
           

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                obj.ReferenceUserId = Guid.Parse(userId);
                obj.Date = DateTime.Now;
                obj.Status = true;
                _context.announcements.Add(obj);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }
        [Authorize(Roles = "HRManager,Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Announcement obj)
            {
            if (obj==null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var data = _context.announcements.Find(obj.AnnouncementId);
                    data.Description = obj.Description;
                    data.Title = obj.Title;
                    data.Date = DateTime.Now;
                    data.StartDate = obj.StartDate;
                    data.EndDate = obj.EndDate;

                    _context.announcements.Update(data);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    
                }
                return RedirectToAction(nameof(Index));
            }
            return View(obj);
        }
        [Authorize(Roles = "HRManager,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.announcements
                .FirstOrDefaultAsync(m => m.AnnouncementId == id);
            if (announcement == null)
            {
                return NotFound();
            }
            _context.announcements.Remove(announcement);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
        [Authorize(Roles = "HRManager,Admin")]
        public ActionResult ChangeStatus(int? id) 
        {
            try
            {
                var data = _context.announcements.Find(id);
                if (id == null || data == null)
                {
                    return NotFound();
                }
                data.Status = !(data.Status);
                _context.announcements.Update(data);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception e)
            {
                return RedirectToAction(nameof(Index));
            }
        }

    }
}

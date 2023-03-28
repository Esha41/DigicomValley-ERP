using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ERP_Project.Data;
using ERP_Project.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace ERP_Project.Controllers
{
    public class LeavesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        public LeavesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Leaves
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead,TeamHead,HRManager")]
        public async Task<IActionResult> Index(string? type)
        {
           

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
                if (type == "pending")
                {
                    var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    var applicationDbContext1 = _context.Leaves.Include(l => l.Employee).Include(l => l.LeavesCategory).Include(l => l.Department_Teams_Head).Where(a => a.Status.ToLower() == "pending"); /*.Where(a=>a.Employee.DepartmentId==employee.DepartmentId&&a.Department_Teams_Head.HeadType=="Department" && a.Employee.Email != employee.Email && a.Employee.ReferenceUserId == employee.ReferenceUserId);*/
                    return View(await applicationDbContext1.ToListAsync());
                }
                if (role.ElementAt(0) == "Admin" && user.Id == "d7241547-861b-4b0a-bc8f-8e26cef589c3")
                {
                    /*   avm.users = _context.Users.ToList();
                       avm.AllEmployees = _context.Employees.ToList();
                       avm.EmployeeLeaves = applicationDbContext.ToList();
   */
                    var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                  
                    var applicationDbContext = _context.Leaves.Include(l => l.Employee).Include(l => l.LeavesCategory).Include(l => l.Department_Teams_Head); /*.Where(a=>a.Employee.DepartmentId==employee.DepartmentId&&a.Department_Teams_Head.HeadType=="Department" && a.Employee.Email != employee.Email && a.Employee.ReferenceUserId == employee.ReferenceUserId);*/
                    return View(await applicationDbContext.ToListAsync());
                }
                 else if (role.ElementAt(0) == "DepartmentHead")
                {
                    var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    var applicationDbContext = _context.Leaves.Include(l => l.Employee).Include(l => l.LeavesCategory).Include(l => l.Department_Teams_Head); /*.Where(a=>a.Employee.DepartmentId==employee.DepartmentId&&a.Department_Teams_Head.HeadType=="Department" && a.Employee.Email != employee.Email && a.Employee.ReferenceUserId == employee.ReferenceUserId);*/
                    return View(await applicationDbContext.ToListAsync());
                }
             /*   else if (role.ElementAt(0) == "TeamHead")
                {
                    var employee = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    var applicationDbContext = _context.Leaves.Include(l => l.Employee).Include(l => l.LeavesCategory).Include(l => l.Department_Teams_Head);*//*.Where(a => a.Employee.DepartmentId == employee.DepartmentId && a.Department_Teams_Head.HeadType == "Team" && a.Employee.Email != employee.Email && a.Employee.ReferenceUserId == employee.ReferenceUserId);*//*
                    return View(await applicationDbContext.ToListAsync());
                }*/
                else if (role.ElementAt(0) == "Admin")
                {
                    var applicationDbContext = _context.Leaves.Include(l => l.Employee).Include(l => l.LeavesCategory).Include(l => l.Department_Teams_Head);/*.Where(a => a.Employee.ReferenceUserId.ToString() == user.Id)*/;
                    return View(await applicationDbContext.ToListAsync());
                }
                else if (role.ElementAt(0) == "HRManager")
                {
                    var applicationDbContext = _context.Leaves.Include(l => l.Employee).Include(l => l.LeavesCategory).Include(l => l.Department_Teams_Head);/*.Where(a => a.Employee.ReferenceUserId.ToString() == user.Id)*/;
                    return View(await applicationDbContext.ToListAsync());
                }
                else
                {
                    var applicationDbContext = _context.Leaves.Include(l => l.Employee).Include(l => l.LeavesCategory).Include(l => l.Department_Teams_Head);
                    return View(await applicationDbContext.ToListAsync());
                }
            }
            else
            {
                string link = Request.Scheme + "://" + Request.Host + "/Identity/Account/Login";
                return Redirect(link);
            }
        }


        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> EmployeeIndex()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                var role = await _userManager.GetRolesAsync(user);
                if (role.ElementAt(0) == "Employee")
                {
                    var emp = _context.Employees.Where(a => a.Email == user.Email).FirstOrDefault();
                    var applicationDbContext = _context.Leaves.Where(a => a.Employee.Email == user.Email).Include(l => l.Employee).Include(l => l.LeavesCategory).Include(l => l.Department_Teams_Head).Where(a => a.EmployeeId == emp.EmployeeId);
                    return View(await applicationDbContext.ToListAsync());
                }
            }
            return View();
        }

        // GET: Leaves/Details/5
        [Authorize(Roles = "Admin,Employee,ManagingDirector,TeamHead,HRManager")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaves = await _context.Leaves
                .Include(l => l.Employee)
                .Include(l => l.LeavesCategory)
                .Include(l => l.Department_Teams_Head)
                .FirstOrDefaultAsync(m => m.LeavesId == id);
            if (leaves == null)
            {
                return NotFound();
            }

            return View(leaves);
        }

        // GET: Leaves/Create
        [Authorize(Roles = "Admin,Employee,ManagingDirector,DepartmentHead,HRManager")]
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var role = await _userManager.GetRolesAsync(user);
            if (role.ElementAt(0) == "Employee")
            {
                ViewData["empName"] = _context.Employees.FirstOrDefault(a => a.Email == user.Email).FullName;
            }
            ViewData["Department_Teams_HeadsId"] = new SelectList(_context.Department_Teams_Heads.Where(a => a.Status == true).ToList(), "Department_Teams_HeadsId", "HeadId");
            ViewData["EmployeeId"] = new SelectList(_context.Employees.Where(a => a.Status == true).ToList(), "EmployeeId", "FullName");
            ViewData["LeavesCategoryId"] = new SelectList(_context.LeavesCategories.Where(a => a.Status == true).ToList(), "LeavesCategoryId", "Name");
            return View();
        }

        // POST: Leaves/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Leaves leaves)
        {
            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
               
                    var user = await _userManager.FindByIdAsync(userId);
                    var role = await _userManager.GetRolesAsync(user);
                if (role.ElementAt(0) == "Employee")
                {
                    leaves.EmployeeId = _context.Employees.FirstOrDefault(a => a.Email == user.Email).EmployeeId;
                }

                    _context.Add(leaves);
                leaves.ReferenceUserId = Guid.Parse(userId);
                _context.Leaves.Add(leaves);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Department_Teams_HeadsId"] = new SelectList(_context.Department_Teams_Heads, "Department_Teams_HeadsId", "HeadId", leaves.Department_Teams_HeadsId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", leaves.EmployeeId);
            ViewData["LeavesCategoryId"] = new SelectList(_context.LeavesCategories, "LeavesCategoryId", "Name", leaves.LeavesCategoryId);
            return View(leaves);
        }

        // GET: Leaves/Edit/5
        [Authorize(Roles = "Admin,Employee,ManagingDirector,HRManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaves = await _context.Leaves.FindAsync(id);
            if (leaves == null)
            {
                return NotFound();
            }
            ViewData["Department_Teams_HeadsId"] = new SelectList(_context.Department_Teams_Heads, "Department_Teams_HeadsId", "HeadId", leaves.Department_Teams_HeadsId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", leaves.EmployeeId);
            ViewData["LeavesCategoryId"] = new SelectList(_context.LeavesCategories, "LeavesCategoryId", "Name", leaves.LeavesCategoryId);
            return View(leaves);
        }

        // POST: Leaves/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Leaves leaves)
        {
            if (id != leaves.LeavesId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(leaves);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LeavesExists(leaves.LeavesId))
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
            ViewData["Department_Teams_HeadsId"] = new SelectList(_context.Department_Teams_Heads, "Department_Teams_HeadsId", "HeadId", leaves.Department_Teams_HeadsId);
            ViewData["EmployeeId"] = new SelectList(_context.Employees, "EmployeeId", "FullName", leaves.EmployeeId);
            ViewData["LeavesCategoryId"] = new SelectList(_context.LeavesCategories, "LeavesCategoryId", "Name", leaves.LeavesCategoryId);
            return View(leaves);
        }

        // GET: Leaves/Delete/5
        [Authorize(Roles = "Admin,ManagingDirector,Employee,HRManager")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaves = await _context.Leaves
                .Include(l => l.Employee)
                .Include(l => l.LeavesCategory)
                .Include(l => l.Department_Teams_Head)
                .FirstOrDefaultAsync(m => m.LeavesId == id);
            if (leaves == null)
            {
                return NotFound();
            }

            return View(leaves);
        }

        // POST: Leaves/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var leaves = await _context.Leaves.FindAsync(id);
            _context.Leaves.Remove(leaves);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LeavesExists(int id)
        {
            return _context.Leaves.Any(e => e.LeavesId == id);
        }
        [Authorize(Roles = "Admin,ManagingDirector,DepartmentHead,HRManager")]
        public ActionResult ChangeLeaveStatus(int? id, string Status)
        {
            try
            {
                EmployeeAttendanceRecord at = new EmployeeAttendanceRecord();
                var Leaves = _context.Leaves.Find(id);
                if (Status.ToLower()=="approved")
                {
                    var leaveDates = Enumerable.Range(0, 1 + Leaves.To.Date.Subtract(Leaves.From.Date).Days).Select(offset => Leaves.From.Date.AddDays(offset)).ToArray();
                    foreach (var obj in leaveDates)
                    {
                        at.EmployeeAttendanceRecordId = 0;
                        at.Date = obj.Date;
                        at.status = "leave";
                        at.EmployeeId = Leaves.EmployeeId;
                        _context.EmployeeAttendanceRecord.Add(at);
                        _context.SaveChanges();
                    }
                }
              
                if (id == null || Leaves == null)
                {
                    return NotFound();
                }
                Leaves.Status = Status;
                _context.Leaves.Update(Leaves);
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

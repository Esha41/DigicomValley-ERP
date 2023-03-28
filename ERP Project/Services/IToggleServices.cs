using ERP_Project.Data;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Services
{
    public interface IToggleServices
    {
        public Task ChangeShiftStatus(int id);
        public Task ChangeShiftTimmingStatus(int id);
        public Task ChangeApplicationsStatus(int id);

        public Task ChangeLeavesCategoriesStatus(int id);
    }

    public class ToggleService : IToggleServices
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment webHostEnvironment;
        public ToggleService(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            webHostEnvironment = hostEnvironment;
            _context = context;

        }

        public async Task ChangeShiftStatus(int id)
        {
            var b = await _context.Shifts.FindAsync(id);
            b.Status = !b.Status;
            _context.Shifts.Update(b);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeShiftTimmingStatus(int id)
        {
            var b = await _context.OfficialShifts.FindAsync(id);
            b.Status = !b.Status;
            _context.OfficialShifts.Update(b);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeApplicationsStatus(int id)
        {
            var b = await _context.Applications.FindAsync(id);
            b.Status = !b.Status;
            _context.Applications.Update(b);
            await _context.SaveChangesAsync();
        }

        public async Task ChangeLeavesCategoriesStatus(int id)
        {
            var b = await _context.LeavesCategories.FindAsync(id);
            b.Status = !b.Status;
            _context.LeavesCategories.Update(b);
            await _context.SaveChangesAsync();
        }
      /*  public async Task ChangeCheckoutApprovalStatus(int id)
        {
            var b = await _context.CheckoutApprovalRequests.FindAsync(id);
            b.ApprovalStatus = !b.Status;
            _context.CheckoutApprovalRequests.Update(b);
            await _context.SaveChangesAsync();
        }*/
    } 
}

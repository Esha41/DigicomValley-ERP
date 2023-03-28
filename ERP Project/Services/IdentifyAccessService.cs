using ERP_Project.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ERP_Project.Services
{
    public class IdentifyAccessService
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        public IdentifyAccessService(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<bool> IsAManagementHead(ClaimsPrincipal user)
        {
             
            var user1 = await _userManager.GetUserAsync(user);
            var emp = _context.Employees.Include(x => x.Department).Where(a => a.Email == user1.Email).FirstOrDefault();

            if(emp.Department.DepartmentName == "HR")
            {
                return true;
            }
            return false;
        }
    }
}

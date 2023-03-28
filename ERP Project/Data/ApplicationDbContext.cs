using ERP_Project.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ERP_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Department_Designations> Department_Designations { get; set; }
        public DbSet<DepartmentTeams> DepartmentTeams { get; set; }
        public DbSet<Department_Teams_Heads> Department_Teams_Heads { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public DbSet<Department_Teams_Employees> department_Teams_Employees { get; set; }
        public DbSet<EmployeeEducation> EmployeeEducation { get; set; }
        public DbSet<EmployeeExperience> EmployeeExperience { get; set; }
        public DbSet<EmployeeRelations> EmployeeRelations { get; set; }
        public DbSet<EmployeeReferences> EmployeeReferences { get; set; }
        public DbSet<EmployeePositions> EmployeePositions { get; set; }
        public DbSet<EmployeeSalaryHistory> EmployeeSalaryHistory { get; set; }
        public DbSet<EmployeeTimeRecord> EmployeeTimeRecord { get; set; }
        public DbSet<EmployeeReports> EmployeeReports { get; set; }
        public DbSet<EmployeeIndividualShift> EmployeeIndividualShift { get; set; }
        public DbSet<OfficialShifts> OfficialShifts { get; set; }
        public DbSet<Shifts> Shifts { get; set; }
        public DbSet<AssignShift> AssignShifts { get; set; }
        public DbSet<EmployeeTimming> EmployeeTimmings { get; set; }
        public DbSet<Skills> Skill { get; set; }
        public DbSet<EmployeeSkills> EmployeeSkill { get; set; }
        public DbSet<EmployeeAttendanceRecord> EmployeeAttendanceRecord { get; set; }
        public DbSet<Applications> Applications { get; set; }
        public DbSet<Applicants> Applicants { get; set; }
        public DbSet<Interview> Interviews { get; set; }
        public DbSet<CheckoutApprovalRequest> CheckoutApprovalRequests { get; set; }
        public DbSet<LeavesCategory> LeavesCategories { get; set; }
        public DbSet<Leaves> Leaves { get; set; }

        public DbSet<IPAddresses> IPAddresse { get; set; }
        public DbSet<Announcement> announcements { get; set; }
        public DbSet<ApplicantRemarks> applicantRemarks { get; set; }
    }
}

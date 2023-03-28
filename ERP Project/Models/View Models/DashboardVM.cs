using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models.View_Models
{
    public class DashboardVM
    {
        public int employeeCount { get; set; }
        public string userRole { get; set; }
        public int deptCount { get; set; }

        public int companyCount { get; set; }
        public int activeEmployeesCount { get; set; }
        public int teamCount { get; set; }
        public List<Applicants> newApplicants = new List<Applicants>();
        public List<Applications> Applications = new List<Applications>();
        public List<CheckoutApprovalRequest> newCheckoutRequest = new List<CheckoutApprovalRequest>();
        public List<IdentityUser> users { get; set; } = new List<IdentityUser>();
        public List<Employees> AllEmployees { get; set; }
        public List<EmployeeTimeRecord> presentEmployees { get; set; }=new List<EmployeeTimeRecord>();
        public List<Employees> absentEmployees { get; set; }= new List<Employees>();
        public List<Employees> EmployeesOnLeaves { get; set; } = new List<Employees>();
        public List<Leaves> leavesEmployees { get; set; }= new List<Leaves>();
    }
}

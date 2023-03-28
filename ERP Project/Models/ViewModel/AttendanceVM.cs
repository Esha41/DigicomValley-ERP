using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models.ViewModel
{
    public class AttendanceVM
    {
        public IEnumerable<IGrouping<DateTime, EmployeeTimeRecord>> empTimeRecordCred { get; set; }
        public List<KeyValuePair<string, string>> empTimeRecordToday = new List<KeyValuePair<string, string>>();
        public string shiftType { get; set; }
        public string emporgReportTime { get; set; }
        public int? emporgBreakTime{ get; set; }
        public string currchkintym { get; set; }
        public string currchkouttym { get; set; }
        public DateTime? emporgEndTime { get; set; }
        public DateTime empExactEndTime { get; set; }
        public string employeeEarlyLeavingReason { get; set; }
        public string ChangeAttendanceStatusReason { get; set; }
        public string employeeReport { get; set; }
        public int empId { get; set; }
        public DateTime Chdate { get; set; }
        public string alertmsg { get; set; }
        public bool AtToggle { get; set; }

        //admin
        public List<int> sundays { get; set; } = new List<int>();
        public List<EmployeeTimeRecord> AllemployeeTimeRecords { get; set; } = new List<EmployeeTimeRecord>();
        public List<Leaves> EmployeeLeaves { get; set; } = new List<Leaves>();
        public List<CheckoutApprovalRequest> unapprovedcheckouts { get; set; }= new List<CheckoutApprovalRequest>();

        public string getMonth { get; set; }
        public int getYear { get; set; } = 0;
        public string getMonthName { get; set; } 
        public Company company { get; set; }
        public Departments departments { get; set; }
        public Employees employees { get; set; }
        public List<Leaves> employeeLeaves { get; set; }
        public List<EmployeeReports> reportsList { get; set; } = new List<EmployeeReports>();
        public List<EmployeePositions> EmployeePositionsList { get; set; } = new List<EmployeePositions>();
        public int selectedMonth { get; set; }
        public List<IdentityUser> users { get; set; } = new List<IdentityUser>();
        public List<Employees> AllEmployees { get; set; }

       public List<KeyValuePair<string, DateTime>> empExactEndTime2 = new List<KeyValuePair<string, DateTime>>();
        public List<KeyValuePair<string, string>> emporgReportTime2 = new List<KeyValuePair<string, string>>();
        public List<Announcement> announcement { get; set; } = new List<Announcement>();
        public List<EmployeeAttendanceRecord> attendance { get; set; } = new List<EmployeeAttendanceRecord>();
        public List<EmployeeTimming> empTimingsList { get; set; } = new List<EmployeeTimming>();
        public int monthBit { get; set; } = 0;
        public bool IsRecordExist { get; set; }
    }
}

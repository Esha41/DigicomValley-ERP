using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models.ViewModel
{
    public class ReportsVM
    {
        public List<KeyValuePair<string, EmployeeTimming>> empExactEndTime2 = new List<KeyValuePair<string, EmployeeTimming>>();
        public IEnumerable<IGrouping<DateTime, EmployeeTimeRecord>> empTimeRecordCred { get; set; }
        public List<EmployeeTimming> empTimingsList { get; set; } = new List<EmployeeTimming>();

        public List<KeyValuePair<string, DateTime>> empExactEndTime3 = new List<KeyValuePair<string, DateTime>>();
        public List<KeyValuePair<string, string>> emporgReportTime2 = new List<KeyValuePair<string, string>>();
        public string shiftType { get; set; }
        public string emporgReportTime { get; set; }
        public int? emporgBreakTime { get; set; }
        public DateTime? emporgEndTime { get; set; }
        public DateTime empExactEndTime { get; set; }


        public DateTime currdate { get; set; }
        public Company company { get; set; }
        public Departments departments { get; set; }
        public Employees employees { get; set; }
        public DateTime getDate1{ get; set; }
        public List<Employees> AllEmployees { get; set; }
        public List<EmployeeTimeRecord> empTimeRecord { get; set; }
        public List<KeyValuePair<int,object>> empActivities = new List<KeyValuePair<int,object>>();
        public List<EmployeeReports> employeeReports { get; set; }
        public List<EmployeeReports> employeePerReport { get; set; } = new List<EmployeeReports>();
        public List<Leaves> employeeLeaves{ get; set; } = new List<Leaves>();
        public List<AssignShift> AssignShiftList { get; set; }
        public List<OfficialShifts> OfficialShiftsList { get; set; }
        public List<EmployeeTimming> EmployeeTimmingList { get; set; }
        public int leavesCount { get; set; }
        public int presentsCount { get; set; }
        public int absentsCount { get; set; }
        public int sundaysCount { get; set; }
        public int getYear { get; set; } = 0;
        public string getMonth { get; set; }
        public bool IsRecordExist { get; set; }
    }
}

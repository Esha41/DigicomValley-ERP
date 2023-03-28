using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models.View_Models
{
    public class ShiftAssignVM
    {
        public Employees employee { get; set; }
        public AssignShift assignShift { get; set; }
        public List<EmployeeTimming> Timming { get; set; } = new List<EmployeeTimming>();
        public List<EmployeeTimming> PrevTimming { get; set; } = new List<EmployeeTimming>();
        public List<OfficialShifts> OfficialTimming { get; set; } = new List<OfficialShifts>();
        public string shiftType { get; set; }
        public string position { get; set; }

    }
}

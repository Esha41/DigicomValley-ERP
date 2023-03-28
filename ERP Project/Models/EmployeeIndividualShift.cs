using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class EmployeeIndividualShift
    {
        [Key]
        public int EmployeeIndividualShiftId { get; set; }
        public string ShiftName { get; set; }
        public DateTime MinStartTime { get; set; } 
        public DateTime StartTime { get; set; }
        public DateTime MaxStartTime { get; set; }
        public DateTime MinEndTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime MaxEndTime { get; set; }
        public int? BreakTime { get; set; }

        public bool IsApproved { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;
        [Display(Name = "Employees")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
    }
}

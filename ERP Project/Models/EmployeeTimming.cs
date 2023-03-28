using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class EmployeeTimming
    {
        [Key]
        public int EmployeeTimmingId { get; set; }
        public DateTime MinStartTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime MaxStartTime { get; set; }
        public DateTime MinEndTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime MaxEndTime { get; set; }
        public int? BreakTime { get; set; }
        public string Day { get; set; }
        public bool IsApproved { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "ShiftAssign")]
        public int? ShiftAssignId { get; set; }

        [ForeignKey("ShiftAssignId")]
        public virtual AssignShift AssignShifts { get; set; }
        public Guid? ReferenceUserId { get; set; }
    }
}

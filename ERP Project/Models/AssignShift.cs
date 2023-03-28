using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class AssignShift
    {
        [Key]
        public int AssignShiftsId { get; set; }
        public string ShiftType { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Shift")]
        public int? ShiftId { get; set; }

        [ForeignKey("ShiftId")]
        public virtual Shifts Shifts { get; set; }

        [Display(Name = "Employees")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }
        public Guid? ReferenceUserId { get; set; }
    }
}

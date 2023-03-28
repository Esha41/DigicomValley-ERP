using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class OfficialShifts
    {
        [Key]
        public int OfficialShiftsId { get; set; }
        public DateTime MinStartTime { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime MaxStartTime { get; set; }
        public DateTime MinEndTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime MaxEndTime { get; set; }
        public int? BreakTime { get; set; }
        public string Day { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [Display(Name = "Shift")]
        public int? ShiftId { get; set; }

        [ForeignKey("ShiftId")]
        public virtual Shifts Shifts { get; set; }
        public Guid? ReferenceUserId { get; set; }
    }
}

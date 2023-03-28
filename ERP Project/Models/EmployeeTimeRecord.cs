using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class EmployeeTimeRecord
    {
        [Key]
        public int EmployeeTimeRecordId { get; set; }
        public string RecordTypeName { get; set; } 
        public DateTime RecordTime { get; set; } 
        public string UserRemarks { get; set; }
        public string AdminRemarks { get; set; }
        public bool IsApproved { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;
        [Display(Name = "Employees")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }
    }
}

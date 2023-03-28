using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Leaves
    {
        [Key]
        public int LeavesId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string NumofDays { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "LeavesCategory")]
        public int? LeavesCategoryId { get; set; }

        [ForeignKey("LeavesCategoryId")]
        public virtual LeavesCategory LeavesCategory { get; set; }

        [Display(Name = "Employee")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }

        [Display(Name = "Department_Teams_Head")]
        public int? Department_Teams_HeadsId { get; set; }

        [ForeignKey("Department_Teams_HeadsId")]
        public virtual Department_Teams_Heads Department_Teams_Head { get; set; }
        public Guid? ReferenceUserId { get; set; }
    }
}

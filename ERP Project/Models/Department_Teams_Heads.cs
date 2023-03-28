using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Department_Teams_Heads
    {
        public int Department_Teams_HeadsId { get; set; }
        public string HeadType { get; set; }  //of team or department
        public int HeadId { get; set; }  //of team or department
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Employees")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }
       // public Guid? ReferenceUserId { get; set; }

    }
}

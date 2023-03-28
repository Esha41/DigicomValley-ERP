using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class EmployeeReferences
    {
        [Key]
        public int EmployeeReferenceId { get; set; }
        public string FullName { get; set; }
        public string CompanyName { get; set; }
        public string Relationship { get; set; }
        public string ContactNo { get; set; }
        public string Address { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Employees")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Department_Designations
    {
        [Key]
        public int Department_DesignationsId { get; set; }
        public string DesignationName{ get; set; }

        [Display(Name = "Departments")]
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Departments Department { get; set; }

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;
        public Guid? ReferenceUserId { get; set; }
    }
}

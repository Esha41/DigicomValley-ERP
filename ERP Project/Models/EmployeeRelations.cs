using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class EmployeeRelations
    {
        [Key]
        public int EmployeeRelationId { get; set; }
        public string RelationshipName { get; set; }
        public string PersonName { get; set; }
        public string PersonContactNo { get; set; }
        public string Type { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;


        [Display(Name = "Employees")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }
    }
}

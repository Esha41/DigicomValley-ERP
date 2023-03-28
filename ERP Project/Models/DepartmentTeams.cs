using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class DepartmentTeams
    {
        [Key]
        public int DepartmentTeamsId { get; set; }
        public string TeamName { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;
        [Display(Name = "Departments")]
        public int? DepartmentId { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Departments Department { get; set; }
        public Guid? ReferenceUserId { get; set; }
    }
}

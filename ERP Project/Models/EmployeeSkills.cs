using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class EmployeeSkills
    {
        [Key]
        public int EmployeeSkillId { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Employees")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }

        [Display(Name = "Skill")]
        public int? SkillsId { get; set; }

        [ForeignKey("SkillsId")]
        public virtual Skills Skill { get; set; }
    }
}

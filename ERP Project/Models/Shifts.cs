using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Shifts
    {
        [Key]
        public int ShiftId { get; set; }
        public string ShiftName { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;
        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        public Guid? ReferenceUserId { get; set; }
    }
}

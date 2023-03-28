using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Applications
    {
        [Key]
        public int ApplicationId { get; set; }
        public string Title { get; set; }
        public string Discription { get; set; }
        public DateTime PostDate { get; set; }
        public string ApplicationStatus { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [Display(Name = "Designation")]
        public int? DesignationId { get; set; }

        [ForeignKey("DesignationId")]
        public virtual Department_Designations Designation { get; set; }
        public Guid? ReferenceUserId { get; set; }


    }
}

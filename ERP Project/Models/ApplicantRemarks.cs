using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class ApplicantRemarks
    {
        [Key]
        public int ApplicantRemarksId { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Applicants")]
        public int? ApplicantsId { get; set; }

        [ForeignKey("ApplicantsId")]
        public virtual Applicants Applicant { get; set; }
        public string Remarks { get; set; }
    }
}

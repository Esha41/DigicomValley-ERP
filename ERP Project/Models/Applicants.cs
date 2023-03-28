using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Applicants
    {
        [Key]
        public int ApplicantsId { get; set; }
        public string Name { get; set; }
        [RegularExpression(@"^\(?([0-9]{4})\)?[-. ]?([0-9]{7})$", ErrorMessage = "Phone No must follow the XXXXXXXXXXX format!.")]
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Portal { get; set; }
        public DateTime InterViewDate { get; set; }
        public string AplicantStatus { get; set; } = "Pending";

        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Application")]
        public int? ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public virtual Applications Application { get; set; }
        public Guid? ReferenceUserId { get; set; }

        public string Description { get; set; }
        public bool SMSsend { get; set; } = false;

    }
}

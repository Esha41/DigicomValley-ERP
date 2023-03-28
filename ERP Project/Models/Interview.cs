using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Interview
    {
        [Key]
        public int InterviewId { get; set; }
        public DateTime? InterviewDate { get; set; }
        public DateTime? ReScheduleDate { get; set; }
        public string Remarks { get; set; }
        public string InterviewStatus { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Applicant")]
        public int? ApplicantId { get; set; }

        [ForeignKey("ApplicantId")]
        public virtual Applicants Applicant { get; set; }

        [Display(Name = "Interviewer")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Interviewer { get; set; }
        public Guid? ReferenceUserId { get; set; }

    }
}

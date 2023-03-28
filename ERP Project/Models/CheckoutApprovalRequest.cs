using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class CheckoutApprovalRequest
    {
        [Key]
        public int CheckoutApprovalRequestId { get; set; }
        public DateTime ResponseTime { get; set; }
        public string ApprovalStatus { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "EmployeeTimeRecords")]
        public int? EmployeeTimeRecordId { get; set; }

        [ForeignKey("EmployeeTimeRecordId")]
        public virtual EmployeeTimeRecord EmployeeTimeRecords { get; set; }

        [Display(Name = "Department_Teams_Head")]
        public int? Department_Teams_HeadsId { get; set; }

        [ForeignKey("Department_Teams_HeadsId")]
        public virtual Department_Teams_Heads Department_Teams_Head { get; set; }
        public Guid? ReferenceUserId { get; set; }
    }
}

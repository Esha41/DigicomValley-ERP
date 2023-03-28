using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Employees
    {
        [Key]
        public int EmployeeId { get; set; }
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public string DateOfBirth { get; set; }
        public string CurrentAddress { get; set; }
        public string PermanentAddress { get; set; }
        public string HomeContactNo { get; set; }
        public string CNIC { get; set; }
        public string Image { get; set; }
        public string MobileContactNo { get; set; }
        public string JoiningDate { get; set; }
        public string CreationDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsAttendanceIPbased { get; set; }
        public bool Status { get; set; } = true;
        public string RemarksForChngStatus { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Department_Designation")]
        public int? Department_DesignationsId { get; set; }

        [ForeignKey("Department_DesignationsId")]
        public virtual Department_Designations Department_Designation { get; set; }


        [Display(Name = "Department")]
        public int? DepartmentId { get; set; } 

        [ForeignKey("DepartmentId")]
        public virtual Departments Department { get; set; }


        [Display(Name = "Company")]
        public int? CompanyId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        public Guid? ReferenceUserId { get; set; }
        public string EmployeeOf { get; set; }

    }
}

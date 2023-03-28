using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class EmployeeSalaryHistory
    {
        [Key]
        public int EmployeeSalaryHistoryId { get; set; }
        public int ReceivedAmount { get; set; }
        public int RemainingAmount { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "Employee Positions")]
        public int? EmployeePositionsId { get; set; }

        [ForeignKey("EmployeePositionsId")]
        public virtual EmployeePositions EmployeePosition { get; set; }
    }
}

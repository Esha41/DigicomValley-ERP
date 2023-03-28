using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Department_Teams_Employees
    {
        public int Department_Teams_EmployeesId { get; set; }
        public bool Status { get; set; }=true;
        public DateTime Date { get; set; } = DateTime.Now;

        [Display(Name = "DepartmentTeams")]
        public int? DepartmentTeamsId { get; set; }

        [ForeignKey("DepartmentTeamsId")]
        public virtual DepartmentTeams GetDepartmentTeam { get; set; }

        [Display(Name = "Employees")]
        public int? EmployeeId { get; set; }

        [ForeignKey("EmployeeId")]
        public virtual Employees Employee { get; set; }

    }
}

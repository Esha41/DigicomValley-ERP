using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models.ViewModel
{
    public class DepartmentsVM
    {
        public Departments departments { get; set; }
        public List<int> EmployeesIds{ get; set; }
        public List<int> TeamEmployeesIds { get; set; }
        public IEnumerable<Department_Teams_Heads> department_Teams_Head_List { get; set; }
        public IEnumerable<Departments> DepartmentsList { get; set; }
        public IEnumerable<Employees> EmployeeList { get; set; }
        public List<Employees> TeamEmployeesList { get; set; }
        public IEnumerable<Company> CompaniesList { get; set; }
        public Company Companies{ get; set; }
        public Department_Designations designations { get; set; }
        public IEnumerable<Department_Designations> Department_Designations_List { get; set; }
        public DepartmentTeams teams { get; set; }
        public IEnumerable<DepartmentTeams> DepartmentTeamsList { get; set; }
        public Department_Teams_Employees teamsEmployees { get; set; }
        public List<Department_Teams_Employees> depteamsEmployeesList { get; set; }
        public List<EmployeePositions> employeepositions { get; set; }
        
    }
}

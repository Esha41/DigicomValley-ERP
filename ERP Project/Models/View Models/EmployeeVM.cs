using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models.View_Models
{
    public class EmployeeVM
    {
        public Employees employee { get; set; }
        public string password { get; set; }
        public string shift { get; set; }
        public string empId { get; set; }
        public int empId2 { get; set; }

        public string RemarksToChngStatus { get; set; }
        public EmployeePositions position { get; set; }
    
        public EmployeeIndividualShift employeeIndividualShift { get; set; }
        public EmployeeRelations PrimaryRelations { get; set; }
        public EmployeeRelations SecondaryRelations { get; set; }
        public EmployeeExperience employeeExperience { get; set; }
        public EmployeeExperience employeeExperience1 { get; set; }
        public EmployeeExperience employeeExperience2 { get; set; }
        public EmployeeReferences employeeReferences { get; set; }
        public EmployeeReferences employeeReferences1 { get; set; }
        public List<int> SkillsId { get; set; }
        public List<Skills> SkillsMultipleSelect = new List<Skills>();
        public List<EmployeeEducation> education { get; set; } = new List<EmployeeEducation>();
        public List<EmployeeExperience> employeeExperienceList { get; set; } = new List<EmployeeExperience>();
        public List<EmployeeSkills> employeeskillsList { get; set; } = new List<EmployeeSkills>();
        public List<EmployeeReferences> employeesReferencesList { get; set; } = new List<EmployeeReferences>();
        //shift details
        public AssignShift assignShift { get; set; }
        public List<EmployeeTimming> Timming { get; set; } = new List<EmployeeTimming>();
        public List<EmployeeTimming> PrevTimming { get; set; } = new List<EmployeeTimming>();
        public List<OfficialShifts> OfficialTimming { get; set; } = new List<OfficialShifts>();
        public string shiftType { get; set; }
        public IEnumerable<Employees> employees { get; set; }
        public IEnumerable<Department_Teams_Heads> depHeadsList { get; set; }
        public IEnumerable<Departments> department { get; set; }
        public IEnumerable<DepartmentTeams> departmentTeams { get; set; }
        public IEnumerable<EmployeePositions> employeePositions { get; set; }
        public IEnumerable<Department_Designations> designations { get; set; }
    }
}

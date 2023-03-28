using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class LeavesCategory
    {
        [Key]
        public int LeavesCategoryId { get; set; }
        public string Name { get; set; }
        public bool Status { get; set; } = true;
        public DateTime Date { get; set; } = DateTime.Now;
        public Guid? ReferenceUserId { get; set; }
    }
}

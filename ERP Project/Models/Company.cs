using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        public string CompanyName { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public Guid? ReferenceUserId { get; set; }
    }
}

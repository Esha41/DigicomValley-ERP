using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class IPAddresses
    {
        [Key]
        public int Id { get; set; }
        public string IpAddreess { get; set; }
    }
}

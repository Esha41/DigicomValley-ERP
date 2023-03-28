using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ERP_Project.Models
{
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Status { get; set; } = true;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;
        public DateTime Date { get; set; } = DateTime.Now;
        public Guid? ReferenceUserId { get; set; }

    }
}

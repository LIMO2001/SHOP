using System;
using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Models
{
    public class Career
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Required]
        [StringLength(100)]
        public string Department { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Requirements { get; set; }

        [Required]
        [StringLength(50)]
        public string EmploymentType { get; set; } // Full-time, Part-time, Contract

        public decimal? SalaryRangeMin { get; set; }
        public decimal? SalaryRangeMax { get; set; }

        public DateTime DatePosted { get; set; }
        public DateTime ApplicationDeadline { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
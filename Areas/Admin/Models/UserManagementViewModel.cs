using System.ComponentModel.DataAnnotations;

namespace LaptopStore.Areas.Admin.Models
{
    public class UserManagementViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "Customer";

        public DateTime CreatedAt { get; set; }

        public bool IsActive { get; set; }

        // For statistics
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
        public DateTime? LastOrderDate { get; set; }
    }

    public class UserListViewModel
    {
        public List<UserManagementViewModel> Users { get; set; } = new List<UserManagementViewModel>();
        public int TotalCount { get; set; }
        public int ActiveUsers { get; set; }
        public int AdminUsers { get; set; }
        public int CustomerUsers { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace CarRentSystem.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; }

        public string Role { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        public string? Password { get; set; } // ✅ Allow null for updates
    }

}

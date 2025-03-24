using System.Text.Json.Serialization;

namespace CarRentSystem.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // Ensure it's just an integer
        public int CarId { get; set; }   // Ensure it's just an integer
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalPrice { get; set; }

        public string Status { get; set; } = "Pending";  // Default to "Pending"

        // Optional: If you want navigation but avoid JSON serialization errors
        [JsonIgnore]
        public User? User { get; set; }

        [JsonIgnore]
        public Car? Car { get; set; }
    }


}

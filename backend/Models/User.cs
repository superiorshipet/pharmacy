using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DawaeeBackend.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        
        public string? ChronicDiseases { get; set; }
        
        public string Role { get; set; } = "Patient";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [JsonIgnore]
        public List<MedicationSchedule> MedicationSchedules { get; set; } = new();
        
        [JsonIgnore]
        public List<ChatMessage> ChatMessages { get; set; } = new();
    }
}

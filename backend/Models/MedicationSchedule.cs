using System.Text.Json.Serialization;

namespace DawaeeBackend.Models
{
    public class MedicationSchedule
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MedicationId { get; set; }
        public DateTime ScheduledTime { get; set; }
        public bool IsTaken { get; set; } = false;
        public DateTime? TakenAt { get; set; }
        
        [JsonIgnore]
        public User User { get; set; } = null!;
        
        [JsonIgnore]
        public Medication Medication { get; set; } = null!;
    }
}

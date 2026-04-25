using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DawaeeBackend.Models
{
    public class Medication
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string NameAr { get; set; } = string.Empty;
        
        [Required]
        public string NameEn { get; set; } = string.Empty;
        
        public string ActiveIngredientAr { get; set; } = string.Empty;
        public string ActiveIngredientEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string WarningsAr { get; set; } = string.Empty;
        public string WarningsEn { get; set; } = string.Empty;
        public string DangerLevel { get; set; } = "low";
        
        [JsonIgnore]
        public List<Interaction> Interactions { get; set; } = new();
        
        [JsonIgnore]
        public List<MedicationSchedule> Schedules { get; set; } = new();
    }
}

namespace DawaeeBackend.Models
{
    public class Interaction
    {
        public int Id { get; set; }
        public int MedicationId { get; set; }
        public int ConflictingMedicationId { get; set; }
        public string WarningAr { get; set; } = string.Empty;
        public string WarningEn { get; set; } = string.Empty;
        
        // Navigation properties
        public Medication Medication { get; set; } = null!;
        public Medication ConflictingMedication { get; set; } = null!;
    }
}

namespace DawaeeBackend.DTOs
{
    public class MedicationDto
    {
        public int Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string ActiveIngredientAr { get; set; } = string.Empty;
        public string ActiveIngredientEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string WarningsAr { get; set; } = string.Empty;
        public string WarningsEn { get; set; } = string.Empty;
        public string DangerLevel { get; set; } = string.Empty;
    }

    public class CreateMedicationDto
    {
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string ActiveIngredientAr { get; set; } = string.Empty;
        public string ActiveIngredientEn { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string WarningsAr { get; set; } = string.Empty;
        public string WarningsEn { get; set; } = string.Empty;
        public string DangerLevel { get; set; } = "low";
    }
}

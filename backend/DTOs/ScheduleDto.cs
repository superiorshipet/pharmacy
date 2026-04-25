namespace DawaeeBackend.DTOs
{
    public class CreateScheduleDto
    {
        public int MedicationId { get; set; }
        public DateTime ScheduledTime { get; set; }
    }

    public class ScheduleResponseDto
    {
        public int Id { get; set; }
        public int MedicationId { get; set; }
        public string MedicationNameAr { get; set; } = string.Empty;
        public string MedicationNameEn { get; set; } = string.Empty;
        public DateTime ScheduledTime { get; set; }
        public bool IsTaken { get; set; }
    }

    public class ToggleTakenDto
    {
        public int ScheduleId { get; set; }
        public bool IsTaken { get; set; }
    }

    public class WeeklyReportDto
    {
        public int TotalMeds { get; set; }
        public int TakenMeds { get; set; }
        public double AdherencePercentage { get; set; }
    }
}

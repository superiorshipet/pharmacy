namespace DawaeeBackend.DTOs
{
    public class PatientListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ChronicDiseases { get; set; }
        public DateTime RegisteredAt { get; set; }
        public int TotalSchedules { get; set; }
        public double AdherenceRate { get; set; }
    }

    public class PatientDetailsDto : PatientListDto
    {
        public List<ScheduleResponseDto> CurrentSchedules { get; set; } = new();
        public List<ChatHistoryDto> ChatHistory { get; set; } = new();
    }

    public class ChatHistoryDto
    {
        public int Id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}

namespace QuizMasterAPI.Models.DTOs
{
    public class TestAnalyticsWithHistoryDto : TestAnalyticsDto
    {
        // Сюда добавляем список прохождений
        public List<UserTestHistoryDto> History { get; set; } = new();
    }
}

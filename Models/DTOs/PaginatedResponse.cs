namespace QuizMasterAPI.Models.DTOs
{
    /// <summary>
    /// Универсальная модель ответа с данными и метаданными пагинации.
    /// </summary>
    /// <typeparam name="T">Тип DTO, который вы возвращаете</typeparam>
    public class PaginatedResponse<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
    }
}

namespace QuizMasterAPI.Models.Entities
{
    public class Topic
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Если true — топик «опросниковый».
        /// Тогда все вопросы внутри него не имеют правильных ответов (isCorrect = null).
        /// </summary>
        public bool IsSurveyTopic { get; set; } = false;

        /// <summary>
        /// Связь с Category (чтобы топики были сгруппированы по категориям).
        /// </summary>
        public int CategoryId { get; set; }           // <-- Новое поле
        public Category? Category { get; set; }       // Навигационное свойство (опционально nullable)
    }
}

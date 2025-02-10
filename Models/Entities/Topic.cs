namespace QuizMasterAPI.Models.Entities
{
    public class Topic
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Новое поле: если true — это «опросниковый» топик.
        /// Тогда все вопросы внутри него не имеют «правильных» ответов.
        /// </summary>
        public bool IsSurveyTopic { get; set; } = false;
    }
}

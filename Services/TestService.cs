using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class TestService : ITestService
    {
        private readonly ITestRepository _testRepository;
        private readonly IQuestionRepository _questionRepository;

        public TestService(
            ITestRepository testRepository,
            IQuestionRepository questionRepository)
        {
            _testRepository = testRepository;
            _questionRepository = questionRepository;
        }

        public async Task<Test> CreateTestAsync(int questionCount)
        {
            // 1. Получаем нужное кол-во случайных вопросов:
            var randomQuestions = await _questionRepository.GetRandomQuestionsAsync(questionCount);

            // 2. Создаем новый тест
            var newTest = new Test
            {
                CreatedAt = DateTime.UtcNow
            };

            // 3. Создаем связи (TestQuestion) к каждому вопросу:
            foreach (var question in randomQuestions)
            {
                var testQuestion = new TestQuestion
                {
                    QuestionId = question.Id
                };
                newTest.TestQuestions.Add(testQuestion);
            }

            // 4. Сохраняем тест в базе:
            var createdTest = await _testRepository.CreateTestAsync(newTest);

            return createdTest;
        }

        public async Task<Test?> GetTestByIdAsync(int id)
        {
            return await _testRepository.GetTestByIdAsync(id);
        }

        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _testRepository.GetAllTestsAsync();
        }

        /// <summary>
        /// Проверяем ответы по тесту
        /// </summary>
        public async Task<TestCheckResultDto> CheckTestAnswersAsync(int testId, List<TestAnswerValidationDto> userAnswers)
        {
            // 1. Загружаем тест со всеми вопросами + вариантами:
            //    (предполагаем, что в репозитории вы делаете .Include(t => t.TestQuestions).ThenInclude(tq => tq.Question))
            var test = await _testRepository.GetTestByIdAsync(testId);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID {testId} not found.");

            // Для удобства подгрузим варианты ответа (если не были подгружены):
            // Можно либо расширить репозиторий GetTestByIdAsync(...Include(tq => tq.Question.AnswerOptions)),
            // либо вручную запросить QuestionRepository.
            // Ниже для наглядности сделаем:
            var questionIds = test.TestQuestions.Select(tq => tq.QuestionId).ToList();
            var questionsWithAnswers = await _questionRepository.GetQuestionsWithAnswersByIdsAsync(questionIds);

            // 2. Начинаем формировать результат
            var result = new TestCheckResultDto
            {
                TotalQuestions = test.TestQuestions.Count
            };

            // 3. Для каждого вопроса в тесте проверим, что пользователь выбрал
            foreach (var testQuestion in test.TestQuestions)
            {
                // Находим соответствующий вопрос (с его вариантами)
                var question = questionsWithAnswers.FirstOrDefault(q => q.Id == testQuestion.QuestionId);
                if (question == null)
                    throw new KeyNotFoundException($"Question with ID {testQuestion.QuestionId} not found.");

                // Ищем ответ пользователя (SelectedAnswerIds) по этому question.Id
                var userAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == question.Id);

                // Если пользователь не передал ничего про этот вопрос, считаем ответ неправильным
                var selectedAnswerIds = userAnswer?.SelectedAnswerIds ?? new List<int>();

                // Получаем все правильные варианты
                var correctAnswerIds = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                // Проверим, верно ли
                // Правильным считаем ответ, если выбран ровно тот набор, что и correctAnswerIds
                bool isCorrect = !correctAnswerIds.Except(selectedAnswerIds).Any()
                                 && !selectedAnswerIds.Except(correctAnswerIds).Any();

                if (isCorrect)
                    result.CorrectCount++;

                // 4. Формируем детальную информацию
                var questionCheck = new QuestionCheckResultDto
                {
                    QuestionId = question.Id,
                    IsCorrect = isCorrect,
                    CorrectAnswers = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Text)
                        .ToList(),
                    SelectedAnswers = question.AnswerOptions
                        .Where(a => selectedAnswerIds.Contains(a.Id))
                        .Select(a => a.Text)
                        .ToList()
                };

                result.Results.Add(questionCheck);
            }

            return result;
        }
    }
}

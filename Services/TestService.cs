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
            var randomQuestions = await _questionRepository.GetRandomQuestionsAsync(questionCount);

            var newTest = new Test
            {
                CreatedAt = DateTime.UtcNow
            };

            foreach (var question in randomQuestions)
            {
                newTest.TestQuestions.Add(new TestQuestion
                {
                    QuestionId = question.Id
                });
            }

            await _testRepository.AddAsync(newTest);
            await _testRepository.SaveChangesAsync();

            return newTest;
        }

        public async Task<Test?> GetTestByIdAsync(int id)
        {
            return await _testRepository.GetTestByIdAsync(id);
        }

        public async Task<IEnumerable<Test>> GetAllTestsAsync()
        {
            return await _testRepository.GetAllTestsAsync();
        }


        public async Task<Test> UpdateTestAsync(int id, List<int> questionIds)
        {
            // 1. Проверяем, существует ли тест
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Тест с Id={id} не найден.");

            // 2. Проверяем, существуют ли все указанные вопросы
            var questions = await _questionRepository.GetQuestionsWithAnswersByIdsAsync(questionIds);
            if (questions.Count != questionIds.Count)
                throw new ArgumentException("Некоторые из указанных вопросов не найдены.");

            // 3. Обновляем связанные вопросы
            test.TestQuestions.Clear();
            foreach (var question in questions)
            {
                test.TestQuestions.Add(new TestQuestion
                {
                    QuestionId = question.Id,
                    TestId = test.Id
                });
            }

            await _testRepository.UpdateAsync(test);
            await _testRepository.SaveChangesAsync();

            return test;
        }

        public async Task DeleteTestAsync(int id)
        {
            var test = await _testRepository.GetTestByIdAsync(id);
            if (test == null)
                throw new KeyNotFoundException($"Тест с Id={id} не найден.");

            await _testRepository.DeleteAsync(test);
            await _testRepository.SaveChangesAsync();
        }


        /// <summary>
        /// Проверяем ответы по тесту
        /// </summary>
        public async Task<TestCheckResultDto> CheckTestAnswersAsync(int testId, List<TestAnswerValidationDto> userAnswers)
        {
            // 1. Загружаем тест со всеми вопросами
            var test = await _testRepository.GetTestByIdAsync(testId);
            if (test == null)
                throw new KeyNotFoundException($"Test with ID {testId} not found.");

            // Множество ID вопросов, реально входящих в тест
            var validQuestionIds = test.TestQuestions.Select(tq => tq.QuestionId).ToHashSet();

            // 2. Проверяем, нет ли вопросов, не входящих в тест
            var invalidUserAnswers = userAnswers.Where(ua => !validQuestionIds.Contains(ua.QuestionId)).ToList();
            if (invalidUserAnswers.Any())
            {
                var invalidIds = string.Join(",", invalidUserAnswers.Select(i => i.QuestionId));
                throw new ArgumentException($"Вопрос(ы) с ID [{invalidIds}] не принадлежат тесту с ID {testId}.");
            }

            // 3. Подгружаем варианты ответов для всех вопросов теста
            var questionsWithAnswers = await _questionRepository.GetQuestionsWithAnswersByIdsAsync(validQuestionIds.ToList());

            // 4. Готовим структуру результата
            var result = new TestCheckResultDto
            {
                TotalQuestions = test.TestQuestions.Count
            };

            // 5. Проверяем ответы пользователя
            foreach (var testQuestion in test.TestQuestions)
            {
                var question = questionsWithAnswers.FirstOrDefault(q => q.Id == testQuestion.QuestionId);
                if (question == null)
                    throw new KeyNotFoundException($"Question with ID {testQuestion.QuestionId} not found in DB.");

                // Ищем, что выбрал пользователь
                var userAnswer = userAnswers.FirstOrDefault(a => a.QuestionId == question.Id);
                var selectedAnswerIds = userAnswer?.SelectedAnswerIds ?? new List<int>();

                // Все правильные варианты ответа
                var correctAnswerIds = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                // Логика проверки (набор выбранных совпадает с набором правильных)
                bool isCorrect = !correctAnswerIds.Except(selectedAnswerIds).Any()
                                 && !selectedAnswerIds.Except(correctAnswerIds).Any();

                if (isCorrect)
                    result.CorrectCount++;

                // Детали
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

using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class UserTestService : IUserTestService
    {
        private readonly IUserTestRepository _userTestRepository;
        private readonly ITestRepository _testRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuizDbContext _context; // Инжектим контекст напрямую


        public UserTestService(
            IUserTestRepository userTestRepository,
            ITestRepository testRepository,
            IQuestionRepository questionRepository,
            QuizDbContext context)
        {
            _userTestRepository = userTestRepository;
            _testRepository = testRepository;
            _questionRepository = questionRepository;
            _context = context; // сохраняем в поле
        }

        /// <summary>
        /// Пользователь начинает прохождение теста.
        /// 1) Находим шаблон Test (testId).
        /// 2) По его настройкам (CountOfQuestions, TopicId) вытаскиваем рандомные вопросы.
        /// 3) Создаём UserTest + UserTestQuestions.
        /// </summary>
        public async Task<UserTest> StartTestAsync(int testId, string userId)
        {
            // 1) Шаблон теста
            var template = await _testRepository.GetTestByIdAsync(testId);
            if (template == null)
                throw new KeyNotFoundException($"Тест-шаблон с ID={testId} не найден.");

            var count = template.CountOfQuestions;
            var topicId = template.TopicId;

            // 2) Рандомные вопросы
            var questions = await _questionRepository.GetRandomQuestionsAsync(count, topicId);

            // 3) Создаём UserTest
            var userTest = new UserTest
            {
                UserId = userId,
                TestId = template.Id,
                TotalQuestions = questions.Count,
                CorrectAnswers = 0,
                IsPassed = false,
                DateCreated = DateTime.UtcNow
            };

            await _userTestRepository.AddAsync(userTest);
            await _userTestRepository.SaveChangesAsync(); // чтобы userTest.Id заполнился

            // Привязываем вопросы
            var userTestQuestions = questions.Select(q => new UserTestQuestion
            {
                UserTestId = userTest.Id,
                QuestionId = q.Id
            }).ToList();

            // Можно добавить через контекст, 
            // или создать метод в репозитории (например, AddRangeAsync)
            // но для простоты используем DbContext напрямую 
            // ИЛИ у нас GenericRepository<T> уже умеет AddAsync, SaveChangesAsync и т.д.

            _context.UserTestQuestions.AddRange(userTestQuestions);
            await _context.SaveChangesAsync();

            return userTest;
        }

        /// <summary>
        /// Проверяем ответы для UserTest (id), сверяем с вариантами вопроса.
        /// Возвращаем dto с количеством правильных.
        /// </summary>
        public async Task<TestCheckResultDto> CheckUserTestAnswersAsync(int userTestId, List<TestAnswerValidationDto> answers, string userId)
        {
            // 1) Ищем UserTest с вопросами
            var userTest = await _userTestRepository.GetUserTestWithQuestionsAsync(userTestId);
            if (userTest == null)
                throw new KeyNotFoundException($"UserTest с ID={userTestId} не найден.");

            // Проверяем, что это UserTest принадлежит тому же userId
            if (userTest.UserId != userId)
                throw new UnauthorizedAccessException("Этот тест принадлежит другому пользователю.");

            // Достаём все ID вопросов, которые были сгенерированы
            var questionIds = userTest.UserTestQuestions.Select(utq => utq.QuestionId).ToList();

            // Загружаем вопросы с вариантами
            // (предполагаем, что наш репозиторий умеет это)
            var questions = await _questionRepository.GetQuestionsWithAnswersByIdsAsync(questionIds);

            // Готовим результат
            var result = new TestCheckResultDto
            {
                TotalQuestions = userTest.TotalQuestions,
                CorrectCount = 0,
                Results = new List<QuestionCheckResultDto>()
            };

            int correctCount = 0;

            // 2) Идём по каждому вопросу в этом UserTest
            foreach (var utq in userTest.UserTestQuestions)
            {
                var question = questions.FirstOrDefault(q => q.Id == utq.QuestionId);
                if (question == null)
                    continue;

                // Смотрим, что пользователь ответил
                var userAnswer = answers.FirstOrDefault(a => a.QuestionId == question.Id);
                var selectedAnswerIds = userAnswer?.SelectedAnswerIds ?? new List<int>();

                var correctAnswerIds = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                bool isCorrect = !correctAnswerIds.Except(selectedAnswerIds).Any()
                                 && !selectedAnswerIds.Except(correctAnswerIds).Any();
                if (isCorrect)
                {
                    correctCount++;
                }

                // Добавляем детализацию
                var questionCheckDto = new QuestionCheckResultDto
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

                result.Results.Add(questionCheckDto);
            }

            result.CorrectCount = correctCount;

            // 3) Сохраняем результат в UserTest
            userTest.CorrectAnswers = correctCount;
            userTest.IsPassed = (correctCount == userTest.TotalQuestions);

            await _context.SaveChangesAsync();

            return result;
        }
    }
}

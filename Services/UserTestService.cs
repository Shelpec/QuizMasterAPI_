using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using Microsoft.EntityFrameworkCore;


namespace QuizMasterAPI.Services
{
    public class UserTestService : IUserTestService
    {
        private readonly IUserTestRepository _userTestRepository;
        private readonly ITestRepository _testRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuizDbContext _context; // или используем только репозитории

        public UserTestService(
            IUserTestRepository userTestRepository,
            ITestRepository testRepository,
            IQuestionRepository questionRepository,
            QuizDbContext context)
        {
            _userTestRepository = userTestRepository;
            _testRepository = testRepository;
            _questionRepository = questionRepository;
            _context = context;
        }

        public async Task<UserTest> CreateAsync(UserTest userTest)
        {
            await _userTestRepository.AddAsync(userTest);
            await _userTestRepository.SaveChangesAsync();
            return userTest;
        }

        public async Task<UserTest?> GetByIdAsync(int id)
        {
            return await _userTestRepository.GetUserTestAsync(id);
        }

        public async Task UpdateAsync(UserTest userTest)
        {
            await _userTestRepository.UpdateAsync(userTest);
            await _userTestRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _userTestRepository.GetUserTestAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"UserTest with id={id} not found");

            await _userTestRepository.DeleteAsync(entity);
            await _userTestRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Пользователь начинает прохождение теста.
        /// 1) Находим шаблон Test (testId).
        /// 2) По его настройкам (CountOfQuestions, TopicId) вытаскиваем рандомные вопросы.
        /// 3) Создаём UserTest + UserTestQuestions.
        /// 4) Возвращаем UserTestDto с вариантами ответов и текстами вопросов.
        /// </summary>
        public async Task<UserTestDto> StartTestAsync(int testId, string userId)
        {
            // 1) Шаблон теста
            var template = await _testRepository.GetTestByIdAsync(testId);
            if (template == null)
                throw new KeyNotFoundException($"Тест-шаблон с ID={testId} не найден.");

            var count = template.CountOfQuestions;
            var topicId = template.TopicId;

            // 2) Рандомные вопросы с вариантами ответов
            var questions = await _questionRepository.GetRandomQuestionsAsync(count, topicId);

            // 3) Создаём UserTest
            var userTest = new UserTest
            {
                UserId = userId,
                TestId = template.Id,
                TotalQuestions = questions.Count,
                CorrectAnswers = 0, // Не сохраняем, так как считаем динамически
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

            _context.UserTestQuestions.AddRange(userTestQuestions);
            await _context.SaveChangesAsync();

            // Загружаем UserTest с вопросами и их вариантами ответов
            var createdUserTest = await _context.UserTests
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.Question)
                        .ThenInclude(q => q.AnswerOptions)
                .FirstOrDefaultAsync(ut => ut.Id == userTest.Id);

            if (createdUserTest == null)
                throw new Exception("Не удалось загрузить созданный UserTest.");

            // Маппим в DTO
            var dto = new UserTestDto
            {
                Id = createdUserTest.Id,
                TestId = createdUserTest.TestId,
                DateCreated = createdUserTest.DateCreated,
                UserTestQuestions = createdUserTest.UserTestQuestions
                    .Select(utq => new UserTestQuestionDto
                    {
                        Id = utq.Id,
                        QuestionId = utq.QuestionId,
                        QuestionText = utq.Question.Text, // Добавляем текст вопроса
                        AnswerOptions = utq.Question.AnswerOptions
                            .Select(ao => new AnswerOptionDto
                            {
                                Id = ao.Id,
                                Text = ao.Text
                                // Если убрали IsCorrect из DTO, не добавляем
                                // Если нужно оставить:
                                // IsCorrect = ao.IsCorrect
                            }).ToList()
                    }).ToList()
            };

            return dto;
        }
    }
}






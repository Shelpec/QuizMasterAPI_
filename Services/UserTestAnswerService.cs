using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Models.Enums;

namespace QuizMasterAPI.Services
{
    public class UserTestAnswerService : IUserTestAnswerService
    {
        private readonly IUserTestAnswerRepository _userTestAnswerRepo;
        private readonly IUserTestRepository _userTestRepository;
        private readonly IQuestionRepository _questionRepository;
        private readonly QuizDbContext _context;
        private readonly ILogger<UserTestAnswerService> _logger;
        private readonly IMapper _mapper;

        public UserTestAnswerService(
            IUserTestAnswerRepository userTestAnswerRepo,
            IUserTestRepository userTestRepository,
            IQuestionRepository questionRepository,
            QuizDbContext context,
            ILogger<UserTestAnswerService> logger,
            IMapper mapper)
        {
            _userTestAnswerRepo = userTestAnswerRepo;
            _userTestRepository = userTestRepository;
            _questionRepository = questionRepository;
            _context = context;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Сохранение ответов пользователя
        /// </summary>
        public async Task SaveAnswersAsync(int userTestId, List<UserAnswerSubmitDto> answers, string userId)
        {
            _logger.LogInformation("SaveAnswersAsync(UserTestId={Id}, CountAnswers={Count})", userTestId, answers.Count);

            // 1) Проверяем, есть ли UserTest
            var userTest = await _userTestRepository.GetUserTestWithEverythingAsync(userTestId);
            if (userTest == null)
                throw new KeyNotFoundException($"UserTest with ID={userTestId} not found.");

            // 2) Проверяем, принадлежит ли этот тест пользователю
            if (userTest.UserId != userId)
                throw new UnauthorizedAccessException("This UserTest belongs to another user.");

            // 3) Сохраняем ответы
            foreach (var dto in answers)
            {
                // Находим соответствующий UserTestQuestion
                var utq = userTest.UserTestQuestions
                    .FirstOrDefault(x => x.Id == dto.UserTestQuestionId);
                if (utq == null) continue;

                // Чистим предыдущие ответы
                utq.UserTestAnswers.Clear();

                // Определяем тип вопроса
                var question = utq.Question;
                if (question == null) continue;

                // Если вопрос - текстовый (OpenText)
                if (question.QuestionType == QuestionTypeEnum.OpenText)
                {
                    // Сохраняем только текст
                    var userTestAnswer = new UserTestAnswer
                    {
                        UserTestQuestionId = utq.Id,
                        // AnswerOptionId = null,
                        UserTextAnswer = dto.UserTextAnswer
                    };
                    await _userTestAnswerRepo.AddAsync(userTestAnswer);
                }
                else
                {
                    // Иначе сохраняем выбранные варианты
                    foreach (var answerOptionId in dto.SelectedAnswerOptionIds)
                    {
                        var userTestAnswer = new UserTestAnswer
                        {
                            UserTestQuestionId = utq.Id,
                            AnswerOptionId = answerOptionId
                        };
                        await _userTestAnswerRepo.AddAsync(userTestAnswer);
                    }
                }
            }

            // 4) Сохраняем
            await _userTestAnswerRepo.SaveChangesAsync();
        }

        /// <summary>
        /// Проверка ответов
        /// </summary>
        public async Task<TestCheckResultDto> CheckAnswersAsync(int userTestId, string userId)
        {
            _logger.LogInformation("CheckAnswersAsync(UserTestId={Id})", userTestId);

            // Загружаем полный UserTest (с вопросами и ответами)
            var userTest = await _userTestRepository.GetUserTestWithEverythingAsync(userTestId);
            if (userTest == null)
                throw new KeyNotFoundException($"UserTest with ID={userTestId} not found.");

            // Проверка владельца
            if (userTest.UserId != userId)
                throw new UnauthorizedAccessException("Not your test.");

            // Формируем результат
            var result = new TestCheckResultDto
            {
                TotalQuestions = userTest.TotalQuestions,
                CorrectCount = 0,
                Results = new List<QuestionCheckResultDto>()
            };

            // Перебираем все вопросы
            foreach (var utq in userTest.UserTestQuestions)
            {
                var question = utq.Question;
                if (question == null) continue;

                // Список ID выбранных AnswerOption
                var chosenIds = utq.UserTestAnswers
                    .Where(a => a.AnswerOptionId.HasValue)
                    .Select(a => a.AnswerOptionId.Value)
                    .ToList();

                // Список ID правильных AnswerOption
                var correctOptions = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                // Список правильных текстов (если IsCorrect = true)
                var correctTexts = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Text)
                    .ToList();

                bool isCorrect = false;

                // Тексты, которые выбрал пользователь
                var selectedTexts = question.AnswerOptions
                    .Where(a => chosenIds.Contains(a.Id))
                    .Select(a => a.Text)
                    .ToList();

                // Если вопрос - OpenText
                if (question.QuestionType == QuestionTypeEnum.OpenText)
                {
                    var userTextAnswer = utq.UserTestAnswers.FirstOrDefault()?.UserTextAnswer;
                    // Логика проверки - если нужно
                    // Если не нужно проверять, просто считаем false/true как вам удобно
                    // Допустим, считаем всегда true?
                    isCorrect = false;
                    //selectedTexts = new List<string> { userTextAnswer ?? "" };
                    selectedTexts = new List<string>();
                    selectedTexts.Add(userTextAnswer ?? "");
                }
                else if (question.QuestionType == QuestionTypeEnum.Survey)
                {
                    // В опроснике все считаем верно
                    isCorrect = true;
                }
                else
                {
                    // SingleChoice / MultipleChoice
                    isCorrect = !correctOptions.Except(chosenIds).Any()
                                && !chosenIds.Except(correctOptions).Any();
                }

                if (isCorrect) result.CorrectCount++;

                result.Results.Add(new QuestionCheckResultDto
                {
                    QuestionId = question.Id,
                    IsCorrect = isCorrect,
                    CorrectAnswers = correctTexts,
                    SelectedAnswers = selectedTexts
                });
            }

            return result;
        }
    }
}

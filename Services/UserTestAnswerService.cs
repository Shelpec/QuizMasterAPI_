using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Models.Enums;
using QuizMasterAPI.Repositories;
using System.Linq;

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
        /// Сохранение ответов пользователя (учитывая таймер)
        /// </summary>
        public async Task SaveAnswersAsync(int userTestId, List<UserAnswerSubmitDto> answers, string userId)
        {
            _logger.LogInformation("SaveAnswersAsync(UserTestId={Id}, CountAnswers={Count})", userTestId, answers.Count);

            // 1) Загружаем UserTest (со всем, включая UserTestQuestions и Answers)
            var userTest = await _userTestRepository.GetUserTestWithEverythingAsync(userTestId);
            if (userTest == null)
                throw new KeyNotFoundException($"UserTest with ID={userTestId} not found.");

            // 2) Проверяем владельца
            if (userTest.UserId != userId)
                throw new UnauthorizedAccessException("This UserTest belongs to another user.");

            // 3) Если EndTime задано и текущее время > EndTime => тест просрочен
            if (userTest.EndTime.HasValue && DateTime.UtcNow > userTest.EndTime.Value)
            {
                // Сохраняем ответы, если хотите, но помечаем, что тест уже просрочен
                SaveAnswersToUserTest(userTest, answers);

                // Принудительно заканчиваем тест
                userTest.IsPassed = false; // считаем, что не прошёл
                // Можно также вычислить TimeSpentSeconds, если есть StartTime
                if (userTest.StartTime.HasValue)
                {
                    userTest.TimeSpentSeconds = (int)((userTest.EndTime.Value - userTest.StartTime.Value).TotalSeconds);
                }

                await _userTestRepository.UpdateAsync(userTest);
                await _userTestAnswerRepo.SaveChangesAsync();

                throw new InvalidOperationException("Time is over! The test was forcibly finished.");
            }
            else
            {
                // Время ещё не истекло — просто сохраняем ответы
                SaveAnswersToUserTest(userTest, answers);
                await _userTestAnswerRepo.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Вспомогательный метод: добавляет (перезаписывает) ответы в <see cref="UserTestQuestion.UserTestAnswers"/>.
        /// </summary>
        private void SaveAnswersToUserTest(UserTest userTest, List<UserAnswerSubmitDto> answers)
        {
            // Для каждого ответа
            foreach (var dto in answers)
            {
                // Находим соответствующий UserTestQuestion
                var utq = userTest.UserTestQuestions
                    .FirstOrDefault(x => x.Id == dto.UserTestQuestionId);
                if (utq == null) continue;

                // Удаляем прежние ответы
                utq.UserTestAnswers.Clear();

                var question = utq.Question;
                if (question == null) continue;

                // Если вопрос OpenText => сохраняем текст
                if (question.QuestionType == QuestionTypeEnum.OpenText)
                {
                    var userTestAnswer = new UserTestAnswer
                    {
                        UserTestQuestionId = utq.Id,
                        UserTextAnswer = dto.UserTextAnswer
                    };
                    utq.UserTestAnswers.Add(userTestAnswer);
                }
                else
                {
                    // Сохраняем выбранные варианты
                    foreach (var answerOptionId in dto.SelectedAnswerOptionIds)
                    {
                        var userTestAnswer = new UserTestAnswer
                        {
                            UserTestQuestionId = utq.Id,
                            AnswerOptionId = answerOptionId
                        };
                        utq.UserTestAnswers.Add(userTestAnswer);
                    }
                }
            }
        }

        /// <summary>
        /// Проверка ответов (подсчёт правильных и т.д.)
        /// </summary>
        public async Task<TestCheckResultDto> CheckAnswersAsync(int userTestId, string userId)
        {
            _logger.LogInformation("CheckAnswersAsync(UserTestId={Id})", userTestId);

            var userTest = await _userTestRepository.GetUserTestWithEverythingAsync(userTestId);
            if (userTest == null)
                throw new KeyNotFoundException($"UserTest with ID={userTestId} not found.");

            if (userTest.UserId != userId)
                throw new UnauthorizedAccessException("Not your test.");

            // Проверяем — не истёк ли лимит
            if (userTest.EndTime.HasValue && DateTime.UtcNow > userTest.EndTime.Value)
            {
                // Время истекло => завершаем
                userTest.IsPassed = false;
                if (userTest.StartTime.HasValue)
                {
                    userTest.TimeSpentSeconds = (int)((userTest.EndTime.Value - userTest.StartTime.Value).TotalSeconds);
                }

                await _userTestRepository.UpdateAsync(userTest);
                await _context.SaveChangesAsync();

                throw new Exception("Время прохождения теста истекло. Тест завершён.");
            }

            // Иначе проверяем ответы
            var result = await CheckAnswersInternalAsync(userTest);

            // Проставляем IsPassed = true, EndTime = now (если нужно)
            userTest.IsPassed = true;
            if (!userTest.EndTime.HasValue)
            {
                userTest.EndTime = DateTime.UtcNow;
            }
            if (userTest.StartTime.HasValue)
            {
                userTest.TimeSpentSeconds = (int)((userTest.EndTime.Value - userTest.StartTime.Value).TotalSeconds);
            }

            // Сохраняем количество правильных
            userTest.CorrectAnswers = result.CorrectCount;

            await _userTestRepository.UpdateAsync(userTest);
            await _context.SaveChangesAsync();

            return result;
        }

        /// <summary>
        /// Внутренняя логика подсчёта правильных ответов
        /// </summary>
        private async Task<TestCheckResultDto> CheckAnswersInternalAsync(UserTest userTest)
        {
            var result = new TestCheckResultDto
            {
                TotalQuestions = userTest.TotalQuestions,
                CorrectCount = 0,
                Results = new List<QuestionCheckResultDto>()
            };

            // Обходим все вопросы
            foreach (var utq in userTest.UserTestQuestions)
            {
                var question = utq.Question;
                if (question == null) continue;

                // Список ID выбранных вариантов (для Single/MultipleChoice)
                var chosenIds = utq.UserTestAnswers
                    .Where(a => a.AnswerOptionId.HasValue)
                    .Select(a => a.AnswerOptionId!.Value)
                    .ToList();

                // Список ID «правильных» вариантов
                var correctIds = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                bool isCorrect = false;

                // Собираем тексты выбранных вариантов (для подсветки на фронте)
                var selectedTexts = question.AnswerOptions
                    .Where(a => chosenIds.Contains(a.Id))
                    .Select(a => a.Text)
                    .ToList();

                // Логика проверки
                if (question.QuestionType == QuestionTypeEnum.OpenText
                 || question.QuestionType == QuestionTypeEnum.Survey)
                {
                    // Теперь и OpenText, и Survey считаем «всегда правильными»
                    isCorrect = true;
                }
                else
                {
                    // Для SingleChoice / MultipleChoice:
                    // сравниваем chosenIds c correctIds
                    isCorrect = !correctIds.Except(chosenIds).Any()
                                && !chosenIds.Except(correctIds).Any();
                }

                if (isCorrect)
                    result.CorrectCount++;

                // Заполняем результат по текущему вопросу
                result.Results.Add(new QuestionCheckResultDto
                {
                    QuestionId = question.Id,
                    IsCorrect = isCorrect,
                    CorrectAnswers = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Text)
                        .ToList(),
                    SelectedAnswers = selectedTexts
                });
            }

            return result;
        }

    }
}

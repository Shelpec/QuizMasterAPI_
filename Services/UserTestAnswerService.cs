using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

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

            var userTest = await _userTestRepository.GetUserTestWithEverythingAsync(userTestId);
            if (userTest == null)
                throw new KeyNotFoundException($"UserTest with ID={userTestId} not found.");

            if (userTest.UserId != userId)
                throw new UnauthorizedAccessException("This UserTest belongs to another user.");

            // Перебираем, сохраняем
            foreach (var dto in answers)
            {
                var utq = userTest.UserTestQuestions
                    .FirstOrDefault(x => x.Id == dto.UserTestQuestionId);
                if (utq == null) continue;

                // Удаляем все прежние ответы
                utq.UserTestAnswers.Clear();

                // Добавляем новые
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

            await _userTestAnswerRepo.SaveChangesAsync();
        }

        /// <summary>
        /// Проверка ответов. Если test.Topic.IsSurveyTopic == true, считаем опросником
        /// </summary>
        public async Task<TestCheckResultDto> CheckAnswersAsync(int userTestId, string userId)
        {
            _logger.LogInformation("CheckAnswersAsync(UserTestId={Id})", userTestId);

            var userTest = await _userTestRepository.GetUserTestWithEverythingAsync(userTestId);
            if (userTest == null)
                throw new KeyNotFoundException($"UserTest with ID={userTestId} not found.");

            if (userTest.UserId != userId)
                throw new UnauthorizedAccessException("Not your test.");

            var isSurvey = userTest.Test.Topic?.IsSurveyTopic == true; // ← признак опросника

            // Создаём результат
            var result = new TestCheckResultDto
            {
                IsSurvey = isSurvey, // <-- теперь фронт будет видеть, что это опрос
                TotalQuestions = userTest.TotalQuestions,
                CorrectCount = 0, // по умолчанию
                Results = new List<QuestionCheckResultDto>()
            };

            if (isSurvey)
            {
                // Если это ОПРОСНИК
                result.CorrectCount = userTest.TotalQuestions; // всё считаем «верным»

                // Перебираем все вопросы
                foreach (var utq in userTest.UserTestQuestions)
                {
                    var chosenIds = utq.UserTestAnswers.Select(a => a.AnswerOptionId).ToList();
                    var question = utq.Question;
                    if (question == null) continue;

                    // Соберём тексты выбранных вариантов
                    var selectedTexts = question.AnswerOptions
                        .Where(a => chosenIds.Contains(a.Id))
                        .Select(a => a.Text)
                        .ToList();

                    // формируем QuestionCheckResultDto
                    var qDto = new QuestionCheckResultDto
                    {
                        QuestionId = question.Id,
                        IsCorrect = true,  // всё верно
                        CorrectAnswers = new List<string>(), // не отображаем «правильные»,
                                                             // или можно SelectedAnswers = ...
                        SelectedAnswers = selectedTexts
                    };

                    result.Results.Add(qDto);
                }
            }
            else
            {
                // Обычная логика теста
                var questionIds = userTest.UserTestQuestions.Select(q => q.QuestionId).ToList();
                var questions = await _questionRepository.GetQuestionsWithAnswersByIdsAsync(questionIds);

                int correctCount = 0;

                foreach (var utq in userTest.UserTestQuestions)
                {
                    var question = questions.FirstOrDefault(q => q.Id == utq.QuestionId);
                    if (question == null) continue;

                    var userChosen = utq.UserTestAnswers.Select(a => a.AnswerOptionId).ToList();
                    var correctOptions = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .ToList();

                    bool isCorrect = !correctOptions.Except(userChosen).Any()
                                     && !userChosen.Except(correctOptions).Any();

                    if (isCorrect) correctCount++;

                    var detail = new QuestionCheckResultDto
                    {
                        QuestionId = question.Id,
                        IsCorrect = isCorrect,
                        CorrectAnswers = question.AnswerOptions
                            .Where(a => a.IsCorrect)
                            .Select(a => a.Text)
                            .ToList(),
                        SelectedAnswers = question.AnswerOptions
                            .Where(a => userChosen.Contains(a.Id))
                            .Select(a => a.Text)
                            .ToList()
                    };

                    result.Results.Add(detail);
                }

                result.CorrectCount = correctCount;
            }

            return result;
        }
    }
}

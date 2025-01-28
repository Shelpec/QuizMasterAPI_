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


        /// <summary>
        /// Сохраняем ответы пользователя в UserTestAnswers, 
        /// а затем динамически считаем, сколько из них правильные.
        /// </summary>
        public async Task<TestCheckResultDto> SubmitAndCheckAnswersAsync(
            int userTestId,
            List<UserAnswerSubmitDto> answers,
            string userId)
        {
            // 1) Находим UserTest + вопросы
            var userTest = await _context.UserTests
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.UserTestAnswers)
                .FirstOrDefaultAsync(ut => ut.Id == userTestId);

            if (userTest == null)
                throw new KeyNotFoundException($"UserTest с Id={userTestId} не найден.");

            // Проверяем, что этот UserTest принадлежит этому userId
            if (userTest.UserId != userId)
                throw new UnauthorizedAccessException("Этот UserTest принадлежит другому пользователю.");

            // 2) Сохраняем новые записи в UserTestAnswers
            // (Если нужно перезаписывать старые ответы - можно удалить их сначала)
            // На практике часто делают: 
            // userTestQuestion.UserTestAnswers.Clear(); 
            // _context.SaveChanges(); 
            // (Но аккуратнее с EF каскадным удалением.)

            foreach (var submitDto in answers)
            {
                // Ищем, действительно ли этот UserTestQuestion принадлежит к данному UserTest
                var utq = userTest.UserTestQuestions
                    .FirstOrDefault(x => x.Id == submitDto.UserTestQuestionId);
                if (utq == null)
                {
                    // пользователь пытается ответить на вопрос, которого нет в этом UserTest
                    continue;
                }

                // Очищаем предыдущие ответы (если нужна пере-отправка)
                utq.UserTestAnswers.Clear();

                // Добавляем каждую выбранную опцию
                foreach (var optionId in submitDto.SelectedAnswerOptionIds)
                {
                    var newAnswer = new UserTestAnswer
                    {
                        UserTestQuestionId = utq.Id,
                        AnswerOptionId = optionId
                    };
                    _context.UserTestAnswers.Add(newAnswer);
                }
            }

            await _context.SaveChangesAsync();

            // 3) Делаем динамическую проверку
            // Сначала собираем все QuestionId, 
            // чтобы подгрузить правильные варианты
            var questionIds = userTest.UserTestQuestions.Select(q => q.QuestionId).ToList();

            // Подгружаем все вопросы (с их AnswerOptions)
            var questions = await _context.Questions
                .Include(q => q.AnswerOptions)
                .Where(q => questionIds.Contains(q.Id))
                .ToListAsync();

            // Готовим результат
            var result = new TestCheckResultDto
            {
                TotalQuestions = userTest.UserTestQuestions.Count,
                CorrectCount = 0,
                Results = new List<QuestionCheckResultDto>()
            };

            // Считаем, сколько правильных
            foreach (var utq in userTest.UserTestQuestions)
            {
                var question = questions.FirstOrDefault(q => q.Id == utq.QuestionId);
                if (question == null)
                    continue;

                // Что выбрал пользователь (ID вариантов)
                var userChosenOptionIds = utq.UserTestAnswers.Select(a => a.AnswerOptionId).ToList();

                // Какие варианты правильные
                var correctOptionIds = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                bool isCorrect =
                    !correctOptionIds.Except(userChosenOptionIds).Any() &&
                    !userChosenOptionIds.Except(correctOptionIds).Any();

                if (isCorrect)
                    result.CorrectCount++;

                // Детали для ответа
                var questionCheck = new QuestionCheckResultDto
                {
                    QuestionId = question.Id,
                    IsCorrect = isCorrect,
                    CorrectAnswers = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Text)
                        .ToList(),
                    SelectedAnswers = question.AnswerOptions
                        .Where(a => userChosenOptionIds.Contains(a.Id))
                        .Select(a => a.Text)
                        .ToList()
                };
                result.Results.Add(questionCheck);
            }

            return result;
        }
    }
}

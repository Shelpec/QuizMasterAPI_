using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repository;
        private readonly ILogger<QuestionService> _logger;
        private readonly IMapper _mapper;
        private readonly QuizDbContext _ctx; // <-- нужно, чтобы формировать query

        public QuestionService(
            IQuestionRepository repository,
            ILogger<QuestionService> logger,
            IMapper mapper,
            QuizDbContext ctx)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _ctx = ctx;
        }

        /// <summary>
        /// Получить все вопросы (как сущности).
        /// </summary>
        public async Task<IEnumerable<Question>> GetAllQuestions()
        {
            _logger.LogInformation("GetAllQuestions()");
            try
            {
                return await _repository.GetAllQuestionsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAllQuestions()");
                throw;
            }
        }

        /// <summary>
        /// То же самое, но возвращаем QuestionDto
        /// </summary>
        public async Task<IEnumerable<QuestionDto>> GetAllQuestionsDto()
        {
            var allQuestions = await _repository.GetAllQuestionsAsync();
            return allQuestions.Select(q => _mapper.Map<QuestionDto>(q));
        }

        public async Task<Question> GetQuestion(int id)
        {
            _logger.LogInformation("GetQuestion(Id={Id})", id);
            try
            {
                var question = await _repository.GetQuestionByIdAsync(id);
                if (question == null)
                {
                    _logger.LogWarning("Question с ID={Id} не найден", id);
                    throw new KeyNotFoundException($"Question with ID {id} not found.");
                }
                return question;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetQuestion(Id={Id})", id);
                throw;
            }
        }

        public async Task<QuestionDto> GetQuestionDto(int id)
        {
            var question = await GetQuestion(id);
            return _mapper.Map<QuestionDto>(question);
        }

        public async Task<Question> CreateQuestion(CreateQuestionDto questionDto)
        {
            _logger.LogInformation("CreateQuestion(Text={Text})", questionDto.Text);
            try
            {
                // DTO -> Question
                var question = _mapper.Map<Question>(questionDto);

                await _repository.AddAsync(question);
                await _repository.SaveChangesAsync();
                return question;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CreateQuestion(Text={Text})", questionDto.Text);
                throw;
            }
        }

        public async Task<Question> UpdateQuestion(int id, UpdateQuestionDto dto)
        {
            _logger.LogInformation("UpdateQuestion(Id={Id})", id);
            try
            {
                var question = await GetQuestion(id);

                // Маппим UpdateQuestionDto -> question (обновляя поля)
                _mapper.Map(dto, question);

                await _repository.UpdateAsync(question);
                await _repository.SaveChangesAsync();
                return question;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в UpdateQuestion(Id={Id})", id);
                throw;
            }
        }

        public async Task<bool> DeleteQuestion(int id)
        {
            _logger.LogInformation("DeleteQuestion(Id={Id})", id);
            try
            {
                var question = await GetQuestion(id);
                await _repository.DeleteAsync(question);
                await _repository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в DeleteQuestion(Id={Id})", id);
                throw;
            }
        }

        /// <summary>
        /// Проверить один вариант ответа (пример).
        /// </summary>
        public async Task<bool> CheckAnswer(int questionId, int selectedAnswerId)
        {
            _logger.LogInformation("CheckAnswer(QuestionId={Qid}, AnswerId={Aid})", questionId, selectedAnswerId);
            var question = await GetQuestion(questionId);

            var selectedAnswer = question.AnswerOptions.FirstOrDefault(a => a.Id == selectedAnswerId);
            if (selectedAnswer == null)
            {
                _logger.LogWarning("Answer option Id={Aid} не найден в QuestionId={Qid}", selectedAnswerId, questionId);
                throw new ArgumentException("Answer option not found.");
            }

            return selectedAnswer.IsCorrect;
        }

        public async Task<IEnumerable<QuestionDto>> GetRandomQuestions(int count)
        {
            _logger.LogInformation("GetRandomQuestions(count={Count})", count);
            try
            {
                var questions = await _repository.GetRandomQuestionsAsync(count);
                return _mapper.Map<IEnumerable<QuestionDto>>(questions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetRandomQuestions(count={Count})", count);
                throw;
            }
        }

        /// <summary>
        /// Пример проверки нескольких ответов.
        /// </summary>
        public async Task<AnswerValidationResponseDto> CheckAnswers(List<AnswerValidationDto> answers)
        {
            _logger.LogInformation("CheckAnswers для {Count} вопросов", answers.Count);
            try
            {
                // Собираем IDs вопросов
                var questionIds = answers.Select(a => a.QuestionId).ToList();
                var questions = await _repository.GetQuestionsWithAnswersByIdsAsync(questionIds);

                var questionDtos = _mapper.Map<List<QuestionDto>>(questions);
                var response = new AnswerValidationResponseDto();

                foreach (var answer in answers)
                {
                    var questionDto = questionDtos.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (questionDto == null)
                        throw new KeyNotFoundException($"Question with ID {answer.QuestionId} not found");

                    // ID правильных ответов
                    var correctIds = questionDto.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .ToList();

                    // Проверяем, совпадает ли выбранный набор
                    var isCorrect =
                        !correctIds.Except(answer.SelectedAnswerIds).Any() &&
                        !answer.SelectedAnswerIds.Except(correctIds).Any();

                    if (isCorrect) response.CorrectCount++;

                    // Формируем подробный результат (опционально)
                    response.Results.Add(new QuestionValidationResultDto
                    {
                        QuestionText = questionDto.Text,
                        CorrectAnswers = questionDto.AnswerOptions
                            .Where(a => a.IsCorrect)
                            .Select(a => a.Text)
                            .ToList(),
                        SelectedAnswers = questionDto.AnswerOptions
                            .Where(a => answer.SelectedAnswerIds.Contains(a.Id))
                            .Select(a => a.Text)
                            .ToList()
                    });
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CheckAnswers");
                throw;
            }
        }

        // Реализация пагинации для вопросов
        public async Task<PaginatedResponse<QuestionDto>> GetAllQuestionsPaginatedAsync(int page, int pageSize)
        {
            _logger.LogInformation("GetAllQuestionsPaginatedAsync(page={Page}, pageSize={PageSize})", page, pageSize);

            var query = _ctx.Questions
                .Include(q => q.AnswerOptions)
                .Include(q => q.Topic)
                .AsQueryable();

            var totalItems = await query.CountAsync();

            var skip = (page - 1) * pageSize;
            var questions = await query
                .OrderBy(q => q.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var dtos = questions.Select(q => _mapper.Map<QuestionDto>(q)).ToList();

            return new PaginatedResponse<QuestionDto>
            {
                Items = dtos,
                TotalItems = totalItems,
                TotalPages = totalPages,
                CurrentPage = page,
                PageSize = pageSize
            };
        }
    }
}

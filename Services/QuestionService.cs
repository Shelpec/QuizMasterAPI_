using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Models.Enums;

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
        public async Task<AnswerValidationResponseDto> CheckAnswers(List<CheckAnswerDto> answers)
        {
            _logger.LogInformation("CheckAnswers для {Count} вопросов", answers.Count);
            try
            {
                // Приведение CheckAnswerDto к AnswerValidationDto
                var answerDtos = answers.Select(a => new AnswerValidationDto
                {
                    QuestionId = a.QuestionId,
                    SelectedAnswerIds = a.SelectedAnswerIds,
                    UserTextAnswer = a.UserTextAnswer
                }).ToList();

                var questionIds = answerDtos.Select(a => a.QuestionId).ToList();
                var questions = await _repository.GetQuestionsWithAnswersByIdsAsync(questionIds);
                var response = new AnswerValidationResponseDto();

                foreach (var answer in answerDtos)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question == null)
                        throw new KeyNotFoundException($"Question with ID {answer.QuestionId} not found");

                    var correctOptions = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .ToList();

                    var correctTexts = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Text)
                        .ToList();

                    bool isCorrect = false;
                    var selectedTexts = question.AnswerOptions
                        .Where(a => answer.SelectedAnswerIds.Contains(a.Id))
                        .Select(a => a.Text)
                        .ToList();

                    // Логика для разных типов вопросов
                    if (question.QuestionType == QuestionTypeEnum.OpenText)
                    {
                        isCorrect = string.Equals(answer.UserTextAnswer?.Trim(), question.CorrectTextAnswer?.Trim(), StringComparison.OrdinalIgnoreCase);
                        selectedTexts = new List<string> { answer.UserTextAnswer ?? "" };
                    }
                    else if (question.QuestionType == QuestionTypeEnum.Survey)
                    {
                        isCorrect = true; // Все ответы в опроснике правильные
                    }
                    else
                    {
                        isCorrect = !correctOptions.Except(answer.SelectedAnswerIds).Any() &&
                                    !answer.SelectedAnswerIds.Except(correctOptions).Any();
                    }

                    if (isCorrect) response.CorrectCount++;

                    response.Results.Add(new QuestionValidationResultDto
                    {
                        QuestionText = question.Text,
                        IsCorrect = isCorrect,
                        CorrectAnswers = correctTexts,
                        SelectedAnswers = selectedTexts
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

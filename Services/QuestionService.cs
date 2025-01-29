using AutoMapper;
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

        public QuestionService(IQuestionRepository repository, ILogger<QuestionService> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Получить все вопросы (возвращаем сущности Question).
        /// Если нужно DTO, сделайте _mapper.Map<IEnumerable<QuestionDto>>(...).
        /// </summary>
        public async Task<IEnumerable<Question>> GetAllQuestions()
        {
            _logger.LogInformation("GetAllQuestions()");
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в GetAllQuestions()");
                throw;
            }
        }

        /// <summary>
        /// Получить сущность Question по Id
        /// </summary>
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

        /// <summary>
        /// Получить QuestionDto (маппим из Question).
        /// </summary>
        public async Task<QuestionDto> GetQuestionDto(int id)
        {
            var question = await GetQuestion(id);
            // AutoMapper: Question -> QuestionDto
            return _mapper.Map<QuestionDto>(question);
        }

        /// <summary>
        /// Создать вопрос (CreateQuestionDto -> Question).
        /// </summary>
        public async Task<Question> CreateQuestion(CreateQuestionDto questionDto)
        {
            _logger.LogInformation("CreateQuestion(Text={Text})", questionDto.Text);
            try
            {
                // Маппим DTO -> сущность Question
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

        /// <summary>
        /// Обновить вопрос (UpdateQuestionDto -> Question).
        /// </summary>
        public async Task<Question> UpdateQuestion(int id, UpdateQuestionDto dto)
        {
            _logger.LogInformation("UpdateQuestion(Id={Id})", id);
            try
            {
                var question = await GetQuestion(id);

                // Обновляем поля question из dto
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

        /// <summary>
        /// Удалить вопрос.
        /// </summary>
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
        /// Проверить один вариант ответа
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

        public async Task<AnswerValidationResponseDto> CheckAnswers(List<AnswerValidationDto> answers)
        {
            _logger.LogInformation("CheckAnswers для {Count} вопросов", answers.Count);
            try
            {
                // Загружаем сущности Question
                var questionIds = answers.Select(a => a.QuestionId).ToList();
                var questions = await _repository.GetQuestionsWithAnswersByIdsAsync(questionIds);

                var questionDtos = _mapper.Map<List<QuestionDto>>(questions);

                var response = new AnswerValidationResponseDto();

                foreach (var answer in answers)
                {
                    // Ищем QuestionDto
                    var questionDto = questionDtos.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (questionDto == null)
                        throw new KeyNotFoundException($"Question with ID {answer.QuestionId} not found");

                    // Собираем ID правильных ответов
                    var correctAnswerIds = questionDto.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .ToList();

                    // Сравниваем
                    var isCorrect =
                        !correctAnswerIds.Except(answer.SelectedAnswerIds).Any() &&
                        !answer.SelectedAnswerIds.Except(correctAnswerIds).Any();

                    // Заполняем результаты
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

                    if (isCorrect)
                    {
                        response.CorrectCount++;
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка в CheckAnswers");
                throw;
            }
        }
    }
}

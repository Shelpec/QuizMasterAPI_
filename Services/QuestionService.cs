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

        public QuestionService(IQuestionRepository repository, ILogger<QuestionService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

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

        private QuestionDto MapToDto(Question question)
        {
            return new QuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                HasMultipleCorrectAnswers = question.AnswerOptions.Count(a => a.IsCorrect) > 1,
                AnswerOptions = question.AnswerOptions.Select(a => new AnswerOptionDto
                {
                    Id = a.Id,
                    Text = a.Text,
                    IsCorrect = a.IsCorrect
                }).ToList()
            };
        }

        public async Task<QuestionDto> GetQuestionDto(int id)
        {
            var question = await GetQuestion(id);
            return MapToDto(question);
        }

        public async Task<Question> CreateQuestion(CreateQuestionDto questionDto)
        {
            _logger.LogInformation("CreateQuestion(Text={Text})", questionDto.Text);
            try
            {
                var question = new Question
                {
                    Text = questionDto.Text,
                    TopicId = questionDto.TopicId,
                    AnswerOptions = questionDto.AnswerOptions
                        .Select(a => new AnswerOption { Text = a.Text, IsCorrect = a.IsCorrect })
                        .ToList()
                };

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

        public async Task<Question> UpdateQuestion(int id, UpdateQuestionDto questionDto)
        {
            _logger.LogInformation("UpdateQuestion(Id={Id})", id);
            try
            {
                var question = await GetQuestion(id);
                question.Text = questionDto.Text;
                question.AnswerOptions = questionDto.AnswerOptions
                    .Select(a => new AnswerOption
                    {
                        Text = a.Text,
                        IsCorrect = a.IsCorrect,
                        QuestionId = id
                    })
                    .ToList();

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
                return questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    HasMultipleCorrectAnswers = q.AnswerOptions.Count(a => a.IsCorrect) > 1,
                    AnswerOptions = q.AnswerOptions.Select(a => new AnswerOptionDto
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect
                    }).ToList()
                });
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
                var questionIds = answers.Select(a => a.QuestionId).ToList();
                var questions = await _repository.GetQuestionsWithAnswersByIdsAsync(questionIds);

                var response = new AnswerValidationResponseDto();

                foreach (var answer in answers)
                {
                    var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                    if (question == null)
                    {
                        throw new KeyNotFoundException($"Question with ID {answer.QuestionId} not found");
                    }

                    var correctAnswers = question.AnswerOptions
                        .Where(a => a.IsCorrect)
                        .Select(a => a.Id)
                        .ToList();

                    var isCorrect =
                        !correctAnswers.Except(answer.SelectedAnswerIds).Any() &&
                        !answer.SelectedAnswerIds.Except(correctAnswers).Any();

                    response.Results.Add(new QuestionValidationResultDto
                    {
                        QuestionText = question.Text,
                        CorrectAnswers = question.AnswerOptions
                            .Where(a => a.IsCorrect)
                            .Select(a => a.Text)
                            .ToList(),
                        SelectedAnswers = question.AnswerOptions
                            .Where(a => answer.SelectedAnswerIds.Contains(a.Id))
                            .Select(a => a.Text)
                            .ToList()
                    });

                    if (isCorrect)
                    {
                        response.Score += (correctAnswers.Count > 1) ? 2 : 2;
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

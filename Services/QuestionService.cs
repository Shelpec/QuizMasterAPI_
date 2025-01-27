using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository _repository;

        public QuestionService(IQuestionRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Получить все вопросы (можно просто _repository.GetAllAsync())
        /// </summary>
        public async Task<IEnumerable<Question>> GetAllQuestions()
        {
            return await _repository.GetAllAsync();
        }

        /// <summary>
        /// Получить конкретный вопрос по ID
        /// </summary>
        public async Task<Question> GetQuestion(int id)
        {
            var question = await _repository.GetQuestionByIdAsync(id);
            if (question == null)
            {
                throw new KeyNotFoundException($"Question with ID {id} not found.");
            }
            return question;
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
            var question = await GetQuestion(id); // Вызов с подгруженными AnswerOptions
            return MapToDto(question);
        }




        /// <summary>
        /// Создать новый вопрос с вариантами
        /// </summary>
        public async Task<Question> CreateQuestion(CreateQuestionDto questionDto)
        {
            var question = new Question
            {
                Text = questionDto.Text,
                TopicId = questionDto.TopicId, // <-- Устанавливаем тему
                AnswerOptions = questionDto.AnswerOptions
                    .Select(a => new AnswerOption { Text = a.Text, IsCorrect = a.IsCorrect })
                    .ToList()
            };

            await _repository.AddAsync(question);
            await _repository.SaveChangesAsync();
            return question;
        }


        /// <summary>
        /// Обновить вопрос
        /// </summary>
        public async Task<Question> UpdateQuestion(int id, UpdateQuestionDto questionDto)
        {
            // 1. Находим существующий вопрос
            var question = await GetQuestion(id);

            // 2. Меняем поля
            question.Text = questionDto.Text;
            question.AnswerOptions = questionDto.AnswerOptions
                .Select(a => new AnswerOption
                {
                    Text = a.Text,
                    IsCorrect = a.IsCorrect,
                    QuestionId = id
                })
                .ToList();

            // 3. Вызываем UpdateAsync + SaveChangesAsync
            await _repository.UpdateAsync(question);
            await _repository.SaveChangesAsync();

            return question;
        }

        /// <summary>
        /// Удалить вопрос
        /// </summary>
        public async Task<bool> DeleteQuestion(int id)
        {
            var question = await GetQuestion(id);
            await _repository.DeleteAsync(question);
            await _repository.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Проверить один вариант ответа
        /// </summary>
        public async Task<bool> CheckAnswer(int questionId, int selectedAnswerId)
        {
            var question = await GetQuestion(questionId);

            var selectedAnswer = question.AnswerOptions.FirstOrDefault(a => a.Id == selectedAnswerId);
            if (selectedAnswer == null)
            {
                throw new ArgumentException("Answer option not found.");
            }

            return selectedAnswer.IsCorrect;
        }

        /// <summary>
        /// Получить случайные вопросы
        /// </summary>
        public async Task<IEnumerable<QuestionDto>> GetRandomQuestions(int count)
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


        /// <summary>
        /// Проверить несколько ответов по разным вопросам
        /// </summary>
        public async Task<AnswerValidationResponseDto> CheckAnswers(List<AnswerValidationDto> answers)
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

                // Детальная информация
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
                    // Простой подсчёт очков:
                    // Если несколько правильных ответов, даём 2 очка, 
                    // если один — тоже 2 (по коду).
                    response.Score += (correctAnswers.Count > 1) ? 2 : 2;
                    response.CorrectCount++;
                }
            }

            return response;
        }
    }
}

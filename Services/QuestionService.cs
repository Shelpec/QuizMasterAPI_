using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
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

        public async Task<IEnumerable<Question>> GetAllQuestions()
        {
            return await _repository.GetAllQuestionsAsync();
        }

        public async Task<Question> GetQuestion(int id)
        {
            var question = await _repository.GetQuestionByIdAsync(id);
            if(question == null)
            {
                throw new KeyNotFoundException($"Question with ID {id} not found.");
            }
            return question;
        }

        public async Task<Question> CreateQuestion(CreateQuestionDto questionDto)
        {
            var question = new Question
            {
                Text = questionDto.Text,
                AnswerOptions = questionDto.AnswerOptions
                .Select(a => new AnswerOption { Text = a.Text, IsCorrect = a.IsCorrect })
                .ToList()
            };
            await _repository.AddQuestionAsync(question);
            return question;
        }

        public async Task<Question> UpdateQuestion(int id, UpdateQuestionDto questionDto)
        {
            var question = await GetQuestion(id);
            question.Text = questionDto.Text;
            question.AnswerOptions = questionDto.AnswerOptions
                .Select(a => new AnswerOption 
                {
                Text = a.Text,
                IsCorrect = a.IsCorrect,
                QuestionId = id}
                ).ToList();
            await _repository.UpdateQuestionAsync(question);
            return question;
        }

        public async Task<bool> DeleteQuestion(int id)
        {
            var question = await GetQuestion(id);
            await _repository.DeleteQuestionAsync(question);
            return true;
        }

        public async Task<bool> CheckAnswer(int questionId, int selectedAnswerId)
        {
            var question = await GetQuestion(questionId);
            var selectedAnser = question.AnswerOptions.FirstOrDefault(a => a.Id == selectedAnswerId);
            if(selectedAnser != null)
            {
                throw new ArgumentException("Anser option not found.");
            }
            return selectedAnser.IsCorrect;
        }

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
                    throw new KeyNotFoundException($"Question with ID {answer.QuestionId} not found.");
                }

                var correctAnswers = question.AnswerOptions
                    .Where(a => a.IsCorrect)
                    .Select(a => a.Id)
                    .ToList();

                var isCorrect = !correctAnswers.Except(answer.SelectedAnswerIds).Any() &&
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
                    response.CorrectCount++;
                }
            }

            return response;
        }

    }
}

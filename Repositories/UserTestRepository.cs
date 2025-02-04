using Microsoft.EntityFrameworkCore;
using QuizMasterAPI.Data;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Repositories
{
    public class UserTestRepository : GenericRepository<UserTest>, IUserTestRepository
    {
        private readonly QuizDbContext _ctx;

        public UserTestRepository(QuizDbContext context) : base(context)
        {
            _ctx = context;
        }

        public async Task<UserTest?> GetUserTestWithQuestionsAsync(int userTestId)
        {
            return await _ctx.UserTests

                // Чтобы подгрузить UserTestQuestions
                .Include(ut => ut.UserTestQuestions)
                    // Чтобы подгрузить сами ответы пользователя:
                    .ThenInclude(utq => utq.UserTestAnswers)

                // Параллельно — загружаем Question и AnswerOptions
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.Question)
                        .ThenInclude(q => q.AnswerOptions)

                .FirstOrDefaultAsync(ut => ut.Id == userTestId);
        }

        public async Task<UserTest?> GetUserTestAsync(int userTestId)
        {
            return await _ctx.UserTests
                .FirstOrDefaultAsync(ut => ut.Id == userTestId);
        }

        // Грузим всё: User, Test(+Topic), UserTestQuestions(+Answers), и Question(+AnswerOptions)
        public async Task<UserTest?> GetUserTestFullAsync(int userTestId)
        {
            return await _ctx.UserTests
                .Include(ut => ut.User)                  // Инфо о пользователе
                .Include(ut => ut.Test)                  // Инфо о тесте
                    .ThenInclude(t => t.Topic)           // Топик теста
                .Include(ut => ut.UserTestQuestions)     // вопросы
                    .ThenInclude(utq => utq.UserTestAnswers) // ответы пользователя
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.Question)
                        .ThenInclude(q => q.AnswerOptions)   // варианты ответа
                .FirstOrDefaultAsync(ut => ut.Id == userTestId);
        }

        // UserTestRepository.cs
        public async Task<List<UserTest>> GetAllUserTestFullAsync()
        {
            // Подгружаем всё то же самое, что в GetUserTestFullAsync, но для всех:
            return await _ctx.UserTests
                .Include(ut => ut.User)
                .Include(ut => ut.Test).ThenInclude(t => t.Topic)
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.UserTestAnswers)
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.Question)
                        .ThenInclude(q => q.AnswerOptions)
                .ToListAsync();
        }

        public async Task<List<UserTest>> GetAllByUserEmailFullAsync(string email)
        {
            // Аналогично, но делаем Join по пользователю
            // Можно либо так:
            return await _ctx.UserTests
                .Include(ut => ut.User)
                .Include(ut => ut.Test).ThenInclude(t => t.Topic)
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.UserTestAnswers)
                .Include(ut => ut.UserTestQuestions)
                    .ThenInclude(utq => utq.Question)
                        .ThenInclude(q => q.AnswerOptions)
                .Where(ut => ut.User.Email == email)
                .ToListAsync();
        }

    }
}

using System.Linq.Expressions;

namespace QuizMasterAPI.Interfaces
{
    /// <summary>
    /// Базовый интерфейс репозитория, обеспечивающий операции CRUD для любой сущности.
    /// </summary>
    /// <typeparam name="T">Сущность EF</typeparam>
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);

        Task SaveChangesAsync();
    }
}

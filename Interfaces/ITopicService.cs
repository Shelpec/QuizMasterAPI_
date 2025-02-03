// Interfaces/ITopicService.cs
using QuizMasterAPI.Models.DTOs;

namespace QuizMasterAPI.Interfaces
{
    public interface ITopicService
    {
        Task<IEnumerable<TopicDto>> GetAllTopicsAsync();
        Task<TopicDto> GetTopicByIdAsync(int id);
        Task<TopicDto> CreateTopicAsync(CreateTopicDto dto);
        Task<TopicDto> UpdateTopicAsync(int id, UpdateTopicDto dto);
        Task<bool> DeleteTopicAsync(int id);
    }
}

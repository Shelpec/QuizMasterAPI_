// Services/TopicService.cs
using AutoMapper;
using Microsoft.Extensions.Logging;
using QuizMasterAPI.Interfaces;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.Services
{
    public class TopicService : ITopicService
    {
        private readonly ITopicRepository _repo;
        private readonly IMapper _mapper;
        private readonly ILogger<TopicService> _logger;

        public TopicService(ITopicRepository repo, IMapper mapper, ILogger<TopicService> logger)
        {
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<TopicDto>> GetAllTopicsAsync()
        {
            var allTopics = await _repo.GetAllTopicsAsync();
            return _mapper.Map<IEnumerable<TopicDto>>(allTopics);
        }

        public async Task<TopicDto> GetTopicByIdAsync(int id)
        {
            var topic = await _repo.GetTopicByIdAsync(id);
            if (topic == null)
                throw new KeyNotFoundException($"Topic with ID={id} not found");

            return _mapper.Map<TopicDto>(topic);
        }
        public async Task<TopicDto> CreateTopicAsync(CreateTopicDto dto)
        {
            var topic = new Topic
            {
                Name = dto.Name,
                IsSurveyTopic = dto.IsSurveyTopic
            };
            await _repo.AddAsync(topic);
            await _repo.SaveChangesAsync();
            return _mapper.Map<TopicDto>(topic);
        }

        public async Task<TopicDto> UpdateTopicAsync(int id, UpdateTopicDto dto)
        {
            var topic = await _repo.GetTopicByIdAsync(id);
            if (topic == null)
                throw new KeyNotFoundException($"Topic with ID={id} not found");

            topic.Name = dto.Name;
            topic.IsSurveyTopic = dto.IsSurveyTopic;

            await _repo.UpdateAsync(topic);
            await _repo.SaveChangesAsync();
            return _mapper.Map<TopicDto>(topic);
        }

        public async Task<bool> DeleteTopicAsync(int id)
        {
            var topic = await _repo.GetTopicByIdAsync(id);
            if (topic == null)
                throw new KeyNotFoundException($"Topic with ID={id} not found");

            await _repo.DeleteAsync(topic);
            await _repo.SaveChangesAsync();
            return true;
        }
    }
}

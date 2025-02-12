using AutoMapper;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;
using QuizMasterAPI.Models.Enums;

namespace QuizMasterAPI.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<AnswerOption, AnswerOptionDto>().ReverseMap();


            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.Topic != null ? src.Topic.Name : null))
                .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.AnswerOptions))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType))
                .ForMember(dest => dest.CorrectTextAnswer, opt => opt.MapFrom(src => src.CorrectTextAnswer))
                .ReverseMap();

            CreateMap<CreateQuestionDto, Question>()
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType))
                .ForMember(dest => dest.CorrectTextAnswer, opt => opt.MapFrom(src => src.CorrectTextAnswer))
                .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.AnswerOptions));

            CreateMap<UpdateQuestionDto, Question>()
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType))
                .ForMember(dest => dest.CorrectTextAnswer, opt => opt.MapFrom(src => src.CorrectTextAnswer))
                .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.AnswerOptions));

            CreateMap<Question, QuestionHistoryDto>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Text))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.QuestionType)) // ✅ Добавлено!
                .ForMember(dest => dest.CorrectTextAnswer, opt => opt.MapFrom(src => src.QuestionType == QuestionTypeEnum.OpenText ? src.CorrectTextAnswer : null))
                .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.AnswerOptions))
                .ReverseMap();

            CreateMap<UserTest, UserTestDto>()
                .ForMember(dest => dest.UserTestQuestions, opt => opt.MapFrom(src => src.UserTestQuestions));

            CreateMap<UserTestQuestion, UserTestQuestionDto>()
                .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.Text))
                .ForMember(dest => dest.QuestionType, opt => opt.MapFrom(src => src.Question.QuestionType)) // ✅ Добавлено!
                .ForMember(dest => dest.CorrectTextAnswer, opt => opt.MapFrom(src => src.Question.QuestionType == QuestionTypeEnum.OpenText ? src.Question.CorrectTextAnswer : null))
                .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.Question.AnswerOptions));

            CreateMap<Test, TestDto>()
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.Topic != null ? src.Topic.Name : null))
                .ReverseMap();

            CreateMap<TestQuestion, TestQuestionDto>().ReverseMap();

            CreateMap<UserTest, UserTestDto>().ReverseMap();


            CreateMap<Topic, TopicDto>().ReverseMap();
            CreateMap<CreateTopicDto, Topic>().ReverseMap();
            CreateMap<UpdateTopicDto, Topic>().ReverseMap();

        }
    }
}

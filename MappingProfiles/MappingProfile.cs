using AutoMapper;
using QuizMasterAPI.Models.DTOs;
using QuizMasterAPI.Models.Entities;

namespace QuizMasterAPI.MappingProfiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            // Mapping AnswerOption -> AnswerOptionDto
            CreateMap<AnswerOption, AnswerOptionDto>()
                .ReverseMap();

            // Question -> QuestionDto (если нужно)
            CreateMap<Question, QuestionDto>()
                .ForMember(dest => dest.TopicName, opt => opt.MapFrom(src => src.Topic.Name))
                .ReverseMap();

            // =========================================
            // Важно: Говорим, что при маппинге UserTestQuestion -> UserTestQuestionDto
            // поле UserTestQuestionDto.answerOptions нужно взять из
            // UserTestQuestion.Question.AnswerOptions
            // =========================================
            CreateMap<UserTestQuestion, UserTestQuestionDto>()
                .ForMember(dest => dest.QuestionText,
                    opt => opt.MapFrom(src => src.Question.Text))
                .ForMember(dest => dest.AnswerOptions,
                    opt => opt.MapFrom(src => src.Question.AnswerOptions));
            CreateMap<AnswerOption, AnswerOptionDto>();


            // UserTest -> UserTestDto
            CreateMap<UserTest, UserTestDto>()
                .ReverseMap();

            //// Question <-> QuestionDto
            //CreateMap<Question, QuestionDto>()
            //    .ForMember(dest => dest.HasMultipleCorrectAnswers,
            //        opt => opt.MapFrom(src => src.AnswerOptions.Count(a => a.IsCorrect) > 1))
            //    .ReverseMap();
            //// ReverseMap() значит можно и QuestionDto -> Question.

            // Test <-> TestDto
            CreateMap<Test, TestDto>().ReverseMap();

            // UserTest <-> UserTestDto
            CreateMap<UserTest, UserTestDto>().ReverseMap();

            //// UserTestQuestion <-> UserTestQuestionDto
            //CreateMap<UserTestQuestion, UserTestQuestionDto>()
            //    // QuestionText берем из Question.Text
            //    .ForMember(dest => dest.QuestionText, opt => opt.MapFrom(src => src.Question.Text))
            //    .ReverseMap();

            CreateMap<CreateQuestionDto, Question>()
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
                .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.AnswerOptions));

            CreateMap<UpdateQuestionDto, Question>()
                .ForMember(dest => dest.TopicId, opt => opt.MapFrom(src => src.TopicId))
                .ForMember(dest => dest.AnswerOptions, opt => opt.MapFrom(src => src.AnswerOptions));

        }
    }
}

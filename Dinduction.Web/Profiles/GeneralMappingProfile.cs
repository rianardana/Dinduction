// Dinduction.Web/Profiles/GeneralMappingProfile.cs
using AutoMapper;
using Dinduction.Domain.Entities;
using Dinduction.Web.Models;

namespace Dinduction.Web.Profiles;

public class GeneralMappingProfile : Profile
{
    public GeneralMappingProfile()
    {
        CreateMap<Role, RoleVM>();
        CreateMap<RoleVM, Role>();

        CreateMap<User, UserVM>();
        CreateMap<UserVM, User>();

        CreateMap<Section, SectionVM>();
        CreateMap<SectionVM, Section>();

        CreateMap<Trainer, TrainerVM>()
                .ForMember(dest => dest.SectionName,
                    opt => opt.MapFrom(src => src.Section != null ? src.Section.SectionName : string.Empty))
                .ForMember(dest => dest.UserName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))
                .ForMember(dest => dest.TrainerName,
                    opt => opt.MapFrom(src => src.User != null ? src.User.EmployeeName : string.Empty));
        CreateMap<TrainerVM, Trainer>();

        CreateMap<MasterTraining, MasterTrainingVM>();
        CreateMap<MasterTrainingVM, MasterTraining>();

        CreateMap<ParticipantUser, ParticipantUserVM>()
        .ForMember(dest => dest.TrainingName, 
            opt => opt.MapFrom(src => src.Training != null ? src.Training.TrainingName : null))
        .ForMember(dest => dest.TrainerName, 
            opt => opt.MapFrom(src => src.Trainer != null && src.Trainer.User != null 
                ? src.Trainer.User.EmployeeName 
                : null))
        .ForMember(dest => dest.UserName, 
            opt => opt.MapFrom(src => src.User != null ? src.User.UserName : null))
        .ForMember(dest => dest.EmployeeName, 
            opt => opt.MapFrom(src => src.User != null ? src.User.EmployeeName : null))
        .ForMember(dest => dest.Department, 
            opt => opt.MapFrom(src => src.User != null ? src.User.Department : null));

        CreateMap<ParticipantUserVM, ParticipantUser>();

        CreateMap<Question, QuestionVM>()
    .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.Training != null && src.Training.Section != null ? src.Training.Section.SectionName : null))
    .ForMember(dest => dest.TrainingName, opt => opt.MapFrom(src => src.Training != null ? src.Training.TrainingName : null));

        CreateMap<QuestionVM, Question>();

        
        CreateMap<Answer, AnswerVM>();
        CreateMap<AnswerVM, Answer>();

        CreateMap<VQuestionAnswer, ViewQuestionAnswerVM>();
        CreateMap<RecordTrainingVM, RecordTraining>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.Score, opt => opt.Ignore());
    }
}
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

        CreateMap<User, UserVM>()
        .ForMember(dest => dest.CurrentPassword, opt => opt.Ignore())
        .ForMember(dest => dest.NewPassword, opt => opt.Ignore())
        .ForMember(dest => dest.ConfNewPassword, opt => opt.Ignore())
        .ForMember(dest => dest.ListRole, opt => opt.Ignore())
        .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role != null ? src.Role.RoleName : string.Empty))
        .ForMember(dest => dest.TrainingType, opt => opt.MapFrom(src => src.TrainingType ?? "I"));;

    CreateMap<UserVM, User>()
        .ForMember(dest => dest.Password, opt => opt.Ignore()) 
        .ForMember(dest => dest.Role, opt => opt.Ignore())
        .ForMember(dest => dest.TrainingType, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.TrainingType) ? src.TrainingType : "I"));

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
            opt => opt.MapFrom(src => src.User != null ? src.User.Department : null))
        .ForMember(dest => dest.TrainingType, 
        opt => opt.MapFrom(src => src.User != null ? src.User.TrainingType : null));;

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

        CreateMap<VRecordMaster, ViewRecordMasterVM>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.ParticipantId, opt => opt.MapFrom(src => src.ParticipantId))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.EmployeeName))
            .ForMember(dest => dest.TrainingId, opt => opt.MapFrom(src => src.TrainingId))
            .ForMember(dest => dest.TrainingName, opt => opt.MapFrom(src => src.TrainingName))
            .ForMember(dest => dest.IsTrue, opt => opt.MapFrom(src => src.IsTrue))
            .ForMember(dest => dest.RecordDate, opt => opt.MapFrom(src => src.RecordDate))
            .ForMember(dest => dest.QuizNumber, opt => opt.MapFrom(src => src.QuizNumber))
            .ForMember(dest => dest.TrainerId, opt => opt.MapFrom(src => src.TrainerId))
            .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.Score))
            .ForMember(dest => dest.TotalTrainingCount, opt => opt.Ignore()) 
            .ForMember(dest => dest.CompletedTrainingCount, opt => opt.Ignore()) 
            .ForMember(dest => dest.Failed, opt => opt.Ignore())
            .ForMember(dest => dest.TrainingType, opt => opt.MapFrom(src => src.TrainingType))
            .ForMember(dest => dest.StepType, opt => opt.MapFrom(src => src.StepType));; 

        CreateMap<LearningMaterial, LearningMaterialVM>()
            .ForMember(d => d.TrainingName, opt => opt.MapFrom(s => s.Training != null ? s.Training.TrainingName : null))
            .ForMember(d => d.CreatedDate, opt => opt.MapFrom(s => DateTime.Now)); 
            
        CreateMap<LearningMaterialUploadVM, LearningMaterial>()
            .ForMember(d => d.FilePath, opt => opt.Ignore()); 

        // === USER SIDE ===
        CreateMap<LearningMaterial, UserLearningMaterialVM>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => 
                !string.IsNullOrEmpty(s.FilePath) ? Path.GetFileNameWithoutExtension(s.FilePath) : "Untitled"));
                
        CreateMap<MasterTraining, LearningStudyVM>();
        CreateMap<RefreshComparisonDto, RefreshComparisonVM>();
        CreateMap<RefreshDetailDto, RefreshDetailVM>();
        CreateMap<RefreshAttendanceDto, RefreshAttendanceVM>();

// ✅ MINIMAL SAFE MAPPING - Cuma map property yang pasti ada
CreateMap<VRecordResult, ViewRecordResultVM>()
    // String properties dengan null-safe fallback
    .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => 
        !string.IsNullOrEmpty(src.EmployeeName) ? src.EmployeeName : "-"))
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => 
        !string.IsNullOrEmpty(src.UserName) ? src.UserName : "-"))
    .ForMember(dest => dest.TrainingName, opt => opt.MapFrom(src => 
        !string.IsNullOrEmpty(src.TrainingName) ? src.TrainingName : "-"))
    
    // Date properties: Langsung assign (karena kemungkinan sudah DateTime?)
    // Kalau error, berarti property-nya nggak ada → akan di-ignore otomatis
    .ForMember(dest => dest.FormDateRegistration, opt => opt.MapFrom(src => 
        src.FormDateRegistration.HasValue ? src.FormDateRegistration.Value : DateTime.Now))
    .ForMember(dest => dest.TrainingDate, opt => opt.MapFrom(src => 
        src.TrainingDate.HasValue ? src.TrainingDate.Value : DateTime.Now))
    
    // Numeric properties
    .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.Score))
    .ForMember(dest => dest.ParticipantId, opt => opt.MapFrom(src => src.ParticipantId))
    .ForMember(dest => dest.TrainingId, opt => opt.MapFrom(src => src.TrainingId))
    
    // Properties yang di-set manual di controller → IGNORE (biar nggak conflict)
    .ForMember(dest => dest.QuestionAnswers, opt => opt.Ignore())
    .ForMember(dest => dest.TrainerName, opt => opt.Ignore())
    .ForMember(dest => dest.FormNumberRegistration, opt => opt.Ignore())
    .ForMember(dest => dest.Purpose1, opt => opt.Ignore())
    .ForMember(dest => dest.Purpose2, opt => opt.Ignore())
    .ForMember(dest => dest.PurposeEnglish1, opt => opt.Ignore())
    .ForMember(dest => dest.PurposeEnglish2, opt => opt.Ignore())
    .ForMember(dest => dest.EvaluationForm, opt => opt.Ignore());


// ✅ TAMBAHKAN INI - Mapping VQuestionAnswerUser → ViewQuestionAnswerUserVM
CreateMap<VQuestionAnswerUser, ViewQuestionAnswerUserVM>()
    // Numeric & Bool (nullable → non-nullable)
    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
    .ForMember(dest => dest.TrainingId, opt => opt.MapFrom(src => src.TrainingId ?? 0))
    .ForMember(dest => dest.ParticipantId, opt => opt.MapFrom(src => src.ParticipantId ?? 0))
    .ForMember(dest => dest.QuizNumber, opt => opt.MapFrom(src => src.QuizNumber ?? 0))
    .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number ?? 0))
    .ForMember(dest => dest.IsTrue, opt => opt.MapFrom(src => src.IsTrue ?? false))
    
    // String (nullable → non-nullable)
    .ForMember(dest => dest.QuestionTraining, opt => opt.MapFrom(src => src.QuestionTraining ?? string.Empty))
    .ForMember(dest => dest.RightAnswer, opt => opt.MapFrom(src => src.RightAnswer ?? string.Empty))
    .ForMember(dest => dest.OptionA, opt => opt.MapFrom(src => src.OptionA ?? string.Empty))
    .ForMember(dest => dest.OptionB, opt => opt.MapFrom(src => src.OptionB ?? string.Empty))
    .ForMember(dest => dest.OptionC, opt => opt.MapFrom(src => src.OptionC ?? string.Empty))
    .ForMember(dest => dest.UserAnswer, opt => opt.MapFrom(src => src.UserAnswer ?? string.Empty))
    .ForMember(dest => dest.ImageQuestion, opt => opt.MapFrom(src => src.ImageQuestion ?? string.Empty))
    .ForMember(dest => dest.ImageRight, opt => opt.MapFrom(src => src.ImageRight ?? string.Empty))
    .ForMember(dest => dest.ImageA, opt => opt.MapFrom(src => src.ImageA ?? string.Empty))
    .ForMember(dest => dest.ImageB, opt => opt.MapFrom(src => src.ImageB ?? string.Empty))
    .ForMember(dest => dest.ImageC, opt => opt.MapFrom(src => src.ImageC ?? string.Empty));
   
    
    }
}
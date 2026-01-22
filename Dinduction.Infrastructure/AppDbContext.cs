using Dinduction.Domain.Entities;       
using Microsoft.EntityFrameworkCore;      

namespace Dinduction.Infrastructure;
public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<MasterTraining> MasterTrainings { get; set; }

    public virtual DbSet<ParticipantUser> ParticipantUsers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<RecordTraining> RecordTrainings { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Section> Sections { get; set; }

    public virtual DbSet<Trainer> Trainers { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VMasterQuestion> VMasterQuestions { get; set; }

    public virtual DbSet<VQuestionAnswer> VQuestionAnswers { get; set; }

    public virtual DbSet<VQuestionAnswerUser> VQuestionAnswerUsers { get; set; }

    public virtual DbSet<VRecordMaster> VRecordMasters { get; set; }

    public virtual DbSet<VRecordResult> VRecordResults { get; set; }

    public virtual DbSet<VRecordTraining> VRecordTrainings { get; set; }

    public virtual DbSet<VResult> VResults { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.ToTable("Answer");

            entity.Property(e => e.ImageA).HasMaxLength(50);
            entity.Property(e => e.ImageB).HasMaxLength(50);
            entity.Property(e => e.ImageC).HasMaxLength(50);
            entity.Property(e => e.ImageRight).HasMaxLength(50);
            entity.Property(e => e.OptionA).HasMaxLength(255);
            entity.Property(e => e.OptionB).HasMaxLength(255);
            entity.Property(e => e.OptionC).HasMaxLength(255);
            entity.Property(e => e.RightAnswer).HasMaxLength(255);

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Answer_Question");
        });

        modelBuilder.Entity<MasterTraining>(entity =>
        {
            entity.ToTable("MasterTraining");

            entity.Property(e => e.EvaluationForm).HasMaxLength(500);
            entity.Property(e => e.FormNumberRegistration).HasMaxLength(50);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.TrainingName).HasMaxLength(500);

            entity.HasOne(d => d.Section).WithMany(p => p.MasterTrainings)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MasterTraining_Section");
        });

        modelBuilder.Entity<ParticipantUser>(entity =>
        {
            entity.ToTable("ParticipantUser");

            entity.Property(e => e.TrainingDate).HasColumnType("datetime");

            entity.HasOne(d => d.Trainer).WithMany(p => p.ParticipantUsers)
                .HasForeignKey(d => d.TrainerId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ParticipantUser_Trainer");

            entity.HasOne(d => d.Training).WithMany(p => p.ParticipantUsers)
                .HasForeignKey(d => d.TrainingId)
                .HasConstraintName("FK_ParticipantUser_MasterTraining");

            entity.HasOne(d => d.User).WithMany(p => p.ParticipantUsers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_ParticipantUser_User");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.ToTable("Question");

            entity.Property(e => e.ImageQuestion).HasMaxLength(50);
            entity.Property(e => e.QuestionTraining).HasMaxLength(500);

            entity.HasOne(d => d.Training).WithMany(p => p.Questions)
                .HasForeignKey(d => d.TrainingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Question_MasterTraining");
        });

        modelBuilder.Entity<RecordTraining>(entity =>
        {
            entity.ToTable("RecordTraining");

            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.UserAnswer).HasMaxLength(500);

            entity.HasOne(d => d.Participant).WithMany(p => p.RecordTrainings)
                .HasForeignKey(d => d.ParticipantId)
                .HasConstraintName("FK_RecordTraining_ParticipantUser");

            entity.HasOne(d => d.Question).WithMany(p => p.RecordTrainings)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_RecordTraining_Question");

            entity.HasOne(d => d.Trainer).WithMany(p => p.RecordTrainings)
                .HasForeignKey(d => d.TrainerId)
                .HasConstraintName("FK_RecordTraining_Trainer");

            entity.HasOne(d => d.Training).WithMany(p => p.RecordTrainings)
                .HasForeignKey(d => d.TrainingId)
                .HasConstraintName("FK_RecordTraining_MasterTraining");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Section>(entity =>
        {
            entity.ToTable("Section");

            entity.Property(e => e.SectionName).HasMaxLength(50);
        });

        modelBuilder.Entity<Trainer>(entity =>
        {
            entity.ToTable("Trainer");

            entity.Property(e => e.Signature).HasMaxLength(50);

            entity.HasOne(d => d.Section).WithMany(p => p.Trainers)
                .HasForeignKey(d => d.SectionId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Trainer_Section");

            entity.HasOne(d => d.User).WithMany(p => p.Trainers)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Trainer_User");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User");

            entity.Property(e => e.Department).HasMaxLength(50);
            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.Password).HasMaxLength(100);
            entity.Property(e => e.UserName).HasMaxLength(50);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_User_Role");
        });

        modelBuilder.Entity<VMasterQuestion>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_MasterQuestion");

            entity.Property(e => e.EvaluationForm).HasMaxLength(500);
            entity.Property(e => e.FormNumberRegistration).HasMaxLength(50);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.TrainingName).HasMaxLength(500);
        });

        modelBuilder.Entity<VQuestionAnswer>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_QuestionAnswer");

            entity.Property(e => e.ImageA).HasMaxLength(50);
            entity.Property(e => e.ImageB).HasMaxLength(50);
            entity.Property(e => e.ImageC).HasMaxLength(50);
            entity.Property(e => e.ImageQuestion).HasMaxLength(50);
            entity.Property(e => e.ImageRight).HasMaxLength(50);
            entity.Property(e => e.OptionA).HasMaxLength(255);
            entity.Property(e => e.OptionB).HasMaxLength(255);
            entity.Property(e => e.OptionC).HasMaxLength(255);
            entity.Property(e => e.QuestionTraining).HasMaxLength(500);
            entity.Property(e => e.RightAnswer).HasMaxLength(255);
        });

        modelBuilder.Entity<VQuestionAnswerUser>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_QuestionAnswerUser");

            entity.Property(e => e.ImageA).HasMaxLength(50);
            entity.Property(e => e.ImageB).HasMaxLength(50);
            entity.Property(e => e.ImageC).HasMaxLength(50);
            entity.Property(e => e.ImageQuestion).HasMaxLength(50);
            entity.Property(e => e.ImageRight).HasMaxLength(50);
            entity.Property(e => e.OptionA).HasMaxLength(255);
            entity.Property(e => e.OptionB).HasMaxLength(255);
            entity.Property(e => e.OptionC).HasMaxLength(255);
            entity.Property(e => e.QuestionTraining).HasMaxLength(500);
            entity.Property(e => e.RightAnswer).HasMaxLength(255);
            entity.Property(e => e.UserAnswer).HasMaxLength(500);
        });

        modelBuilder.Entity<VRecordMaster>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_RecordMaster");

            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.TrainingName).HasMaxLength(500);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<VRecordResult>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_RecordResult");

            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.EvaluationForm).HasMaxLength(500);
            entity.Property(e => e.FormNumberRegistration).HasMaxLength(50);
            entity.Property(e => e.OptionA).HasMaxLength(255);
            entity.Property(e => e.OptionB).HasMaxLength(255);
            entity.Property(e => e.OptionC).HasMaxLength(255);
            entity.Property(e => e.QuestionTraining).HasMaxLength(500);
            entity.Property(e => e.RightAnswer).HasMaxLength(255);
            entity.Property(e => e.Signature).HasMaxLength(50);
            entity.Property(e => e.TrainingName).HasMaxLength(500);
            entity.Property(e => e.UserAnswer).HasMaxLength(500);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<VRecordTraining>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_RecordTraining");

            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.TrainingName).HasMaxLength(500);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        modelBuilder.Entity<VResult>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("V_Result");

            entity.Property(e => e.EmployeeName).HasMaxLength(100);
            entity.Property(e => e.RecordDate).HasColumnType("datetime");
            entity.Property(e => e.TrainingName).HasMaxLength(500);
            entity.Property(e => e.UserName).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

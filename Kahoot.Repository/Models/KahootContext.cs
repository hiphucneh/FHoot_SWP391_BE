using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Kahoot.Repository.Models;

public partial class KahootContext : DbContext
{
    public KahootContext()
    {
    }

    public KahootContext(DbContextOptions<KahootContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Answer> Answers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<QuizSession> QuizSessions { get; set; }

    public virtual DbSet<QuizSessionTeam> QuizSessionTeams { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserAnswer> UserAnswers { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__D4825024FC889802");

            entity.ToTable("Answer");

            entity.HasIndex(e => e.QuestionId, "UX_Answer_Correct")
                .IsUnique()
                .HasFilter("([IsCorrect]=(1))");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Question).WithOne(p => p.Answer)
                .HasForeignKey<Answer>(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Answer_Question");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8CEE9A52D7");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.TimeLimitSec).HasDefaultValue(15);

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Question_Quiz");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quiz__8B42AE6EF5BB13DF");

            entity.ToTable("Quiz");

            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Quiz_CreatedBy");
        });

        modelBuilder.Entity<QuizSession>(entity =>
        {
            entity.HasKey(e => e.QuizSessionId).HasName("PK__QuizSess__12115251D90FB3CB");

            entity.ToTable("QuizSession");

            entity.Property(e => e.QuizSessionId).HasColumnName("QuizSessionID");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.EndedManually).HasDefaultValue(false);
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.SessionName).HasMaxLength(255);
            entity.Property(e => e.StartTime).HasColumnType("datetime");

            entity.HasOne(d => d.Quiz).WithMany(p => p.QuizSessions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuizSession_Quiz");
        });

        modelBuilder.Entity<QuizSessionTeam>(entity =>
        {
            entity.HasKey(e => e.QuizSessionTeamId).HasName("PK__QuizSess__9B66CE9155570409");

            entity.ToTable("QuizSessionTeam");

            entity.Property(e => e.QuizSessionTeamId).HasColumnName("QuizSessionTeamID");
            entity.Property(e => e.QuizSessionId).HasColumnName("QuizSessionID");
            entity.Property(e => e.TeamName).HasMaxLength(255);
            entity.Property(e => e.TotalScore).HasDefaultValue(0);

            entity.HasOne(d => d.QuizSession).WithMany(p => p.QuizSessionTeams)
                .HasForeignKey(d => d.QuizSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QuizSessionTeam_QuizSession");

            entity.HasMany(d => d.Users).WithMany(p => p.QuizSessionTeams)
                .UsingEntity<Dictionary<string, object>>(
                    "QuizSessionTeamMember",
                    r => r.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TeamMember_User"),
                    l => l.HasOne<QuizSessionTeam>().WithMany()
                        .HasForeignKey("QuizSessionTeamId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TeamMember_Team"),
                    j =>
                    {
                        j.HasKey("QuizSessionTeamId", "UserId").HasName("PK__QuizSess__4A1E425BBB3BE414");
                        j.ToTable("QuizSessionTeamMember");
                        j.IndexerProperty<int>("QuizSessionTeamId").HasColumnName("QuizSessionTeamID");
                        j.IndexerProperty<int>("UserId").HasColumnName("UserID");
                    });
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3A71ED98E8");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B6160D9B97906").IsUnique();

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCACFC0049CB");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534598DCF3B").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FcmToken)
                .HasMaxLength(255)
                .HasColumnName("fcmToken");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.RefreshTokenExpiryTime).HasColumnType("datetime");
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Active");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Role");
        });

        modelBuilder.Entity<UserAnswer>(entity =>
        {
            entity.HasKey(e => e.UserAnswerId).HasName("PK__UserAnsw__47CE235F10477B10");

            entity.ToTable("UserAnswer");

            entity.Property(e => e.UserAnswerId).HasColumnName("UserAnswerID");
            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.AnswerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.QuizSessionId).HasColumnName("QuizSessionID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Answer).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.AnswerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswer_Answer");

            entity.HasOne(d => d.Question).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswer_Question");

            entity.HasOne(d => d.QuizSession).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.QuizSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswer_QuizSession");

            entity.HasOne(d => d.User).WithMany(p => p.UserAnswers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserAnswer_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

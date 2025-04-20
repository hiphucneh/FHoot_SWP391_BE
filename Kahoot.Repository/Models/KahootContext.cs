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

    public virtual DbSet<Player> Players { get; set; }

    public virtual DbSet<PlayerAnswer> PlayerAnswers { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<QuestionSession> QuestionSessions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__D4825024A1E3D538");

            entity.ToTable("Answer");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Answer_Question");
        });

        modelBuilder.Entity<Player>(entity =>
        {
            entity.HasKey(e => e.PlayerId).HasName("PK__Player__4A4E74A84407D288");

            entity.ToTable("Player");

            entity.Property(e => e.PlayerId).HasColumnName("PlayerID");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.TeamId).HasColumnName("TeamID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Team).WithMany(p => p.Players)
                .HasForeignKey(d => d.TeamId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Player_Team");

            entity.HasOne(d => d.User).WithMany(p => p.Players)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_Player_User");
        });

        modelBuilder.Entity<PlayerAnswer>(entity =>
        {
            entity.HasKey(e => e.PlayerAnswerId).HasName("PK__PlayerAn__B300DB8CE35AF5E4");

            entity.ToTable("PlayerAnswer");

            entity.Property(e => e.PlayerAnswerId).HasColumnName("PlayerAnswerID");
            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.AnswerTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PlayerId).HasColumnName("PlayerID");
            entity.Property(e => e.QuestionSessionId).HasColumnName("QuestionSessionID");

            entity.HasOne(d => d.Answer).WithMany(p => p.PlayerAnswers)
                .HasForeignKey(d => d.AnswerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PA_Answer");

            entity.HasOne(d => d.Player).WithMany(p => p.PlayerAnswers)
                .HasForeignKey(d => d.PlayerId)
                .HasConstraintName("FK_PA_Player");

            entity.HasOne(d => d.QuestionSession).WithMany(p => p.PlayerAnswers)
                .HasForeignKey(d => d.QuestionSessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PA_QuestionSession");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8CAB798F41");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.TimeLimitSec).HasDefaultValue(15);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK_Question_Quiz");
        });

        modelBuilder.Entity<QuestionSession>(entity =>
        {
            entity.HasKey(e => e.QuestionSessionId).HasName("PK__Question__B37E2729EF71AB8D");

            entity.ToTable("QuestionSession");

            entity.Property(e => e.QuestionSessionId).HasColumnName("QuestionSessionID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.RunAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.SortOrder).HasDefaultValue(1);

            entity.HasOne(d => d.Question).WithMany(p => p.QuestionSessions)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_QS_Question");

            entity.HasOne(d => d.Session).WithMany(p => p.QuestionSessions)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK_QS_Session");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quiz__8B42AE6E6B402A0F");

            entity.ToTable("Quiz");

            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.Quizzes)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Quiz_User");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3A040AF42D");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B616015E81294").IsUnique();

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__Session__C9F4927049EACCA8");

            entity.ToTable("Session");

            entity.HasIndex(e => e.SessionCode, "UQ__Session__30AEBB8412CFA3B0").IsUnique();

            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndAt).HasColumnType("datetime");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.SessionCode).HasMaxLength(20);
            entity.Property(e => e.SessionName).HasMaxLength(255);

            entity.HasOne(d => d.Quiz).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK_Session_Quiz");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK__Team__123AE7B998CEF53D");

            entity.ToTable("Team");

            entity.Property(e => e.TeamId).HasColumnName("TeamID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.TeamName).HasMaxLength(255);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Session).WithMany(p => p.Teams)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK_Team_Session");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC9DA4D035");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D1053456E09D26").IsUnique();

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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

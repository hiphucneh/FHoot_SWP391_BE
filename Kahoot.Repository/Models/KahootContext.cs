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

    public virtual DbSet<GameSession> GameSessions { get; set; }

    public virtual DbSet<GameSessionUser> GameSessionUsers { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<PlayerResponse> PlayerResponses { get; set; }

    public virtual DbSet<Question> Questions { get; set; }

    public virtual DbSet<Quiz> Quizzes { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserPackage> UserPackages { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(local);database=Kahoot;uid=sa;pwd=12345;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Answer>(entity =>
        {
            entity.HasKey(e => e.AnswerId).HasName("PK__Answer__D482502484AF3CDD");

            entity.ToTable("Answer");

            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");

            entity.HasOne(d => d.Question).WithMany(p => p.Answers)
                .HasForeignKey(d => d.QuestionId)
                .HasConstraintName("FK_Answer_Question");
        });

        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__GameSess__C9F49270DD157122");

            entity.ToTable("GameSession");

            entity.HasIndex(e => e.SessionCode, "UQ__GameSess__30AEBB84E4F5D741").IsUnique();

            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.HostUserId).HasColumnName("HostUserID");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.SessionCode).HasMaxLength(20);
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Waiting");

            entity.HasOne(d => d.HostUser).WithMany(p => p.GameSessions)
                .HasForeignKey(d => d.HostUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GameSession_User");

            entity.HasOne(d => d.Quiz).WithMany(p => p.GameSessions)
                .HasForeignKey(d => d.QuizId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GameSession_Quiz");
        });

        modelBuilder.Entity<GameSessionUser>(entity =>
        {
            entity.HasKey(e => e.SessionUserId).HasName("PK__GameSess__96CEF1E0806C16D6");

            entity.ToTable("GameSessionUser");

            entity.Property(e => e.SessionUserId).HasColumnName("SessionUserID");
            entity.Property(e => e.JoinedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Score).HasDefaultValue(0);
            entity.Property(e => e.SessionId).HasColumnName("SessionID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Session).WithMany(p => p.GameSessionUsers)
                .HasForeignKey(d => d.SessionId)
                .HasConstraintName("FK_GameSessionUser_Session");

            entity.HasOne(d => d.User).WithMany(p => p.GameSessionUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GameSessionUser_User");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.PackageId).HasName("PK__Package__322035EC6B4DF7EF");

            entity.ToTable("Package");

            entity.HasIndex(e => e.PackageName, "UQ__Package__73856F7A0C0A5003").IsUnique();

            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.PackageName).HasMaxLength(100);
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
        });

        modelBuilder.Entity<PlayerResponse>(entity =>
        {
            entity.HasKey(e => e.ResponseId).HasName("PK__PlayerRe__1AAA640CB285227F");

            entity.ToTable("PlayerResponse");

            entity.Property(e => e.ResponseId).HasColumnName("ResponseID");
            entity.Property(e => e.AnswerId).HasColumnName("AnswerID");
            entity.Property(e => e.AnsweredAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PointsAwarded).HasDefaultValue(0);
            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.SessionUserId).HasColumnName("SessionUserID");

            entity.HasOne(d => d.Answer).WithMany(p => p.PlayerResponses)
                .HasForeignKey(d => d.AnswerId)
                .HasConstraintName("FK_PlayerResponse_Answer");

            entity.HasOne(d => d.Question).WithMany(p => p.PlayerResponses)
                .HasForeignKey(d => d.QuestionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlayerResponse_Question");

            entity.HasOne(d => d.SessionUser).WithMany(p => p.PlayerResponses)
                .HasForeignKey(d => d.SessionUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlayerResponse_SessionUser");
        });

        modelBuilder.Entity<Question>(entity =>
        {
            entity.HasKey(e => e.QuestionId).HasName("PK__Question__0DC06F8C1B0C73F9");

            entity.ToTable("Question");

            entity.Property(e => e.QuestionId).HasColumnName("QuestionID");
            entity.Property(e => e.QuestionType)
                .HasMaxLength(50)
                .HasDefaultValue("MultipleChoice");
            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.TimeLimit).HasDefaultValue(30);

            entity.HasOne(d => d.Quiz).WithMany(p => p.Questions)
                .HasForeignKey(d => d.QuizId)
                .HasConstraintName("FK_Question_Quiz");
        });

        modelBuilder.Entity<Quiz>(entity =>
        {
            entity.HasKey(e => e.QuizId).HasName("PK__Quiz__8B42AE6E29A6B458");

            entity.ToTable("Quiz");

            entity.Property(e => e.QuizId).HasColumnName("QuizID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.QuizName).HasMaxLength(255);
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
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE3A36AA694B");

            entity.ToTable("Role");

            entity.HasIndex(e => e.RoleName, "UQ__Role__8A2B61606022413F").IsUnique();

            entity.Property(e => e.RoleId)
                .ValueGeneratedNever()
                .HasColumnName("RoleID");
            entity.Property(e => e.RoleName).HasMaxLength(50);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CCAC508F5C90");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D1053472D47147").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.FcmToken)
                .HasMaxLength(255)
                .HasColumnName("fcmToken");
            entity.Property(e => e.FullName).HasMaxLength(255);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.Location).HasMaxLength(20);
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(20);
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

        modelBuilder.Entity<UserPackage>(entity =>
        {
            entity.HasKey(e => e.UserPackageId).HasName("PK__UserPack__AE9B91FAAFBEB76E");

            entity.ToTable("UserPackage");

            entity.Property(e => e.UserPackageId).HasColumnName("UserPackageID");
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.StartDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Package).WithMany(p => p.UserPackages)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK_UserPackage_Package");

            entity.HasOne(d => d.User).WithMany(p => p.UserPackages)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_UserPackage_User");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

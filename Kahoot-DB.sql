USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Kahoot')
BEGIN
    CREATE DATABASE Kahoot;
END;
GO

USE Kahoot;
GO

-- Bỏ ràng buộc FOREIGN KEY trước khi xóa tất cả bảng
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += 'ALTER TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '] 
    DROP CONSTRAINT [' + CONSTRAINT_NAME + '];' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS
WHERE CONSTRAINT_TYPE = 'FOREIGN KEY';
EXEC sp_executesql @sql;

-- Xóa tất cả bảng
SET @sql = '';
SELECT @sql += 'DROP TABLE IF EXISTS [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '];' + CHAR(13)
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE';
EXEC sp_executesql @sql;
GO

-- ================================================
-- Mô tả: Định nghĩa các bảng cho ứng dụng game Kahoot
-- Yêu cầu: Người dùng phải đăng nhập mới được chơi game.
-- ================================================

-- ============================
-- 1. Bảng quản lý vai trò (Role)
-- ============================
CREATE TABLE Role (
    RoleID   INT           PRIMARY KEY,
    RoleName NVARCHAR(50)  UNIQUE NOT NULL
);

-- ============================
-- 2. Bảng người dùng ([User])
-- ============================
CREATE TABLE [User] (
    UserID                 INT           IDENTITY(1,1) PRIMARY KEY,
    FullName               NVARCHAR(255) NOT NULL,
    Email                  NVARCHAR(255) UNIQUE NOT NULL,
    Password               NVARCHAR(255) NOT NULL,
    Age                    INT           CHECK (Age > 0),
    Avatar                 NVARCHAR(MAX) NULL,
    fcmToken               NVARCHAR(255) NULL,
    Status                 NVARCHAR(50)  CHECK (Status IN ('Active','Inactive')) DEFAULT 'Active',
    RoleID                 INT           NOT NULL,
    RefreshToken           NVARCHAR(MAX) NULL,
    RefreshTokenExpiryTime DATETIME      NULL,
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleID) REFERENCES Role(RoleID)
	);

-- ============================
-- 3. Bảng Quiz
-- ============================
CREATE TABLE Quiz (
    QuizID     INT           IDENTITY(1,1) PRIMARY KEY,
    Title      NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    ImgUrl     NVARCHAR(MAX) NULL,
    CreatedBy  INT           NOT NULL,
    CreatedAt  DATETIME      NOT NULL DEFAULT GETDATE(),
    UpdatedAt  DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Quiz_User FOREIGN KEY (CreatedBy) REFERENCES [User](UserID)
);

-- ============================
-- 4. Bảng Question
-- ============================
CREATE TABLE Question (
    QuestionID     INT        IDENTITY(1,1) PRIMARY KEY,
    QuizID         INT        NOT NULL,
    QuestionText   NVARCHAR(MAX) NOT NULL,
    IsRandomAnswer BIT        NOT NULL DEFAULT 0,
    ImgUrl         NVARCHAR(MAX) NULL,
    SortOrder      INT        NOT NULL DEFAULT 0,
    TimeLimitSec   INT        NOT NULL DEFAULT 15,
    CreatedAt      DATETIME   NOT NULL DEFAULT GETDATE(),
    UpdatedAt      DATETIME   NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Question_Quiz FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID) ON DELETE CASCADE
);

-- ============================
-- 5. Bảng Answer
-- ============================
CREATE TABLE Answer (
    AnswerID   INT        IDENTITY(1,1) PRIMARY KEY,
    QuestionID INT        NOT NULL,
    AnswerText NVARCHAR(MAX) NOT NULL,
    IsCorrect  BIT        NOT NULL DEFAULT 0,
    CreatedAt  DATETIME2  NOT NULL DEFAULT SYSUTCDATETIME(),
    UpdatedAt  DATETIME2  NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Answer_Question FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID) ON DELETE CASCADE
);

-- ============================
-- 6. Bảng Session
-- ============================
CREATE TABLE [Session] (
    SessionID   INT        IDENTITY(1,1) PRIMARY KEY,
    QuizID      INT        NOT NULL,
    SessionName NVARCHAR(255) NOT NULL,
    SessionCode NVARCHAR(20)  NULL UNIQUE,
    CreatedAt   DATETIME   NOT NULL DEFAULT GETDATE(),
    EndAt       DATETIME   NULL,
    EndedManually BIT      NOT NULL DEFAULT 0,
    CONSTRAINT FK_Session_Quiz FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID) ON DELETE CASCADE
);


CREATE TABLE QuestionSession(
    QuestionSessionID INT     IDENTITY(1,1) PRIMARY KEY,
    SessionID         INT     NOT NULL,
    QuestionID        INT     NOT NULL,
    SortOrder         INT     NOT NULL DEFAULT 1,
    RunAt             DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_QS_Session  FOREIGN KEY (SessionID)  REFERENCES Session(SessionID)  ON DELETE CASCADE,
    CONSTRAINT FK_QS_Question FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID) 
);

-- ============================
-- 9. Bảng Team
-- ============================
CREATE TABLE Team (
    TeamID     INT        IDENTITY(1,1) PRIMARY KEY,
    SessionID  INT        NOT NULL,
    TeamName   NVARCHAR(255) NOT NULL,
    TotalScore INT        NOT NULL DEFAULT 0,
    CreatedAt  DATETIME   NOT NULL DEFAULT GETDATE(),
    UpdatedAt  DATETIME   NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Team_Session FOREIGN KEY (SessionID) REFERENCES [Session](SessionID) ON DELETE CASCADE
);

-- ============================
-- 10. Bảng Player
-- ============================
CREATE TABLE Player (
    PlayerID INT        IDENTITY(1,1) PRIMARY KEY,
    UserID   INT        NOT NULL,
    TeamID   INT        NOT NULL,
	Name       NVARCHAR(100) NOT NULL,
    ImageURL   NVARCHAR(500) NULL,
    JoinedAt DATETIME   NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Player_User FOREIGN KEY (UserID) REFERENCES [User](UserID) ON DELETE CASCADE,
    CONSTRAINT FK_Player_Team FOREIGN KEY (TeamID) REFERENCES Team(TeamID) 
);

-- ============================
-- 11. Bảng PlayerAnswer
-- ============================
CREATE TABLE PlayerAnswer (
    PlayerAnswerID    INT      IDENTITY(1,1) PRIMARY KEY,
    PlayerID          INT      NOT NULL,
    QuestionSessionID INT      NOT NULL,
    AnswerID          INT      NOT NULL,
    AnswerTime        DATETIME NOT NULL DEFAULT GETDATE(),
	AnswerOrder      INT        NOT NULL DEFAULT 0,
    IsCorrect         BIT      NOT NULL DEFAULT 0,
    Score             INT      NOT NULL DEFAULT 0,
    CONSTRAINT FK_PA_Player          FOREIGN KEY (PlayerID)          REFERENCES Player(PlayerID)              ON DELETE CASCADE,
    CONSTRAINT FK_PA_QuestionSession FOREIGN KEY (QuestionSessionID) REFERENCES QuestionSession(QuestionSessionID) ,
    CONSTRAINT FK_PA_Answer          FOREIGN KEY (AnswerID)          REFERENCES Answer(AnswerID)             
);

CREATE TABLE SystemConfiguration (
    ConfigId    INT             IDENTITY(1,1) PRIMARY KEY,
    Name        NVARCHAR(255)   NOT NULL UNIQUE, -- tên config phải unique
    MinValue    FLOAT           NULL,
    MaxValue    FLOAT           NULL,
    Unit        NVARCHAR(50)    NULL,
    IsActive    BIT             NULL DEFAULT 1,
    Description NVARCHAR(MAX)   NULL,
    CreatedAt   DATETIME        NULL DEFAULT GETDATE(),
    UpdatedAt   DATETIME        NULL DEFAULT GETDATE()
);
GO

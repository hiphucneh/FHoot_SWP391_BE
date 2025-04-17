USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'Kahoot')
BEGIN
    CREATE DATABASE Kahoot;
END;
GO

USE Kahoot;
GO

-- Bỏ ràng buộc FOREIGN KEY trước khi xóa bảng
DECLARE @sql NVARCHAR(MAX) = N'';
SELECT @sql += 'ALTER TABLE [' + TABLE_SCHEMA + '].[' + TABLE_NAME + '] DROP CONSTRAINT [' + CONSTRAINT_NAME + '];' + CHAR(13)
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
-- File: DatabaseForKahootGame.sql
-- Mô tả: Định nghĩa các bảng cho ứng dụng game Kahoot
-- Yêu cầu: Người dùng phải đăng nhập mới được chơi game.
-- ================================================

-- ============================
-- 1. Bảng quản lý vai trò (Role)
-- ============================
CREATE TABLE Role (
    RoleID INT PRIMARY KEY,
    RoleName NVARCHAR(50) UNIQUE NOT NULL
);

-- ============================
-- 2. Bảng người dùng ([User])
-- ============================
CREATE TABLE [User] (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(255) NOT NULL,
    Email NVARCHAR(255) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Age INT CHECK (Age > 0),
    Avatar NVARCHAR(MAX) NULL,
    fcmToken NVARCHAR(255) NULL,
    Status NVARCHAR(50) CHECK (Status IN ('Active', 'Inactive')) DEFAULT 'Active',
    RoleID INT NOT NULL,
    RefreshToken NVARCHAR(MAX) NULL,
    RefreshTokenExpiryTime DATETIME NULL,
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleID) REFERENCES Role(RoleID)
);

CREATE TABLE Quiz (
    QuizID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    CreatedBy INT NOT NULL,      
    Createdat DATETIME DEFAULT GETDATE(),
    Updateat DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Quiz_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES [User](UserID)
);

CREATE TABLE Question (
    QuestionID INT IDENTITY(1,1) PRIMARY KEY,
    QuizID INT NOT NULL,
    QuestionText NVARCHAR(MAX) NOT NULL,
    SortOrder INT NOT NULL,      -- Hỗ trợ thay đổi vị trí (drag & drop)
    TimeLimitSec INT NOT NULL DEFAULT 15,  -- Thời gian trả lời cho câu hỏi, ví dụ: 15 giây
    Createdat DATETIME DEFAULT GETDATE(),
    Updateat DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Question_Quiz FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID)
);


CREATE TABLE Answer (
    AnswerID INT IDENTITY(1,1) PRIMARY KEY,
    QuestionID INT NOT NULL,
    AnswerText NVARCHAR(MAX) NOT NULL,
    IsCorrect BIT NOT NULL,
	Createdat DATETIME DEFAULT GETDATE(),
    Updateat DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Answer_Question FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID)
);

-- Đảm bảo mỗi question chỉ có 1 đáp án đúng
CREATE UNIQUE INDEX UX_Answer_Correct ON Answer(QuestionID)
WHERE IsCorrect = 1;


CREATE TABLE [Session] (
    SessionID INT IDENTITY(1,1) PRIMARY KEY,
    QuizID INT NOT NULL,
    SessionName NVARCHAR(255) NOT NULL,  -- Tên phiên thi
    Createdat DATETIME DEFAULT GETDATE(),
    Endat DATETIME DEFAULT GETDATE(),        -- Thời gian kết thúc dự kiến (có thể được cập nhật lại khi host bấm dừng)
    EndedManually BIT DEFAULT 0,         -- 0: kết thúc tự động (khi hết câu hỏi); 1: host bấm kết thúc bài thi
    CONSTRAINT FK_QuizSession_Quiz FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID)
);


CREATE TABLE UserAnswer (
    UserAnswerID INT IDENTITY(1,1) PRIMARY KEY,
    SessionID INT NOT NULL,
    UserID INT NOT NULL,
    QuestionID INT NOT NULL,
    AnswerID INT NOT NULL,
    AnswerOrder INT NOT NULL,  -- Thứ tự trả lời của User trong phiên thi
    IsCorrect BIT NOT NULL,    -- Đánh dấu đúng/sai
    Score INT NOT NULL DEFAULT 0,  -- Điểm của câu trả lời của học sinh
    AnswerTime DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_UserAnswer_QuizSession FOREIGN KEY (SessionID) REFERENCES Session(SessionID),
    CONSTRAINT FK_UserAnswer_User FOREIGN KEY (UserID) REFERENCES [User](UserID),
    CONSTRAINT FK_UserAnswer_Question FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID),
    CONSTRAINT FK_UserAnswer_Answer FOREIGN KEY (AnswerID) REFERENCES Answer(AnswerID)
);


CREATE TABLE Team (
    TeamID INT IDENTITY(1,1) PRIMARY KEY,
    SessionID INT NOT NULL,
    TeamName NVARCHAR(255) NOT NULL,
    TotalScore INT DEFAULT 0,  -- Tổng điểm của nhóm (có thể được cập nhật sau khi tổng hợp điểm của các thành viên)
    CONSTRAINT FK_QuizSessionTeam_QuizSession FOREIGN KEY (SessionID) REFERENCES Session(SessionID)
);


CREATE TABLE TeamUser (
    TeamID INT NOT NULL,
    UserID INT NOT NULL,
    PRIMARY KEY (TeamID, UserID),
    CONSTRAINT FK_TeamMember_Team FOREIGN KEY (TeamID) REFERENCES Team(TeamID),
    CONSTRAINT FK_TeamMember_User FOREIGN KEY (UserID) REFERENCES [User](UserID)
);


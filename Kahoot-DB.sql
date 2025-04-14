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
    Phone NVARCHAR(20),
    Age INT CHECK (Age > 0),
    Gender NVARCHAR(10) CHECK (Gender IN ('Male', 'Female', 'Other')),
    Location NVARCHAR(20),
    Avatar NVARCHAR(MAX) NULL,
    fcmToken NVARCHAR(255) NULL,
    Status NVARCHAR(50) CHECK (Status IN ('Active', 'Inactive')) DEFAULT 'Active',
    EnableReminder BIT NULL, -- bật thông báo nhắc nhở
    RoleID INT NOT NULL,
    RefreshToken NVARCHAR(MAX) NULL,
    RefreshTokenExpiryTime DATETIME NULL,
    CONSTRAINT FK_User_Role FOREIGN KEY (RoleID) REFERENCES Role(RoleID)
);

-- ============================
-- 3. Bảng Package
-- ============================
CREATE TABLE Package (
    PackageID INT IDENTITY(1,1) PRIMARY KEY,
    PackageName NVARCHAR(100) UNIQUE NOT NULL,
    Price FLOAT CHECK (Price >= 0),
    Duration INT CHECK (Duration > 0),
    Description NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

-- ============================
-- 4. Bảng UserPackage
-- ============================
CREATE TABLE UserPackage (
    UserPackageID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    PackageID INT NOT NULL,
    StartDate DATETIME DEFAULT GETDATE(),
    ExpiryDate DATETIME NOT NULL,
    Status NVARCHAR(50),
    CONSTRAINT FK_UserPackage_User FOREIGN KEY (UserID) REFERENCES [User](UserID) ON DELETE CASCADE,
    CONSTRAINT FK_UserPackage_Package FOREIGN KEY (PackageID) REFERENCES Package(PackageID) ON DELETE CASCADE
);

-- ============================
-- 5. Bảng Quiz: Lưu thông tin bộ câu hỏi (quiz)
-- ============================
CREATE TABLE Quiz (
    QuizID INT IDENTITY(1,1) PRIMARY KEY,
    QuizName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(500) NULL,
    CreatedBy INT NOT NULL,  -- Người tạo quiz từ bảng [User]
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Quiz_User FOREIGN KEY (CreatedBy) REFERENCES [User](UserID)
);

-- ============================
-- 6. Bảng Question: Lưu các câu hỏi thuộc từng Quiz
-- ============================
CREATE TABLE Question (
    QuestionID INT IDENTITY(1,1) PRIMARY KEY,
    QuizID INT NOT NULL,  -- Tham chiếu đến Quiz
    Content NVARCHAR(MAX) NOT NULL,
    QuestionType NVARCHAR(50) CHECK (QuestionType IN ('MultipleChoice', 'TrueFalse')) DEFAULT 'MultipleChoice',
    TimeLimit INT CHECK (TimeLimit > 0) DEFAULT 30,  -- Thời gian trả lời (giây)
    CONSTRAINT FK_Question_Quiz FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID) ON DELETE CASCADE
);

-- ============================
-- 7. Bảng Answer: Lưu các đáp án cho mỗi câu hỏi
-- ============================
CREATE TABLE Answer (
    AnswerID INT IDENTITY(1,1) PRIMARY KEY,
    QuestionID INT NOT NULL,  -- Tham chiếu đến Question
    AnswerText NVARCHAR(MAX) NOT NULL,
    IsCorrect BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_Answer_Question FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID) ON DELETE CASCADE
);

-- ============================
-- 8. Bảng GameSession: Phiên chơi game dựa trên một quiz cụ thể
-- ============================
CREATE TABLE GameSession (
    SessionID INT IDENTITY(1,1) PRIMARY KEY,
    QuizID INT NOT NULL,         -- Tham chiếu đến Quiz được chơi
    HostUserID INT NOT NULL,     -- Người tạo phiên (chủ phòng) từ bảng [User]
    SessionCode NVARCHAR(20) NOT NULL UNIQUE,  -- Mã phòng để tham gia game
    CreatedAt DATETIME DEFAULT GETDATE(),
    StartTime DATETIME NULL,
    EndTime DATETIME NULL,
    Status NVARCHAR(50) CHECK (Status IN ('Waiting', 'InProgress', 'Ended')) DEFAULT 'Waiting',
    CONSTRAINT FK_GameSession_Quiz FOREIGN KEY (QuizID) REFERENCES Quiz(QuizID),
    CONSTRAINT FK_GameSession_User FOREIGN KEY (HostUserID) REFERENCES [User](UserID)
);

-- ============================
-- 9. Bảng GameSessionUser: Lưu danh sách người chơi tham gia phiên game và điểm số của họ
-- ============================
CREATE TABLE GameSessionUser (
    SessionUserID INT IDENTITY(1,1) PRIMARY KEY,
    SessionID INT NOT NULL,  -- Tham chiếu đến phiên game
    UserID INT NOT NULL,     -- Tham chiếu đến bảng [User]
    JoinedAt DATETIME DEFAULT GETDATE(),
    Score INT DEFAULT 0,     -- Điểm số của người chơi trong phiên
    CONSTRAINT FK_GameSessionUser_Session FOREIGN KEY (SessionID) REFERENCES GameSession(SessionID) ON DELETE CASCADE,
    CONSTRAINT FK_GameSessionUser_User FOREIGN KEY (UserID) REFERENCES [User](UserID)
);

-- ============================
-- 10. Bảng PlayerResponse: Lưu trữ câu trả lời của người chơi cho từng câu hỏi trong phiên game
-- ============================
CREATE TABLE PlayerResponse (
    ResponseID INT IDENTITY(1,1) PRIMARY KEY,
    SessionUserID INT NOT NULL,  -- Tham chiếu đến người chơi trong phiên game (GameSessionUser)
    QuestionID INT NOT NULL,     -- Tham chiếu đến câu hỏi (Question)
    AnswerID INT NULL,           -- Đáp án được chọn (có thể NULL nếu không trả lời)
    AnsweredAt DATETIME DEFAULT GETDATE(),
    PointsAwarded INT DEFAULT 0, -- Số điểm đạt được cho câu hỏi
    CONSTRAINT FK_PlayerResponse_SessionUser FOREIGN KEY (SessionUserID) REFERENCES GameSessionUser(SessionUserID),
    CONSTRAINT FK_PlayerResponse_Question FOREIGN KEY (QuestionID) REFERENCES Question(QuestionID),
    CONSTRAINT FK_PlayerResponse_Answer FOREIGN KEY (AnswerID) REFERENCES Answer(AnswerID)
);

-- ================================================
-- Kết thúc file định nghĩa cơ sở dữ liệu cho ứng dụng game Kahoot.
-- ================================================

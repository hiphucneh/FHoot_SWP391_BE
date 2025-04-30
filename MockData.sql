USE Kahoot;
GO

ALTER TABLE Role NOCHECK CONSTRAINT ALL;
ALTER TABLE [User] NOCHECK CONSTRAINT ALL;
ALTER TABLE Quiz NOCHECK CONSTRAINT ALL;
ALTER TABLE Question NOCHECK CONSTRAINT ALL;
ALTER TABLE Answer NOCHECK CONSTRAINT ALL;
ALTER TABLE [Session] NOCHECK CONSTRAINT ALL;
ALTER TABLE QuestionSession NOCHECK CONSTRAINT ALL;
ALTER TABLE Team NOCHECK CONSTRAINT ALL;
ALTER TABLE Player NOCHECK CONSTRAINT ALL;
ALTER TABLE PlayerAnswer NOCHECK CONSTRAINT ALL;
GO

DELETE FROM PlayerAnswer;
DELETE FROM Player;
DELETE FROM QuestionSession;
DELETE FROM Team;
DELETE FROM [Session];
DELETE FROM Answer;
DELETE FROM Question;
DELETE FROM Quiz;
DELETE FROM [User];
DELETE FROM Role;
delete from SystemConfiguration

INSERT INTO Role (RoleID, RoleName) VALUES
(1, 'Admin'),
(2, 'User'),
(3, 'Teacher');

SET IDENTITY_INSERT [dbo].[User] ON;
GO

INSERT into [dbo].[User] ([UserID], [FullName], [Email], [Password], [Age], [Avatar], [fcmToken], [Status], [RoleID]) VALUES 
(1, N'New User', N'user@example.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', 21, N'', NULL, N'Active', 2),
(2, N'Admin', N'admin@example.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', 30, N'', NULL, N'Active', 1),
(3, N'Teacher', N'teacher@example.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', 20, N'', NULL, N'Active', 3);

INSERT INTO SystemConfiguration (Name, MinValue, MaxValue, Unit, IsActive, Description)
VALUES 
('MaxPlayersPerTeam', 1, 10, 'players', 1, 'Số lượng người chơi tối đa trong 1 team'),
('QuizTimeLimit', 10, 300, 'seconds', 1, 'Giới hạn thời gian cho mỗi quiz'),
('MaxQuestionTextLength', 1, 500, 'characters', 1, N'Số ký tự tối đa cho QuestionText'),
('MaxAnswerTextLength', 1, 250, 'characters', 1, N'Số ký tự tối đa cho AnswerText');
SET IDENTITY_INSERT [kahoot].[dbo].[User] OFF;
SET IDENTITY_INSERT Package ON;
INSERT Package (PackageID, PackageName, PackageType, Price, Duration, Description) 
VALUES (1, 'Premium Basic 1 Tháng', 'Basic', 150000.00, 30, N'Truy cập các tính năng Host cơ bản  trong 1 tháng'),
(2, 'Premium Basic 3 Tháng', 'Basic', 400000.00, 30, N'Truy cập các tính năng Host cơ bản  trong 3 tháng'),
(3, 'Premium Basic 9 Tháng', 'Basic', 1000000.00, 30, N'Truy cập các tính năng Host cơ bản  trong 9 tháng');
SET IDENTITY_INSERT Package OFF;

SET IDENTITY_INSERT [kahoot].[dbo].[UserPackage] ON;
INSERT UserPackage (UserPackageID, UserID, PackageID, StartDate, ExpiryDate, Status) VALUES 
(1, 1, 1, GETDATE(), GETDATE() + 30, 'Active'),
(2, 3, 1, '2024-06-20', DATEADD(DAY, 30, '2024-06-20'), 'Expired'),
(3, 4, 1, '2024-02-05', DATEADD(DAY, 30, '2024-02-05'), 'Expired'),
(4, 5, 1, '2024-12-01', DATEADD(DAY, 30, '2024-12-01'), 'Expired'),
(5, 6, 1, '2025-01-01', DATEADD(DAY, 30, '2025-01-01'), 'Expired'),
(6, 7, 1, '2025-02-14', DATEADD(DAY, 30, '2025-02-14'), 'Expired'),
(7, 8, 1, '2025-02-14', DATEADD(DAY, 30, '2025-02-14'), 'Expired'),
(8, 9, 1, '2025-02-14', DATEADD(DAY, 30, '2025-02-14'), 'Expired'),
(9, 10, 1, '2025-03-31', DATEADD(DAY, 30, '2025-03-31'), 'Expired');
SET IDENTITY_INSERT [kahoot].[dbo].[UserPackage] OFF;
INSERT INTO Role (RoleID, RoleName) VALUES
(1, 'Admin'),
(2, 'Customer');

SET IDENTITY_INSERT [dbo].[User] ON 
GO
INSERT [dbo].[User] ([UserID], [FullName], [Email], [Password], [Phone], [Age], [Gender], [Location], [Avatar], [fcmToken], [Status], [RoleID]) VALUES 
(1, N'New User', N'user@example.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '21', 'Male', 'VietNam', N'', NULL, N'Active', 2),
(2, N'Admin', N'admin@example.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, NULL, NULL, NULL, N'', NULL, N'Active', 1),
(3, N'Tran Van Tai', N'tranvantai@gmail.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '20', 'Male', 'VietNam', N'', NULL, N'Active', 2),
(4, N'Nguyen Thi Hanh', N'hanhnt@gmail.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '18', 'Female', 'VietNam', N'', NULL, N'Active', 2),
(5, N'Pham Nguyen', N'nguyenpham@gmail.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '21', 'Male', 'VietNam', N'', NULL, N'Active', 2),
(6, N'Nguyen Tuan Kiet', N'kietnguyen@gmail.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '22', 'Male', 'VietNam', N'', NULL, N'Active', 2),
(7, N'Dinh Hoang Nam', N'namdh@gmail.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '22', 'Male', 'VietNam', N'', NULL, N'Active', 2),
(8, N'Tran Thi Thao', N'thaotran@gmail.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '25', 'Female', 'VietNam', N'', NULL, N'Active', 2),
(9, N'Nguyen Thanh Phat', N'phat123@gmail.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '21', 'Male', 'VietNam', N'', NULL, N'Active', 2),
(10, N'Chau Nhuan Phat', N'chaunhuanphat@gmail.com', N'AQAAAAIAAYagAAAAEJC6r2LMSMDB1nSfXeYadVFihZL+PHOrpKK4g6s0kDy9LRR4sYRlbYbjDh3pF95RZg==', NULL, '33', 'Male', 'VietNam', N'', NULL, N'Active', 2);

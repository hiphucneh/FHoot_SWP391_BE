using Kahoot.Common.BusinessResult;
using Kahoot.Common;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Service.Helpers;
using Kahoot.Service.Interface;
using Kahoot.Service.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Kahoot.Service.ModelDTOs.Request;
using Kahoot.Service.ModelDTOs.Response;
using Mapster;
using System.Globalization;
using Kahoot.Service.Model.Request;
using CsvHelper;
using CsvHelper.Configuration;
using ClosedXML.Excel;
using System.Linq.Expressions;

namespace Kahoot.Service.Services
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public QuizService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
        {
            _unitOfWork ??= unitOfWork;
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetUserIdClaim()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            return user?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
        }
        public async Task<IBusinessResult> FindQuizById(int id)
        {
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == id)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();
            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");

            var response = quiz.Adapt<QuizResponse>();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz found", response);
        }
        public async Task<IBusinessResult> CreateQuiz(QuizRequest request)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            }
            var quiz = new Quiz();
            quiz.CreatedBy = int.Parse(userIdClaim);
            quiz.CreatedAt = DateTime.UtcNow;
            quiz.UpdatedAt = DateTime.UtcNow;
            quiz.Description = request.Description;
            quiz.Title = request.Title;
            if (quiz.ImgUrl != null) {
                try
                {
                    var cloudinaryHelper = new CloudinaryHelper();
                    quiz.ImgUrl = await cloudinaryHelper.UploadImageWithCloudDinary(request.ImgUrl);
                }
                catch (Exception ex)
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "Upload image error!");
                }
            }
            
            await _unitOfWork.QuizRepository.AddAsync(quiz);
            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz created successfully", quiz);
        }
        public async Task<IBusinessResult> UpdateQuiz(int quizId, QuizRequest request)
        {
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == quizId)
                .FirstOrDefaultAsync();

            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");

            quiz.Title = request.Title;
            quiz.Description = request.Description;
            if (quiz.ImgUrl != null)
            {
                try
                {
                    var cloudinaryHelper = new CloudinaryHelper();
                    quiz.ImgUrl = await cloudinaryHelper.UploadImageWithCloudDinary(request.ImgUrl);
                }
                catch (Exception ex)
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "Upload image error!");
                }
            }
            quiz.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.QuizRepository.UpdateAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz updated successfully", quiz);
        }
        public async Task<IBusinessResult> DeleteQuiz(int quizId)
        {
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(x => x.QuizId == quizId)
                .FirstOrDefaultAsync();

            if (quiz == null)
            {
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");
            }

            // Xóa quiz và commit thay đổi
            await _unitOfWork.QuizRepository.DeleteAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Quiz deleted successfully");
        }
        public async Task<IBusinessResult> AddQuestionsToQuiz(int quizId, List<QuestionRequest> questionRequests)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);
            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == quizId && q.CreatedBy == userId)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();

            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");

            var nextOrder = quiz.Questions.Any()
                ? quiz.Questions.Max(q => q.SortOrder) + 1
                : 1;

            foreach (var qr in questionRequests)
            {
                var question = new Question
                {
                    QuizId = quizId,
                    QuestionText = qr.QuestionText,
                    TimeLimitSec = qr.TimeLimitSec,
                    IsRandomAnswer = qr.IsRandomAnswer,
                    SortOrder = nextOrder++,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                question.Answers = qr.Answers.Select(ar => new Answer
                {
                    AnswerText = ar.AnswerText,
                    IsCorrect = ar.IsCorrect,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                quiz.Questions.Add(question);
            }

            quiz.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.QuizRepository.UpdateAsync(quiz);

            await _unitOfWork.SaveChangesAsync();
            return new BusinessResult(Const.HTTP_STATUS_OK, "Questions added successfully");
        }
        public async Task<IBusinessResult> SortOrderAsync(int quizId, int questionId, int newSortOrder)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == quizId && q.CreatedBy == userId)
                .Include(q => q.Questions)
                .FirstOrDefaultAsync();

            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz not found");

            var questions = quiz.Questions.OrderBy(q => q.SortOrder).ToList();
            var total = questions.Count;

            var target = questions.FirstOrDefault(q => q.QuestionId == questionId);
            if (target == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Question not found in this quiz");

            if (newSortOrder < 1 || newSortOrder > total)
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, $"newSortOrder phải nằm trong khoảng 1 đến {total}");

            var oldOrder = target.SortOrder;
            if (newSortOrder == oldOrder)
                return new BusinessResult(Const.HTTP_STATUS_OK, "No change needed");

            if (newSortOrder > oldOrder)
            {
                foreach (var q in questions)
                {
                    if (q.SortOrder > oldOrder && q.SortOrder <= newSortOrder)
                        q.SortOrder--;
                }
            }
            else
            {
                foreach (var q in questions)
                {
                    if (q.SortOrder >= newSortOrder && q.SortOrder < oldOrder)
                        q.SortOrder++;
                }
            }

            target.SortOrder = newSortOrder;
            quiz.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.QuizRepository.UpdateAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "SortOrder updated successfully");
        }
        public async Task<IBusinessResult> UpdateQuestionsForQuiz(int quizId, List<QuestionRequest> questionRequests)
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == quizId && q.CreatedBy == userId)
                .Include(q => q.Questions)
                    .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();

            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz không tồn tại hoặc không phải của bạn");

            quiz.Questions.Clear();

            int sortOrder = 1;
            foreach (var qr in questionRequests)
            {
                var question = new Question
                {
                    QuizId = quizId,
                    QuestionText = qr.QuestionText,
                    TimeLimitSec = qr.TimeLimitSec,
                    IsRandomAnswer = qr.IsRandomAnswer,
                    SortOrder = sortOrder++,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                question.Answers = qr.Answers.Select(ar => new Answer
                {
                    AnswerText = ar.AnswerText,
                    IsCorrect = ar.IsCorrect,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                quiz.Questions.Add(question);
            }

            // 4) Cập nhật timestamp và save
            quiz.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.QuizRepository.UpdateAsync(quiz);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Cập nhật câu hỏi và đáp án thành công");
        }
        public async Task<IBusinessResult> GetMyQuizzes()
        {
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");

            int userId = int.Parse(userIdClaim);
            var quizzes = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.CreatedBy == userId)
                .Include(q => q.Questions)
                .ThenInclude(q => q.Answers)
                .ToListAsync();
            var response = quizzes.Adapt<List<QuizResponse>>();
            return new BusinessResult(Const.HTTP_STATUS_OK, "OK", response);
        }
        public async Task<IBusinessResult> GetAllQuizzesPaging(string? search, int pageNumber, int pageSize)
        {
            Expression<Func<Quiz, bool>> predicate = null;

            if (!string.IsNullOrWhiteSpace(search))
            {
                predicate = q => q.Title.Contains(search);
            }

            var quizzes = await _unitOfWork.QuizRepository.GetPagedAsync(
                pageNumber,
                pageSize,
                predicate: predicate,
                orderBy: q => q.OrderByDescending(x => x.CreatedAt)
            );

            var response = quizzes.Adapt<List<QuizResponse>>();

            return new BusinessResult(Const.HTTP_STATUS_OK, "OK", response);
        }


        public async Task<IBusinessResult> AddImageToQuestion(int questionId, ImageUpload request)
        {
            var question = await _unitOfWork.QuestionRepository
                .GetByWhere(q => q.QuestionId == questionId)
                .FirstOrDefaultAsync();

            if (question == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Question not found");

            try
            {
                var cloudinaryHelper = new CloudinaryHelper();
                question.ImgUrl = await cloudinaryHelper.UploadImageWithCloudDinary(request.ImgUrl);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION,
                    "Upload image failed: " + ex.Message);
            }
            question.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.QuestionRepository.UpdateAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Image added to question", question);
        }
        public async Task<IBusinessResult> DeleteQuestion(int questionId)
        {
            var question = await _unitOfWork.QuestionRepository
                .GetByWhere(q => q.QuestionId == questionId)
                .Include(q => q.Answers)
                .FirstOrDefaultAsync();

            if (question == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Question not found");

            await _unitOfWork.QuestionRepository.DeleteAsync(question);
            await _unitOfWork.SaveChangesAsync();

            return new BusinessResult(Const.HTTP_STATUS_OK, "Question deleted successfully");
        }
        public async Task<IBusinessResult> ImportQuestionsFromFile(int quizId, IFormFile file)
        {
            // --- 1) Kiểm tra đăng nhập & quyền ---
            var userIdClaim = GetUserIdClaim();
            if (string.IsNullOrEmpty(userIdClaim))
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "User chưa đăng nhập hoặc không hợp lệ");
            var userId = int.Parse(userIdClaim);

            var quiz = await _unitOfWork.QuizRepository
                .GetByWhere(q => q.QuizId == quizId && q.CreatedBy == userId)
                .Include(q => q.Questions).ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();
            if (quiz == null)
                return new BusinessResult(Const.HTTP_STATUS_NOT_FOUND, "Quiz không tồn tại hoặc không phải của bạn");

            // --- 2) Đọc file vào List<CsvQuestionDto> ---
            List<CsvQuestionDto> records;
            var ext = Path.GetExtension(file.FileName).ToLower();

            if (ext == ".csv")
            {
                // a) Cấu hình CsvHelper để bỏ qua bad data, missing field...
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    IgnoreBlankLines = true,
                    Delimiter = ",",
                    BadDataFound = null,
                    MissingFieldFound = null,
                    HeaderValidated = null
                };

                using var reader = new StreamReader(file.OpenReadStream());
                using var csv = new CsvReader(reader, config);
                records = csv.GetRecords<CsvQuestionDto>().ToList();
            }
            else if (ext == ".xlsx" || ext == ".xls")
            {
                // b) Dùng ClosedXML đọc Excel
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                using var workbook = new XLWorkbook(ms);
                var sheet = workbook.Worksheets.First();
                // Giả sử header nằm ở dòng 1, dữ liệu bắt đầu từ dòng 2
                records = sheet.RowsUsed()
                               .Skip(1)
                               .Select(r => new CsvQuestionDto
                               {
                                   QuestionText = r.Cell(1).GetString().Trim(),
                                   Answer1 = r.Cell(2).GetString().Trim(),
                                   Answer2 = r.Cell(3).GetString().Trim(),
                                   Answer3 = r.Cell(4).GetString().Trim(),
                                   Answer4 = r.Cell(5).GetString().Trim(),
                                   TimeLimitSec = r.Cell(6).GetValue<int>(),
                                   CorrectAnswers = r.Cell(7).GetString().Trim()
                               })
                               .ToList();
            }
            else
            {
                return new BusinessResult(Const.HTTP_STATUS_BAD_REQUEST, "Chỉ hỗ trợ file .csv, .xlsx hoặc .xls");
            }

            // --- 3) Map sang QuestionRequest và AnswerRequest ---
            var questionRequests = new List<QuestionRequest>();
            foreach (var dto in records)
            {
                var corrects = dto.GetCorrectIndexes().ToHashSet();
                var answers = new List<AnswerRequest>();

                if (!string.IsNullOrWhiteSpace(dto.Answer1))
                    answers.Add(new AnswerRequest { AnswerText = dto.Answer1, IsCorrect = corrects.Contains(1) });
                if (!string.IsNullOrWhiteSpace(dto.Answer2))
                    answers.Add(new AnswerRequest { AnswerText = dto.Answer2, IsCorrect = corrects.Contains(2) });
                if (!string.IsNullOrWhiteSpace(dto.Answer3))
                    answers.Add(new AnswerRequest { AnswerText = dto.Answer3, IsCorrect = corrects.Contains(3) });
                if (!string.IsNullOrWhiteSpace(dto.Answer4))
                    answers.Add(new AnswerRequest { AnswerText = dto.Answer4, IsCorrect = corrects.Contains(4) });

                questionRequests.Add(new QuestionRequest
                {
                    QuestionText = dto.QuestionText,
                    TimeLimitSec = dto.TimeLimitSec,
                    IsRandomAnswer = false,
                    Answers = answers
                });
            }

            // --- 4) Tái sử dụng logic thêm câu hỏi có sẵn ---
            var result = await AddQuestionsToQuiz(quizId, questionRequests);
            return result;
        }
    }
}

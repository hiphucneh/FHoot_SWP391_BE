﻿using Kahoot.Common.BusinessResult;
using Kahoot.Repository.Models;
using Kahoot.Service.Model.Request;
using Kahoot.Service.ModelDTOs.Request;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Service.Interface
{
    public interface IQuizService
    {
        Task<IBusinessResult> FindQuizById(int id);
        Task<IBusinessResult> CreateQuiz(QuizRequest request);
        Task<IBusinessResult> DeleteQuiz(int quizId);
        Task<IBusinessResult> GetMyQuizzes();
        Task<IBusinessResult> UpdateQuiz(int quizId, QuizRequest request);
        Task<IBusinessResult> AddQuestionsToQuiz(int quizId, List<QuestionRequest> questionRequests);
        Task<IBusinessResult> UpdateQuestionsForQuiz(int quizId, List<QuestionRequest> questionRequests);
        Task<IBusinessResult> DeleteQuestion(int questionId);
        Task<IBusinessResult> AddImageToQuestion(int questionId, ImageUpload request);
        Task<IBusinessResult> SortOrderAsync(int quizId, int questionId, int newSortOrder);
        Task<IBusinessResult> ImportQuestionsFromFile(int quizId, IFormFile file);
        Task<IBusinessResult> GetAllQuizzesPaging(string? search, int pageNumber, int pageSize);
        Task<IBusinessResult> CreateQuizAI(CreateQuizAIRequest request);
        Task<IBusinessResult> GenerateAnswersForQuestionAI(string content);

    }
}

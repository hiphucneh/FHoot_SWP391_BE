using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using Kahoot.Repository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kahoot.Repository.Repositories
{
    public class QuestionSessionRepository : GenericRepository<QuestionSession>, IQuestionSessionRepository
    {
        public QuestionSessionRepository(KahootContext context) : base(context)
        {
        }
    }
}
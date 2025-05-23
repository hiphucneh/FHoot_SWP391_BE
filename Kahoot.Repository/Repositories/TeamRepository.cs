﻿using Kahoot.Repository.Base;
using Kahoot.Repository.Interface;
using Kahoot.Repository.Models;
using System;

namespace Kahoot.Repository.Repositories
{
    public class TeamRepository : GenericRepository<Team>, ITeamRepository
    {
        public TeamRepository(KahootContext context) : base(context)
        {
        }
    }

}
﻿using Fricke.Authorization.Core.Data;
using Microsoft.EntityFrameworkCore;

namespace Authorization.Core.Tests.Data
{
    public class AppDbContext : AuthDbContext<AppUser, AppRole>
    {
        /// <summary>
        /// Used for testing.
        /// </summary>
        public AppDbContext()
        { }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using oAuthJWT.Models;

namespace oAuthJWT
{
    public class oAuthJTWContext : DbContext
    {
        public DbSet<User> Users { get; set; }

        public oAuthJTWContext(DbContextOptions<oAuthJTWContext> options) : base(options) {}
    }
}
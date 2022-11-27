using Allup.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Allup.DAL
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<Language> Languages { get; set; }
    }
}

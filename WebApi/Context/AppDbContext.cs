using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Context
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Crm> Crms { get; set; }
    }
}

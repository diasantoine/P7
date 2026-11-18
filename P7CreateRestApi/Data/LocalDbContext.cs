using Dot.Net.WebApi.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dot.Net.WebApi.Data
{
    public class LocalDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public LocalDbContext(DbContextOptions<LocalDbContext> options) : base(options) { }

        public DbSet<BidList> BidLists => Set<BidList>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BidList>().ToTable("BidList");
        }
    }
}

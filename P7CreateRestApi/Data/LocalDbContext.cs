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
        public DbSet<CurvePoint> CurvePoints => Set<CurvePoint>();
        public DbSet<Rating> Ratings => Set<Rating>();
        public DbSet<RuleName> RuleNames => Set<RuleName>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BidList>().ToTable("BidList");
            builder.Entity<CurvePoint>().ToTable("CurvePoint");
            builder.Entity<Rating>().ToTable("Rating");
            builder.Entity<RuleName>().ToTable("RuleName");
        }
    }
}

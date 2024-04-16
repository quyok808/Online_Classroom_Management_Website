using DoAnMon.IdentityCudtomUser;
using DoAnMon.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoAnMon.Data
{
    public class ApplicationDbContext : IdentityDbContext<CustomUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<BaiTap> baiTaps { get; set; }
        public DbSet<BaiTapDetail> baiTapsDetail { get; set; }
        public DbSet<ClassRoom> classRooms { get; set; }
        public DbSet<ClassroomDetail> classroomDetail { get; set; }
        public DbSet<BaiGiang> BaiGiang { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BaiTapDetail>()
                .HasKey(b => new { b.ClassId, b.BaiTapId });

            modelBuilder.Entity<ClassroomDetail>()
                .HasKey(b => new { b.ClassId, b.UserId });

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            });

            modelBuilder.Entity<ClassroomDetail>()
                .HasOne(cd => cd.Role)
                .WithMany()
                .HasForeignKey(cd => cd.RoleId)
                .IsRequired(false);

			modelBuilder.Entity<ClassRoom>()
				.HasMany(c => c.BaiGiangs)
				.WithOne(bg => bg.ClassRoom)
				.HasForeignKey(bg => bg.ClassId);

		}

    }
}

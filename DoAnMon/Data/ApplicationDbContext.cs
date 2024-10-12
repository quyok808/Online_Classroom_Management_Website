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

		protected ApplicationDbContext()
		{
		}

		public DbSet<BaiTap> baiTaps { get; set; }
        public DbSet<ClassRoom> classRooms { get; set; }
        public DbSet<ClassroomDetail> classroomDetail { get; set; }
        public DbSet<BaiGiang> BaiGiang { get; set; }
        public DbSet<Message> Messages { get; set; }
		public DbSet<BaiNop> BaiNop { get; set; }
        public DbSet<BangDiem> bangDiem { get; set; }
        public DbSet<DiemDanh> diemDanh { get; set; }
		public DbSet<FileAttachment> FileAttachments { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder) 
        { 

            modelBuilder.Entity<ClassroomDetail>()
                .HasKey(b => new { b.ClassRoomId, b.UserId });

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            });

			modelBuilder.Entity<ClassRoom>()
				.HasMany(c => c.BaiGiangs)
				.WithOne(bg => bg.ClassRoom)
				.HasForeignKey(bg => bg.ClassId);

			modelBuilder.Entity<Message>()
			.HasMany(m => m.FileAttachments)
			.WithOne(f => f.Message)
			.HasForeignKey(f => f.MessageId);


		}

	}
}

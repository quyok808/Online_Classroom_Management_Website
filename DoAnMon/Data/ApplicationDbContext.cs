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
        public DbSet<Post> posts { get; set; }
        public DbSet<LeaveRequest> leaveRequest { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Criterion> Criteria { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<Rubric> Rubric { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder) 
        { 

            modelBuilder.Entity<ClassroomDetail>()
                .HasKey(b => new { b.ClassRoomId, b.UserId });
            
            modelBuilder.Entity<IdentityUserLogin<string>>(entity =>
            {
                entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });
            });

			modelBuilder.Entity<ClassRoom>()
				.HasMany(c => c.BaiGiangs)
				.WithOne(bg => bg.ClassRoom)
				.HasForeignKey(bg => bg.ClassId);

            // Cấu hình quan hệ Criterion - Rubric
            modelBuilder.Entity<Criterion>()
                .HasOne(c => c.Rubric)
                .WithMany(r => r.Criteria)
                .HasForeignKey(c => c.RubricId)
                .OnDelete(DeleteBehavior.Restrict); // Tránh cascade

            // Cấu hình quan hệ Evaluation - Criterion
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Criterion)
                .WithMany(c => c.Evaluations)
                .HasForeignKey(e => e.CriterionId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade xóa nếu cần

            // Cấu hình quan hệ Evaluation - Student
            modelBuilder.Entity<Evaluation>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Evaluations)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade xóa nếu cần

            // Cấu hình quan hệ Student - Rubric
            modelBuilder.Entity<Student>()
                .HasOne(s => s.Rubric)
                .WithMany(r => r.Student)
                .HasForeignKey(s => s.RubricId)
                .OnDelete(DeleteBehavior.Restrict); // Tránh cascade

            // Configure the one-to-one relationship between ClassRoom and Rubric
            modelBuilder.Entity<ClassRoom>()
                .HasOne(c => c.Rubric) // ClassRoom has one Rubric
                .WithOne(r => r.ClassRoom) // Rubric has one ClassRoom
                .HasForeignKey<Rubric>(r => r.ClassRoomId); // Rubric is the dependent and has the foreign key

            base.OnModelCreating(modelBuilder);
        }

	}
}

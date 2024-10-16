using DoAnMon.Data;
using DoAnMon.Models;
using DoAnMon.SendMail;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore; // Đừng quên thêm using này
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DoAnMon.IdentityCudtomUser;

public class EmailSchedulerService : IHostedService, IDisposable
{
    private Timer _timer;
    private readonly IServiceProvider _serviceProvider; 
    private readonly Mail _mail;

    public EmailSchedulerService(IServiceProvider serviceProvider) // Sử dụng IServiceProvider
    {
        _serviceProvider = serviceProvider; // Lưu IServiceProvider
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CheckAndSendEmails, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private async void CheckAndSendEmails(object state)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var mail = scope.ServiceProvider.GetRequiredService<Mail>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // Lấy ApplicationDbContext từ scope

            var classes = await GetClassesWithUpcomingSessionsAsync(context);

            foreach (var classRoom in classes)
            {
                var startTime = classRoom.StartDate.Add(classRoom.StartTime);
                if (startTime <= DateTime.Now.AddMinutes(30) && startTime >= DateTime.Now)
                {
                    var subject = $"Nhắc nhở: Lớp học {classRoom.Name} sắp diễn ra!";
                    var body = $"Chào bạn,<br><br>Xin nhắc nhở rằng lớp học {classRoom.Name} sẽ diễn ra vào {startTime}.<br><br>Chúc bạn học tốt!";

                    // Lấy danh sách sinh viên từ ClassroomDetail với context
                    var students = await GetStudentsInClassAsync(classRoom.Id, context);

                    foreach (var student in students)
                    {
                        if (!string.IsNullOrEmpty(student.Email))
                        {
                            await mail.SendEmailAsync(student.Email, subject, body);
                        }
                    }
                }
            }
        }
    }

    private async Task<List<CustomUser>> GetStudentsInClassAsync(string classRoomId, ApplicationDbContext context)
    {
        return await context.classroomDetail
            .Where(cd => cd.ClassRoomId == classRoomId)
            .Select(cd => cd.User) // Lấy danh sách sinh viên từ ClassroomDetail
            .ToListAsync();
    }

    private async Task<List<ClassRoom>> GetClassesWithUpcomingSessionsAsync(ApplicationDbContext context)
    {
        var now = DateTime.Now;
        return await context.classRooms
            .Include(c => c.User) // Nạp thêm User vào ClassRoom
            .Where(c =>
                c.StartDate.Date == now.Date &&
                c.StartTime > now.TimeOfDay &&
                c.StartTime <= now.TimeOfDay.Add(TimeSpan.FromMinutes(30)))
            .ToListAsync();
    }



    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}


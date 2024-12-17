using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using DoAnMon.Models;
using Microsoft.Extensions.Options;

namespace DoAnMon.SendMail
{
    public class Mail
    {
        private readonly SmtpSettings _smtpSettings;

        public Mail(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string body)
        {
            // Kiểm tra địa chỉ email
            if (!IsValidEmail(email))
            {
                throw new FormatException("Địa chỉ email không hợp lệ.");
            }

            using (var smtpClient = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
                smtpClient.EnableSsl = _smtpSettings.EnableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(email);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

		public async Task SendEmailsInBatchesAsync(List<string> emails, string subject, string body, int batchSize = 50)
		{
			var invalidEmails = emails.Where(email => !IsValidEmail(email)).ToList();
			if (invalidEmails.Any())
			{
				throw new FormatException($"Các địa chỉ email không hợp lệ: {string.Join(", ", invalidEmails)}");
			}

			using (var smtpClient = new SmtpClient(_smtpSettings.Server, _smtpSettings.Port))
			{
				smtpClient.Credentials = new NetworkCredential(_smtpSettings.Username, _smtpSettings.Password);
				smtpClient.EnableSsl = _smtpSettings.EnableSsl;

				for (int i = 0; i < emails.Count; i += batchSize)
				{
					var batch = emails.Skip(i).Take(batchSize).ToList();

					var mailMessage = new MailMessage
					{
						From = new MailAddress(_smtpSettings.SenderEmail, _smtpSettings.SenderName),
						Subject = subject,
						Body = body,
						IsBodyHtml = true,
					};

					foreach (var email in batch)
					{
						mailMessage.To.Add(email);
					}

					try
					{
						await smtpClient.SendMailAsync(mailMessage);
						Console.WriteLine($"Đã gửi email đến nhóm: {string.Join(", ", batch)}");
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Lỗi khi gửi email nhóm: {ex.Message}");
					}

					// Xóa danh sách email trong `To` để tái sử dụng
					mailMessage.To.Clear();
				}
			}
		}


		// Phương thức kiểm tra địa chỉ email
		private bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}

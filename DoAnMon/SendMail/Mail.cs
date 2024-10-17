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

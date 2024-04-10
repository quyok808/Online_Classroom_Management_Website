using System.Net.Mail;
using System.Net;

namespace DoAnMon.SendMail
{
    public class Mail
    {
        public async Task<bool> SendEmailAsync(string email, string subject, string confirmLink)
        {
            try
            {
                MailMessage message = new MailMessage("ONLYA@gmail.com", email, subject, confirmLink);
                message.IsBodyHtml = true;
                SmtpClient smtpClient = new SmtpClient();
                message.Body = confirmLink;

                smtpClient.Port = 587;
                smtpClient.Host = "smtp.gmail.com";


                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential("quyok8080@gmail.com", "uaab ylsf uikl mnnd"/*password của phần bảo mật khác*/);
                smtpClient.Send(message);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

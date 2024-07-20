using BackendNet.Dtos.Mail;
using BackendNet.Services.IService;
using BackendNet.Setting;
using MailKit.Net.Smtp;
using MimeKit;

namespace BackendNet.Services
{
    public class EmailService : IEmailService
    {
        private EmailSetting emailSetting;
        public EmailService(EmailSetting emailSetting)
        {
            this.emailSetting = emailSetting;
        }

        public async Task SendEmail(SingleMailRequest mailRequest)
        {
            try
            {
                var email = new MimeMessage();

                email.From.Add(new MailboxAddress(emailSetting.DisplayName, emailSetting.Email));
                email.To.Add(MailboxAddress.Parse(mailRequest.ToEmail));
                email.Subject = mailRequest.Subject;

                var body = new BodyBuilder();
                body.HtmlBody = mailRequest.Body;
                email.Body = body.ToMessageBody();

                using (var smtp = new SmtpClient())
                {
                    await smtp.ConnectAsync(emailSetting.Host, emailSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(emailSetting.Email, emailSetting.Password);
                    _ = Task.Run(() =>
                    {
                        try
                        {
                            smtp.Send(email);
                        }
                        finally
                        {
                            smtp.Disconnect(true);
                        }
                    });
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task SendMultiEmail(MultiMailRequest mailRequests)
        {
            try
            {
                var email = new MimeMessage();

                email.From.Add(new MailboxAddress(emailSetting.DisplayName, emailSetting.Email));
               
                email.Subject = mailRequests.Subject;

                var body = new BodyBuilder();
                body.HtmlBody = mailRequests.Body;
                email.Body = body.ToMessageBody();
                email.To.Add(new GroupAddress("undisclosed-recipients"));
                using (var smtp = new SmtpClient())
                {
                    await smtp.ConnectAsync(emailSetting.Host, emailSetting.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync(emailSetting.Email, emailSetting.Password);
                    smtp.Send(message: email, sender: new MailboxAddress(emailSetting.DisplayName, emailSetting.Email), recipients: mailRequests.ToEmails);
                    smtp.Disconnect(true);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

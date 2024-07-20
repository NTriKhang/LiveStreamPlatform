using MimeKit;

namespace BackendNet.Dtos.Mail
{
    public class MultiMailRequest
    {
        public IEnumerable<MailboxAddress> ToEmails { set; get; }
        public string Subject { set; get; }
        public string Body { set; get; }
        public MultiMailRequest(string subject, string body, List<string> toEmails)
        {
            this.Subject = subject;
            this.Body = body;
            ToEmails = toEmails.Select(toEmails => new MailboxAddress(string.Empty, toEmails));
        }
    }
}

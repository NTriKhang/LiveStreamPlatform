using MimeKit;

namespace BackendNet.Dtos.Mail
{
    public class MultiMailRequest
    {
        public IEnumerable<MailboxAddress> ToEmails { set; get; }
        public string Subject { set; get; }
        public string Body { set; get; }
    }
}

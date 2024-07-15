namespace BackendNet.Dtos.Mail
{
    public class SingleMailRequest
    {
        public string ToEmail { set; get; }
        public string Subject { set; get; }
        public string Body { set; get; }
    }
}

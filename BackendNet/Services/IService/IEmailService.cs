using BackendNet.Dtos.Mail;

namespace BackendNet.Services.IService
{
    public interface IEmailService
    {
        Task SendEmail(SingleMailRequest mailRequest);
        Task SendMultiEmail(MultiMailRequest mailRequests);
    }
}

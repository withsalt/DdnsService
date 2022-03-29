using System.Threading.Tasks;

namespace DdnsService.Services
{
    public interface IEmailNoticeService
    {
        Task<(SendStatus, string)> Send(string title
            , string body
            , string[] attachments = null
            , bool isBodyHtml = false);
    }
}

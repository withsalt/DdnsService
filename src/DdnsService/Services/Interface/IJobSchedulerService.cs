using System.Threading.Tasks;

namespace DdnsService.Services
{
    public interface IJobSchedulerService
    {
        Task Start();
    }
}

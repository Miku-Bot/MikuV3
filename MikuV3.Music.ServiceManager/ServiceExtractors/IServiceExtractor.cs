using MikuV3.Music.ServiceManager.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MikuV3.Music.ServiceManager.ServiceExtractors
{
    public interface IServiceExtractor
    {
        Task<List<ServiceResult>> GetServiceResult(string url);
    }
}

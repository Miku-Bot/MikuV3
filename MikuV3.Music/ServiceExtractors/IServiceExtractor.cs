using MikuV3.Music.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.ServiceExtractors
{
    public interface IServiceExtractor
    {
        Task<ServiceResult> GetServiceResult(string url);
    }
}

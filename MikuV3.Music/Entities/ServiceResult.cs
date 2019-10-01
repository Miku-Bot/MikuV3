using MikuV3.Music.Enums;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.Entities
{
    public class ServiceResult
    {
        public ContentService ContentService { get; set; }
        public CacheStatus CacheStatus { get; set; }
        public bool Slow { get; set; }
        public HttpResponseMessage Response { get; set; }
        public MemoryStream PreCache { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public TimeSpan Length { get; set; }
        public string DirectUrl { get; set; }
        public string Url { get; set; }
        public Task FillCacheTask { get; set; }

        public async Task FillCache()
        {
            PreCache = new MemoryStream();
            CacheStatus = CacheStatus.Rendering;
            await Response.Content.CopyToAsync(PreCache);
            CacheStatus = CacheStatus.Cached;
        }
    }
}

using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Enums;
using MikuV3.Music.ServiceManager.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.ServiceManager.ServiceExtractors
{
    public class YoutubeSingle : IServiceExtractor
    {
        HttpClient _c { get; set; }

        /// <summary>
        /// Get the ServiceResult from a single Youtube video, its fast so no caching
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<ServiceResult>> GetServiceResult(string url)
        {
            _c = new HttpClient();
            var psi = new ProcessStartInfo()
            {
                FileName = @"youtube-dl.exe",
                Arguments = $"-i --no-warnings -J --no-playlist \"{url}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ytDlprocess = Process.Start(psi);
            var ytDlJson = await ytDlprocess.StandardOutput.ReadToEndAsync();
            var ytDlResult = JsonConvert.DeserializeObject<YTDL.Root>(ytDlJson);
            var bestAudio = ytDlResult.formats.OrderByDescending(x => x.abr).First();
            var urls = new List<string>();
            if (bestAudio.fragments?.Count() != 0 && bestAudio.fragments != null)
            {
                foreach (var fragments in bestAudio.fragments)
                {
                    urls.Add($"{bestAudio.fragment_base_url}{fragments.path}");
                }
            }
            else
            {
                urls.Add(bestAudio.url);
            }
            var serviceResult = new List<ServiceResult>();
            serviceResult.Add(new ServiceResult(ContentService.Youtube,
                Playlist.No,
                _c,
                TimeSpan.FromSeconds(ytDlResult.duration),
                urls,
                url,
                TimeConversion.ParseYTDLDate(ytDlResult.upload_date),
                ytDlResult.uploader,
                ytDlResult.uploader_url,
                ytDlResult.title,
                ytDlResult.thumbnail));
            return serviceResult;
        }
    }
}

using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MikuV3.Music.ServiceManager.ServiceExtractors
{
    public class BilibiliSingle : IServiceExtractor
    {
        HttpClient _c { get; set; }

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
            var ytDLProcess = Process.Start(psi);
            var ytDlJson = await ytDLProcess.StandardOutput.ReadToEndAsync();
            var ytDlResult = JsonConvert.DeserializeObject<YTDL.Root>(ytDlJson);
            var bestAudio = ytDlResult.formats.OrderByDescending(x => x.abr).First();
            var urls = new List<string>();
            if (bestAudio.fragments?.Count() != 0 && bestAudio.fragments != null)
            {
                foreach (var fragment in bestAudio.fragments)
                {
                    urls.Add($"{bestAudio.fragment_base_url}{fragment.path}");
                }
            }
            else
            {
                urls.Add(bestAudio.url);
            }
            var ServiceResults = new List<ServiceResult>();

            //Bilibili needs these set to allow downloading/access to the videofile
            _c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:68.0) Gecko/20100101 Firefox/68.0");
            _c.DefaultRequestHeaders.Add("Referer", url);

            ServiceResults.Add(new ServiceResult(Enums.ContentService.BiliBili,
                Enums.Playlist.No,
                _c,
                TimeSpan.FromSeconds(ytDlResult.duration),
                urls,
                url,
                TimeConversion.ParseYTDLDate(ytDlResult.upload_date),
                ytDlResult.uploader,
                "https://space.bilibili.com/" + ytDlResult.uploader_id,
                ytDlResult.title,
                ytDlResult.thumbnail));
            return ServiceResults;
        }
    }
}

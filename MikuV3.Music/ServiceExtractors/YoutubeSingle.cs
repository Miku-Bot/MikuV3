using MikuV3.Music.Entities;
using MikuV3.Music.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.ServiceExtractors
{
    public class YoutubeSingle : IServiceExtractor
    {
        HttpClient _c { get; set; }
        YTDL.Root ytdlGot { get; set; }

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
            var ytdl = Process.Start(psi);
            var json = await ytdl.StandardOutput.ReadToEndAsync();
            ytdlGot = JsonConvert.DeserializeObject<YTDL.Root>(json);
            var aud = ytdlGot.formats.OrderByDescending(x => x.abr).First();
            var urls = new List<string>();
            if (aud.fragments?.Count() != 0 && aud.fragments != null)
            {
                foreach (var frag in aud.fragments)
                {
                    urls.Add($"{aud.fragment_base_url}{frag.path}");
                }
            }
            else
            {
                urls.Add(aud.url);
            }
            var sr = new List<ServiceResult>();
            sr.Add(new ServiceResult(Enums.ContentService.Youtube, Enums.Playlist.No, _c, TimeSpan.FromSeconds(ytdlGot.duration), urls, url, TimeConversion.ParseYTDLDate(ytdlGot.upload_date), null, ytdlGot.thumbnail, ytdlGot.uploader, ytdlGot.uploader_url, ytdlGot.title));
            //sr.FillCacheTask = Task.Run(sr.FillCache);
            return sr;
        }
    }
}

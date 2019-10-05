﻿using MikuV3.Music.Entities;
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
    public class BilibiliSingle : IServiceExtractor
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
            _c.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:68.0) Gecko/20100101 Firefox/68.0");
            _c.DefaultRequestHeaders.Add("Referer", url);
            sr.Add(new ServiceResult(Enums.ContentService.BiliBili, _c, TimeSpan.FromSeconds(ytdlGot.duration), urls, url, null, ytdlGot.uploader, ytdlGot.title));
            //sr.FillCacheTask = Task.Run(sr.FillCache);
            return sr;
        }
    }
}

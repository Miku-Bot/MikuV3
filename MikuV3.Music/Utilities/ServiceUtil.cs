using MikuV3.Music.Entities;
using MikuV3.Music.Enums;
using Newtonsoft.Json;
using NYoutubeDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.Utilities
{
    public class ServiceUtil
    {
        public async Task<ContentService> GetService(string url)
        {
            var psi = new ProcessStartInfo
            {
                FileName = @"youtube-dl.exe",
                Arguments = $@"-J -i -f bestaudio --playlist-items 1 --no-warnings ""{url}""",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ytdlp = Process.Start(psi);
            var json = await ytdlp.StandardOutput.ReadToEndAsync();
            var ytdl = JsonConvert.DeserializeObject<YTDL.Root>(json);
            ytdlp.Close();
            if (ytdl?.extractor.ToLower() == "youtube" | ytdl?.extractor.ToLower() == "youtube:playlist")
            {
                return ContentService.Youtube;
            }
            return ContentService.Search;
        }
    }
}

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
            ytDlprocess.Dispose();
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

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //_c.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~YoutubeSingle()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}

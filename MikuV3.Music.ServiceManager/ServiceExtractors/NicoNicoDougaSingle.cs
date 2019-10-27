using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Enums;
using MikuV3.Music.ServiceManager.Exceptions;
using MikuV3.Music.ServiceManager.Helpers;
using Newtonsoft.Json;

namespace MikuV3.Music.ServiceManager.ServiceExtractors
{
    public class NicoNicoDougaSingle : IServiceExtractor
    {
        HttpClientHandler handler { get; set; }
        HttpClient _c { get; set; }
        YTDL.Root ytDlResult { get; set; }
        string url { get; set; }
        Task LoginTask { get; set; }
        Task UriTask { get; set; }

        public NicoNicoDougaSingle()
        {
            var cc = new CookieContainer();
            handler = new HttpClientHandler
            {
                CookieContainer = cc,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                UseCookies = true,
                UseDefaultCredentials = false
            };
            _c = new HttpClient(handler);
        }

        /// <summary>
        /// Get the ServiceResult from NND, Its slow, so it gets cached immediately
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<ServiceResult>> GetServiceResult(string url)
        {
            this.url = url;
            LoginTask = Task.Run(DoLogin);
            UriTask = Task.Run(GetDirectUri);
            await Task.WhenAll(LoginTask, UriTask);
            var serviceResult = new List<ServiceResult>();
            var bestVids = ytDlResult.formats.OrderByDescending(x => x.abr);
            var bestVids2 = bestVids.Where(x => x.abr == bestVids.First().abr);
            var bestAudio = bestVids2.OrderBy(x => x.height).First();
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
                ytDlResult.thumbnail, true));
            return serviceResult;
        }

        /// <summary>
        /// Log into NND with the supplied mail and password
        /// </summary>
        /// <returns></returns>
        private async Task DoLogin()
        {
            string loginForm = $"mail={ServiceResolver.NicoNicoDougaConfig.Mail}&password={ServiceResolver.NicoNicoDougaConfig.Password}&site=nicometro";
            var body = new StringContent(loginForm, Encoding.UTF8, "application/x-www-form-urlencoded");
            string login = "https://secure.nicovideo.jp/secure/login?site=niconico";
            body.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var doLogin = await _c.PostAsync(new Uri(login), body);
        }

        /// <summary>
        /// Gets the direct URL (and also sets the metadata)
        /// </summary>
        /// <returns></returns>
        private async Task GetDirectUri()
        {
            var split = url.Split("/".ToCharArray());
            var nndID = split.First(x => x.StartsWith("sm") || x.StartsWith("nm")).Split("?")[0];
            var psi = new ProcessStartInfo()
            {
                FileName = @"youtube-dl.exe",
                Arguments = $"-i --no-warnings -J --no-playlist -u {ServiceResolver.NicoNicoDougaConfig.Mail} -p {ServiceResolver.NicoNicoDougaConfig.Password} \"https://www.nicovideo.jp/watch/{nndID}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ytDlprocess = Process.Start(psi);
            var ytDlJson = await ytDlprocess.StandardOutput.ReadToEndAsync();
            ytDlprocess.Dispose();
            ytDlResult = JsonConvert.DeserializeObject<YTDL.Root>(ytDlJson);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~NicoNicoDougaSingle()
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

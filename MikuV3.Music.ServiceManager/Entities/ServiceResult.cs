using MikuV3.Music.ServiceManager.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MikuV3.Music.ServiceManager.Entities
{
    public class ServiceResult : IDisposable
    {
        public ContentService ContentService { get; set; }
        public Playlist Playlist { get; set; }
        public CacheStatus CacheStatus { get; set; }
        HttpClient _c { get; set; }
        public ConcurrentQueue<Stream> PCMQueue { get; set; }
        public string Artist { get; }
        public string ArtistUrl { get; }
        public string ThumbnailUrl { get; }
        public DateTime UploadDate { get; }
        public string Title { get; }
        public TimeSpan Length { get; }
        public Stopwatch CurrentPosition { get; }
        List<string> DirectUrls { get; }
        public string Url { get;}
        public Task FillCacheTask { get; private set; }
        public bool Slow { get; set; }
        long ContentLength = 0;
        long Status = 0;
        int Percentage = 0;

        public ServiceResult(ContentService contentService,
            Playlist playlist,
            HttpClient client,
            TimeSpan length,
            List<string> directUrls, 
            string url,
            DateTime uploadDate,
            string artist = "n/a",
            string artistUrl = "n/a",
            string title = "n/a",
            string thumbnailUrl = null,
            bool slow = false)
        {
            ContentService = contentService;
            Playlist = playlist;
            _c = client;
            Length = length;
            ThumbnailUrl = thumbnailUrl;
            ArtistUrl = artistUrl;
            UploadDate = uploadDate;
            DirectUrls = directUrls;
            Url = url;
            Artist = artist;
            Title = title;
            Slow = slow;
            CurrentPosition = new Stopwatch();
        }

        /// <summary>
        /// Start caching the PCM data
        /// </summary>
        /// <returns></returns>
        public TaskStatus StartCaching()
        {
            if (FillCacheTask == null)
            {
                FillCacheTask = Task.Run(FillCache);
                return FillCacheTask.Status;
            }
            return FillCacheTask.Status;
        }

        /// <summary>
        /// Starts FFMPEG and converts the data from the provided URL(s) to raw PCM
        /// </summary>
        /// <returns></returns>
        private async Task FillCache()
        {
            try
            {
                await Task.Delay(0);
                CacheStatus = CacheStatus.Rendering;
                var psi2 = new ProcessStartInfo()
                {
                    FileName = @"ffmpeg.exe",
                    Arguments = $@"-hide_banner -loglevel panic -i - -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi2);
                var inputTask = Task.Run(async () =>
                {
                    try
                    {
                        foreach (var part in DirectUrls)
                        {
                            var Response = await _c.GetAsync(part, HttpCompletionOption.ResponseHeadersRead);
                            ContentLength += (long)Response.Content.Headers.ContentLength;
                        }
                        foreach (var part in DirectUrls)
                        {
                            var Response = await _c.GetAsync(part, HttpCompletionOption.ResponseHeadersRead);
                            try
                            {
                                using (var thedata = await Response.Content.ReadAsStreamAsync())
                                {
                                    int read = -1;
                                    while (read != 0)
                                    {
                                        var cacheMemoryStream = new MemoryStream();
                                        byte[] buffer = new byte[3840];
                                        if (Slow)
                                        {
                                            while (((read = thedata.Read(buffer, 0, buffer.Length)) > 0))
                                            {
                                                Status += Convert.ToInt64(read);
                                                var statusMath = (100.0 / Convert.ToDouble(ContentLength)) * Convert.ToDouble(Status);
                                                Percentage = Convert.ToInt32(statusMath);
                                                cacheMemoryStream.Write(buffer, 0, read);
                                                if (Percentage > 65) break;
                                            }
                                            CacheStatus = CacheStatus.PlayReady;
                                            Slow = false;
                                        }
                                        else
                                        {
                                            var statusMath = (100.0 / Convert.ToDouble(ContentLength)) * Convert.ToDouble(Status);
                                            Percentage = Convert.ToInt32(statusMath);
                                            read = thedata.Read(buffer, 0, buffer.Length);
                                            cacheMemoryStream.Write(buffer, 0, read);
                                            Status += Convert.ToInt64(read);
                                        }
                                        cacheMemoryStream.Position = 0;
                                        await cacheMemoryStream.CopyToAsync(ffmpeg.StandardInput.BaseStream, 3840);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }
                        ffmpeg.StandardInput.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
                var outputTask = Task.Run(async () => {
                    await Task.Delay(0);
                    var ms = new MemoryStream();
                    PCMQueue = new ConcurrentQueue<Stream>();
                    var ffout = ffmpeg.StandardOutput.BaseStream;
                    int read;
                    byte[] buffer = new byte[3840];
                    while ((read = ffout.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        ms.Write(buffer, 0, read);
                        ms.Position = 0;
                        PCMQueue.Enqueue(ms);
                        //ms.Dispose();
                        ms = new MemoryStream();
                        //Console.WriteLine(PCMQueue.Count);
                    }
                    ffmpeg.StandardOutput.Close();
                    Percentage = 100;
                    ffmpeg.Dispose();
                    CacheStatus = CacheStatus.Cached;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //_c?.Dispose();
                    PCMQueue?.Clear();
                    DirectUrls?.Clear();
                    FillCacheTask?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ServiceResult()
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
            //GC.SuppressFinalize(this);
        }
        #endregion
    }
}

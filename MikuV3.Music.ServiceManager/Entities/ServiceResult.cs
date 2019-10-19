using MikuV3.Music.ServiceManager.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace MikuV3.Music.ServiceManager.Entities
{
    public class ServiceResult
    {
        public ContentService ContentService { get; set; }
        public Playlist Playlist { get; set; }
        public CacheStatus CacheStatus { get; set; }
        HttpClient _c { get; set; }
        public Stream PCMCache { get; set; }
        public string Artist { get; set; }
        public string ArtistUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime UploadDate { get; set; }
        public string Title { get; set; }
        public TimeSpan Length { get; set; }
        //Dont know if this will work out for keeping track of the trackposition, but might work
        public Stopwatch CurrentPosition = new Stopwatch();
        public List<string> DirectUrls { get; set; }
        public string Url { get; set; }
        public Task FillCacheTask { get; set; }
        public bool Slow = false;
        Task statusTask { get; set; }
        public long ContentLength = 0;
        public long Status = 0;
        public int Percentage = 0;

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
        }

        public async Task FillCache()
        {
            try
            {
                await Task.Delay(0);
                CacheStatus = CacheStatus.Rendering;
                var psi2 = new ProcessStartInfo()
                {
                    FileName = @"ffmpeg.exe",
                    Arguments = $@"-i - -ac 2 -f s16le -ar 48000 pipe:1",
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi2);
                var inputTask = Task.Run(async () =>
                {
                    try
                    {
                        foreach(var part in DirectUrls)
                        {
                            var Response = await _c.GetAsync(part, HttpCompletionOption.ResponseHeadersRead);
                            ContentLength += (long)Response.Content.Headers.ContentLength;
                        }
                        foreach (var part in DirectUrls)
                        {
                            var Response = await _c.GetAsync(part, HttpCompletionOption.ResponseHeadersRead);
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
                                    await cacheMemoryStream.CopyToAsync(ffmpeg.StandardInput.BaseStream);
                                }
                            }
                        }
                        ffmpeg.StandardInput.Close();
                        Percentage = 100;
                        CacheStatus = CacheStatus.Cached;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                });
                var ffout = ffmpeg.StandardOutput.BaseStream;
                PCMCache = new BufferedStream(ffout);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public async Task PrintStatus()
        {
            do
            {
                await Task.Delay(10000);
                Console.WriteLine(Percentage + "% | " + Status + "/" + ContentLength);
            } while (Percentage < 100);
        }
    }
}

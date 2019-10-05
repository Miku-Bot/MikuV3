using MikuV3.Music.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MikuV3.Music.Entities
{
    public class ServiceResult
    {
        public ContentService ContentService { get; set; }
        public CacheStatus CacheStatus { get; set; }
        public HttpClient Client { get; set; }
        public HttpResponseMessage ResponseMsg { get; set; }
        public Stream PCMCache { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public TimeSpan Length { get; set; }
        public Stopwatch CurrentPosition = new Stopwatch();
        public List<string> DirectUrls { get; set; }
        public string Url { get; set; }
        public Task FillCacheTask { get; set; }
        public bool Slow = false;
        public Task StatusTask { get; set; }
        public long ContentLength = 0;
        public long Status = 0;
        public int Percentage = 0;

        public ServiceResult(ContentService cs, HttpClient c, TimeSpan l, List<string> du, string u, HttpResponseMessage resp = null, string a = "n/a", string t = "n/a", bool s = false)
        {
            ContentService = cs;
            Client = c;
            Length = l;
            DirectUrls = new List<string>();
            DirectUrls = du;
            Url = u;
            ResponseMsg = resp;
            Artist = a;
            Title = t;
            Slow = s;
        }

        public async Task FillCache()
        {
            try
            {
                await Task.Delay(0);
                CacheStatus = CacheStatus.Rendering;
                var Response = ResponseMsg;
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
                            Response = await Client.GetAsync(part, HttpCompletionOption.ResponseHeadersRead);
                            ContentLength += (long)Response.Content.Headers.ContentLength;
                        }
                        //StatusTask = Task.Run(PrintStatus);
                        foreach (var part in DirectUrls)
                        {
                            Response = await Client.GetAsync(part, HttpCompletionOption.ResponseHeadersRead);
                            using (var thedata = await Response.Content.ReadAsStreamAsync())
                            {
                                int read = -1;
                                while (read != 0)
                                {
                                    var mem = new MemoryStream();
                                    byte[] buffer = new byte[3840];
                                    if (Slow)
                                    {
                                        while (((read = thedata.Read(buffer, 0, buffer.Length)) > 0))
                                        {
                                            Status += Convert.ToInt64(read);
                                            var stat = (100.0 / Convert.ToDouble(ContentLength)) * Convert.ToDouble(Status);
                                            Percentage = Convert.ToInt32(stat);
                                            mem.Write(buffer, 0, read);
                                            if (Percentage > 65) break;
                                        }
                                        Slow = false;
                                    }
                                    else
                                    {
                                        var stat = (100.0 / Convert.ToDouble(ContentLength)) * Convert.ToDouble(Status);
                                        Percentage = Convert.ToInt32(stat);
                                        read = thedata.Read(buffer, 0, buffer.Length);
                                        mem.Write(buffer, 0, read);
                                        Status += Convert.ToInt64(read);
                                    }
                                    mem.Position = 0;
                                    await mem.CopyToAsync(ffmpeg.StandardInput.BaseStream);
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
                Console.WriteLine("Doing the cache thing");
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

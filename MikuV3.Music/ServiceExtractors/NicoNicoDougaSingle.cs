using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using MikuV3.Music.Entities;
using MikuV3.Music.Exceptions;
using MikuV3.Music.ServiceExtractors.ServiceEntities;
using Newtonsoft.Json;

namespace MikuV3.Music.ServiceExtractors
{
    public class NicoNicoDougaSingle : IServiceExtractor
    {
        HttpClientHandler handler { get; set; }
        public HttpClient _c { get; set; }
        NNDHTML.Root h5 { get; set; }
        NNDFlash.Root fl { get; set; }
        public string Artist { get; set; }
        public string ArtistUrl { get; set; }
        public string ThumbnailUrl { get; set; }
        public DateTime UploadDate { get; set; } 
        public string Title { get; set; }
        public TimeSpan Length { get; set; }
        public List<string> DirectUrls { get; set; }
        public string Url { get; set; }

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
            DirectUrls = new List<string>();
        }

        public async Task<List<ServiceResult>> GetServiceResult(string url)
        {
            var login = await DoLogin();
            if (!login.IsSuccessStatusCode) throw new NNDLoginException(login.ReasonPhrase);
            var durl = await GetDirectUri(url);
            if (durl == null)return null;
            var sr = new List<ServiceResult>();
            sr.Add(new ServiceResult(Enums.ContentService.NicoNicoDouga, Enums.Playlist.No, _c, Length, DirectUrls, Url, UploadDate, null, ThumbnailUrl, Artist, ArtistUrl, Title, true));
            return sr;
        }

        public async Task<HttpResponseMessage> DoLogin()
        {
            string loginForm = $"mail={Bot.config.NndConfig.Mail}&password={Bot.config.NndConfig.Password}&site=nicometro";
            var body = new StringContent(loginForm, Encoding.UTF8, "application/x-www-form-urlencoded");
            string login = "https://secure.nicovideo.jp/secure/login?site=niconico";
            body.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var doLogin = await _c.PostAsync(new Uri(login), body);
            return doLogin;
        }

        public async Task<string> GetDirectUri(string url)
        {
            var split = url.Split("/".ToCharArray());
            var nndID = split.First(x => x.StartsWith("sm") || x.StartsWith("nm")).Split("?")[0];
            //Console.WriteLine(nndID);
            var videoPage = await _c.GetStringAsync(new Uri($"https://www.nicovideo.jp/watch/{nndID}"));
            var parser = new HtmlParser();
            var parsedDoc = await parser.ParseDocumentAsync(videoPage);
            var h5Part = parsedDoc.GetElementById("js-initial-watch-data");
            if (h5Part != null)
                h5 = JsonConvert.DeserializeObject<NNDHTML.Root>(h5Part.GetAttribute("data-api-data"));
            else
                fl = JsonConvert.DeserializeObject<NNDFlash.Root>(parsedDoc.GetElementById("watchAPIDataContainer").TextContent);
            Url = $"https://www.nicovideo.jp/watch/{nndID}";
            if (fl == null)
            {
                Title = h5.video.originalTitle;
                Artist = h5.owner?.nickname == null ? "n/a" : h5.owner.nickname;
                ArtistUrl = "https://www.nicovideo.jp/user/" + (h5.owner?.id == null ? "n/a" : h5.owner.id);
                ThumbnailUrl = h5.video.largeThumbnailURL == null ? h5.video.thumbnailURL : h5.video.largeThumbnailURL;
                Console.WriteLine(h5.video.postedDateTime);
                UploadDate = DateTime.Parse(h5.video.postedDateTime);
                Length = TimeSpan.FromSeconds((int)h5.video.duration);
                DirectUrls.Add(h5.video.smileInfo.url);
                return h5.video.smileInfo.url;
            }
            else 
            {
                var directVideoUri = fl.flashvars.flvInfo.Replace("%253A%252F%252F", "://").Replace("%252F", "/").Replace("%253F", "?").Replace("%253D", "=").Split("%3D").First(x => x.StartsWith("http")).Split("%26")[0];
                Title = fl.videoDetail.title_original;
                Artist = fl.uploaderInfo?.nickname == null ? "n/a" : fl.uploaderInfo.nickname;
                ArtistUrl = "https://www.nicovideo.jp/user/" + (fl.uploaderInfo?.id == null ? "n/a" : fl.uploaderInfo.id);
                ThumbnailUrl = fl.videoDetail.large_thumbnail == null ? fl.videoDetail.thumbnail : fl.videoDetail.large_thumbnail;
                Console.WriteLine(fl.videoDetail.postedAt);
                UploadDate = DateTime.Parse(fl.videoDetail.postedAt);
                Length = TimeSpan.FromSeconds(fl.videoDetail.length);
                DirectUrls.Add(directVideoUri);
                return directVideoUri;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Html.Parser;
using MikuV3.Music.ServiceExtractors;
using MikuV3.Music.ServiceExtractors.ServiceEntities;
using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Exceptions;
using Newtonsoft.Json;

namespace MikuV3.Music.ServiceManager.ServiceExtractors
{
    public class NicoNicoDougaSingle : IServiceExtractor
    {
        HttpClientHandler handler { get; set; }
        HttpClient _c { get; set; }
        NNDHTML.Root htmls5Json { get; set; }
        NNDFlash.Root flashJson { get; set; }
        string artist { get; set; }
        string artistUrl { get; set; }
        string thumbnailUrl { get; set; }
        DateTime uploadDate { get; set; } 
        string title { get; set; }
        TimeSpan length { get; set; }
        List<string> directUrls { get; set; }
        string url { get; set; }

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
            directUrls = new List<string>();
        }

        /// <summary>
        /// Get the ServiceResult from NND, Its slow, so it gets cached immediately
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<List<ServiceResult>> GetServiceResult(string url)
        {
            var login = await DoLogin();
            if (!login.IsSuccessStatusCode) throw new NNDLoginException(login.ReasonPhrase);
            var getDirectUrl = await GetDirectUri(url);
            if (getDirectUrl == null)return null;
            var serviceResults = new List<ServiceResult>();
            serviceResults.Add(new ServiceResult(Enums.ContentService.NicoNicoDouga,
                Enums.Playlist.No,
                _c,
                length,
                directUrls,
                this.url,
                uploadDate,
                artist,
                artistUrl,
                title,
                thumbnailUrl,
                true));
            return serviceResults;
        }

        /// <summary>
        /// Log into NND with the supplied mail and password
        /// </summary>
        /// <returns></returns>
        public async Task<HttpResponseMessage> DoLogin()
        {
            string loginForm = $"mail={ServiceResolver.NicoNicoDougaConfig.Mail}&password={ServiceResolver.NicoNicoDougaConfig.Password}&site=nicometro";
            var body = new StringContent(loginForm, Encoding.UTF8, "application/x-www-form-urlencoded");
            string login = "https://secure.nicovideo.jp/secure/login?site=niconico";
            body.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var doLogin = await _c.PostAsync(new Uri(login), body);
            return doLogin;
        }

        /// <summary>
        /// Gets the direct URL (and also sets the metadata)
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public async Task<string> GetDirectUri(string url)
        {
            var split = url.Split("/".ToCharArray());
            var nndID = split.First(x => x.StartsWith("sm") || x.StartsWith("nm")).Split("?")[0];
            var videoPage = await _c.GetStringAsync(new Uri($"https://www.nicovideo.jp/watch/{nndID}"));
            var parser = new HtmlParser();
            var parsedDoc = await parser.ParseDocumentAsync(videoPage);
            var html5Part = parsedDoc.GetElementById("js-initial-watch-data");
            if (html5Part != null)
                htmls5Json = JsonConvert.DeserializeObject<NNDHTML.Root>(html5Part.GetAttribute("data-api-data"));
            else
                flashJson = JsonConvert.DeserializeObject<NNDFlash.Root>(parsedDoc.GetElementById("watchAPIDataContainer").TextContent);
            this.url = $"https://www.nicovideo.jp/watch/{nndID}";
            if (flashJson == null)
            {
                title = htmls5Json.video.originalTitle;
                artist = htmls5Json.owner?.nickname == null ? "n/a" : htmls5Json.owner.nickname;
                artistUrl = "https://www.nicovideo.jp/user/" + (htmls5Json.owner?.id == null ? "n/a" : htmls5Json.owner.id);
                thumbnailUrl = htmls5Json.video.largeThumbnailURL == null ? htmls5Json.video.thumbnailURL : htmls5Json.video.largeThumbnailURL;
                uploadDate = DateTime.Parse(htmls5Json.video.postedDateTime);
                length = TimeSpan.FromSeconds((int)htmls5Json.video.duration);
                directUrls.Add(htmls5Json.video.smileInfo.url);
                return htmls5Json.video.smileInfo.url;
            }
            else 
            {
                var directVideoUri = flashJson.flashvars.flvInfo.Replace("%253A%252F%252F", "://").Replace("%252F", "/").Replace("%253F", "?").Replace("%253D", "=").Split("%3D").First(x => x.StartsWith("http")).Split("%26")[0];
                title = flashJson.videoDetail.title_original;
                artist = flashJson.uploaderInfo?.nickname == null ? "n/a" : flashJson.uploaderInfo.nickname;
                artistUrl = "https://www.nicovideo.jp/user/" + (flashJson.uploaderInfo?.id == null ? "n/a" : flashJson.uploaderInfo.id);
                thumbnailUrl = flashJson.videoDetail.large_thumbnail == null ? flashJson.videoDetail.thumbnail : flashJson.videoDetail.large_thumbnail;
                Console.WriteLine(flashJson.videoDetail.postedAt);
                uploadDate = DateTime.Parse(flashJson.videoDetail.postedAt);
                length = TimeSpan.FromSeconds(flashJson.videoDetail.length);
                directUrls.Add(directVideoUri);
                return directVideoUri;
            }
        }
    }
}

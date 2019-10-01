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
    public class NicoNicoDouga : IServiceExtractor
    {
        HttpClientHandler handler { get; set; }
        HttpClient _c { get; set; }
        NNDHTML.Root h5 { get; set; }
        NNDFlash.Root fl { get; set; }
        ServiceResult ServiceResult { get; set; }

        public async Task<ServiceResult> GetServiceResult(string url)
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
            var login = await DoLogin();
            if (!login.IsSuccessStatusCode) throw new NNDLoginException(login.ReasonPhrase);
            var durl = await GetDirectUri(url);
            if (durl == null)return null;
            ServiceResult.Response = await _c.GetAsync(durl, HttpCompletionOption.ResponseHeadersRead);
            ServiceResult.FillCacheTask = Task.Run(ServiceResult.FillCache);
            return ServiceResult;
        }

        private async Task<HttpResponseMessage> DoLogin()
        {
            string loginForm = $"mail={Bot.config.NndConfig.Mail}&password={Bot.config.NndConfig.Password}&site=nicometro";
            var body = new StringContent(loginForm, Encoding.UTF8, "application/x-www-form-urlencoded");
            string login = "https://secure.nicovideo.jp/secure/login?site=niconico";
            body.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
            var doLogin = await _c.PostAsync(new Uri(login), body);
            return doLogin;
        }

        private async Task<string> GetDirectUri(string url)
        {
            var split = url.Split("/".ToCharArray());
            var nndID = split.First(x => x.StartsWith("sm") || x.StartsWith("nm")).Split("?")[0];
            var videoPage = await _c.GetStringAsync(new Uri($"https://www.nicovideo.jp/watch/{nndID}"));
            var parser = new HtmlParser();
            var parsedDoc = await parser.ParseDocumentAsync(videoPage);
            var h5Part = parsedDoc.GetElementById("js-initial-watch-data");
            if (h5Part != null)
                h5 = JsonConvert.DeserializeObject<NNDHTML.Root>(h5Part.GetAttribute("data-api-data"));
            else
                fl = JsonConvert.DeserializeObject<NNDFlash.Root>(parsedDoc.GetElementById("watchAPIDataContainer").TextContent);
            ServiceResult.DirectUrl = $"https://www.nicovideo.jp/watch/{nndID}";
            ServiceResult.Url = $"https://www.nicovideo.jp/watch/{nndID}";
            ServiceResult.Slow = true;
            ServiceResult.ContentService = Enums.ContentService.NicoNicoDouga;
            if (fl == null)
            {
                ServiceResult.Title = h5.video.originalTitle;
                ServiceResult.Artist = h5.owner?.nickname == null ? "n/a" : h5.owner.nickname;
                ServiceResult.Length = TimeSpan.FromSeconds((int)h5.video.duration);
                return h5.video.smileInfo.url;
            }
            else 
            {
                var directVideoUri = fl.flashvars.flvInfo.Replace("%253A%252F%252F", "://").Replace("%252F", "/").Replace("%253F", "?").Replace("%253D", "=").Split("%3D").First(x => x.StartsWith("http")).Split("%26")[0];
                return directVideoUri;
            }
        }
    }
}

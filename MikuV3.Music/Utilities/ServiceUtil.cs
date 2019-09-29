using MikuV3.Music.Entities;
using MikuV3.Music.Enums;
using Newtonsoft.Json;
using NYoutubeDL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MikuV3.Music.Utilities
{
    public class ServiceUtil
    {
        public async Task<ContentServiceMatch> GetService(string url)
        {
            //URL Check
            try { new Uri(url); }
            catch { return new ContentServiceMatch(ContentService.Search, Playlist.No); }
            //Youtube Single
            if (Regex.IsMatch(url, YT) && !Regex.IsMatch(url, YT_PL)) return new ContentServiceMatch(ContentService.Youtube, Playlist.No);
            //Youtube Single + Playlist
            else if (Regex.IsMatch(url, YT) && Regex.IsMatch(url, YT_PL)) return new ContentServiceMatch(ContentService.Youtube, Playlist.Yes);
            //Youtube Playlist
            else if (!Regex.IsMatch(url, YT) && Regex.IsMatch(url, YT_PL)) return new ContentServiceMatch(ContentService.Youtube, Playlist.No);
            //Soundcloud Single
            else if (Regex.IsMatch(url, SC)) return new ContentServiceMatch(ContentService.Soundcloud, Playlist.No);
            //Soundcloud "Playlist"
            else if (Regex.IsMatch(url, SC_PL) || Regex.IsMatch(url, SC_SET) || Regex.IsMatch(url, SC_STATION)) return new ContentServiceMatch(ContentService.Soundcloud, Playlist.Only);
            return new ContentServiceMatch(ContentService.Direct, Playlist.No);
        }
        //used from youtube-dl
        private readonly string YT = "(?x)^" +
            "(" +
            "(?:https?://|//)" +
            "(?:(?:(?:(?:\\w+\\.)?[yY][oO][uU][tT][uU][bB][eE](?:-nocookie)?\\.com/|" +
            "(?:www\\.)?deturl\\.com/www\\.youtube\\.com/|" +
            "(?:www\\.)?pwnyoutube\\.com/|" +
            "(?:www\\.)?hooktube\\.com/|" +
            "(?:www\\.)?yourepeat\\.com/|" +
            "tube\\.majestyc\\.net/|" +
            "(?:(?:www|dev)\\.)?invidio\\.us/|" +
            "(?:(?:www|no)\\.)?invidiou\\.sh/|" +
            "(?:(?:www|fi|de)\\.)?invidious\\.snopyta\\.org/|" +
            "(?:www\\.)?invidious\\.kabi\\.tk/|" +
            "(?:www\\.)?invidious\\.enkirton\\.net/|" +
            "(?:www\\.)?invidious\\.13ad\\.de/|" +
            "(?:www\\.)?invidious\\.mastodon\\.host/|" +
            "(?:www\\.)?invidious\\.nixnet\\.xyz/|" +
            "(?:www\\.)?invidious\\.drycat\\.fr/|" +
            "(?:www\\.)?tube\\.poal\\.co/|" +
            "(?:www\\.)?vid\\.wxzm\\.sx/|" +
            "(?:www\\.)?yt\\.elukerio\\.org/|" +
            "(?:www\\.)?kgg2m7yk5aybusll\\.onion/|" +
            "(?:www\\.)?qklhadlycap4cnod\\.onion/|" +
            "(?:www\\.)?axqzx4s6s54s32yentfqojs3x5i7faxza6xo3ehd4bzzsg2ii4fv2iid\\.onion/|" +
            "(?:www\\.)?c7hqkpkpemu6e7emz5b4vyz7idjgdvgaaa3dyimmeojqbgpea3xqjoid\\.onion/|" +
            "(?:www\\.)?fz253lmuao3strwbfbmx46yu7acac2jz27iwtorgmbqlkurlclmancad\\.onion/|" +
            "(?:www\\.)?invidious\\.l4qlywnpwqsluw65ts7md3khrivpirse744un3x7mlskqauz5pyuzgqd\\.onion/|" +
            "(?:www\\.)?owxfohz4kjyv25fvlqilyxast7inivgiktls3th44jhk3ej3i7ya\\.b32\\.i2p/|" +
            "youtube\\.googleapis\\.com/)" +
            "(?:.*?\\#/)?" +
            "(?:" +
            "(?:(?:v|embed|e)/(?!videoseries))" +
            "|(?:" +
            "(?:(?:watch|movie)(?:_popup)?(?:\\.php)?/?)?" +
            "(?:\\?|\\#!?)" +
            "(?:.*?[&;])??" +
            "v=" +
            ")" +
            "))" +
            "|(?:" +
            "youtu\\.be|" +
            "vid\\.plus|" +
            "zwearz\\.com/watch|" +
            ")/" +
            "|(?:www\\.)?cleanvideosearch\\.com/media/action/yt/watch\\?videoId=" +
            ")" +
            ")?" +
            "([0-9A-Za-z_-]{11})" +
            "(?!.*?\\blist=" +
            "(?:" +
            "(?:PL|LL|EC|UU|FL|RD|UL|TL|OLAK5uy_)[0-9A-Za-z-_]{10,}s|" +
            "WL" +
            ")" +
            ")" +
            "(?(1).+)?";

        private readonly string YT_PL = "(?x)(?:" +
            "(?:https?://)?" +
            "(?:\\w+\\.)?" +
            "(?:" +
            "(?:" +
            "youtube\\.com|" +
            "invidio\\.us" +
            ")" +
            "/" +
            "(?:" +
            "(?:course|view_play_list|my_playlists|artist|playlist|watch|embed/(?:videoseries|[0-9A-Za-z_-]{11}))" +
            "\\? (?:.*?[&;])*? (?:p|a|list)=" +
            "|  p/" +
            ")|" +
            "youtu\\.be/[0-9A-Za-z_-]{11}\\?.*?\blist=" +
            ")" +
            "(" +
            "(?:PL|LL|EC|UU|FL|RD|UL|TL|OLAK5uy_)?[0-9A-Za-z-_]{10,}" +
            "|(?:MC)[\\w\\.]*" +
            ")" +
            ".*" +
            "|" +
            "((?:PL|LL|EC|UU|FL|RD|UL|TL|OLAK5uy_)[0-9A-Za-z-_]{10,}s)" +
            ")";

        private readonly string SC = "(?x)^(?:https?://)?" +
            "(?:(?:(?:www\\.|m\\.)?soundcloud\\.com/" +
            "(?!stations/track)" +
            "(?P<uploader>[\\w\\d-]+)/" +
            "(?!(?:tracks|albums|sets(?:/.+?)?|reposts|likes|spotlight)/?(?:$|[?#]))" +
            "(?P<title>[\\w\\d-]+)/?" +
            "(?P<token>[^?]+?)?(?:[?].*)?$)" +
            "|(?:api\\.soundcloud\\.com/tracks/(?P<track_id>\\d+)" +
            "(?:/?\\?secret_token=(?P<secret_token>[^&]+))?)" +
            "|(?P<player>(?:w|player|p.)\\.soundcloud\\.com/player/?.*?url=.*)" +
            ")";

        private readonly string SC_SET = "https?://(?:(?:www|m)\\.)?soundcloud\\.com/(?<uploader>[0-9A-Za-z_0-9-]+)/sets/(?<slug_title>[0-9A-Za-z_0-9-]+)(?:/(?<token>[^?/]+))?";
        private readonly string SC_STATION = "https?://(?:(?:www|m)\\.)?soundcloud\\.com/stations/track/[^/]+/(?<id>[^/?#&]+)";
        private readonly string SC_PL = "https?://api\\.soundcloud\\.com/playlists/(?<id>[0-9]+)(?:/?\\?secret_token=(?<token>[^&]+?))?$";
    }
}

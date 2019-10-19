using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Enums;
using System;
using System.Text.RegularExpressions;

namespace MikuV3.Music.ServiceManager
{
    public class ServiceResolver
    {
        public static NicoNicoDougaConfig NicoNicoDougaConfig { get; private set; }

        public ServiceResolver(NicoNicoDougaConfig nicoNicoDougaConfig)
        {
            NicoNicoDougaConfig = nicoNicoDougaConfig;
        }

        /// <summary>
        /// Get the service (and wether or not its a playlist) 
        /// - if the URL is invalit it will return "Search"
        /// - if the serive is unknown/not implemented it will default to "direct"
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public ContentServiceMatch GetService(string url)
        {
            //URL Check
            try { new Uri(url); }
            catch { return new ContentServiceMatch(ContentService.Search, Playlist.No); }

            //Youtube Single
            if ((Regex.IsMatch(url, YT) && !Regex.IsMatch(url, YT_PL))
                || Regex.IsMatch(url, YT_LIVE)) return new ContentServiceMatch(ContentService.Youtube, Playlist.No);
            //Youtube Single + Playlist
            else if (Regex.IsMatch(url, YT) && Regex.IsMatch(url, YT_PL)) return new ContentServiceMatch(ContentService.Youtube, Playlist.Yes);
            //Youtube Playlist
            else if (!Regex.IsMatch(url, YT) && Regex.IsMatch(url, YT_PL)) return new ContentServiceMatch(ContentService.Youtube, Playlist.Only);

            //Soundcloud Single
            else if (Regex.IsMatch(url, SC)) return new ContentServiceMatch(ContentService.Soundcloud, Playlist.No);
            //Soundcloud Playlisttypes
            else if (Regex.IsMatch(url, SC_PL)
                || Regex.IsMatch(url, SC_SET)
                || Regex.IsMatch(url, SC_STATION)) return new ContentServiceMatch(ContentService.Soundcloud, Playlist.Only);

            //NND Single
            else if (Regex.IsMatch(url, NND)) return new ContentServiceMatch(ContentService.NicoNicoDouga, Playlist.No);
            //NND Mylist
            else if (Regex.IsMatch(url, NND_PL)) return new ContentServiceMatch(ContentService.NicoNicoDouga, Playlist.Only);

            //BiliBili Single
            else if (Regex.IsMatch(url, BB)
                || Regex.IsMatch(url, BB_AUDIO)) return new ContentServiceMatch(ContentService.BiliBili, Playlist.No);
            //BiliBili Playlisttypes
            else if (Regex.IsMatch(url, BB_ANIME)
                || Regex.IsMatch(url, BB_AUDIO_ALBUM)) return new ContentServiceMatch(ContentService.BiliBili, Playlist.Only);

            //Bandcamp + Weekly
            else if (Regex.IsMatch(url, BC)
                || Regex.IsMatch(url, BC_WEEKLY)) return new ContentServiceMatch(ContentService.Bandcamp, Playlist.No);
            //Bandcamp Album
            else if (Regex.IsMatch(url, BC_ALBUM)) return new ContentServiceMatch(ContentService.Bandcamp, Playlist.Only);

            //Vimeo Single
            else if (Regex.IsMatch(url, VIMEO)
                || Regex.IsMatch(url, VIMEO_VOD)
                || Regex.IsMatch(url, VIMEO_REVIEW)) return new ContentServiceMatch(ContentService.Vimeo, Playlist.No);
            //Vimeo Playlisttypes
            else if (Regex.IsMatch(url, VIMEO_ALBUM)
                || Regex.IsMatch(url, VIMEO_LIKES)) return new ContentServiceMatch(ContentService.Vimeo, Playlist.Only);

            //Twitch
            else if (Regex.IsMatch(url, TWITCH)
                || Regex.IsMatch(url, TWITCH_CHAPTER)
                || Regex.IsMatch(url, TWITCH_CLIP)
                || Regex.IsMatch(url, TWITCH_VIDEO)
                || Regex.IsMatch(url, TWITCH_VOD)) return new ContentServiceMatch(ContentService.Twitch, Playlist.No);

            //Mixer
            else if (Regex.IsMatch(url, MIXER) || Regex.IsMatch(url, MIXER_VOD)) return new ContentServiceMatch(ContentService.Mixer, Playlist.No);

            return new ContentServiceMatch(ContentService.Direct, Playlist.No);
        }

        #region Regex's
        //used from youtube-dl

        //Youtube
        private readonly string YT = "^((?:https?://|//)(?:(?:(?:(?:[0-9A-Za-z_]+\\.)?[yY][oO][uU][tT][uU][bB][eE](?:-nocookie)?\\.com/|(?:www\\.)?deturl\\.com/www\\.youtube\\.com/|(?:www\\.)?pwnyoutube\\.com/|(?:www\\.)?hooktube\\.com/|(?:www\\.)?yourepeat\\.com/|tube\\.majestyc\\.net/|(?:(?:www|dev)\\.)?invidio\\.us/|(?:(?:www|no)\\.)?invidiou\\.sh/|(?:(?:www|fi|de)\\.)?invidious\\.snopyta\\.org/|(?:www\\.)?invidious\\.kabi\\.tk/|(?:www\\.)?invidious\\.enkirton\\.net/|(?:www\\.)?invidious\\.13ad\\.de/|(?:www\\.)?invidious\\.mastodon\\.host/|(?:www\\.)?invidious\\.nixnet\\.xyz/|(?:www\\.)?invidious\\.drycat\\.fr/|(?:www\\.)?tube\\.poal\\.co/|(?:www\\.)?vid\\.wxzm\\.sx/|(?:www\\.)?yt\\.elukerio\\.org/|(?:www\\.)?kgg2m7yk5aybusll\\.onion/|(?:www\\.)?qklhadlycap4cnod\\.onion/|(?:www\\.)?axqzx4s6s54s32yentfqojs3x5i7faxza6xo3ehd4bzzsg2ii4fv2iid\\.onion/|(?:www\\.)?c7hqkpkpemu6e7emz5b4vyz7idjgdvgaaa3dyimmeojqbgpea3xqjoid\\.onion/|(?:www\\.)?fz253lmuao3strwbfbmx46yu7acac2jz27iwtorgmbqlkurlclmancad\\.onion/|(?:www\\.)?invidious\\.l4qlywnpwqsluw65ts7md3khrivpirse744un3x7mlskqauz5pyuzgqd\\.onion/|(?:www\\.)?owxfohz4kjyv25fvlqilyxast7inivgiktls3th44jhk3ej3i7ya\\.b32\\.i2p/|youtube\\.googleapis\\.com/)(?:.*?#/)?(?:(?:(?:v|embed|e)/(?!videoseries))|(?:(?:(?:watch|movie)(?:_popup)?(?:\\.php)?/?)?(?:\\?|#!?)(?:.*?[&;])??v=)))|(?:youtu\\.be|vid\\.plus|zwearz\\.com/watch|)/|(?:www\\.)?cleanvideosearch\\.com/media/action/yt/watch\\?videoId=))?([0-9A-Za-z_-]{11})(?!.*?\\blist=(?:(?:PL|LL|EC|UU|FL|RD|UL|TL|OLAK5uy_)[0-9A-Za-z_-]{10,}s|WL))(?(1).+)?";
        private readonly string YT_LIVE = "(?<base_url>https?://(?:[0-9A-Za-z_]+\\.)?youtube\\.com/(?:(?:user|channel|c)/)?(?<id>[^/]+))/live";
        private readonly string YT_PL = "(?:(?:https?://)?(?:[0-9A-Za-z_]+\\.)?(?:(?:youtube\\.com|invidio\\.us)/(?:(?:course|view_play_list|my_playlists|artist|playlist|watch|embed/(?:videoseries|[0-9A-Za-z_-]{11}))\\?(?:.*?[&;])*?(?:p|a|list)=|p/)|youtu\\.be/[0-9A-Za-z_-]{11}\\?.*?\\blist=)((?:PL|LL|EC|UU|FL|RD|UL|TL|OLAK5uy_)?[0-9A-Za-z_-]{10,}|(?:MC)[0-9A-Za-z_.]*).*|((?:PL|LL|EC|UU|FL|RD|UL|TL|OLAK5uy_)[0-9A-Za-z_-]{10,}s))";

        //Soundcloud
        private readonly string SC = "^(?:https?://)?(?:(?:(?:www\\.|m\\.)?soundcloud\\.com/(?!stations/track)(?<uploader>[0-9A-Za-z_0-9-]+)/(?!(?:tracks|albums|sets(?:/.+?)?|reposts|likes|spotlight)/?(?:$|[?#]))(?<title>[0-9A-Za-z_0-9-]+)/?(?<token>[^?]+?)?(?:[?].*)?$)|(?:api\\.soundcloud\\.com/tracks/(?<track_id>[0-9]+)(?:/?\\?secret_token=(?<secret_token>[^&]+))?)|(?<player>(?:w|player|p.)\\.soundcloud\\.com/player/?.*?url=.*))";
        private readonly string SC_SET = "https?://(?:(?:www|m)\\.)?soundcloud\\.com/(?<uploader>[0-9A-Za-z_0-9-]+)/sets/(?<slug_title>[0-9A-Za-z_0-9-]+)(?:/(?<token>[^?/]+))?";
        private readonly string SC_STATION = "https?://(?:(?:www|m)\\.)?soundcloud\\.com/stations/track/[^/]+/(?<id>[^/?#&]+)";
        private readonly string SC_PL = "https?://api\\.soundcloud\\.com/playlists/(?<id>[0-9]+)(?:/?\\?secret_token=(?<token>[^&]+?))?$";

        //NicoNicoDouga
        private readonly string NND = "https?://(?:www\\.|secure\\.|sp\\.)?nicovideo\\.jp/watch/(?<id>(?:[a-z]{2})?[0-9]+)";
        private readonly string NND_PL = "https?://(?:www\\.)?nicovideo\\.jp/mylist/(?<id>[0-9]+)";

        //Bilibili
        private readonly string BB = "https?://(?:www\\.|bangumi\\.|)bilibili\\.(?:tv|com)/(?:video/av|anime/(?<anime_id>[0-9]+)/play#)(?<id>[0-9]+)";
        private readonly string BB_ANIME = "https?://bangumi\\.bilibili\\.com/anime/(?<id>[0-9]+)";
        private readonly string BB_AUDIO = "https?://(?:www\\.)?bilibili\\.com/audio/au(?<id>[0-9]+)";
        private readonly string BB_AUDIO_ALBUM = "https?://(?:www\\.)?bilibili\\.com/audio/am(?<id>[0-9]+)";

        //Bandcamp
        private readonly string BC = "https?://[^/]+\\.bandcamp\\.com/track/(?<title>[^/?#&]+)";
        private readonly string BC_WEEKLY = "https?://(?:www\\.)?bandcamp\\.com/?\\?(?:.*?&)?show=(?<id>[0-9]+)";
        private readonly string BC_ALBUM = "https?://(?:(?<subdomain>[^.]+)\\.)?bandcamp\\.com(?:/album/(?<album_id>[^/?#&]+))?";

        //Vimeo
        private readonly string VIMEO = "https?://(?:(?:www|(?<player>player))\\.)?vimeo(?<pro>pro)?\\.com/(?!(?:channels|album|showcase)/[^/?#]+/?(?:$|[?#])|[^/]+/review/|ondemand/)(?:.*?/)?(?:(?:play_redirect_hls|moogaloop\\.swf)\\?clip_id=)?(?:videos?/)?(?<id>[0-9]+)(?:/[0-9a-f]+)?/?(?:[?&].*)?(?:[#].*)?$";
        private readonly string VIMEO_VOD = "https?://(?:www\\.)?vimeo\\.com/ondemand/(?<id>[^/?#&]+)";
        private readonly string VIMEO_REVIEW = "(?<url>https://vimeo\\.com/[^/]+/review/(?<id>[^/]+)/[0-9a-f]{10})";
        private readonly string VIMEO_ALBUM = "https://vimeo\\.com/(?:album|showcase)/(?<id>[0-9]+)(?:$|[?#]|/(?!video))";
        private readonly string VIMEO_LIKES = "https://(?:www\\.)?vimeo\\.com/(?<id>[^/]+)/likes/?(?:$|[?#]|sort:)";

        //Twitch
        private readonly string TWITCH = "https?://(?:(?:(?:www|go|m)\\.)?twitch\\.tv/|player\\.twitch\\.tv/\\?.*?\bchannel=)(?<id>[^/#?]+)";
        private readonly string TWITCH_VIDEO = "https?://(?:(?:www|go|m)\\.)?twitch\\.tv/[^/]+/b/(?<id>[0-9]+)";
        private readonly string TWITCH_VOD = "https?://(?:(?:(?:www|go|m)\\.)?twitch\\.tv/(?:[^/]+/v(?:ideo)?|videos)/|player\\.twitch\\.tv/\\?.*?\\bvideo=v)(?<id>[0-9]+)";
        private readonly string TWITCH_CHAPTER = "https?://(?:(?:www|go|m)\\.)?twitch\\.tv/[^/]+/c/(?<id>[0-9]+)";
        private readonly string TWITCH_CLIP = "https?://(?:clips\\.twitch\\.tv/(?:[^/]+/)*|(?:www\\.)?twitch\\.tv/[^/]+/clip/)(?<id>[^/?#&]+)";

        //Mixer
        private readonly string MIXER = "https?://(?:[0-9A-Za-z_]+\\.)?(?:beam\\.pro|mixer\\.com)/(?<id>[^/?#&]+)";
        private readonly string MIXER_VOD = "https?://(?:[0-9A-Za-z_]+\\.)?(?:beam\\.pro|mixer\\.com)/[^/?#&]+\\?.*?\\bvod=(?<id>[^?#&]+)";
        #endregion
    }
}

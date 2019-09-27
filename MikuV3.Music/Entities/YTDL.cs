using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Entities
{
    public class YTDL
    {

        public class Root
        {
            public string webpage_url { get; set; }
            public string uploader { get; set; }
            public string title { get; set; }
            public string extractor_key { get; set; }
            public string id { get; set; }
            public string webpage_url_basename { get; set; }
            public Entry[] entries { get; set; }
            public string uploader_url { get; set; }
            public string extractor { get; set; }
            public string _type { get; set; }
            public string uploader_id { get; set; }
        }

        public class Entry
        {
            public string upload_date { get; set; }
            public int asr { get; set; }
            public int playlist_index { get; set; }
            public string display_id { get; set; }
            public int abr { get; set; }
            public string thumbnail { get; set; }
            public object track { get; set; }
            public string acodec { get; set; }
            public string vcodec { get; set; }
            public object series { get; set; }
            public float average_rating { get; set; }
            public string uploader { get; set; }
            public string[] tags { get; set; }
            public object episode_number { get; set; }
            public string format_note { get; set; }
            public string extractor { get; set; }
            public string container { get; set; }
            public object requested_subtitles { get; set; }
            public object end_time { get; set; }
            public string format { get; set; }
            public Automatic_Captions automatic_captions { get; set; }
            public float tbr { get; set; }
            public string playlist_uploader { get; set; }
            public int? like_count { get; set; }
            public string uploader_url { get; set; }
            public Format[] formats { get; set; }
            public Http_Headers http_headers { get; set; }
            public string webpage_url { get; set; }
            public string ext { get; set; }
            public Fragment1[] fragments { get; set; }
            public object start_time { get; set; }
            public string fragment_base_url { get; set; }
            public string extractor_key { get; set; }
            public string url { get; set; }
            public string protocol { get; set; }
            public object height { get; set; }
            public string id { get; set; }
            public string webpage_url_basename { get; set; }
            public Thumbnail[] thumbnails { get; set; }
            public string format_id { get; set; }
            public object annotations { get; set; }
            public string uploader_id { get; set; }
            public string[] categories { get; set; }
            public string title { get; set; }
            public string description { get; set; }
            public object creator { get; set; }
            public object language { get; set; }
            public object width { get; set; }
            public string channel_id { get; set; }
            public object artist { get; set; }
            public object filesize { get; set; }
            public object alt_title { get; set; }
            public int n_entries { get; set; }
            public object chapters { get; set; }
            public Subtitles subtitles { get; set; }
            public int duration { get; set; }
            public string playlist_id { get; set; }
            public object fps { get; set; }
            public string playlist_title { get; set; }
            public object license { get; set; }
            public object season_number { get; set; }
            public string manifest_url { get; set; }
            public int? dislike_count { get; set; }
            public string channel_url { get; set; }
            public object release_year { get; set; }
            public int age_limit { get; set; }
            public object release_date { get; set; }
            public string playlist { get; set; }
            public string playlist_uploader_id { get; set; }
            public object is_live { get; set; }
            public object album { get; set; }
            public int view_count { get; set; }
        }

        public class Automatic_Captions
        {
        }

        public class Http_Headers
        {
            public string AcceptCharset { get; set; }
            public string AcceptEncoding { get; set; }
            public string AcceptLanguage { get; set; }
            public string UserAgent { get; set; }
            public string Accept { get; set; }
        }

        public class Subtitles
        {
        }

        public class Format
        {
            public string protocol { get; set; }
            public int? height { get; set; }
            public string format_id { get; set; }
            public Fragment[] fragments { get; set; }
            public string acodec { get; set; }
            public string vcodec { get; set; }
            public string manifest_url { get; set; }
            public int? width { get; set; }
            public object language { get; set; }
            public string container { get; set; }
            public int? filesize { get; set; }
            public string format { get; set; }
            public string format_note { get; set; }
            public float? tbr { get; set; }
            public int abr { get; set; }
            public Http_Headers1 http_headers { get; set; }
            public int? fps { get; set; }
            public string ext { get; set; }
            public int? asr { get; set; }
            public string fragment_base_url { get; set; }
            public string url { get; set; }
            public string player_url { get; set; }
            public int quality { get; set; }
        }

        public class Http_Headers1
        {
            public string AcceptCharset { get; set; }
            public string AcceptEncoding { get; set; }
            public string AcceptLanguage { get; set; }
            public string UserAgent { get; set; }
            public string Accept { get; set; }
        }

        public class Fragment
        {
            public string path { get; set; }
            public float duration { get; set; }
        }

        public class Fragment1
        {
            public string path { get; set; }
            public float duration { get; set; }
        }

        public class Thumbnail
        {
            public string id { get; set; }
            public string url { get; set; }
        }

    }
}

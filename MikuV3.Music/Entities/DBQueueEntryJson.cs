using MikuV3.Music.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Entities
{
    public class DBQueueEntryJson
    {
        public DBQueueEntryJson()
        {

        }
        public DBQueueEntryJson(ServiceResult sr)
        {
            ContentService = sr.ContentService;
            Playlist = sr.Playlist;
            Artist = sr.Artist;
            ArtistUrl = sr.ArtistUrl;
            ThumbnailUrl = sr.ThumbnailUrl;
            UploadDate = sr.UploadDate;
            Title = sr.Title;
            Length = sr.Length;
            Url = sr.Url;
            Slow = sr.Slow;
        }

        [JsonProperty("ContentService")]
        public ContentService ContentService { get; set; }

        [JsonProperty("Playlist")]
        public Playlist Playlist { get; set; }

        [JsonProperty("Artist")]
        public string Artist { get; set; }

        [JsonProperty("ArtistUrl")]
        public string ArtistUrl { get; set; }

        [JsonProperty("ThumbnailUrl")]
        public string ThumbnailUrl { get; set; }

        [JsonProperty("UploadDate")]
        public DateTimeOffset UploadDate { get; set; }

        [JsonProperty("Title")]
        public string Title { get; set; }

        [JsonProperty("Length")]
        public TimeSpan Length { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("Slow")]
        public bool Slow { get; set; }
    }
}

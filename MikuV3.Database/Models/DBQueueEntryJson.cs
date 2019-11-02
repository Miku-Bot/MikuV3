using MikuV3.Music.ServiceManager.Entities;
using MikuV3.Music.ServiceManager.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Database.Entities
{
    public class DBQueueEntryJson
    {
        public DBQueueEntryJson()
        {}

        public DBQueueEntryJson(ServiceResult serviceResult)
        {
            ContentService = serviceResult.ContentService;
            Playlist = serviceResult.Playlist;
            Artist = serviceResult.Artist;
            ArtistUrl = serviceResult.ArtistUrl;
            ThumbnailUrl = serviceResult.ThumbnailUrl;
            UploadDate = serviceResult.UploadDate;
            Title = serviceResult.Title;
            Length = serviceResult.Length;
            Url = serviceResult.Url;
            Slow = serviceResult.Slow;
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

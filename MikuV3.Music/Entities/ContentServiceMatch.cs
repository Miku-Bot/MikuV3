using MikuV3.Music.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Entities
{
    public class ContentServiceMatch
    {
        public ContentService ContentService { get; set; }
        public Playlist Playlist { get; set; }
        public ContentServiceMatch(ContentService cs , Playlist p)
        {
            ContentService = cs;
            Playlist = p;
        }
    }
}

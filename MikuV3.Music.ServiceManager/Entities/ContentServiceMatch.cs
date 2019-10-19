using MikuV3.Music.ServiceManager.Enums;

namespace MikuV3.Music.ServiceManager.Entities
{
    public class ContentServiceMatch
    {
        public ContentService ContentService { get; set; }
        public Playlist Playlist { get; set; }
        public ContentServiceMatch(ContentService contentService , Playlist playlist)
        {
            ContentService = contentService;
            Playlist = playlist;
        }
    }
}

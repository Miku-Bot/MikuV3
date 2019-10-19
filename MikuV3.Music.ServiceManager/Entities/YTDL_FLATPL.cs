namespace MikuV3.Music.ServiceManager.Entities
{
    public class YTDL_FLATPL
    {

        public class Rootobject
        {
            public string id { get; set; }
            public string extractor { get; set; }
            public string _type { get; set; }
            public string uploader_url { get; set; }
            public string uploader_id { get; set; }
            public string uploader { get; set; }
            public Entry[] entries { get; set; }
            public string extractor_key { get; set; }
            public string webpage_url { get; set; }
            public string webpage_url_basename { get; set; }
            public string title { get; set; }
        }

        public class Entry
        {
            public string id { get; set; }
            public string ie_key { get; set; }
            public string _type { get; set; }
            public string url { get; set; }
            public string title { get; set; }
        }

    }
}

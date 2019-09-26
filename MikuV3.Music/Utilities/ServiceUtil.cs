using MikuV3.Music.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Utilities
{
    public class ServiceUtil
    {
        public ContentService GetService(string url)
        {
            return ContentService.Search;
        }
    }
}

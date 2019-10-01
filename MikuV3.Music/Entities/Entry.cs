using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Entities
{
    public class Entry
    {
        public ServiceResult ServiceResult { get; set; }
        public DateTimeOffset additionTime { get; set; }
        public Entry(ServiceResult sr)
        {
            ServiceResult = sr;
            additionTime = DateTimeOffset.UtcNow;
        }
    }
}

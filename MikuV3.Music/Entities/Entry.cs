using MikuV3.Music.ServiceManager.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.Entities
{
    public class Entry
    {
        public ServiceResult ServiceResult { get; set; }
        public DateTimeOffset AdditionTime { get; set; }
        public Entry(ServiceResult serviceResult)
        {
            ServiceResult = serviceResult;
            AdditionTime = DateTimeOffset.UtcNow;
        }
    }
}

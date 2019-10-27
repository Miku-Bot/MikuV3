using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.ServiceManager.Entities
{
    public class PacketPart
    {
        public int Count { get; set; }
        public byte[] Buffer { get; set; }

        public PacketPart(int count, byte[] buffer)
        {
            Count = count;
            Buffer = buffer;
        }
    }
}

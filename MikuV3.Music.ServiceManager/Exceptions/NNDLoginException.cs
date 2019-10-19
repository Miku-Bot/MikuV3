using System;
using System.Collections.Generic;
using System.Text;

namespace MikuV3.Music.ServiceManager.Exceptions
{
    public class NNDLoginException : Exception
    {
        string Reason { get; set; }
        public NNDLoginException(string reason) : base("NND Login failed")
        {
            Reason = reason;
        }

        public override string ToString()
        {
            return $"NND login failed with reason: {Reason}\n" +
                $"{base.StackTrace}";
        }
    }
}

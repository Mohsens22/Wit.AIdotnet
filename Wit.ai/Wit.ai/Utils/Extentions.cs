using System;
using System.Collections.Generic;
using System.Text;
using Wit.ai.Models;

namespace Wit.ai.Utils
{
    public static class Extentions
    {
        public static string ToContentType(this string type)
        {
            if (type.ToLower().Contains("mp3"))
                return "audio/mpeg3";
            if (type.ToLower().Contains("wav"))
                return "audio/wav";
            if (type.ToLower().Contains("ulaw"))
                return "audio/ulaw";
            else
                return "audio/raw";
        }
    }
}

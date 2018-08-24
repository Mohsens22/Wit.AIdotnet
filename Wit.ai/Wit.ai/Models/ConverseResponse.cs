using System.Collections.Generic;

namespace Wit.ai.Models
{
    public class ConverseResponse
    {
        public string Type { get; set; }
        public string Msg { get; set; }
        public List<string> QuickReplies { get; set; }
        public string Action { get; set; }
        public Dictionary<string, dynamic> Entities { get; set; }
        public float Confidence { get; set; }
    }
}
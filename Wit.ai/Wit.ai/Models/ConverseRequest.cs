using Newtonsoft.Json;
using System.Collections.Generic;

namespace Wit.ai.Models
{
    public class ConverseRequest
    {
        public string SessionId { get; set; }
        [JsonProperty("q")]
        public string Message { get; set; }
        public WitContext Context { get; set; }
        public Dictionary<string, dynamic> Entities { get; set; }

        public ConverseRequest()
        {
        }

        public ConverseRequest(string sessionId, string message, WitContext context)
        {
            SessionId = sessionId;
            Message = message;
            Context = context;
        }
    }
}
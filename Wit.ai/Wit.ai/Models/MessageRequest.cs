using Newtonsoft.Json;

namespace Wit.ai.Models
{
    public class MessageRequest
    {
        [JsonProperty("q")]
        public string Query { get; set; }
        [JsonProperty("msg_id")]
        public string MsgId { get; set; }
        [JsonProperty("thread_id")]
        public int ThreadId { get; set; }

        public MessageRequest(string query, string msgId = "", int threadId = 0)
        {
            Query = query;
            MsgId = msgId;
            ThreadId = threadId;
        }
    }
}
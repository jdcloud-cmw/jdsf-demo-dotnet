using System;
using Newtonsoft.Json;

namespace OpenTracingDemo.Models
{
    public class GameInfo
    {
        public GameInfo()
        {
        }
        [JsonProperty(PropertyName = "gameName")]
        public string GameName { get; set; }

        [JsonProperty(PropertyName ="gameId")]
        public string GameId { get; set; }
    }
}

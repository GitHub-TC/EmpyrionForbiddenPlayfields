using EmpyrionNetAPIDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EmpyrionForbiddenPlayfields
{
    public class ForbiddenPlayfield
    {
        public string Name { get; set; }
        public string WarpBackTo { get; set; }
        public string CustomMessage { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MessagePriorityType? MessageType { get; set; }
        public int? RepeatSeconds { get; set; }
        public AllowedPlayer[] PlayerInfo { get; set; }
        public AllowedFaction[] FactionInfo { get; set; }
    }

    public class AllowedFaction
    {
        public string Abbr { get; set; }
    }

    public class AllowedPlayer
    {
        public string Name { get; set; }
        public string SteamId { get; set; }
    }

    public class Configuration
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public LogLevel LogLevel { get; set; } = LogLevel.Message;
        [JsonConverter(typeof(StringEnumConverter))]
        public PermissionType FreeTravelPermision { get; set; } = PermissionType.GameMaster;
        [JsonConverter(typeof(StringEnumConverter))]
        public MessagePriorityType MessageType { get; set; } = MessagePriorityType.Alarm;
        public int RepeatSeconds { get; set; } = 5;
        public ForbiddenPlayfield[] ForbiddenPlayfields { get; set; } = new[]{ new ForbiddenPlayfield() {
                FactionInfo  = new[] { new AllowedFaction() },
                PlayerInfo   = new[] { new AllowedPlayer () },
            }
        };
    }
}
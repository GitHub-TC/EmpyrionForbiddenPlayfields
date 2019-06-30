using EmpyrionNetAPIDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EmpyrionForbiddenPlayfields
{
    public class ForbiddenPlayfield
    {
        public string Name { get; set; }
        public string WarpBackTo { get; set; }
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
        public ForbiddenPlayfield[] ForbiddenPlayfields { get; set; } = new[]{ new ForbiddenPlayfield() {
                FactionInfo  = new[] { new AllowedFaction() },
                PlayerInfo   = new[] { new AllowedPlayer () },
            }
        };
    }
}
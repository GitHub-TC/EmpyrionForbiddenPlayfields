using Eleon.Modding;
using EmpyrionNetAPIAccess;
using EmpyrionNetAPIDefinitions;
using EmpyrionNetAPITools;
using EmpyrionNetAPITools.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EmpyrionForbiddenPlayfields
{
    public class EmpyrionForbiddenPlayfields : EmpyrionModBase
    {
        private int CurrentPlayerIndex;
        public ConfigurationManager<Configuration> Configuration { get; set; }
        public ModGameAPI DediAPI { get; private set; }
        public FactionInfoList FactionData { get; set; }
        public ConcurrentDictionary<string, ManualResetEvent> PlayerAlerts { get; set; } = new ConcurrentDictionary<string, ManualResetEvent>();
        public List<int> CheckPlayer { get; set; }

        public EmpyrionForbiddenPlayfields()
        {
            EmpyrionConfiguration.ModName = "EmpyrionForbiddenPlayfields";
        }

        public override void Initialize(ModGameAPI dediAPI)
        {
            DediAPI = dediAPI;
            LogLevel = LogLevel.Message;

            Log($"**EmpyrionForbiddenPlayfields: loaded", LogLevel.Message);

            LoadConfiuration();
            LogLevel = Configuration.Current.LogLevel;

            TaskTools.Intervall(30000, () => UpdateFactionData().Wait());
            TaskTools.Intervall(1000,  () => TestNextPlayer());
        }

        private void TestNextPlayer()
        {
            var list = CheckPlayer;
            if (list == null || list.Count == 0) return;

            if (list.Count <= CurrentPlayerIndex) CurrentPlayerIndex = 0;

            CheckPlayerLocation(list[CurrentPlayerIndex++]).Wait();
        }

        private async Task CheckPlayerLocation(int playerId)
        {
            try
            {
                var player = await Request_Player_Info(playerId.ToId());

                if (CheckForForbiddenPlayfield(player))
                {
                    if (PlayerAlerts.TryRemove(player.steamId, out var messages)) messages.Set();
                }
            }
            catch (Exception error)
            {
                Log($"TestNextPlayer: {error}", LogLevel.Debug);
            }
        }

        private async Task UpdateFactionData()
        {
            try
            {
                FactionData = await Request_Get_Factions(1.ToId());
                var list    = await Request_Player_List();
                CheckPlayer = list?.list?.ToList();
            }
            catch (System.Exception error)
            {
                Log($"UpdateFactionData: {error}", LogLevel.Debug);
            }
        }

        private bool CheckForForbiddenPlayfield(PlayerInfo player)
        {
            if (player.permission >= (int)Configuration.Current.FreeTravelPermision) return true;

            var checkplayfield = Configuration.Current.ForbiddenPlayfields.FirstOrDefault(P => P.Name == player.playfield);
            if (checkplayfield == null) return true;

            var faction = FactionData?.factions?.FirstOrDefault(F => F.factionId == player.factionId);
            if (faction.HasValue &&
                checkplayfield.FactionInfo != null && 
                checkplayfield.FactionInfo.Any(F => string.Compare(F.Abbr, faction.Value.abbrev?.Trim(), StringComparison.InvariantCultureIgnoreCase) == 0)) return true;

            var checkplayer = checkplayfield.PlayerInfo?.FirstOrDefault(P => P.SteamId == player.steamId);
            if (checkplayer != null)
            {
                if (string.IsNullOrEmpty(checkplayer.Name))
                {
                    checkplayer.Name = player.playerName;
                    Configuration.Save();
                }
                return true;
            }

            checkplayer = checkplayfield.PlayerInfo?.FirstOrDefault(P => string.Compare(P.Name, player.playerName.Trim(), StringComparison.InvariantCultureIgnoreCase) == 0);
            if (checkplayer != null)
            {
                if (string.IsNullOrEmpty(checkplayer.SteamId))
                {
                    checkplayer.SteamId = player.steamId;
                    Configuration.Save();
                }
                return true;
            }

            if (string.IsNullOrEmpty(checkplayfield.WarpBackTo))
            {
                if (!PlayerAlerts.TryGetValue(player.steamId, out _))
                {
                    PlayerAlerts.TryAdd(player.steamId,
                        TaskTools.Intervall(10000, () =>
                        {
                            Request_InGameMessage_SinglePlayer(Timeouts.NoResponse, 
                                (string.IsNullOrEmpty(checkplayfield.CustomMessage) ? $"Please leave this playfield '{player.playfield}', it is reserved!" : checkplayfield.CustomMessage)
                                .ToIdMsgPrio(player.entityId,
                                checkplayfield.MessageType   ?? Configuration.Current.MessageType,
                                checkplayfield.RepeatSeconds ?? Configuration.Current.RepeatSeconds));
                            CheckPlayerLocation(player.entityId).Wait();
                        })
                    );
                }

                return false;
            }

            return false;
        }

        private void LoadConfiuration()
        {
            Configuration = new ConfigurationManager<Configuration>
            {
                ConfigFilename = Path.Combine(EmpyrionConfiguration.SaveGameModPath, @"Configuration.json"),
                CreateDefaults = (c) => {
                    c.ForbiddenPlayfields = new[] {
                    new ForbiddenPlayfield(){ 
                        Name            = "Name of the playfield",
                        CustomMessage   = "override default message with custom message",
                        RepeatSeconds   = 10,
                        MessageType     = MessagePriorityType.Info,
                        PlayerInfo      = new []{ new AllowedPlayer () { SteamId = "steamid", Name="ingame name"  } },
                        FactionInfo     = new []{ new AllowedFaction() { Abbr = "faction abbrev" } },
                    } };
                }
            };

            Configuration.Load();
            Configuration.Save();
        }


    }
}

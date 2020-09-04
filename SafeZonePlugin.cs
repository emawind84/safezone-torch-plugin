using NLog;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Torch.API.Session;
using Torch.Commands;
using Torch.Managers.ChatManager;
using Torch.Session;
using VRage.Game.Entity;
using VRageMath;

namespace SafeZonePlugin
{
    public class SafeZonePlugin : TorchPluginBase
    {
        public static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private Persistent<SafeZoneConfig> _config;
        public SafeZoneConfig Config => _config?.Data;

        private readonly Stopwatch stopWatch = new Stopwatch();

        /// <inheritdoc />
        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            SetupConfig();

            var sessionManager = Torch.Managers.GetManager<TorchSessionManager>();
            if (sessionManager != null)
                sessionManager.SessionStateChanged += SessionChanged;
            else
                Log.Warn("No session manager loaded!");
        }

        /// <inheritdoc />
        public override void Update()
        {
            base.Update();

            try
            {
                /* stopWatch not running? Nothing to do */
                if (!stopWatch.IsRunning)
                    return;

                /* Session not loaded? Nothing to do */
                if (Torch.CurrentSession == null || Torch.CurrentSession.State != TorchSessionState.Loaded)
                    return;

                var elapsed = stopWatch.Elapsed;
                if (elapsed.TotalSeconds < 2)
                    return;
                
                stopWatch.Restart();

                var players = MySession.Static.Players.GetOnlinePlayers();
                Parallel.ForEach(players, player =>
                {
                    BoundingSphereD boundingSphereD = new BoundingSphereD(player.GetPosition(), 50.0);
                    var entities = new List<MyEntity>();
                    MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref boundingSphereD, entities, MyEntityQueryType.Both);
                    IEnumerable<MyCubeGrid> grids = entities.OfType<MyCubeGrid>();

                    MyCubeGrid safeZoneGrid = null;
                    IMySafeZoneBlock safeZoneBlock = null;
                    foreach (var grid in grids)
                    {
                        if (safeZoneGrid != null) break;
                        var safeZoneBlocks = grid.GetBlocks().Where(blk => blk.FatBlock is IMySafeZoneBlock);
                        foreach (var _slimBlock in safeZoneBlocks)
                        {
                            var _safeZone = _slimBlock.FatBlock as IMySafeZoneBlock;
                            var ownerSteamId = MySession.Static.Players.TryGetSteamId(_safeZone.OwnerId);
                            if (_safeZone.CustomData.IndexOf($"[{Config.CustomDataTag}]", StringComparison.InvariantCultureIgnoreCase) != -1
                            && ownerSteamId != 0L && MySession.Static.IsUserSpaceMaster(ownerSteamId))
                            {
                                safeZoneGrid = grid;
                                safeZoneBlock = _safeZone;
                                break;
                            }
                        }
                    }

                    if (safeZoneGrid != null && safeZoneGrid.DestructibleBlocks)
                    {
                        if (!Config.ProtectedEntityIds.Contains(safeZoneGrid.EntityId))
                            Config.ProtectedEntityIds.Add(safeZoneGrid.EntityId);
                        safeZoneGrid.DestructibleBlocks = false;
                    }

                    if (safeZoneBlock != null)
                    {
                        var definition = MyDefinitionManager.Static.GetCubeBlockDefinition(safeZoneBlock.BlockDefinition);
                        safeZoneBlock.CustomName = definition.DisplayNameText + string.Format(" [{0}: {1}]", 
                            Config.CustomDataTag, safeZoneBlock.Enabled ? "ON" : "OFF");
                    }
                    
                    BoundingSphereD closeBoundingSphereD = new BoundingSphereD(player.GetPosition(), 10.0);
                    entities = new List<MyEntity>();
                    MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref boundingSphereD, entities, MyEntityQueryType.Dynamic);
                    grids = entities.OfType<MyCubeGrid>();

                    foreach(var grid in grids)
                    {
                        if (grid != safeZoneGrid && grid.BigOwners.Contains(player.Identity.IdentityId))
                        {
                            if (safeZoneBlock != null && safeZoneBlock.Enabled && grid.DestructibleBlocks)
                            {
                                Log.Debug("safezone in");
                                Torch.CurrentSession.Managers.GetManager<ChatManagerServer>()?.SendMessageAsOther(safeZoneGrid.DisplayName ?? "Server", 
                                    $"{grid.DisplayName} damage protection on", Color.Green, player.Client.SteamUserId);
                                grid.DestructibleBlocks = false;
                                if (!Config.ProtectedEntityIds.Contains(grid.EntityId))
                                    Config.ProtectedEntityIds.Add(grid.EntityId);
                            }
                            else if ((safeZoneBlock == null || !safeZoneBlock.Enabled) && Config.ProtectedEntityIds.Contains(grid.EntityId))
                            {
                                Log.Debug("safezone out");
                                Torch.CurrentSession.Managers.GetManager<ChatManagerServer>()?.SendMessageAsOther("Server", 
                                    $"{grid.DisplayName} damage protection off", Color.Red, player.Client.SteamUserId);
                                grid.DestructibleBlocks = true;
                                Config.ProtectedEntityIds.Remove(grid.EntityId);
                            }
                        }
                    };

                });

                var protectedEntityIds = new List<long>(Config.ProtectedEntityIds);
                foreach (var entityId in protectedEntityIds) {
                    if (!MyEntities.TryGetEntityById(entityId, out MyEntity entity))
                    {
                        Config.ProtectedEntityIds.Remove(entityId);
                    }
                };

                Log.Debug($"Completed in {stopWatch.ElapsedMilliseconds}ms");

                stopWatch.Restart();
            }
            catch (Exception e)
            {
                Log.Error(e, "Something is not right");
            }
        }

        private void SetupConfig()
        {
            var configFile = Path.Combine(StoragePath, "SafeZoneConfig.cfg");

            try
            {
                _config = Persistent<SafeZoneConfig>.Load(configFile);
            }
            catch (Exception e)
            {
                Log.Warn(e);
            }

            if (_config?.Data == null)
            {
                Log.Debug("Create Default Config, because none was found!");

                _config = new Persistent<SafeZoneConfig>(configFile, new SafeZoneConfig());
                _config.Save();
            }
        }

        private void SessionChanged(ITorchSession session, TorchSessionState newState)
        {
            if (newState == TorchSessionState.Loaded)
            {
                stopWatch.Start();
                Log.Debug("Session loaded, start backup timer!");
            }
            else if (newState == TorchSessionState.Unloading)
            {
                stopWatch.Stop();
                Log.Debug("Session Unloading, suspend backup timer!");
            }
        }
    }
}

using System.Collections.Generic;
using LiteNetLibManager;
using System.Diagnostics;
using System.IO;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using ConcurrentCollections;
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
using UnityEngine;
#endif

namespace MultiplayerARPG.MMO
{
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
    [DefaultExecutionOrder(DefaultExecutionOrders.MAP_SPAWN_NETWORK_MANAGER)]
#endif
    public partial class MapSpawnNetworkManager : LiteNetLibManager.LiteNetLibManager, IAppServer
    {
        private struct StartingSceneData
        {
            public string channelId;
            public string mapName;
            public bool isAllocate;
            public string instanceId;
        }

        public const string DEFAULT_CHANNEL_ID = "default";
        public const string ALLOCATE_CHANNEL_ID = "__ALLOC__";

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        [Header("Central Network Connection")]
#endif
        public string clusterServerAddress = "127.0.0.1";
        public int clusterServerPort = 6010;
        public string machineAddress = "127.0.0.1";

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        [Header("Map Spawn Settings")]
#endif
        public string exePath = "./Build.exe";
        public bool notSpawnInBatchMode = false;
        public int startPort = 8000;
        public string batchModeArguments = "-batchmode -nographics";
        public List<string> spawningChannelIds = new List<string>();
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public List<BaseMapInfo> spawningMaps = new List<BaseMapInfo>();
        public List<SpawnAllocateMapData> spawningAllocateMaps = new List<SpawnAllocateMapData>();
#endif
#if NET || NETCOREAPP
        public List<string> spawningMapByNames = new List<string>();
        public List<SpawnAllocateMapByNameData> spawningAllocateMapByNames = new List<SpawnAllocateMapByNameData>();
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        [Header("Running In Editor")]
#endif
        public bool isOverrideExePath;
        public string overrideExePath = "./Build.exe";
        public bool editorNotSpawnInBatchMode;

        private int _spawningPort = -1;
        private int _portCounter = -1;
        /// <summary>
        /// Free ports which can use for start map server
        /// </summary>
        private readonly ConcurrentQueue<int> _freePorts = new ConcurrentQueue<int>();
        /// <summary>
        /// Actions which will invokes in main thread
        /// </summary>
        private readonly ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();
        /// <summary>
        /// Map servers processes id
        /// </summary>
        private readonly ConcurrentHashSet<int> _processes = new ConcurrentHashSet<int>();
        /// <summary>
        /// List of Map servers that restarting in update loop
        /// </summary>
        private readonly ConcurrentQueue<StartingSceneData> _restartingScenes = new ConcurrentQueue<StartingSceneData>();

        public string ExePath
        {
            get
            {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
                if (Application.isEditor && isOverrideExePath)
                    return overrideExePath;
#endif
                return exePath;
            }
        }

        public bool NotSpawnInBatchMode
        {
            get
            {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
                if (Application.isEditor)
                    return editorNotSpawnInBatchMode;
#endif
                return notSpawnInBatchMode;
            }
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public ClusterClient ClusterClient { get; private set; }
#endif
        public string ClusterServerAddress { get { return clusterServerAddress; } }
        public int ClusterServerPort { get { return clusterServerPort; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string ChannelId { get { return string.Empty; } }
        public string RefId { get { return string.Empty; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.MapSpawnServer; } }

#if NET || NETCOREAPP
        public MapSpawnNetworkManager() : base()
        {
            Initialize();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        protected override void Start()
        {
            Initialize();
            base.Start();
        }
#endif

        protected virtual void Initialize()
        {
            useWebSocket = false;
            maxConnections = int.MaxValue;
            if (spawningChannelIds == null || spawningChannelIds.Count == 0)
                spawningChannelIds = new List<string>() { DEFAULT_CHANNEL_ID };
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            ClusterClient = new ClusterClient(this);
            ClusterClient.onResponseAppServerRegister = OnResponseAppServerRegister;
            ClusterClient.RegisterRequestHandler<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.RequestSpawnMap, HandleRequestSpawnMap);
#endif
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
            _spawningPort = -1;
            _portCounter = -1;
            // Clear free ports
            while (_freePorts.TryDequeue(out _))
            {
                // Do nothing
            }
            // Clear main thread actions
            while (_mainThreadActions.TryDequeue(out _))
            {
                // Do nothing
            }
            // Clear processes
            List<int> processIds = new List<int>(_processes);
            foreach (int processId in processIds)
            {
                Process.GetProcessById(processId).Kill();
                _processes.TryRemove(processId);
            }
            // Clear restarting scenes
            while (_restartingScenes.TryDequeue(out _))
            {
                // Do nothing
            }
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            ClusterClient.OnAppStart();
            _spawningPort = startPort;
            _portCounter = startPort;
            base.OnStartServer();
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public override void OnStopServer()
        {
            ClusterClient.OnAppStop();
            Clean();
            base.OnStopServer();
        }
#endif

        public override void OnStartClient(LiteNetLibClient client)
        {
            this.InvokeInstanceDevExtMethods("OnStartClient", client);
            base.OnStartClient(client);
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        protected override void Update()
        {
            base.Update();

            if (IsServer)
            {
                ClusterClient.Update();
                if (ClusterClient.IsAppRegistered)
                {
                    if (_restartingScenes.Count > 0)
                    {
                        while (_restartingScenes.TryDequeue(out StartingSceneData startSceneData))
                        {
                            SpawnMap(startSceneData.mapName, startSceneData.channelId, startSceneData.isAllocate, startSceneData.instanceId, default, false, default, true);
                        }
                    }
                }
                if (_mainThreadActions.Count > 0)
                {
                    Action tempMainThreadAction;
                    while (_mainThreadActions.TryDequeue(out tempMainThreadAction))
                    {
                        if (tempMainThreadAction != null)
                            tempMainThreadAction.Invoke();
                    }
                }
            }
        }
#endif

        protected override void OnDestroy()
        {
            Clean();
            base.OnDestroy();
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        internal async UniTaskVoid HandleRequestSpawnMap(
            RequestHandlerData requestHandler,
            RequestSpawnMapMessage request,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result)
        {
            await UniTask.Yield();
            UITextKeys message = UITextKeys.NONE;
            if (!ClusterClient.IsAppRegistered)
                message = UITextKeys.UI_ERROR_APP_NOT_READY;
            else if (string.IsNullOrEmpty(request.mapName))
                message = UITextKeys.UI_ERROR_EMPTY_SCENE_NAME;

            if (message != UITextKeys.NONE)
            {
                result.InvokeError(new ResponseSpawnMapMessage()
                {
                    message = message
                });
            }
            SpawnMap(request, result, false);
        }
#endif

        private void OnResponseAppServerRegister(AckResponseCode responseCode)
        {
            if (responseCode != AckResponseCode.Success)
                return;
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            List<string> spawningMapByNames = new List<string>();
            if (spawningMaps == null || spawningMaps.Count == 0)
            {
                spawningMaps = new List<BaseMapInfo>();
                spawningMaps.AddRange(GameInstance.MapInfos.Values);
            }
            foreach (BaseMapInfo spawningMap in spawningMaps)
            {
                if (spawningMap == null || !spawningMap.IsSceneSet())
                    continue;
                spawningMapByNames.Add(spawningMap.Id);
            }
            List<SpawnAllocateMapByNameData> spawningAllocateMapByNames = new List<SpawnAllocateMapByNameData>();
            foreach (SpawnAllocateMapData spawningAllocateMap in spawningAllocateMaps)
            {
                if (spawningAllocateMap.mapInfo == null || !spawningAllocateMap.mapInfo.IsSceneSet())
                    continue;
                spawningAllocateMapByNames.Add(new SpawnAllocateMapByNameData()
                {
                    mapName = spawningAllocateMap.mapInfo.Id,
                    allocateAmount = spawningAllocateMap.allocateAmount,
                });
            }
#endif
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            SpawnMaps(spawningMapByNames).Forget();
            SpawnAllocateMaps(spawningAllocateMapByNames).Forget();
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private async UniTaskVoid SpawnMaps(List<string> list)
        {
            foreach (string entry in list)
            {
                foreach (string channelId in spawningChannelIds)
                {
                    SpawnMap(channelId, entry, false, string.Empty, default, false, default, true);
                    // Add some delay before spawn next map
#if NET || NETCOREAPP
                    await Task.Delay(100);
#else
                    await UniTask.Delay(100);
#endif
                }
            }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private async UniTaskVoid SpawnAllocateMaps(List<SpawnAllocateMapByNameData> list)
        {
            foreach (SpawnAllocateMapByNameData entry in list)
            {
                for (int i = 0; i < entry.allocateAmount; ++i)
                {
                    SpawnMap(ALLOCATE_CHANNEL_ID, entry.mapName, true, $"{ALLOCATE_CHANNEL_ID}_{i}", default, false, default, true);
                    // Add some delay before spawn next map
#if NET || NETCOREAPP
                    await Task.Delay(100);
#else
                    await UniTask.Delay(100);
#endif
                }
            }
        }
#endif

        private void FreePort(int port)
        {
            _freePorts.Enqueue(port);
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private void SpawnMap(
            RequestSpawnMapMessage message,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result,
            bool autoRestart)
        {
            SpawnMap(message.channelId, message.mapName, false, message.instanceId, message.instanceWarpPosition, message.instanceWarpOverrideRotation, message.instanceWarpRotation, autoRestart, result);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        private void SpawnMap(
            string channelId, string mapName, bool isAllocate,
            string instanceId, Vec3 instanceWarpPosition, bool instanceWarpOverrideRotation, Vec3 instanceWarpRotation,
            bool autoRestart,
            RequestProceedResultDelegate<ResponseSpawnMapMessage> result = null)
        {
            // Port to run map server
            if (_freePorts.Count > 0)
            {
                _freePorts.TryDequeue(out _spawningPort);
            }
            else
            {
                _spawningPort = _portCounter++;
            }
            int port = _spawningPort;

            // Path to executable
            string path = ExePath;
            if (string.IsNullOrEmpty(path))
            {
                path = File.Exists(Environment.GetCommandLineArgs()[0])
                    ? Environment.GetCommandLineArgs()[0]
                    : Process.GetCurrentProcess().MainModule.FileName;
            }

            if (LogInfo)
                Logging.Log(LogTag, "Starting process from: " + path);

            // Spawning Process Info
            ProcessStartInfo startProcessInfo = new ProcessStartInfo(path)
            {
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = (!NotSpawnInBatchMode ? batchModeArguments : string.Empty) +
                    $"  {ProcessArguments.ARG_CHANNEL_ID} {channelId}" +
                    $"  {ProcessArguments.ARG_MAP_NAME} {mapName}" +
                    (isAllocate ?
                        $" {ProcessArguments.ARG_ALLOCATE}" +
                        $" {ProcessArguments.ARG_INSTANCE_ID} {instanceId}"
                        : string.Empty) +
                    (!isAllocate && !string.IsNullOrEmpty(instanceId) ?
                        $" {ProcessArguments.ARG_INSTANCE_ID} {instanceId}" +
                        $" {ProcessArguments.ARG_INSTANCE_POSITION_X} {instanceWarpPosition.x}" +
                        $" {ProcessArguments.ARG_INSTANCE_POSITION_Y} {instanceWarpPosition.y}" +
                        $" {ProcessArguments.ARG_INSTANCE_POSITION_Z} {instanceWarpPosition.z}" +
                        $" {(instanceWarpOverrideRotation ? ProcessArguments.ARG_INSTANCE_OVERRIDE_ROTATION : string.Empty)}" +
                        $" {ProcessArguments.ARG_INSTANCE_ROTATION_X} {instanceWarpRotation.x}" +
                        $" {ProcessArguments.ARG_INSTANCE_ROTATION_Y} {instanceWarpRotation.y}" +
                        $" {ProcessArguments.ARG_INSTANCE_ROTATION_Z} {instanceWarpRotation.z}"
                        : string.Empty) +
                    $" {ProcessArguments.ARG_CENTRAL_ADDRESS} {clusterServerAddress}" +
                    $" {ProcessArguments.ARG_CENTRAL_PORT} {clusterServerPort}" +
                    $" {ProcessArguments.ARG_MACHINE_ADDRESS} {machineAddress}" +
                    $" {ProcessArguments.ARG_MAP_PORT} {port}" +
                    $" {ProcessArguments.ARG_START_MAP_SERVER}",
            };

            if (LogInfo)
                Logging.Log(LogTag, "Starting process with args: " + startProcessInfo.Arguments);

            int processId = 0;
            bool processStarted = false;
            try
            {
                new Thread(() =>
                {
                    try
                    {
                        using (Process process = Process.Start(startProcessInfo))
                        {
                            processId = process.Id;
                            _processes.Add(processId);

                            processStarted = true;

                            _mainThreadActions.Enqueue(() =>
                            {
                                if (LogInfo)
                                    Logging.Log(LogTag, "Process started. Id: " + processId);
                                // Notify server that it's successfully handled the request
                                if (result != null)
                                {
                                    result.InvokeSuccess(new ResponseSpawnMapMessage()
                                    {
                                        message = UITextKeys.NONE,
                                    });
                                }
                            });
                            process.WaitForExit();
                        }
                    }
                    catch (Exception e)
                    {
                        if (!processStarted)
                        {
                            _mainThreadActions.Enqueue(() =>
                            {
                                if (LogFatal)
                                {
                                    Logging.LogError(LogTag, "Tried to start a process at: '" + path + "' but it failed. Make sure that you have set correct the 'exePath' in 'MapSpawnNetworkManager' component");
                                    Logging.LogException(LogTag, e);
                                }

                                // Notify server that it failed to spawn map scene handled the request
                                if (result != null)
                                {
                                    result.InvokeError(new ResponseSpawnMapMessage()
                                    {
                                        message = UITextKeys.UI_ERROR_CANNOT_EXCUTE_MAP_SERVER,
                                    });
                                }
                            });
                        }
                    }
                    finally
                    {
                        // Remove the process
                        _processes.TryRemove(processId);

                        // Restarting scene
                        if (autoRestart)
                        {
                            _restartingScenes.Enqueue(new StartingSceneData()
                            {
                                channelId = channelId,
                                mapName = mapName,
                                isAllocate = isAllocate,
                                instanceId = instanceId,
                            });
                        }

                        _mainThreadActions.Enqueue(() =>
                        {
                            // Release the port number
                            FreePort(port);

                            if (LogInfo)
                                Logging.Log(LogTag, "Process spawn id: " + processId + " killed.");
                        });
                    }

                }).Start();
            }
            catch (Exception e)
            {
                if (result != null)
                {
                    result.InvokeError(new ResponseSpawnMapMessage()
                    {
                        message = UITextKeys.UI_ERROR_UNKNOW,
                    });
                }

                // Restarting scene
                if (autoRestart)
                {
                    _restartingScenes.Enqueue(new StartingSceneData()
                    {
                        channelId = channelId,
                        mapName = mapName,
                        isAllocate = isAllocate,
                        instanceId = instanceId,
                    });
                }

                if (LogFatal)
                    Logging.LogException(LogTag, e);
            }
        }
#endif
    }
}

namespace MultiplayerARPG.MMO
{
    public static partial class ProcessArguments
    {
        // Central server
        public const string CONFIG_CENTRAL_ADDRESS = "centralAddress";
        public const string ARG_CENTRAL_ADDRESS = "-" + CONFIG_CENTRAL_ADDRESS;
        public const string CONFIG_CENTRAL_PORT = "centralPort";
        public const string ARG_CENTRAL_PORT = "-" + CONFIG_CENTRAL_PORT;
        public const string CONFIG_CENTRAL_MAX_CONNECTIONS = "centralMaxConnections";
        public const string ARG_CENTRAL_MAX_CONNECTIONS = "-" + CONFIG_CENTRAL_MAX_CONNECTIONS;
        // Central web-socket connection (for login/character management)
        public const string CONFIG_USE_WEB_SOCKET = "useWebSocket";
        public const string ARG_USE_WEB_SOCKET = "-" + CONFIG_USE_WEB_SOCKET;
        public const string CONFIG_WEB_SOCKET_SECURE = "webSocketSecure";
        public const string ARG_WEB_SOCKET_SECURE = "-" + CONFIG_WEB_SOCKET_SECURE;
        public const string CONFIG_WEB_SOCKET_CERT_PATH = "webSocketCertPath";
        public const string ARG_WEB_SOCKET_CERT_PATH = "-" + CONFIG_WEB_SOCKET_CERT_PATH;
        public const string CONFIG_WEB_SOCKET_CERT_PASSWORD = "webSocketCertPassword";
        public const string ARG_WEB_SOCKET_CERT_PASSWORD = "-" + CONFIG_WEB_SOCKET_CERT_PASSWORD;
        // Cluster server
        public const string CONFIG_CLUSTER_PORT = "clusterPort";
        public const string ARG_CLUSTER_PORT = "-" + CONFIG_CLUSTER_PORT;
        public const string CONFIG_MACHINE_ADDRESS = "machineAddress";
        public const string ARG_MACHINE_ADDRESS = "-" + CONFIG_MACHINE_ADDRESS;
        // Map spawn server
        public const string CONFIG_MAP_SPAWN_PORT = "mapSpawnPort";
        public const string ARG_MAP_SPAWN_PORT = "-" + CONFIG_MAP_SPAWN_PORT;
        public const string CONFIG_SPAWN_EXE_PATH = "spawnExePath";
        public const string ARG_SPAWN_EXE_PATH = "-" + CONFIG_SPAWN_EXE_PATH;
        public const string CONFIG_NOT_SPAWN_IN_BATCH_MODE = "notSpawnInBatchMode";
        public const string ARG_NOT_SPAWN_IN_BATCH_MODE = "-" + CONFIG_NOT_SPAWN_IN_BATCH_MODE;
        public const string CONFIG_SPAWN_START_PORT = "spawnStartPort";
        public const string ARG_SPAWN_START_PORT = "-" + CONFIG_SPAWN_START_PORT;
        public const string CONFIG_SPAWN_MAPS = "spawnMaps";
        public const string ARG_SPAWN_MAPS = "-" + CONFIG_SPAWN_MAPS;
        // Map server
        public const string CONFIG_MAP_PORT = "mapPort";
        public const string ARG_MAP_PORT = "-" + CONFIG_MAP_PORT;
        public const string CONFIG_MAP_MAX_CONNECTIONS = "mapMaxConnections";
        public const string ARG_MAP_MAX_CONNECTIONS = "-" + CONFIG_MAP_MAX_CONNECTIONS;
        public const string CONFIG_MAP_ID = "mapId";
        public const string ARG_MAP_ID = "-" + CONFIG_MAP_ID;
        public const string CONFIG_INSTANCE_ID = "instanceId";
        public const string ARG_INSTANCE_ID = "-" + CONFIG_INSTANCE_ID;
        public const string CONFIG_INSTANCE_POSITION_X = "instancePositionX";
        public const string ARG_INSTANCE_POSITION_X = "-" + CONFIG_INSTANCE_POSITION_X;
        public const string CONFIG_INSTANCE_POSITION_Y = "instancePositionY";
        public const string ARG_INSTANCE_POSITION_Y = "-" + CONFIG_INSTANCE_POSITION_Y;
        public const string CONFIG_INSTANCE_POSITION_Z = "instancePositionZ";
        public const string ARG_INSTANCE_POSITION_Z = "-" + CONFIG_INSTANCE_POSITION_Z;
        public const string CONFIG_INSTANCE_OVERRIDE_ROTATION = "instanceOverrideRotation";
        public const string ARG_INSTANCE_OVERRIDE_ROTATION = "-" + CONFIG_INSTANCE_OVERRIDE_ROTATION;
        public const string CONFIG_INSTANCE_ROTATION_X = "instanceRotationX";
        public const string ARG_INSTANCE_ROTATION_X = "-" + CONFIG_INSTANCE_ROTATION_X;
        public const string CONFIG_INSTANCE_ROTATION_Y = "instanceRotationY";
        public const string ARG_INSTANCE_ROTATION_Y = "-" + CONFIG_INSTANCE_ROTATION_Y;
        public const string CONFIG_INSTANCE_ROTATION_Z = "instanceRotationZ";
        public const string ARG_INSTANCE_ROTATION_Z = "-" + CONFIG_INSTANCE_ROTATION_Z;
        // Database manager server
        public const string CONFIG_USE_CUSTOM_DATABASE_CLIENT = "useCustomDatabaseClient";
        public const string ARG_USE_CUSTOM_DATABASE_CLIENT = "-" + CONFIG_USE_CUSTOM_DATABASE_CLIENT;
        public const string CONFIG_DATABASE_OPTION_INDEX = "databaseOptionIndex";
        public const string ARG_DATABASE_OPTION_INDEX = "-" + CONFIG_DATABASE_OPTION_INDEX;
        public const string CONFIG_DATABASE_DISABLE_CACHE_READING = "databaseDisableCacheReading";
        public const string ARG_DATABASE_DISABLE_CACHE_READING = "-" + CONFIG_DATABASE_DISABLE_CACHE_READING;
        public const string CONFIG_DATABASE_ADDRESS = "databaseManagerAddress";
        public const string ARG_DATABASE_ADDRESS = "-" + CONFIG_DATABASE_ADDRESS;
        public const string CONFIG_DATABASE_PORT = "databaseManagerPort";
        public const string ARG_DATABASE_PORT = "-" + CONFIG_DATABASE_PORT;
        // Start servers
        public const string CONFIG_START_CENTRAL_SERVER = "startCentralServer";
        public const string ARG_START_CENTRAL_SERVER = "-" + CONFIG_START_CENTRAL_SERVER;
        public const string CONFIG_START_MAP_SPAWN_SERVER = "startMapSpawnServer";
        public const string ARG_START_MAP_SPAWN_SERVER = "-" + CONFIG_START_MAP_SPAWN_SERVER;
        public const string CONFIG_START_DATABASE_SERVER = "startDatabaseServer";
        public const string ARG_START_DATABASE_SERVER = "-" + CONFIG_START_DATABASE_SERVER;
        public const string CONFIG_START_MAP_SERVER = "startMapServer";
        public const string ARG_START_MAP_SERVER = "-" + CONFIG_START_MAP_SERVER;
    }
}

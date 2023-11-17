using System;
using System.IO;
using System.Linq;
using System.Net;
using Nuke.Source.Tooling;

namespace Nuke.Source
{
    public static partial class Tasks
    {
        public class DepotsOptions
        {
            public DepotsMode Mode { get; set; }
            public bool Verbose { get; set; }
            public long AppId { get; set; }
            public NetworkCredential SteamCredentials { get; set; }
            public string DepotDirectory { get; set; }
            public string GameName { get; set; }
        }

        public enum DepotsMode : short
        {
            UNKNOWN = 0,
            STEAM_CMD = 1,
            DEPOT_DOWNLOADER = 2
        }

        // ReSharper disable once CognitiveComplexity
        public static void Depots(Action<DepotsOptions> options)
        {
            var op = new DepotsOptions();
            options?.Invoke(op);

            if (!Directory.Exists(op.DepotDirectory)) Directory.CreateDirectory(op.DepotDirectory);

            if (op.Mode == DepotsMode.STEAM_CMD)
            {
                Abstractions.Tasks.Source(_ => new STEAMCMD()
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetUserName(op.SteamCredentials.UserName)
                    .SetPassword(op.SteamCredentials.Password)
                    .EnableValidate()
                    .SetInstallDir(op.DepotDirectory));
            }
            else if (op.Mode == DepotsMode.DEPOT_DOWNLOADER)
            {
                Abstractions.Tasks.Source(_ => new DEPOTDOWNLOADER()
                    .SetVerbose(op.Verbose)
                    .SetAppId(op.AppId)
                    .SetUserName(op.SteamCredentials.UserName)
                    .SetPassword(op.SteamCredentials.Password)
                    .SetInstallDir(op.DepotDirectory));
            }

            var installDirectory = Path.Combine(op.DepotDirectory, op.AppId.ToString());
            if (!Directory.Exists(installDirectory) || !Directory.GetFiles(installDirectory).Any())
                throw new DirectoryNotFoundException("Depot download was unsuccessful");

            var gameBinDir = new DirectoryInfo(Path.Combine(installDirectory, op.GameName, "bin"));
            var sourcetestBinDir = new DirectoryInfo(Path.Combine(installDirectory, "sourcetest", "bin"));

            if (!gameBinDir.Exists && sourcetestBinDir.Exists)
            {
                gameBinDir.Create();
                sourcetestBinDir.CopyAll(gameBinDir, true);
            }
        }
    }
}

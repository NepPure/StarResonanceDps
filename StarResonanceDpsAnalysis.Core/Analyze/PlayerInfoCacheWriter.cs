using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using StarResonanceDpsAnalysis.Core.Analyze.Models;

namespace StarResonanceDpsAnalysis.Core.Analyze
{
    public class PlayerInfoCacheWriter
    {
        private string SavePath { get; set; }
        private PlayerInfoCacheFileV3_0_0 PlayerInfoCacheFile { get; set; }

        private PlayerInfoCacheWriter(string path, IEnumerable<PlayerInfoFileData> playerInfos)
            : this(path, new PlayerInfoCacheFileV3_0_0()
            {
                FileVersion = PlayerInfoCacheFileVersion.V3_0_0,
                PlayerInfos = [.. playerInfos]
            })
        {
        }

        private PlayerInfoCacheWriter(string path, PlayerInfoCacheFileV3_0_0 playerInfoCacheFile)
        {
            SavePath = path;
            PlayerInfoCacheFile = playerInfoCacheFile;
        }

        private void WriteToFile()
        {
            // 修改此函数时, 请注意同时修改 PlayerInfoCacheReader

            if (!Directory.Exists(SavePath))
            {
                Directory.CreateDirectory(SavePath);
            }

            var filePath = Path.Combine(SavePath, "PlayerInfoCache.dat");

            using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.WriteThrough);
            using var bw = new BsonDataWriter(fs);
            var serializer = new JsonSerializer();
            serializer.Serialize(bw, PlayerInfoCacheFile);
        }

        public async Task WriteToFileAsync()
        {
            await Task.Run(WriteToFile);
        }

        public static void WriteToFile(string path, PlayerInfoCacheFileV3_0_0 playerInfoCacheFile)
        {
            var writer = new PlayerInfoCacheWriter(path, playerInfoCacheFile);
            writer.WriteToFile();
        }

        public static void WriteToFile(string path, IEnumerable<PlayerInfoFileData> playerInfos)
        {
            var writer = new PlayerInfoCacheWriter(path, playerInfos);
            writer.WriteToFile();
        }

        public static async Task WriteToFileAsync(string path, PlayerInfoCacheFileV3_0_0 playerInfoCacheFile)
        {
            await Task.Run(() => WriteToFile(path, playerInfoCacheFile));
        }

        public static async Task WriteToFileAsync(string path, IEnumerable<PlayerInfoFileData> playerInfoCacheFile)
        {
            await Task.Run(() => WriteToFile(path, playerInfoCacheFile));
        }
    }
}

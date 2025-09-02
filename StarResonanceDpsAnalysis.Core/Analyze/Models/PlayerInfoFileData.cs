using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using StarResonanceDpsAnalysis.Core.Data.Models;
using StarResonanceDpsAnalysis.Core.Extends.System;

namespace StarResonanceDpsAnalysis.Core.Analyze.Models
{
    public class PlayerInfoFileData
    {
        public long? UID { get; internal set; }
        public string? Name { get; internal set; }
        public int? ProfessionID { get; internal set; }
        public string? SubProfessionName { get; internal set; }
        public int? CombatPower { get; internal set; }
        public int? Critical { get; internal set; }
        public int? Lucky { get; internal set; }
        public long? MaxHP { get; internal set; }
        public byte[] Hash { get; internal set; } = [];

        private static byte[] CreateMD5(PlayerInfoFileData data) =>
            MD5.HashData($"{data.UID}_{data.Name}_{data.ProfessionID}_{data.SubProfessionName}_{data.CombatPower}_{data.Critical}_{data.Lucky}_{data.MaxHP}".GetBytes());
        public bool TestHash() => TestHash(this);
        public static bool TestHash(PlayerInfoFileData data) => data.Hash.SequenceEqual(CreateMD5(data));

        public static implicit operator PlayerInfoFileData(PlayerInfo p)
        { 
            var tmp = new PlayerInfoFileData()
            {
                UID = p.UID,
                Name = p.Name,
                ProfessionID = p.ProfessionID,
                SubProfessionName = p.SubProfessionName,
                CombatPower = p.CombatPower,
                Critical = p.Critical,
                Lucky = p.Lucky,
                MaxHP = p.MaxHP
            };
            tmp.Hash = CreateMD5(tmp);
            return tmp;
        }
    }
}

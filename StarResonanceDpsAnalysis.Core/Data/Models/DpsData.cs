using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Data.Models
{
    public struct DpsData
    {
        public long UUID { get; internal set; }
        public long? UID { get; internal set; }
        public string Name { get; internal set; }
        public int? ProfessionID { get; internal set; }
        public int? CombatPower { get; internal set; }
        public long StartLoggedTick { get; internal set; }
        public long LastLoggedTick { get; internal set; }
        public long TotalAttackDamage { get; internal set; }
        public long TotalTakenDamage { get; internal set; }
        public long TotalHeal { get; internal set; }

    }
}

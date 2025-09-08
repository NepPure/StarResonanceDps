using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Data.Models
{
    public class PlayerInfo
    {
        public long UID { get;  set; }
        public string? Name { get;  set; }
        public int? ProfessionID { get;  set; }
        public string? SubProfessionName { get;  set; }
        public int? CombatPower { get;  set; }
        public int? Level { get;  set; }
        public int? RankLevel { get;  set; }
        public int? Critical { get;  set; }
        public int? Lucky { get;  set; }
        public long? MaxHP { get;  set; }
        public long? HP { get;  set; }
    }
}

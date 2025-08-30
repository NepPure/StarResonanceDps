using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core.Data.Models
{
    /// <summary>
    /// 战斗日志分段
    /// </summary>
    public struct BattleLogSection()
    {
        /// <summary>
        /// 分段开始索引
        /// </summary>
        public int StartIndex { get; set; }
        /// <summary>
        /// 分段结束索引 (如果为最后一个分段, 则为 -1)
        /// </summary>
        public int EndIndex { get; set; } = -1;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Core
{
    public class PacketData(object packet)
    {
        public object Packet { get; set; } = packet;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 星痕共鸣DPS统计
{
    public static class ProtoFieldHelper
    {
        /// <summary>
        /// 从 protobuf 字典中尝试获取原始字段值（不进行类型转换或解码）
        /// </summary>
        public static object? TryGetRaw(Dictionary<int, object> dict, int tag)
        {
            dict.TryGetValue(tag, out var val);
            return val;
        }

        /// <summary>
        /// 尝试从字段中获取嵌套解码结构（即 ProtoValue.Decoded）
        /// </summary>
        public static Dictionary<int, object>? TryGetDecoded(Dictionary<int, object> dict, int tag)
        {
            if (dict.TryGetValue(tag, out var val) && val is ProtoValue pv && pv.Decoded != null)
                return pv.Decoded;
            return null;
        }

        /// <summary>
        /// 尝试从字段中获取 UInt64（支持 long 和 ulong 类型）
        /// </summary>
        public static ulong TryGetU64(Dictionary<int, object> dict, int tag)
        {
            if (dict.TryGetValue(tag, out var val))
            {
                if (val is ulong u) return u;
                if (val is long l) return (ulong)l;
            }
            return 0;
        }

        /// <summary>
        /// 尝试从字段中读取布尔值（非 0 即为 true）
        /// </summary>
        public static bool TryGetBool(Dictionary<int, object> dict, int tag)
        {
            if (dict.TryGetValue(tag, out var val))
            {
                if (val is ulong u) return u != 0;
                if (val is long l) return l != 0;
            }
            return false;
        }

        /// <summary>
        /// 从 4 字节数组读取大端序 32 位整数
        /// </summary>
        public static int ReadInt32BigEndian(byte[] buf)
        {
            if (buf.Length != 4)
                throw new ArgumentException("必须是4字节");

            return (buf[0] << 24) | (buf[1] << 16) | (buf[2] << 8) | buf[3];
        }
    }

}

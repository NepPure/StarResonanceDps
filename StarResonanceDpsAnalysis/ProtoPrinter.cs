using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis
{
    public static class ProtoPrinter
    {
        public static void Print(byte[] data, int indent = 0)
        {
            int pos = 0;
            ReadMessage(data, ref pos, data.Length, indent);
        }

        private static void ReadMessage(ReadOnlySpan<byte> data, ref int pos, int end, int indent)
        {
            while (pos < end)
            {
                uint key = ReadVarint32(data, ref pos);
                int tag = (int)(key >> 3);
                int wireType = (int)(key & 0x07);

                Console.Write(new string(' ', indent * 2));
                Console.Write($"Tag {tag} (wire {wireType}): ");

                switch (wireType)
                {
                    case 0: // varint
                        ulong v = ReadVarint64(data, ref pos);
                        // 同时尝试 ZigZag 反解，帮你判断是否是 sint32/sint64
                        long zigzag = (long)((v >> 1) ^ (~(v & 1) + 1));
                        Console.WriteLine($"{(long)v}  (zigzag:{zigzag})");
                        break;

                    case 1: // fixed64/double
                        ulong f64 = BinaryPrimitives.ReadUInt64LittleEndian(data.Slice(pos, 8));
                        pos += 8;
                        double d = BitConverter.Int64BitsToDouble((long)f64);
                        Console.WriteLine($"{f64}  (double:{d})");
                        break;

                    case 2: // length-delimited: bytes/string/sub-message/packed
                        int len = (int)ReadVarint32(data, ref pos);
                        var bytes = data.Slice(pos, len).ToArray();
                        pos += len;
                        Console.WriteLine($"len={len}");

                        // 1) 试 UTF8 预览
                        try
                        {
                            var s = System.Text.Encoding.UTF8.GetString(bytes);
                            if (!string.IsNullOrEmpty(s) && s.IndexOf('\0') < 0)
                                Console.WriteLine(new string(' ', (indent + 1) * 2) + $"utf8: \"{s}\"");
                        }
                        catch { }

                        // 2) 试子消息递归
                        try
                        {
                            int innerPos = 0;
                            var span = bytes.AsSpan();
                            // 简单探测：能读到至少一个 key 就尝试递归
                            uint _ = ReadVarint32(span, ref innerPos);
                            innerPos = 0;
                            Console.WriteLine(new string(' ', (indent + 1) * 2) + "{");
                            ReadMessage(span, ref innerPos, bytes.Length, indent + 1);
                            Console.WriteLine(new string(' ', (indent + 1) * 2) + "}");
                        }
                        catch
                        {
                            Console.WriteLine(new string(' ', (indent + 1) * 2) + BitConverter.ToString(bytes));
                        }
                        break;

                    case 5: // fixed32/float
                        uint f32 = BinaryPrimitives.ReadUInt32LittleEndian(data.Slice(pos, 4));
                        pos += 4;
                        float fl = BitConverter.Int32BitsToSingle((int)f32);
                        Console.WriteLine($"{f32}  (float:{fl})");
                        break;

                    default:
                        Console.WriteLine("unknown-wire");
                        return;
                }
            }
        }

        private static uint ReadVarint32(ReadOnlySpan<byte> data, ref int pos)
        {
            uint result = 0; int shift = 0;
            while (true)
            {
                byte b = data[pos++];
                result |= (uint)(b & 0x7F) << shift;
                if ((b & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }

        private static ulong ReadVarint64(ReadOnlySpan<byte> data, ref int pos)
        {
            ulong result = 0; int shift = 0;
            while (true)
            {
                byte b = data[pos++];
                result |= (ulong)(b & 0x7F) << shift;
                if ((b & 0x80) == 0) break;
                shift += 7;
            }
            return result;
        }
    }
}

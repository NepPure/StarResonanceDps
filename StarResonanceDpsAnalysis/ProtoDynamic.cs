using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ProtoValue
{
    public byte[] Raw;
    public Dictionary<int, object> Decoded;

    public ProtoValue(byte[] raw, Dictionary<int, object> decoded = null)
    {
        Raw = raw;
        Decoded = decoded;
    }

    public override string ToString() => BitConverter.ToString(Raw);
}

public static class ProtoDynamic
{
    public static string FormatProtoValue(object value, int indent = 0)
    {
        string pad = new(' ', indent);
        if (value is ProtoValue pv)
        {
            if (pv.Decoded != null)
            {
                var lines = new List<string> { $"{pad}[ProtoValue] Nested:" };
                foreach (var kv in pv.Decoded)
                {
                    string sub = FormatProtoValue(kv.Value, indent + 2);
                    lines.Add($"{pad}  Field {kv.Key}:\n{sub}");
                }
                return string.Join("\n", lines);
            }
            else if (pv.Raw != null)
            {
                return $"{pad}{pv.Raw} ({pv.Raw.GetType().Name})";
            }
            else
            {
                return $"{pad}(null)";
            }
        }
        else if (value is List<object> list)
        {
            var lines = new List<string> { $"{pad}[List] ({list.Count} items)" };
            int index = 0;
            foreach (var item in list)
            {
                string sub = FormatProtoValue(item, indent + 2);
                lines.Add($"{pad}  [{index++}]:\n{sub}");
            }
            return string.Join("\n", lines);
        }
        else
        {
            return $"{pad}{value} ({value?.GetType().Name ?? "null"})";
        }
    }


    public static Dictionary<int, object> Decode(byte[] data)
    {
        var result = new Dictionary<int, object>();
        using var ms = new MemoryStream(data);
        using var reader = new BinaryReader(ms);

        while (ms.Position < ms.Length)
        {
            uint key = ReadVarint(reader);
            // 检查uint到int的转换是否会溢出
            if (key > (long)int.MaxValue * 8 + 7)
            {
                throw new OverflowException("Key too large to convert to int");
            }
            int tag = (int)(key >> 3);
            int wireType = (int)(key & 0x7);

            object value = null;

            switch (wireType)
            {
                case 0: // Varint
                    value = ReadVarint64(reader);
                    break;

                case 1: // 64-bit
                    if (reader.BaseStream.Length - reader.BaseStream.Position >= 8)
                    {
                        value = reader.ReadUInt64();
                    }
                    else
                    {
                        throw new InvalidDataException("Insufficient data to read UInt64");
                    }
                    break;

                case 2: // Length-delimited
                    uint len = ReadVarint(reader);
                    byte[] buf = reader.ReadBytes((int)len);

                    try
                    {
                        var inner = Decode(buf);
                        value = new ProtoValue(buf, inner);
                    }
                    catch
                    {
                        value = new ProtoValue(buf);
                    }
                    break;

                case 5: // 32-bit
                    value = reader.ReadUInt32();
                    break;

                case 4: // End group (已废弃)
                    // 忽略废弃的End group类型
                    break;
                default:
                    // 遇到未知wireType，需要跳过整个字段值
                    // 对于未知类型，我们尝试按Length-delimited格式处理（最常见的可变长度格式）
                    try
                    {
                        // 尝试按Length-delimited格式读取长度并跳过
                        uint unknownLen = ReadVarint(reader);
                        // 跳过指定长度的字节
                        if (ms.Position + unknownLen <= ms.Length)
                        {
                            ms.Position += unknownLen;
                        }
                        else
                        {
                            // 如果超出流长度，直接跳到流末尾
                            ms.Position = ms.Length;
                        }
                    }
                    catch
                    {
                        // 如果读取失败，至少跳过一个字节以避免死循环
                        if (ms.Position < ms.Length)
                        {
                            reader.ReadByte();
                        }
                    }
                    continue;
            }

            if (result.TryGetValue(tag, out var existing))
            {
                if (existing is List<object> list)
                {
                    list.Add(value);
                }
                else
                {
                    result[tag] = new List<object> { existing, value };
                }
            }
            else
            {
                result[tag] = value;
            }
        }

        return result;
    }
    public static byte[] Encode(Dictionary<int, object> data)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        foreach (var kv in data)
        {
            int tag = kv.Key;
            if (kv.Value is List<object> list)
            {
                foreach (var item in list)
                    WriteField(writer, tag, item);
            }
            else
            {
                WriteField(writer, tag, kv.Value);
            }
        }

        return ms.ToArray();
    }

    private static void WriteField(BinaryWriter writer, int tag, object value)
    {
        if (value == null) return;

        if (value is ulong || value is long || value is int || value is uint)
        {
            WriteVarint(writer, (uint)(tag << 3 | 0));
            WriteVarint(writer, Convert.ToUInt64(value));
        }
        else if (value is double d)
        {
            WriteVarint(writer, (uint)(tag << 3 | 1));
            writer.Write(BitConverter.GetBytes(d));
        }
        else if (value is float f)
        {
            WriteVarint(writer, (uint)(tag << 3 | 5));
            writer.Write(BitConverter.GetBytes(f));
        }
        else if (value is string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            WriteVarint(writer, (uint)(tag << 3 | 2));
            WriteVarint(writer, (uint)bytes.Length);
            writer.Write(bytes);
        }
        else if (value is byte[] bytes)
        {
            WriteVarint(writer, (uint)(tag << 3 | 2));
            WriteVarint(writer, (uint)bytes.Length);
            writer.Write(bytes);
        }
        else if (value is ProtoValue pv)
        {
            var buf = pv.Raw;
            WriteVarint(writer, (uint)(tag << 3 | 2));
            WriteVarint(writer, (uint)buf.Length);
            writer.Write(buf);
        }
        else if (value is Dictionary<int, object> nested)
        {
            var buf = Encode(nested);
            WriteVarint(writer, (uint)(tag << 3 | 2));
            WriteVarint(writer, (uint)buf.Length);
            writer.Write(buf);
        }
        else
        {
            throw new Exception($"无法编码类型: {value.GetType()}");
        }
    }

    private static uint ReadVarint(BinaryReader reader)
    {
        uint result = 0;
        int shift = 0;
        byte b;
        do
        {
            b = reader.ReadByte();
            result |= (uint)(b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);
        return result;
    }

    private static ulong ReadVarint64(BinaryReader reader)
    {
        ulong result = 0;
        int shift = 0;
        byte b;
        do
        {
            b = reader.ReadByte();
            result |= (ulong)(b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);
        return result;
    }

    private static void WriteVarint(BinaryWriter writer, ulong value)
    {
        while (value >= 0x80)
        {
            writer.Write((byte)((value & 0x7F) | 0x80));
            value >>= 7;
        }
        writer.Write((byte)value);
    }
}

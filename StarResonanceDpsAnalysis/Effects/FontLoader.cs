using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace StarResonanceDpsAnalysis.Effects
{
    public static class FontLoader
    {
        private static readonly PrivateFontCollection _pfc = new();
        private static readonly Dictionary<string, Font> _families = [];

        public static bool TryLoadFontFromBytes(string fontKey, byte[] bytes, float fontSize, out Font? font)
        {
            font = null;

            try
            {
                if (_families.TryGetValue(fontKey, out font)) return true;

                font = LoadFontFromBytes(bytes, fontSize);

                _families.Add(fontKey, font);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"字体从内存转换时出错: {ex.Message}\r\n{ex.StackTrace}");

                return false;
            }
        }

        private static Font LoadFontFromBytes(byte[] bytes, float fontSize) 
        {
            var fontPtr = Marshal.AllocCoTaskMem(bytes.Length);
            Marshal.Copy(bytes, 0, fontPtr, bytes.Length);

            try
            {
                var prevLength = _pfc.Families.Length;
                _pfc.AddMemoryFont(fontPtr, bytes.Length);
                if (_pfc.Families.Length <= prevLength)
                {
                    throw new Exception("Bytes convert to fontfamily failed.");
                }

                return new Font(_pfc.Families[^1], fontSize);
            }
            finally
            {
                Marshal.FreeCoTaskMem(fontPtr);
            }
        }
    }
}

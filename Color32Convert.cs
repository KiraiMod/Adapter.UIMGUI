using System;
using UnityEngine;

namespace KiraiMod.Adapter.UIMGUI
{
    // this likely doesn't match specs
    public static class Color32Convert
    {
        public static Color32 FromString(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                throw new ArgumentNullException(nameof(str));

            if (str.StartsWith("#"))
                str = str[1..];

            byte r, g, b, a;
            if (str.Length == 3 || str.Length == 4)
            {
                r = (byte)(byte.Parse(str[0].ToString()) * 17);
                g = (byte)(byte.Parse(str[1].ToString()) * 17);
                b = (byte)(byte.Parse(str[2].ToString()) * 17);
                a = str.Length == 4 ? (byte)(byte.Parse(str[4].ToString()) * 17) : (byte)255;
            }
            else if (str.Length == 6 || str.Length == 8)
            {
                r = byte.Parse(str[0..2]);
                g = byte.Parse(str[2..4]);
                b = byte.Parse(str[4..6]);
                a = str.Length == 4 ? (byte)(byte.Parse(str[6..8].ToString()) * 17) : (byte)255;
            }
            else throw new ArgumentException(nameof(str));

            return new Color32(r, g, b, a);
        }

        public static string FromColor32(Color32 color) => $"#{color.r:X}{color.g:X}{color.b:X}{color.a:X}";
    }
}

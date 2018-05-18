using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ByteProtocol
{
    public static class ByteArrayExtension
    {
        public static int Find(this byte[] src, byte[] find)
        {
            int index = -1;
            int matchIndex = 0;
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else if (src[i] == find[0])
                {
                    matchIndex = 1;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        public static byte[] Replace(this byte[] src, byte[] search, byte[] repl)
        {
            byte[] dst = null;
            int index = src.Find(search);
            if (index >= 0)
            {
                dst = new byte[src.Length - search.Length + repl.Length];
                Buffer.BlockCopy(src, 0, dst, 0, index);
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                Buffer.BlockCopy(
                    src,
                    index + search.Length,
                    dst,
                    index + repl.Length,
                    src.Length - (index + search.Length));
                return dst;
            }
            else
                return src;
        }

        public static byte[] Replace(this byte[] src, byte[] search, byte repl) => Replace(src, search, new byte[1] { repl });

        public static byte[] Replace(this byte[] src, byte search, byte repl) => Replace(src, new byte[1] { search }, repl);

        public static byte[] Merge(this byte[] src, byte[] dest)
        {
            byte[] newArray = new byte[src.Length + dest.Length];
            Array.Copy(src, newArray, src.Length);
            Array.Copy(dest, 0, newArray, src.Length, dest.Length);
            return newArray;
        }

        public static byte[] Merge(this byte[] src, byte dest) => Merge(src, new byte[] { dest });

        public static byte[] Merge(this byte src, byte[] dest) => Merge(new byte[] { src }, dest);

        public static byte[] Merge(this byte src, byte dest) => Merge(new byte[] { src }, dest);

        public static byte[] SubArray(this byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static byte[] AsciiToByte(this byte[] ascii)
        {
            var hex = Encoding.ASCII.GetString(ascii);
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        #region Numbers

        public static byte[] ToLEBytes(this ushort num) => new byte[] { (byte)(num & 0xff00), (byte)(num & 0xff) };
        public static byte[] ToLEBytes(this short num) => new byte[] { (byte)(num & 0xff00), (byte)(num & 0xff) };
        static byte[] ToLEBytes(this int num) => new byte[] { (byte)(num & 0xff000000), (byte)(num & 0xff0000), (byte)(num & 0xff00), (byte)(num & 0xff) };
        static byte[] ToLEBytes(this uint num) => new byte[] { (byte)(num & 0xff000000), (byte)(num & 0xff0000), (byte)(num & 0xff00), (byte)(num & 0xff) };
        public static short ToLeShort(this byte[] data) => (short)((data[1] << 8) | data[0]);
        public static uint ToLeUInt(this byte[] data) => (uint)((data[3] << 24) | (data[2] << 16) | (data[1] << 8) | data[0]);
        public static int ToLeUShort(this byte[] data) => (ushort)((data[1] << 8) | data[0]);
        public static int ToLeInt(this byte[] data) => (data[3] << 24) | (data[2] << 16) | (data[1] << 8) | data[0];

        #endregion
    }
}

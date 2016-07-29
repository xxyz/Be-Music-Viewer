using System;
using System.Collections.Generic;
using System.Linq;

namespace BMSParser
{
    public enum NoteType { BGM, Scratch1p, Key1p, Key2p, Scratch2p }

    public class BMSUtil
    {
        public static NoteType GetNoteType(int x)
        {
            if (x == 0)
                return NoteType.BGM;
            else if (x == 8)
                return NoteType.Scratch1p;
            else if (x < 8)
                return NoteType.Key1p;
            else if (x == 16)
                return NoteType.Scratch2p;
            else if (x < 16)
                return NoteType.Key2p;
            return NoteType.BGM;
        }

        //split string by chunkSize
        public static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }

        //Hexadecimal to Int
        public static int HexToInt(string hex)
        {
            string sample = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int result = 0;
            for (int i = 0; i < hex.Length; i++)
            {
                result *= 36;
                for (int j = 0; j < sample.Length; j++)
                {
                    if (hex[i] == sample[j])
                    {
                        result += j;
                    }
                }
            }

            return result;
        }
    }
}
using System;

namespace BMSParser_new
{
    class Note
    {
        public int x; //lane number(1~7: note, 8: scratch...)
        public ulong y; //pulse number
        public ulong l = 0; //note's length (0: normal, >0: LN)
        public bool c = true; //restart audio or not

        //temporary field
        public int measureNum;
        public double measureDiv;

        public Note(int x, int measureNum, double measureDiv)
        {
            this.x = x;
            this.measureNum = measureNum;
            this.measureDiv = measureDiv;
        }
    }
}
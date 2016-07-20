namespace BMSParser_new
{
    //bar-line event
    class BarLine
    {
        public ulong y; //pulse number

        /*temporary field*/
        //measure number
        public int measureNum;
        public ulong accumPulse;
        //measure's length
        public double measureLength;

        public BarLine(int measureNum, double measureLength)
        {
            this.measureNum = measureNum;
            this.measureLength = measureLength;
        }
    }
}
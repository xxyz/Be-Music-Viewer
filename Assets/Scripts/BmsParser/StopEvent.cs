namespace BMSParser_new
{
    class StopEvent
    {
        public ulong y; //pulse num
        public ulong duration; // stop duration (pulse)


        //temporary field
        public int measureNum;
        public double measureDiv;

        public StopEvent(ulong duration, int measureNum, double measureDiv)
        {
            this.duration = duration;
            this.measureNum = measureNum;
            this.measureDiv = measureDiv;
        }
    }
}

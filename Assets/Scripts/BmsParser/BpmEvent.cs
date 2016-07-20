namespace BMSParser_new
{
    class BpmEvent
    {
        
        public ulong y; // pulse num
        public double bpm;

        public int id; //Bpm Change's id(not needed on BMSON)

        //temporary field
        public int measureNum;
        public double measureDiv;

        public BpmEvent(int id, ulong bpm)
        {
            this.id = id;
            this.bpm = bpm;
        }

        public BpmEvent(int id, ulong y, ulong bpm)
        {
            this.id = id;
            this.y = y;
            this.bpm = bpm;
        }

        public BpmEvent(double bpm, int measureNum, double measureDiv)
        {
            this.bpm = bpm;
            this.measureNum = measureNum;
            this.measureDiv = measureDiv;
        }
    }
}

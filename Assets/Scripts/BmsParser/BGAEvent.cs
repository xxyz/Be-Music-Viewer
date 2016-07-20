namespace BMSParser_new
{
    class BGAEvent
    {
        public ulong y;
        public ulong id;

        //temporary field
        public int measureNum;
        public double measureDiv;

        public BGAEvent(int id, int measureNum, double measureDiv)
        {
            this.id = (ulong)id;
            this.measureNum = measureNum;
            this.measureDiv = measureDiv;
        }
    }
}

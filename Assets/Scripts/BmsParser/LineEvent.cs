namespace BMSParser_new
{
    class LineEvent : BmsEvent
    {
        public ulong accumY = 0;
        public double measureLength = 1;

        public LineEvent(int measure, double measureLength)
        {
            this.measure = measure;
            this.measureLength = measureLength;
        }

        public LineEvent(ulong argY)
        {
            this.y = argY;
            eventType = EventType.LineEvent;
        }
    }
}
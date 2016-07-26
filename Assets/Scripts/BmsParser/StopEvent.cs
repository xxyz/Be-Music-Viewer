namespace BMSParser_new
{
    class StopEvent : BmsEvent
    {
        public ulong duration; // stop duration (pulse)
        public double durationTime;
        public ulong id; // id for searching StopHeader



        public StopEvent(ulong duration, int measure, double measureDiv)
        {
            this.duration = duration;
            this.measure = measure;
            this.measureDiv = measureDiv;
            eventType = EventType.StopEvent;
        }
    }
}
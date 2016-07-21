namespace BMSParser_new
{
    class StopEvent : BmsEvent
    {
        public ulong duration; // stop duration (pulse)

        public ulong id; // id for searching StopHeader

        public StopEvent(ulong id, int measure, double measureDiv)
        {
            this.id = id;
            this.measure = measure;
            this.measureDiv = measureDiv;
            eventType = EventType.StopEvent;
        }
    }
}
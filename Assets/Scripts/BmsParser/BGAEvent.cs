namespace BMSParser_new
{
    class BGAEvent : BmsEvent
    {
        public ulong id;
        
        public BGAEvent(int id, int measure, double measureDiv, EventType eventType)
        {
            this.id = (ulong)id;
            this.measure = measure;
            this.measureDiv = measureDiv;
            this.eventType = eventType;
        }
    }
}

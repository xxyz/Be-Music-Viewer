namespace BMSParser
{
    public class BGAEvent : BmsEvent
    {
        public ulong id;
        public bool isVideo;
        
        public BGAEvent(int id, int measure, double measureDiv, EventType eventType, bool isVideo)
        {
            this.id = (ulong)id;
            this.measure = measure;
            this.measureDiv = measureDiv;
            this.eventType = eventType;
            this.isVideo = isVideo;
        }
    }
}

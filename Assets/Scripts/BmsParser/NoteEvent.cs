namespace BMSParser_new
{
    class NoteEvent : BmsEvent
    {
        public int x; //lane
        public ulong l = 0; //length
        public bool c = true; // restart sound?
        public ulong id; // id from SoundHeader

        public NoteEvent (int x, ulong l, bool c, int id, int measure, double measureDiv)
        {
            this.x = x;
            this.l = l;
            this.c = c;
            this.id = (ulong)id;
            this.measure = measure;
            this.measureDiv = measureDiv;
            eventType = EventType.NoteEvent;
        }
    }
}
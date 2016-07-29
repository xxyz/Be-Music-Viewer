namespace BMSParser
{
    public class NoteEvent : BmsEvent
    {
        public int x; //lane
        public ulong l = 0; //length
        public bool c = true; // restart sound?
        public ulong id; // id from SoundHeader
        public int channel; // origianl bms style channel

        public NoteEvent (int x, ulong l, bool c, int id, int measure, double measureDiv, int channel)
        {
            this.x = x;
            this.l = l;
            this.c = c;
            this.id = (ulong)id;
            this.measure = measure;
            this.measureDiv = measureDiv;
            this.channel = channel;
            eventType = EventType.NoteEvent;
        }
    }
}
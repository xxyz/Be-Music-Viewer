namespace BMSParser
{
    public class BpmEvent : BmsEvent
    {
        public double bpm;

        public int id; //Bpm Change's id(not needed on BMSON)

        public BpmEvent(int id, double bpm)
        {
            this.id = id;
            this.bpm = bpm;
            this.eventType = EventType.BpmEvent;
        }

        public BpmEvent(double bpm, int measure, double measureDiv)
        {
            this.bpm = bpm;
            this.measure = measure;
            this.measureDiv = measureDiv;
            this.eventType = EventType.BpmEvent;
        }
    }
}
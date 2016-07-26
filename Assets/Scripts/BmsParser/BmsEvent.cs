using System;

namespace BMSParser_new
{
    enum EventType { LineEvent, NoteEvent, BGAEvent, LayerEvent, PoorEvent, BpmEvent, StopEvent };

    abstract class BmsEvent : IComparable<BmsEvent>
    {
        public ulong y;
        public double time = 0;
        public int measure;
        public double measureDiv = 0;
        public EventType eventType;

        public void calcY(ulong resolution, double measureLength, ulong accumPulse)
        {
            this.y = (ulong)(resolution * measureDiv * measureLength + accumPulse);
        }

        public int CompareTo(BmsEvent other)
        {
            int result;
            if((result = measure.CompareTo(other.measure)) == 0)
            {
                if((result = measureDiv.CompareTo(other.measureDiv)) == 0)
                    return eventType.CompareTo(other.eventType);
            }
                
            return result;
        }
    }
}
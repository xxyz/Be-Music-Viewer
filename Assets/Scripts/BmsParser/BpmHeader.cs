namespace BMSParser
{
    public class BpmHeader
    {
        public int id; //id of file
        public double bpm;

        public BpmHeader(int id, double bpm)
        {
            this.id = id;
            this.bpm = bpm;
        }
    }
}

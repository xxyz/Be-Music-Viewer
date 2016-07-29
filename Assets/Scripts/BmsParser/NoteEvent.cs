namespace BMSParser
{
    public enum NoteEventType { plain, mine, invisible }

    public class NoteEvent : BmsEvent
    {
        public int x; //lane
        public ulong l = 0; //length
        public bool c = true; // restart sound?
        public ulong id; // id from SoundHeader
        public int channel; // origianl bms style channel
        public NoteEventType noteEventType = NoteEventType.plain;

        public NoteEvent (bool c, int id, int measure, double measureDiv, int channel)
        {
            x = GetBmsOnX(channel);
            this.c = c;
            this.id = (ulong)id;
            this.measure = measure;
            this.measureDiv = measureDiv;
            this.channel = channel;
            eventType = EventType.NoteEvent;
            noteEventType = GetNoteEventType(channel);
        }

        private NoteEventType GetNoteEventType(int channel)
        {
            if (channel >= 469 && channel <= 513)
                return NoteEventType.mine;
            else if (channel >= 109 && channel <= 153)
                return NoteEventType.invisible;

            return NoteEventType.plain;
        }

        private int GetBmsOnX(int channel)
        {
            //1p key
            if (channel == 42)
                return 8;
            else if (channel >= 37 && channel <= 41)
                return channel - 36;
            else if (channel >= 44 && channel <= 45)
                return channel - 38;
            //2p key
            else if (channel >= 73 && channel <= 77)
                return channel - 64;
            else if (channel >= 80 && channel <= 82)
                return channel - 66;
            else if (channel == 78)
                return 16;
            //1p invisible
            else if (channel >= 109 && channel <= 113)
                return channel - 108;
            else if (channel >= 116 && channel <= 117)
                return channel - 110;
            //2p invisible
            else if (channel >= 145 && channel <= 149)
                return channel - 136;
            else if (channel >= 152 && channel <= 153)
                return channel - 138;
            //1p ln
            else if (channel >= 181 && channel <= 185)
                return channel - 180;
            else if (channel >= 188 && channel <= 189)
                return channel - 182;
            else if (channel == 186)
                return 8;
            //2p ln
            else if (channel >= 217 && channel <= 221)
                return channel - 208;
            else if (channel >= 224 && channel <= 225)
                return channel - 210;
            else if (channel == 222)
                return 16;
            //pms
            else if (channel >= 74 && channel <= 77)
                return channel - 68;
            //1p landMine
            else if (channel >= 469 && channel <= 473)
                return channel - 468;
            else if (channel >= 476 && channel <= 477)
                return channel - 470;
            //2p landMine
            else if (channel >= 505 && channel <= 509)
                return channel - 496;
            else if (channel >= 512 && channel <= 513)
                return channel - 498;
            return 0;
        }
    }
}
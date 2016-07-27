namespace BMSParser_new
{
    public enum NoteType { BGM, Scratch1p, Key1p, Key2p, Scratch2p }

    public class BMSUtil
    {

        public static NoteType GetNoteType(int x)
        {
            if (x == 0)
                return NoteType.BGM;
            else if (x == 8)
                return NoteType.Scratch1p;
            else if (x < 8)
                return NoteType.Key1p;
            else if (x == 16)
                return NoteType.Scratch2p;
            else if (x < 16)
                return NoteType.Key2p;
            return NoteType.BGM;
        }
    }
}
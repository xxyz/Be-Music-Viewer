using System.Collections.Generic;

namespace BMSParser_new
{
    class SoundChannel
    {
        public string name; //sound file's name
        public List<Note> notes = new List<Note>();

        public int id; //not used in BMSON

        public SoundChannel(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }
}
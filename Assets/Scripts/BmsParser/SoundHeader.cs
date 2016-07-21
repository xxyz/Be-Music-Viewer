namespace BMSParser_new
{
    class SoundHeader
    {
        public ulong id;
        public string name;

        public SoundHeader(int id, string name)
        {
            this.id = (ulong)id;
            this.name = name;
        }
    }
}

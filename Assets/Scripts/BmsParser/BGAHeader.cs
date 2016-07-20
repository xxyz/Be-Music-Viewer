namespace BMSParser_new
{
    class BGAHeader
    {
        public ulong id; //id of file
        public string name; //picture file name

        public BGAHeader(int id, string name)
        {
            this.id = (ulong)id;
            this.name = name;
        }
    }
}

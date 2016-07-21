class StopHeader
{
    public ulong id; //id of file
    public ulong duration;

    public StopHeader(int id, ulong duration)
    {
        this.id = (ulong)id;
        this.duration = duration;
    }
}
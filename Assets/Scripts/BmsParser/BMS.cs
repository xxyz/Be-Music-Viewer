using System.Collections.Generic;

namespace BMSParser_new
{
    class BMS
    {
        public string version; //bmson version
        public string path; //BMS absolute path
        public BMSInfo info = new BMSInfo(); //header info
        public List<BarLine> lines = new List<BarLine>(); //bar-lines' location in pulses
        public List<BpmEvent> bpm_events = new List<BpmEvent>();
        public List<StopEvent> stop_events = new List<StopEvent>();
        public List<SoundChannel> sound_channels = new List<SoundChannel>(); //note data
        public BGA bga = new BGA();
    }
}
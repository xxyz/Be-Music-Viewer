using System.Collections.Generic;

namespace BMSParser_new
{
    class BMS
    {
        public string version; //bmson version
        public string path; //BMS absolute path
        public BMSInfo info = new BMSInfo(); //header info

        public List<BmsEvent> bmsEvents = new List<BmsEvent>();

        public BGA bga = new BGA();
    }
}
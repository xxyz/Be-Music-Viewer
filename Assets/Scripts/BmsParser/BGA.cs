using System.Collections.Generic;

namespace BMSParser_new
{
    class BGA
    {
        public List<BGAHeader> bga_header = new List<BGAHeader>(); //pictures' id and filename
        public List<BGAEvent> bga_events = new List<BGAEvent>(); // pictures' sequence
        public List<BGAEvent> layer_events = new List<BGAEvent>(); // overlay pictures' sequence
        public List<BGAEvent> poor_events = new List<BGAEvent>(); // pictures' sequence when missed
    }
}

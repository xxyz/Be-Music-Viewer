using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BMSParser
{
    class BMSParser
    {

        private List<int> measureSetIndex = new List<int>();

        public BMS Parse(string path)
        {
            BMS bms = new BMS();
            //default encoding: Shift-JIS?
            Encoding encoding = Encoding.GetEncoding(932);
            String line;

            if (!File.Exists(path))
            {
                return null;
            }

            using (FileStream fs = File.OpenRead(path))
            {
                //detect charset
                Ude.CharsetDetector cdet = new Ude.CharsetDetector();
                cdet.Feed(fs);
                cdet.DataEnd();
                /*
                if(cdet.Charset != null)
                {
                    Console.WriteLine("Charset: {0}, confidence: {1}", cdet.Charset, cdet.Confidence);
                    encoding = Encoding.GetEncoding(cdet.Charset);
                }
                else
                {
                    Console.WriteLine("Detection Failed");
                }
                */
            }

            using (StreamReader sr = new StreamReader(path, encoding))
            {
                bms.path = Directory.GetParent(path).FullName;

                while((line = sr.ReadLine()) != null)
                {
                    ProcessBMSLine(line.Trim(), bms);
                }
            }
            SetSubtitle(bms.info);
            CalculatePulse(bms);
            FillRealTime(bms);

            return bms;
        }

        private void ProcessBMSLine(String line, BMS bms)
        {
            char[] seperators = { ' ', '\t', '　' };
            String[] args = line.Split(seperators, 2);

            if ( !(args[0].StartsWith("#")) )
                return;

            args[0] = args[0].ToUpper();

            if (args.Count() > 1)
            {
                if (args[0].StartsWith("#WAV")) //add soundchannel, note information will be added later.
                {
                    int id = BMSUtil.HexToInt(args[0].Substring(4, 2));
                    bms.info.soundHeaders.Add( new SoundHeader(id, args[1]) );
                }
                else if (args[0].StartsWith("#BMP"))
                {
                    int id = BMSUtil.HexToInt(args[0].Substring(4, 2));
                    bms.bga.bga_header.Add(new BGAHeader(id, args[1]));
                }
                else if (args[0] == "#BPM") //init bpm
                {
                    bms.info.init_bpm = Convert.ToDouble(args[1]);
                }
                else if (args[0].StartsWith("#BPM")) //bpm changing events, add to bpmHeader
                {
                    int id = BMSUtil.HexToInt(args[0].Substring(4, 2));
                    bms.info.bpmHeaders[id] =  new BpmHeader(id, Convert.ToDouble(args[1]));
                }
                else if (args[0].StartsWith("#STOP")) //Stop events
                {
                    int id = BMSUtil.HexToInt(args[0].Substring(5, 2));
                    bms.info.stopHeaders[id] = new StopHeader(id, Convert.ToUInt64(args[1]));
                }
                else if (args[0] == "#TITLE")
                {
                    bms.info.title = args[1];
                }
                else if (args[0] == "#SUBTITLE")
                {
                    bms.info.subtitle = args[1];
                }
                else if (args[0] == "#PLAYER")
                {
                    int player = Convert.ToInt32(args[1]);

                    if(player == 3)
                    {
                        bms.info.mode_hint = "beat-14k";
                    }
                }
                else if (args[0] == "#GENRE")
                {
                    bms.info.genre = args[1];
                }
                else if (args[0] == "#ARTIST")
                {
                    bms.info.artist = args[1];
                }
                else if (args[0] == "#PLAYLEVEL")
                {
                    if (!ulong.TryParse(args[1], out bms.info.level))
                        bms.info.level = 0;                    
                }
                else if (args[0] == "#RANK")
                {
                    int rank = Convert.ToInt32(args[1]);

                    //adjust judge_rank based on LR2's #RANK criteria.
                    if (rank == 0)
                        bms.info.judge_rank *= (8 / 21);
                    else if (rank == 1)
                        bms.info.judge_rank *= (15 / 21);
                    else if (rank == 2)
                        bms.info.judge_rank *= (18 / 21);
                }
                else if (args[0] == "#STAGEFILE")
                {
                    bms.info.eyecatch_image = args[1];
                }
                else if (args[0] == "#TOTAL")
                {
                    bms.info.total = Convert.ToDouble(args[1]);
                }
                else if (args[0] == "#VOLWAV")
                {
                    //VOLWAV is Obsoleted
                }
                else if (args[0] == "#SUBARTIST")
                {
                    bms.info.subartists.Add(args[1]);
                }
                else if (args[0] == "#BANNER")
                {
                    bms.info.banner_image = args[1];
                }
                else if (args[0] == "#DIFFICULTY")
                {
                    bms.info.difficulty = Convert.ToInt32(args[1]);
                }
                else if (args[0] == "#LNOBJ")
                {
                    bms.info.lnObj = BMSUtil.HexToInt(args[1]);
                }
                else if (args[0] == "#LNTYPE")
                {
                    //LNTYPE is almost obsolete?
                }
                else if (args[0] == "#BACKBMP")
                {
                    bms.info.back_bmp = args[1];
                }
                else if (args[0] == "#COMMENT")
                {
                    bms.info.comment = args[1];
                }
            }
            else
            {
                args = line.Split(':');

                if (args.Count() < 2 || args[0].Length < 6)
                    return;
                //TODO if 192 % args.count != 0 change resolution
                int measure = Convert.ToInt32(args[0].Substring(1, 3));
                int channel = BMSUtil.HexToInt(args[0].Substring(4, 2));

                if (bms.info.maxMeasure < measure)
                    bms.info.maxMeasure = measure;

                //measure length channel
                if(channel == 2)
                {
                    bms.bmsEvents.Add(new LineEvent(measure, Convert.ToDouble(args[1])));
                    measureSetIndex.Add(measure);
                    return;
                }

                IEnumerable<string> notes = BMSUtil.Split(args[1], 2);
                IEnumerator<string> noteEnum = notes.GetEnumerator();
                int argsLength = notes.Count<string>();
                int argIndex = 0;
                double measureDiv;

                while(noteEnum.MoveNext())
                {
                    if(noteEnum.Current!= "00")
                    {
                        measureDiv = (double)argIndex / argsLength;
                        int id = BMSUtil.HexToInt(noteEnum.Current);

                        if (channel == 3)
                        {
                            //channel 03 use 00-FF hex
                            int bpm = Convert.ToInt32(noteEnum.Current, 16);
                            bms.bmsEvents.Add(new BpmEvent(bpm, measure, measureDiv));
                        }
                        //bga base
                        else if(channel == 4)
                        {
                            bool isVideo = false;
                            
                            if (bms.bga.bga_header.Find(x => (int)x.id == id) != null)
                            {
                                isVideo = IsVideo(bms.bga.bga_header.Find(x => (int)x.id == id).name);
                            }
                            
                            bms.bmsEvents.Add(new BGAEvent(id, measure, measureDiv, EventType.BGAEvent, isVideo));
                        }
                        //bga poor
                        else if (channel == 6)
                        {
                            bool isVideo = false;
                            if (bms.bga.bga_header.Find(x => (int)x.id == id) != null)
                            {
                                isVideo = IsVideo(bms.bga.bga_header.Find(x => (int)x.id == id).name);
                            }
                            bms.bmsEvents.Add(new BGAEvent(id, measure, measureDiv, EventType.PoorEvent, isVideo));
                        }
                        //bga layer
                        else if(channel == 7)
                        {
                            bool isVideo = false;
                            if (bms.bga.bga_header.Find(x => (int)x.id == id) != null)
                            {
                                isVideo = IsVideo(bms.bga.bga_header.Find(x => (int)x.id == id).name);
                            }
                            bms.bmsEvents.Add(new BGAEvent(id, measure, measureDiv, EventType.LayerEvent, isVideo));
                        }
                        //channel 08 => Find bpm from bpmHeader and add BpmEvent
                        else if (channel == 8)
                        {
                            double bpm = bms.info.bpmHeaders.Find(x => (x != null && x.id == id)).bpm;
                            bms.bmsEvents.Add(new BpmEvent(bpm, measure, measureDiv));
                        }
                        //stop
                        else if(channel == 9)
                        {
                            ulong stopDuration = bms.info.stopHeaders.Find(x => (x != null && x.id == (ulong)id)).duration;
                            bms.bmsEvents.Add(new StopEvent(stopDuration, measure, measureDiv));
                        }
                        //note channel
                        else
                            bms.bmsEvents.Add(new NoteEvent(true, id, measure, measureDiv, channel));
                    }
                    argIndex++;
                }
            }
        } 

        private void CalculatePulse(BMS bms)
        {
            //fill omitted line events
            for(int i = 1; i <= bms.info.maxMeasure; i++)
            {
                if(!measureSetIndex.Contains(i))
                {
                    bms.bmsEvents.Add(new LineEvent(i, 1));
                }
            }

            bms.bmsEvents.Sort();


            ulong[] accumYList = new ulong[bms.info.maxMeasure+10];
            double[] measureLengthList = Enumerable.Repeat((double)1, bms.info.maxMeasure + 1).ToArray();

            for (int i = 0; i < bms.bmsEvents.Count(); i++)
            {
                if (bms.bmsEvents[i].eventType == EventType.LineEvent)
                {
                    measureLengthList[bms.bmsEvents[i].measure] = ((LineEvent)bms.bmsEvents[i]).measureLength;
                }
            }
            
            //fill accumYList
            accumYList[0] = 0;
            for (int i = 1; i <= measureLengthList.Length; i++)
            {
                accumYList[i] = accumYList[i - 1] + (ulong)(measureLengthList[i - 1] * bms.info.resolution);
            }
            
            //calcY bmsEvents and resolution
            foreach (BmsEvent be in bms.bmsEvents)
            {
                be.calcY(bms.info.resolution, measureLengthList[be.measure], accumYList[be.measure]);
            }
            bms.bmsEvents.Sort();

            ProcessLn(bms);
        }

        private void ProcessLn(BMS bms)
        {
            int eventCount = bms.bmsEvents.Count();
            for (int i = 0; i < eventCount; i++)
            {
                if (bms.bmsEvents[i].eventType == EventType.NoteEvent)
                {
                    NoteEvent ne = (NoteEvent)bms.bmsEvents[i];

                    //green note
                    if ((ne.channel >= 181 && ne.channel <= 189) || (ne.channel >= 217 && ne.channel <= 225))
                    {
                        int j = i + 1;
                        //find end note
                        while (j < eventCount - 1 && (bms.bmsEvents[j].eventType != EventType.NoteEvent ||
                            ((NoteEvent)bms.bmsEvents[j]).x != ne.x))
                        {
                            j++;
                        }

                        if (j < eventCount)
                        {
                            ne.l = bms.bmsEvents[j].y - ne.y;
                            bms.bmsEvents.RemoveAt(j);
                            eventCount--;
                        }
                    }
                    //lnobj
                    else if ((int)ne.id == bms.info.lnObj)
                    {
                        int j = i - 1;

                        while (j >= 0 && (bms.bmsEvents[j].eventType != EventType.NoteEvent ||
                            ((NoteEvent)bms.bmsEvents[j]).x != ne.x))
                        {
                            j--;
                        }
                        if (j != -1)
                        {
                            ((NoteEvent)bms.bmsEvents[j]).l = ne.y - bms.bmsEvents[j].y;
                            bms.bmsEvents.RemoveAt(i);
                            eventCount--;
                            i--;
                        }
                    }
                }
            }
        }

        private void SetSubtitle(BMSInfo bmsInfo)
        {

            if (!string.IsNullOrEmpty(bmsInfo.subtitle) || string.IsNullOrEmpty(bmsInfo.title))
                return;

            char openBraket;

            if((openBraket = GetOpenBracket(bmsInfo.title[bmsInfo.title.Length-1])) != '0')
            {
                int openBraketPos = bmsInfo.title.Substring(0, bmsInfo.title.Length - 1).LastIndexOf(openBraket);
                if(openBraketPos != -1)
                {
                    bmsInfo.subtitle = bmsInfo.title.Substring(openBraketPos);
                    bmsInfo.title = bmsInfo.title.Substring(0, openBraketPos);
                }
            }
        }

        private char GetOpenBracket(char input)
        {
            if (input == ']')
                return '[';
            else if (input == ')')
                return '(';
            else if (input == '}')
                return '{';
            else if (input == '~' || input == '-')
                return input;

            return '0';
        }

        
        
        

        private void FillRealTime(BMS bms)
        {
            double currTime = 0;
            ulong currPulse = 0;
            ulong deltaPulse = 0;
            double currBpm = bms.info.init_bpm;
            ulong resolution = bms.info.resolution;
            double pulseConst = 4 * 60 / (currBpm * resolution);
            BmsEvent currEvent;

            for (int i = 0; i < bms.bmsEvents.Count; i++)
            {
                currEvent = bms.bmsEvents[i];
                deltaPulse = currEvent.y - currPulse;
                currPulse = currEvent.y;

                currTime += pulseConst * deltaPulse;
                currEvent.time = currTime;

                if(currEvent.eventType == EventType.BpmEvent)
                {
                    currBpm = ((BpmEvent)currEvent).bpm;
                    pulseConst = 4 * 60 / (currBpm * resolution);
                }
                else if(currEvent.eventType == EventType.StopEvent)
                {
                    ((StopEvent)currEvent).durationTime = pulseConst * ((StopEvent)currEvent).duration;
                }
            }
        }
        
        public static bool IsVideo(string name)
        {
            string extension = Path.GetExtension(name);
            if (extension == ".bmp" || extension == ".png" || extension == ".jpg")
                return false;

            return (extension == ".mpg" || extension == ".avi" || extension == ".mpeg" || extension == ".mp4");
        }

    }
}

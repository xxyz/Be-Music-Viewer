using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BMSParser_new
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

            CalculatePulse(bms);

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
                    int id = HexToInt(args[0].Substring(4, 2));
                    bms.info.soundHeaders.Add( new SoundHeader(id, args[1]) );
                }
                else if (args[0].StartsWith("#BMP"))
                {
                    int id = HexToInt(args[0].Substring(4, 2));
                    bms.bga.bga_header.Add(new BGAHeader(id, args[1]));
                }
                else if (args[0] == "#BPM") //init bpm
                {
                    bms.info.init_bpm = Convert.ToDouble(args[1]);
                }
                else if (args[0].StartsWith("#BPM")) //bpm changing events, add to bpmHeader
                {
                    int id = HexToInt(args[0].Substring(4, 2));
                    bms.info.bpmHeaders[id] =  new BpmHeader(id, Convert.ToDouble(args[1]));
                }
                else if (args[0].StartsWith("#STOP")) //Stop events
                {
                    int id = HexToInt(args[0].Substring(5, 2));
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
                    bms.info.level = Convert.ToUInt64(args[1]);
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
                    bms.info.lnObj = HexToInt(args[1]);
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
                int channel = HexToInt(args[0].Substring(4, 2));

                if (bms.info.maxMeasure < measure)
                    bms.info.maxMeasure = measure;

                //measure length channel
                if(channel == 2)
                {
                    bms.bmsEvents.Add(new LineEvent(measure, Convert.ToDouble(args[1])));
                    measureSetIndex.Add(measure);
                    return;
                }

                IEnumerable<string> notes = Split(args[1], 2);
                IEnumerator<string> noteEnum = notes.GetEnumerator();
                int argsLength = notes.Count<string>();
                int argIndex = 0;
                double measureDiv;

                while(noteEnum.MoveNext())
                {
                    if(noteEnum.Current!= "00")
                    {
                        measureDiv = (double)argIndex / argsLength;
                        int id = HexToInt(noteEnum.Current);

                        if (channel == 3)
                        {
                            //channel 03 use 00-FF hex
                            int bpm = Convert.ToInt32(noteEnum.Current, 16);
                            bms.bmsEvents.Add(new BpmEvent(bpm, measure, measureDiv));
                        }
                        //bga base
                        else if(channel == 4)
                        {
                            bms.bmsEvents.Add(new BGAEvent(id, measure, measureDiv, EventType.BGAEvent));
                        }
                        //bga poor
                        else if (channel == 6)
                        {
                            bms.bmsEvents.Add(new BGAEvent(id, measure, measureDiv, EventType.PoorEvent));
                        }
                        //bga layer
                        else if(channel == 7)
                        {
                            bms.bmsEvents.Add(new BGAEvent(id, measure, measureDiv, EventType.LayerEvent));
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
                        {
                            int bmsOnChannel = getBmsOnX(channel);
                            bms.bmsEvents.Add(new NoteEvent(bmsOnChannel, 0, true, id, measure, measureDiv));
                        }
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
        }

        private int getBmsOnX(int channel)
        {
            //1p scratch
            if (channel == 42)
                return 8;
            //1p key 1~5
            else if (channel >= 37 && channel <= 41)
                return channel - 36;
            //1p key 6~7
            else if (channel >= 44 && channel <= 45)
                return channel - 38;
            //2p key 1~5
            else if (channel >= 73 && channel <= 77)
                return channel - 64;
            else if (channel >= 80 && channel <= 82)
                return channel - 66;
            else if (channel == 78)
                return 16;
            return 0;
        }
        
        
        //split string by chunkSize
        public static IEnumerable<string> Split(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
        //Hexadecimal to Int
        public static int HexToInt(string hex)
        {
            String sample = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            int result = 0;
            for (int i = 0; i < hex.Length; i++)
            {
                result *= 36;
                for (int j = 0; j < sample.Length; j++)
                {
                    if (hex[i] == sample[j])
                    {
                        result += j;
                    }
                }
            }

            return result;
        }
    }
}

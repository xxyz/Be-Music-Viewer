using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using I18N;
using I18N.CJK;

namespace BMSParser_new
{
    class BMSParser
    {
        private int maxMeasure = 0;

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
                //creating weird encdoing error only in Unity...
                if(/*cdet.Charset != null*/ false)
                {
                    Console.WriteLine("Charset: {0}, confidence: {1}", cdet.Charset, cdet.Confidence);
                    encoding = Encoding.GetEncoding(cdet.Charset);
                }
                else
                {
                    Console.WriteLine("Detection Failed");
                }
            }

            using (StreamReader sr = new StreamReader(path, encoding))
            {
                bms.path = path;
                
                //Addnon-sound channel
                bms.sound_channels.Add(new SoundChannel(-1, ""));

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
                    bms.sound_channels.Add( new SoundChannel(id, args[1]) );
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
                else if (args[0].StartsWith("#STOP")) //Stop events, pulse should be filled after processing lines
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

                int measure = Convert.ToInt32(args[0].Substring(1, 3));
                int channel = HexToInt(args[0].Substring(4, 2));

                if (maxMeasure < measure)
                    bms.info.maxMeasure = measure;

                //measure length channel
                if(channel == 2)
                {
                    bms.lines.Add( new BarLine(measure, Convert.ToDouble(args[1])) );
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
                            bms.bpm_events.Add(new BpmEvent(bpm, measure, measureDiv));
                        }
                        //bga base
                        else if(channel == 4)
                        {
                            bms.bga.bga_events.Add(new BGAEvent(id, measure, measureDiv));
                        }
                        //bga poor
                        else if (channel == 6)
                        {
                            bms.bga.poor_events.Add(new BGAEvent(id, measure, measureDiv));
                        }
                        //bga layer
                        else if(channel == 7)
                        {
                            bms.bga.layer_events.Add(new BGAEvent(id, measure, measureDiv));
                        }
                        //channel 08 => Find bpm from bpmHeader and add BpmEvent
                        else if (channel == 8)
                        {
                            double bpm = bms.info.bpmHeaders[id].bpm;
                            bms.bpm_events.Add(new BpmEvent(bpm, measure, measureDiv));
                        }
                        //stop
                        else if(channel == 9)
                        {
                            ulong stopDuration = bms.info.stopHeaders[id].duration;
                            bms.stop_events.Add(new StopEvent(stopDuration, measure, measureDiv));
                        }
                        //note channel
                        else
                        {
                            SoundChannel sc = bms.sound_channels.Find(x => x.id == id);
                            
                            //non-sound note channel
                            if (sc == null)
                                sc = bms.sound_channels.Find(x => x.id == -1);

                            int bmsOnChannel = getBmsOnX(channel);
                            sc.notes.Add(new Note(bmsOnChannel, measure, measureDiv));
                        }
                    }
                    argIndex++;
                }
            }
        }

        private void CalculatePulse(BMS bms)
        {
            ulong accumPulse = 0;
            for (int i = 0; i <= bms.info.maxMeasure; i++)
            {
                BarLine barLine = bms.lines.Find(x => x.measureNum == i);
                if (barLine == null)
                {
                    barLine = new BarLine(i, 1);
                    bms.lines.Add(barLine);
                }

                barLine.accumPulse = accumPulse;
                barLine.y = (ulong)(bms.info.resolution * barLine.measureLength + accumPulse);


                accumPulse += (ulong)(bms.info.resolution * barLine.measureLength);
            }
            bms.lines = bms.lines.OrderBy(o => o.y).ToList();

            foreach(SoundChannel sc in bms.sound_channels)
            {
                foreach(Note note in sc.notes)
                {
                    note.y = (ulong)(bms.info.resolution * note.measureDiv + bms.lines[note.measureNum].accumPulse);
                }
                sc.notes = sc.notes.OrderBy(o => o.y).ToList();
            }
            foreach(BpmEvent be in bms.bpm_events)
            {
                be.y = (ulong)(bms.info.resolution * be.measureDiv + bms.lines[be.measureNum].accumPulse);
            }

            foreach(StopEvent se in bms.stop_events)
            {
                se.y = (ulong)(bms.info.resolution * se.measureDiv + bms.lines[se.measureNum].accumPulse);
            }

            foreach(BGAEvent be in bms.bga.bga_events)
            {
                be.y = (ulong)(bms.info.resolution * be.measureDiv + bms.lines[be.measureNum].accumPulse);
            }
            bms.bga.bga_events = bms.bga.bga_events.OrderBy(o => o.y).ToList();

            foreach (BGAEvent be in bms.bga.poor_events)
            {
                be.y = (ulong)(bms.info.resolution * be.measureDiv + bms.lines[be.measureNum].accumPulse);
            }
            bms.bga.poor_events = bms.bga.poor_events.OrderBy(o => o.y).ToList();

            foreach (BGAEvent be in bms.bga.layer_events)
            {
                be.y = (ulong)(bms.info.resolution * be.measureDiv + bms.lines[be.measureNum].accumPulse);
            }
            bms.bga.layer_events = bms.bga.layer_events.OrderBy(o => o.y).ToList();
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
        public static int HexToInt(String hex)
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

using System.Collections.Generic;

namespace BMSParser
{
    //BMS header information
    public class BMSInfo
    {
        public string title;
        public string subtitle = "";
        public string artist;
        public List<string> subartists = new List<string>();
        public string genre;
        public string mode_hint = "beat-7k";//mode_hint's possible values: beat-5k, beat-7k, beat-10k, beatk14k, popn-5k, popn-9k 
        public string chart_name;
        public ulong level;
        public double init_bpm;
        public double judge_rank = 100;
        public double total = 160; // 160 is LR2's default total value.
        public string back_image; // A static background image's filename
        public string eyecatch_image; //Loading Picture filename
        public string banner_image; // filename
        public string preview_music; // preview music filename
        public ulong resolution = 192; //bmson's default is 240...

        //below field is not used in BMSON
        public int volwav = 100;
        public int difficulty;
        public string back_bmp; //#backbmp
        public string comment;

        public List<BpmHeader> bpmHeaders = new List<BpmHeader>(new BpmHeader[1322]);
        public List<StopHeader> stopHeaders = new List<StopHeader>(new StopHeader[1322]);
        public List<SoundHeader> soundHeaders = new List<SoundHeader>();
        


        public int lnObj = -1;

        public int maxMeasure;
    }
}
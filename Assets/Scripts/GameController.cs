using UnityEngine;
using System.Collections;
using BMSParser_new;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;
using System.Drawing;
using UnityEngine.Audio;

public class GameController : MonoBehaviour {

    public string BmsPath;
    public UnityEngine.Font[] judgeFonts;
    public Sprite[] judgeSprites;
    public GameObject soundchannelpre;

    public GameObject noteScratch;
    public GameObject noteWhite;
    public GameObject noteBlue;
    public GameObject noteWhiteLn;
    public GameObject noteBlueLn;
    public GameObject noteScratchLn;

    public GameObject linePre;
    public GameObject slider;

    public GameObject bga;
    public GameObject bgaLayer;
    public GameObject bgaVideo;
    public GameObject bgaLayerVideo;

    private GameObject judge;
    public AudioMixerGroup keyMixer;
    public AudioMixerGroup backMixer;
    public ulong pulseOffset = 0;
    public int highSpeed = 100;


    private BMS bms;

    private SpriteRenderer bgImage;
    Sprite[] bgaSprites;

    private SpriteRenderer layerImage;
    Sprite[] layerSprites;

    private GameObject[] soundObjects;

    private int eventCounter = 0;
    private ulong pulse100000 = 0;
    private ulong pulse = 0;
    private double bmsTime = 0;
    private uint measure = 0;
    private ulong lastPulse;
    private int combo = 0;

    private const float highSpeedConstant = 0.01f;
    private double pulseConstant;
    private int eventLength;
    private float worldScreenHeight;
    private float worldScreenWidth;

    private Text titleText, subtitleText, artistText, bpmText, bgaText, soundText, 
        layerText, pulseText, genreText, timeText, measureText, totalText, resolutionText, comboText;
    private UnityEngine.UI.Image judgeImage;

    private AudioSource[] audioSources;

    private GstUnityBridgeTexture gstBga;
    private GstUnityBridgeTexture gstLayer;

    void Awake ()
    {
        //set camera variables
        worldScreenHeight = Camera.main.orthographicSize * 2;
        worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        if(LoadBMS.path != "" && LoadBMS.path != null)
            BmsPath = LoadBMS.path;

        //parse BMS
        BMSParser bmsParser = new BMSParser();
        bms = bmsParser.Parse(BmsPath);

        if(bms == null)
        {
            Debug.Log("Parsing Failed");
        }

        //get components
        bgImage = bga.GetComponent<SpriteRenderer>();
        layerImage = bgaLayer.GetComponent<SpriteRenderer>();
        gstBga = bgaVideo.GetComponent<GstUnityBridgeTexture>();
        gstLayer = bgaLayerVideo.GetComponent<GstUnityBridgeTexture>();

        //set variables
        ulong maxSoundObjectId = 0;
        ulong maxBgaHeaderId = 0;

        foreach (BmsEvent be in bms.bmsEvents)
        {
            if (be.eventType == BMSParser_new.EventType.NoteEvent && ((NoteEvent)be).id > maxSoundObjectId)
                maxSoundObjectId = ((NoteEvent)be).id;
        }
        foreach (BGAHeader bh in bms.bga.bga_header)
        {
            if (bh.id > maxBgaHeaderId)
            {
                maxBgaHeaderId = bh.id;
            }
        }

        bgaSprites = new Sprite[maxBgaHeaderId+10];
        layerSprites = new Sprite[maxBgaHeaderId+10];
        eventLength = bms.bmsEvents.Count;
        
        pulseConstant = bms.info.init_bpm * bms.info.resolution / (60 * 4);
        lastPulse = bms.bmsEvents[bms.bmsEvents.Count - 1].y;

        judge = GameObject.Find("Judge");
        judgeImage = GameObject.Find("JudgeImage").GetComponent<UnityEngine.UI.Image>();
        titleText = GameObject.Find("TitleText").GetComponent<Text>();
        subtitleText = GameObject.Find("SubtitleText").GetComponent<Text>();
        artistText = GameObject.Find("ArtistText").GetComponent<Text>();
        bpmText = GameObject.Find("BpmText").GetComponent<Text>();
        bgaText = GameObject.Find("BgaText").GetComponent<Text>();
        layerText = GameObject.Find("LayerText").GetComponent<Text>();
        pulseText = GameObject.Find("PulseText").GetComponent<Text>();
        genreText = GameObject.Find("GenreText").GetComponent<Text>();
        timeText = GameObject.Find("TimeText").GetComponent<Text>();
        measureText = GameObject.Find("MeasureText").GetComponent<Text>();
        totalText = GameObject.Find("TotalText").GetComponent<Text>();
        resolutionText = GameObject.Find("ResolutionText").GetComponent<Text>();
        soundText = GameObject.Find("SoundCountText").GetComponent<Text>();
        comboText = GameObject.Find("ComboText").GetComponent<Text>();


        soundObjects = new GameObject[maxSoundObjectId+10];

        //set Text
        titleText.text = "Title: " + bms.info.title;
        subtitleText.text = "Subtitle: " + bms.info.subtitle;
        artistText.text = "Artist: " + bms.info.artist;
        bpmText.text = "Bpm: " + bms.info.init_bpm;
        genreText.text = "Genre: " + bms.info.genre;
        totalText.text = "Total: " + bms.info.total;
        measureText.text = "Measure: 0/" + bms.info.maxMeasure;
        resolutionText.text = "Resolution: " + bms.info.resolution;
        

        LoadBga();

        LoadSound(bms.info.soundHeaders);
        NotePlacement();

        
        Component[] sources = FindObjectsOfType(typeof(AudioSource)) as Component[];
        audioSources = new AudioSource[sources.Length];
        sources.CopyTo(audioSources, 0);


        //Check how many sounds are playing..
        InvokeRepeating("CurrentlyPlaying", 1f, 0.5f);
    }
    public void CurrentlyPlaying()
    {
        int currentlyPlaying = 0;
        foreach (AudioSource source in audioSources)
        {
            if (source.isPlaying)
                currentlyPlaying++;
        }

        soundText.text = "Sound: " + currentlyPlaying + "/256";
    }


    // Update is called once per frame
    void Update () {

        if (eventCounter >= eventLength)
            return;

        if(bms.bmsEvents[eventCounter].y < pulse)
        {
            while(eventCounter < eventLength && bms.bmsEvents[eventCounter].y < pulse)
            {
                    ExecuteBmsEvent(bms.bmsEvents[eventCounter]);
                eventCounter++;
            }
        }

        bmsTime += Time.deltaTime;
        TimeSpan timeSpan = TimeSpan.FromSeconds(bmsTime);
        ulong deltaPulse100000 = (ulong)(Time.deltaTime * 131072 * pulseConstant);
        pulse100000 += deltaPulse100000;
        pulse = pulse100000 / 131072;
        pulseText.text = "Pulse: " + pulse + "/" + lastPulse;
        timeText.text = "Time: " + string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);

        slider.transform.position = new Vector3(-2.33f, -2 - (float)(pulse * (highSpeed / pulseConstant)) * highSpeedConstant);
    }

    
    void NotePlacement()
    {
        foreach(BmsEvent be in bms.bmsEvents)
        {
            if(be.eventType == BMSParser_new.EventType.NoteEvent)
            {
                NoteEvent ne = (NoteEvent)be;
                GameObject drawPrefab = noteWhite;
                GameObject drawLnPrefab = noteWhiteLn;
                float xPos = 0.16f;
                bool drawFlag = true;
                if(ne.x >= 1 && ne.x <= 7)
                {
                    xPos *= ne.x;
                    if ((ne.x % 2) == 1)
                        drawPrefab = noteWhite;
                    else
                    {
                        drawPrefab = noteBlue;
                        drawLnPrefab = noteBlueLn;
                    }
                        
                }
                else if(ne.x == 8)
                {
                    drawPrefab = noteScratch;
                    drawLnPrefab = noteScratchLn;
                    xPos = 0f;
                }
                else
                {
                    drawFlag = false;
                }
                if (drawFlag)
                {
                    GameObject note = Instantiate(drawPrefab) as GameObject;
                    note.transform.SetParent(slider.transform);
                    note.transform.position = new Vector3(xPos-2.33f, (float)(ne.y * (highSpeed / pulseConstant)) * highSpeedConstant, 0);
                    //ln
                    if(ne.l > 0)
                    {
                        GameObject lnNote = Instantiate(drawLnPrefab) as GameObject;
                        lnNote.transform.SetParent(slider.transform);
                        lnNote.transform.position = new Vector3(xPos - 2.33f, (float)(ne.y * (highSpeed / pulseConstant)) * highSpeedConstant, 0);
                        lnNote.transform.localScale = new Vector3(1, (worldScreenHeight / 200 * 10000) * ne.l * (float)((highSpeed / pulseConstant)) * highSpeedConstant, 1);
                    }
                }
            }
            else if(be.eventType == BMSParser_new.EventType.LineEvent)
            {
                GameObject line = Instantiate(linePre) as GameObject;
                line.transform.SetParent(slider.transform);
                line.transform.position = new Vector3(-2.33f, (float)(be.y * (highSpeed / pulseConstant)) * highSpeedConstant, 0);
            }
        }
    }

    void ExecuteBmsEvent(BmsEvent be)
    {
        if(be == null)
        {
            Debug.Log("Null BmsEvent");
            return;
        }

        if (be.eventType == BMSParser_new.EventType.BGAEvent ||
            be.eventType == BMSParser_new.EventType.LayerEvent ||
            be.eventType == BMSParser_new.EventType.PoorEvent)
            ExecuteBgaEvent((BGAEvent)be);
        else if (be.eventType == BMSParser_new.EventType.BpmEvent)
            ExecuteBpmEvent((BpmEvent)be);
        else if (be.eventType == BMSParser_new.EventType.LineEvent)
            ExecuteLineEvent((LineEvent)be);
        else if (be.eventType == BMSParser_new.EventType.NoteEvent)
            ExecuteNoteEvent((NoteEvent)be);
        else if (be.eventType == BMSParser_new.EventType.StopEvent)
            ExecuteStopEvent((StopEvent)be);

    }

    void ExecuteBgaEvent(BGAEvent bgE)
    {
        //sprite Missing
        if (bgaSprites.Length <= (int)bgE.id || (!bgE.isVideo && bgaSprites[bgE.id] == null))
        {
            Debug.Log(bgE.isVideo);
            Debug.Log("Sprite Missing! Id(" + bgE.id + ")");
            if (bgE.eventType == BMSParser_new.EventType.BGAEvent)
                bgImage.sprite = null;
            else
                layerImage.sprite = null;
            return;
        }

        //change bga sprite
        if (bgE.eventType == BMSParser_new.EventType.BGAEvent)
        {
            if (bgE.isVideo)
            {
                bgImage.sprite = null;
                string fileName = bms.bga.bga_header.Find(x => x.id == bgE.id).name;
                gstBga.m_URI = "file:///"+ Uri.EscapeUriString((bms.path + "/" + fileName).Replace("\\", "/"));
                gstBga.enabled = true;
                bgaText.text = "BGA: " + fileName;
                return;
            }
            

            bgImage.sprite = bgaSprites[bgE.id];
            bgaText.text = "BGA: " + bgaSprites[bgE.id].name;

            

            if (bgImage.sprite == null)
            {
                Debug.Log("Sprite Missing: " + bgaSprites[bgE.id].name);
            }
            else
            {
                gstBga.enabled = false;
                //TODO scale aspect ratio option
                bga.transform.localScale = new Vector3(
                worldScreenHeight / bgImage.sprite.bounds.size.x,
                worldScreenHeight / bgImage.sprite.bounds.size.y, 1);
            }
        }
        else if (bgE.eventType == BMSParser_new.EventType.LayerEvent)
        {
            if (bgE.isVideo)
            {
                layerImage.sprite = null;
                string fileName = bms.bga.bga_header.Find(x => x.id == bgE.id).name;
                gstLayer.m_URI = "file:///" + (bms.path + "/" + fileName).Replace("\\", "/");
                gstLayer.enabled = true;
                layerText.text = "Layer: " + fileName;
                return;
            }
            layerImage.sprite = layerSprites[bgE.id];
            layerText.text = "Layer: " + layerSprites[bgE.id].name;
            if (layerImage.sprite == null)
            {
                Debug.Log("Sprite Missing: " + layerSprites[bgE.id].name);
            }
            else
            {
                bgaLayer.transform.localScale = new Vector3(
                worldScreenHeight / layerImage.sprite.bounds.size.x,
                worldScreenHeight / layerImage.sprite.bounds.size.y, 1);

            }
        }
    }

    void ExecuteBpmEvent(BpmEvent be)
    {
        pulseConstant = be.bpm * bms.info.resolution / (60 * 4);
        bpmText.text = "Bpm: " + be.bpm;
    }

    void ExecuteLineEvent(LineEvent be)
    {
        measure++;
        measureText.text = "Measure: " + measure + "/" + bms.info.maxMeasure;
    }

    void ExecuteNoteEvent(NoteEvent be)
    {
        try
        {
            AudioSource audioSource = soundObjects[be.id].GetComponent<AudioSource>();
            AudioMixerGroup targetMixer = backMixer;

            if (audioSource.clip.loadState == AudioDataLoadState.Loaded)
            {
                if(be.x != 0)
                {
                    targetMixer = keyMixer;
                    combo++;
                    StopCoroutine("ShowCombo");
                    StartCoroutine("ShowCombo", be.y);
                }
                audioSource.outputAudioMixerGroup = targetMixer;
                audioSource.Play();
            }
            else
                Debug.Log("AudioSource" + audioSource.clip.name + "Play Failed");
        }
        catch
        {
            if(bms.info.soundHeaders.Count <= (int)be.id || bms.info.soundHeaders[(int)be.id] == null)
                Debug.Log("Audio File id(" + be.id + ") missing");
            else
                Debug.Log("Audio File '" + bms.info.soundHeaders[(int)be.id].name + "' missing");
        }
    }
    void ExecuteStopEvent(StopEvent be)
    {

    }

    IEnumerator ShowCombo(ulong length)
    {

        judge.SetActive(true);
        comboText.text = combo.ToString();
        for (int i = 102; i >= 0; i--)
        {
            judgeImage.sprite = judgeSprites[i % 3];
            comboText.font = judgeFonts[i % 3];
            yield return new WaitForSeconds(0.03f);
            
        }

        judge.SetActive(false);
    }

    //doesn't support bmp
    public static Texture2D LoadImageFromPath(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            if (String.Equals(Path.GetExtension(filePath).ToLower(), ".bmp"))
            {
                Bitmap bitmap = new Bitmap(filePath);
                tex = new Texture2D(bitmap.Width, bitmap.Height);
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        System.Drawing.Color pixel = bitmap.GetPixel(x, y);
                        float r = Normalize(pixel.R, 0f, 255f);
                        float g = Normalize(pixel.G, 0f, 255f);
                        float b = Normalize(pixel.B, 0f, 255f);
                        tex.SetPixel(x, bitmap.Height-y-1, new UnityEngine.Color(r,g,b));
                    }
                }
            }
            else
            {
                fileData = File.ReadAllBytes(filePath);
                tex = new Texture2D(256, 256);
                tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
            }
        }
        return tex;
    }

    public static float Normalize(float current, float min, float max)
    {
        return (current - min) / (max - min);
    }
    
    //old bga used Black as Transparent Color
    public static Texture2D BlackToTransparent(Texture2D tex)
    {
        UnityEngine.Color[] pix = tex.GetPixels(0, 0, tex.width, tex.height); 
        for(int p = 0; p<pix.Length; p++)
        {
            if(pix[p].r == 0 && pix[p].g == 0 && pix[p].b == 0)
            {
                pix[p].a = 0;
            }
        }
        tex.SetPixels(0, 0, tex.width, tex.height, pix);
        tex.Apply();
        return tex;
    }

    void LoadBga()
    {
        Texture2D bgaTexture;
        foreach (BGAHeader bh in bms.bga.bga_header)
        {
            

            string path = bms.path + "\\" + bh.name;


            if (Path.GetExtension(path) == ".mpg")
            {
                return;
            }


            if (!File.Exists(path))
                path = Path.ChangeExtension(path, ".png");
            if(!File.Exists(path))
                path = Path.ChangeExtension(path, ".jpg");
            if (!File.Exists(path))
                path = Path.ChangeExtension(path, ".bmp");

            bgaTexture = LoadImageFromPath(path);
            if(bgaTexture != null)
            {
                bgaSprites[bh.id] = Sprite.Create(bgaTexture, new Rect(0, 0, bgaTexture.width, bgaTexture.height), new Vector2(0, 0));
                bgaSprites[bh.id].name = bh.name;
                layerSprites[bh.id] = Sprite.Create(BlackToTransparent(bgaTexture), new Rect(0, 0, bgaTexture.width, bgaTexture.height), new Vector2(0, 0));
                layerSprites[bh.id].name = bh.name;
            }
        }
    }

    void LoadSound(List<SoundHeader> sound_channels)
    {
        Transform soundTrans = GameObject.Find("Sound").transform;

        foreach (SoundHeader sc in sound_channels)
        {
            if (sc.name != "")
            {
                string path = bms.path + "\\" + sc.name;

                if (!File.Exists(path) && String.Equals(Path.GetExtension(path).ToLower(), ".wav"))
                {
                    path = Path.ChangeExtension(path, ".ogg");
                }

                if (File.Exists(path))
                {
                    soundObjects[sc.id] = Instantiate(soundchannelpre) as GameObject;
                    soundObjects[sc.id].name = sc.name;
                    soundObjects[sc.id].transform.SetParent(soundTrans);

                    WWW www = new WWW("file:///" + path.Replace("#", "%23"));

                    AudioClip clip = www.GetAudioClip(false, false);
                    /*
                    while (!www.isDone)
                    {
                        Debug.Log("not Done "+ sc.name);
                        yield return null;
                    }
                    */
                    clip.name = Path.GetFileName(path);

                    soundObjects[sc.id].GetComponent<AudioSource>().clip = clip;
                }
            }
        }

    }
}

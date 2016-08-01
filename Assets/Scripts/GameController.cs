using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.Audio;
using BMSParser;

public enum LnComboType { LR2, IIDX, ruvit }

public class GameController : MonoBehaviour {

    public LnComboType lnComboType = LnComboType.LR2;

    //Drawing prefabs
    public UnityEngine.Font[] judgeFonts;
    public Sprite[] judgeSprites;
    public GameObject loadingScreen;
    public GameObject soundchannelpre;
    public GameObject noteScratch;
    public GameObject noteWhite;
    public GameObject noteBlue;
    public GameObject noteScartchInvisible;
    public GameObject noteWhiteInvisible;
    public GameObject noteBlueInvisible;
    public GameObject noteWhiteLn;
    public GameObject noteBlueLn;
    public GameObject noteScratchLn;
    public GameObject noteMine;
    public GameObject linePre;
    public GameObject slider;

    public GameObject bga;
    public GameObject bgaLayer;
    public GameObject bgaVideo;
    public GameObject bgaLayerVideo;
    public GameObject graphPanel;

    private GameObject judge;
    public AudioMixerGroup masterMixer;
    public AudioMixerGroup keyMixer;
    public AudioMixerGroup backMixer;
    public int[] randomLane = new int[] { 1, 2, 3, 4, 5, 6, 7 };
    public int highSpeed = 300;

    private BMSLoader bmsLoader;
    private BMS bms;
    private bool isLoaded = false;
    private float timeOffset = 0.0f;

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
    private ulong resolution;

    private double currBpm;
    private float highSpeedConstant = 0.01f;
    private double pulseConstant;
    private int eventLength;
    private float worldScreenHeight;
    private float worldScreenWidth;
    private Text 
        titleText, subtitleText, artistText, bpmText, bgaText, soundText, 
        layerText, pulseText, genreText, timeText, measureText, totalText,
        resolutionText, comboText, loadingStatusText;
    private UnityEngine.UI.Image judgeImage;

    private AudioSource[] audioSources;

    private GstUnityBridgeTexture gstBga;
    private GstUnityBridgeTexture gstLayer;

    IEnumerator Start ()
    {
        bmsLoader = GameObject.Find("BMSLoader").GetComponent<BMSLoader>();
        bmsLoader.loadBMS();
        bms = bmsLoader.bms;
        yield return new WaitForSeconds(0.1f);

        Time.timeScale = 0.0f;
        //set camera variables
        worldScreenHeight = Camera.main.orthographicSize * 2;
        worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        if(bms == null)
        {
            Debug.Log("Parsing Failed");
        }

        currBpm = bms.info.init_bpm;

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
            if (be.eventType == BMSParser.EventType.NoteEvent && ((NoteEvent)be).id > maxSoundObjectId)
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
        resolution = bms.info.resolution;

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
        loadingStatusText = GameObject.Find("LoadingStatusText").GetComponent<Text>();

        soundObjects = new GameObject[1296];

        //set Text
        titleText.text = "Title: " + bms.info.title;
        subtitleText.text = "Subtitle: " + bms.info.subtitle;
        artistText.text = "Artist: " + bms.info.artist;
        bpmText.text = "Bpm: " + bms.info.init_bpm;
        genreText.text = "Genre: " + bms.info.genre;
        totalText.text = "Total: " + bms.info.total;
        measureText.text = "Measure: 0/" + bms.info.maxMeasure;
        resolutionText.text = "Resolution: " + bms.info.resolution;


        loadingStatusText.text = "Loading BGA...";
        yield return null;
        LoadBga();

        loadingStatusText.text = "Loading Sound...";
        yield return null;
        LoadSound(bms.info.soundHeaders);

        loadingStatusText.text = "Drawing Notes...";
        yield return null;
        NotePlacement();

        
        Component[] sources = FindObjectsOfType(typeof(AudioSource)) as Component[];
        audioSources = new AudioSource[sources.Length];
        sources.CopyTo(audioSources, 0);

        //Check how many sounds are playing..
        InvokeRepeating("CurrentlyPlaying", 1f, 0.5f);

        DrawBMSGraph dbg = graphPanel.GetComponent<DrawBMSGraph>();
        dbg.DrawGraph(bms.bmsEvents);
        loadingStatusText.text = "Done";
        yield return null;

        loadingScreen.SetActive(false);
        isLoaded = true;
        timeOffset = Time.time;
        dbg.timeOffset = timeOffset;

        Time.timeScale = 1.0f;
        
        
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

    
    void Update () {

        while (!isLoaded)
            return;

        while (eventCounter < eventLength && bms.bmsEvents[eventCounter].time < Time.time - timeOffset)
        {
            ExecuteBmsEvent(bms.bmsEvents[eventCounter]);
            eventCounter++;
        }
        
        ulong deltaPulse100000 = (ulong)(Time.deltaTime * 131072 * pulseConstant);
        
        slider.transform.Translate(Vector3.down * Time.deltaTime * highSpeed * highSpeedConstant);

        if (eventCounter < eventLength)
        {
            bmsTime += Time.deltaTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(bmsTime);
            pulse100000 += deltaPulse100000;
            pulse = pulse100000 / 131072;
            pulseText.text = "Pulse: " + pulse + "/" + lastPulse;
            timeText.text = "Time: " + string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
        }

        //slider.transform.position = new Vector3(-2.33f, -2 - Time.time * highSpeed * highSpeedConstant);
    }

    
    void NotePlacement()
    {
        foreach(BmsEvent be in bms.bmsEvents)
        {
            if(be.eventType == BMSParser.EventType.NoteEvent)
            {

                NoteEvent ne = (NoteEvent)be;

                GameObject drawPrefab = noteWhite;
                GameObject drawLnPrefab = noteWhiteLn;
                float xPos = 0.16f;
                bool drawFlag = true;

                if (ne.noteEventType == NoteEventType.invisible)
                {
                    drawFlag = false;
                }
                else if (BMSUtil.GetNoteType(ne.x) == NoteType.Key1p)
                {
                    int lane = randomLane[ne.x-1];
                    xPos *= lane;
                    if (ne.noteEventType == NoteEventType.plain)
                    {
                        if ((lane % 2) == 1)
                            drawPrefab = noteWhite;
                        else
                        {
                            drawPrefab = noteBlue;
                            drawLnPrefab = noteBlueLn;
                        }
                    }
                    else if(ne.noteEventType == NoteEventType.mine)
                    {
                        drawPrefab = noteMine;
                    }
                        
                }
                else if(BMSUtil.GetNoteType(ne.x) == NoteType.Scratch1p)
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
                    note.transform.position = new Vector3(xPos-2.33f, (float)(ne.y * (highSpeed / pulseConstant)) * highSpeedConstant - 0.95502f, 0);
                    //ln
                    if(ne.l > 0)
                    {
                        GameObject lnNote = Instantiate(drawLnPrefab) as GameObject;
                        lnNote.transform.SetParent(slider.transform);
                        lnNote.transform.position = new Vector3(xPos - 2.33f, (float)(ne.y * (highSpeed / pulseConstant)) * highSpeedConstant - 0.95502f, 0);
                        lnNote.transform.localScale = new Vector3(1, (worldScreenHeight / 200 * 10000) * ne.l * (float)((highSpeed / pulseConstant)) * highSpeedConstant, 1);
                    }
                }
            }
            else if(be.eventType == BMSParser.EventType.LineEvent)
            {
                GameObject line = Instantiate(linePre) as GameObject;
                line.transform.SetParent(slider.transform);
                line.transform.position = new Vector3(-2.33f, (float)(be.y * (highSpeed / pulseConstant)) * highSpeedConstant - 0.95502f, 0);
            }
        }
    }

    void ExecuteBmsEvent(BmsEvent be)
    {
        if(be == null)
            return;

        if (be.eventType == BMSParser.EventType.BGAEvent ||
            be.eventType == BMSParser.EventType.LayerEvent ||
            be.eventType == BMSParser.EventType.PoorEvent)
            ExecuteBgaEvent((BGAEvent)be);
        else if (be.eventType == BMSParser.EventType.BpmEvent)
            ExecuteBpmEvent((BpmEvent)be);
        else if (be.eventType == BMSParser.EventType.LineEvent)
            ExecuteLineEvent((LineEvent)be);
        else if (be.eventType == BMSParser.EventType.NoteEvent)
            ExecuteNoteEvent((NoteEvent)be);
        else if (be.eventType == BMSParser.EventType.StopEvent)
            ExecuteStopEvent((StopEvent)be);

    }

    void ExecuteBgaEvent(BGAEvent bgE)
    {
        //sprite Missing
        if (bgaSprites.Length <= (int)bgE.id || (!bgE.isVideo && bgaSprites[bgE.id] == null))
        {
            Debug.Log("Sprite Missing! Id(" + bgE.id + ")");
            if (bgE.eventType == BMSParser.EventType.BGAEvent)
                bgImage.sprite = null;
            else
                layerImage.sprite = null;
            return;
        }

        //change bga sprite
        if (bgE.eventType == BMSParser.EventType.BGAEvent)
        {
            if (bgE.isVideo)
            {
                bgImage.sprite = null;
                string fileName = bms.bga.bga_header.Find(x => x.id == bgE.id).name;
                //gstBga.m_URI = "file:///"+ Uri.EscapeUriString((bms.path + "/" + fileName).Replace("\\", "/"));
                //gstBga.enabled = true;
                bgaText.text = "BGA: " + fileName;
                gstBga.Play();
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
        else if (bgE.eventType == BMSParser.EventType.LayerEvent)
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
        highSpeedConstant *= (float)(be.bpm / currBpm);
        currBpm = be.bpm;
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
        //invisible notes & landmine notes
        if (be.noteEventType == NoteEventType.mine || be.noteEventType == NoteEventType.invisible)
            return;

        

        //none sound object
        if (soundObjects[be.id] == null)
        {
            /*
            if (bms.info.soundHeaders.Count <= (int)be.id || bms.info.soundHeaders[(int)be.id] == null)
                Debug.Log("Audio File id(" + be.id + ") missing");
            else
                Debug.Log("Audio File '" + bms.info.soundHeaders[(int)be.id].name + "' missing");
            */
            StartCoroutine("AddCombo", be.l);
        }
        else
        {
            AudioSource audioSource = soundObjects[be.id].GetComponent<AudioSource>();
            AudioMixerGroup targetMixer = backMixer;
            if (audioSource.clip.loadState == AudioDataLoadState.Loaded)
            {
                if (be.x != 0)
                {
                    targetMixer = keyMixer;
                    
                }
                audioSource.outputAudioMixerGroup = targetMixer;
                audioSource.Play();
            }
            else
                Debug.Log("AudioSource" + audioSource.clip.name + "Play Failed");

            
        }

        if (be.x != 0)
            StartCoroutine(AddCombo(be.l));


        /*}
        catch
        {
            if(bms.info.soundHeaders.Count <= (int)be.id || bms.info.soundHeaders[(int)be.id] == null)
                Debug.Log("Audio File id(" + be.id + ") missing");
            else
                Debug.Log("Audio File '" + bms.info.soundHeaders[(int)be.id].name + "' missing");
        }*/
    }
    void ExecuteStopEvent(StopEvent be)
    {
        StartCoroutine(StopTime(be.durationTime));
    }

    IEnumerator StopTime(double time)
    {
        Time.timeScale = 0.0f;
        float pauseEndTime = Time.realtimeSinceStartup + (float)time;
        while (Time.realtimeSinceStartup < pauseEndTime)
        {
            yield return 0;
        }

        Time.timeScale = 1.0f;
    }

    IEnumerator AddCombo(ulong length)
    {
        if (length == 0)
        {
            combo++;
            StopCoroutine("ShowCombo");
            StartCoroutine("ShowCombo");
        }
        //combo add when ln ends
        else if(lnComboType == LnComboType.LR2)
        {
            yield return new WaitForSeconds((float)(length / pulseConstant));
            combo++;
            StopCoroutine("ShowCombo");
            StartCoroutine("ShowCombo");
        }
        //combo add when ln starts and ends
        else if(lnComboType == LnComboType.IIDX)
        {
            combo++;
            StopCoroutine("ShowCombo");
            StartCoroutine("ShowCombo");

            yield return new WaitForSeconds((float)(length / pulseConstant));
            combo++;
            StopCoroutine("ShowCombo");
            StartCoroutine("ShowCombo");
        }
        //ln tic combo
        //TODO: If frame rate is too low, combo adds too slowly.
        else if(lnComboType == LnComboType.ruvit)
        {
            Debug.Log((float)(resolution / (16 * pulseConstant)));
            length -= resolution / 16;

            while (length > 0)
            {
                combo++;
                StopCoroutine("ShowCombo");
                StartCoroutine("ShowCombo");
                length -= resolution / 16;
                yield return new WaitForSeconds((float)(resolution / (16 * pulseConstant)));
            }
        }
        yield return null;
    }

    IEnumerator ShowCombo()
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
    
    

    void LoadBga()
    {
        Texture2D bgaTexture;
        foreach (BGAHeader bh in bms.bga.bga_header)
        {
            string path = bms.path + "\\" + bh.name;

            string extension = Path.GetExtension(path);
            if (extension == ".mpg" || extension == ".mpeg" || extension == ".avi")
            {
                string fileName = path;
                gstBga.m_URI = "file:///" + Uri.EscapeUriString((path).Replace("\\", "/"));
                gstBga.enabled = true;
                return;
            }

            if (!File.Exists(path))
                path = Path.ChangeExtension(path, ".png");
            if(!File.Exists(path))
                path = Path.ChangeExtension(path, ".jpg");
            if (!File.Exists(path))
                path = Path.ChangeExtension(path, ".bmp");

            bgaTexture = Util.LoadImageFromPath(path);
            if(bgaTexture != null)
            {
                bgaSprites[bh.id] = Sprite.Create(bgaTexture, new Rect(0, 0, bgaTexture.width, bgaTexture.height), new Vector2(0, 0));
                bgaSprites[bh.id].name = bh.name;
                layerSprites[bh.id] = Sprite.Create(Util.BlackToTransparent(bgaTexture), new Rect(0, 0, bgaTexture.width, bgaTexture.height), new Vector2(0, 0));
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
                    WWWSoundLoad(path, sc, soundTrans);
                }
            }
        }
    }

    void WWWSoundLoad(string path, SoundHeader sc, Transform soundTrans)
    {
        soundObjects[sc.id] = Instantiate(soundchannelpre) as GameObject;
        soundObjects[sc.id].name = sc.name;
        soundObjects[sc.id].transform.SetParent(soundTrans);

        WWW www = new WWW("file:///" + path.Replace("#", "%23"));

        AudioClip clip = www.GetAudioClip(false, false);
        while (!www.isDone)
        {
           
        }
        clip.name = Path.GetFileName(path);

        soundObjects[sc.id].GetComponent<AudioSource>().clip = clip;

        string status = clip.loadState.ToString();
    }
}

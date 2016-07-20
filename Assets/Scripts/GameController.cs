using UnityEngine;
using System.Collections;
using BMSParser_new;
using UnityEngine.UI;
using System.IO;
using System.Collections.Generic;
using System;

public class GameController : MonoBehaviour {

    public string BmsPath = "D:/BMS/BMS/[しらいし]Moon-gate/_Moon-gate_1n.bms";
    public GameObject soundchannelpre;
    public ulong pulseOffset;

    private BMS bms;

    private GameObject bga;
    private SpriteRenderer bgImage;
    private int bgIndex = 0;
    Sprite[] bgaSprites;

    private GameObject bgaLayer;
    private SpriteRenderer layerImage;
    private int layerIndex = 0;
    Sprite[] layerSprites;

    private int bpmIndex = 0;

    private ulong pulse100000 = 0;
    private ulong pulse = 0;

    private double pulseConstant;
    private int eventLength;
    private float worldScreenHeight;
    private float worldScreenWidth;
    private NotePlayer[] notePlayers;

    private Text titleText, artistText, bpmText, bgaText, layerText, pulseText, genreText;
    

    void Start ()
    {
        //set camera variables
        worldScreenHeight = Camera.main.orthographicSize * 2;
        worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        //parse BMS
        BMSParser bmsParser = new BMSParser();
        bms = bmsParser.Parse(BmsPath);

        //get components
        bga = GameObject.Find("BGA Back");
        Debug.Log(bga);
        bgImage = bga.GetComponent<SpriteRenderer>();
        Debug.Log(bgImage);
        bgaLayer = GameObject.Find("BGA Layer");
        layerImage = bgaLayer.GetComponent<SpriteRenderer>();

        //set variables
        
        bgaSprites = new Sprite[bms.bga.bga_header[bms.bga.bga_header.Count - 1].id + 100];
        layerSprites = new Sprite[bms.bga.bga_header[bms.bga.bga_header.Count - 1].id + 100];
        eventLength = bms.bga.bga_events.Count;
        
        pulseConstant = bms.info.init_bpm * bms.info.resolution / (60 * 4);

        titleText = GameObject.Find("TitleText").GetComponent<Text>();
        artistText = GameObject.Find("ArtistText").GetComponent<Text>();
        bpmText = GameObject.Find("BpmText").GetComponent<Text>();
        bgaText = GameObject.Find("BgaText").GetComponent<Text>();
        layerText = GameObject.Find("LayerText").GetComponent<Text>();
        pulseText = GameObject.Find("PulseText").GetComponent<Text>();
        genreText = GameObject.Find("GenreText").GetComponent<Text>();

        //set Text
        titleText.text = "Title: " + bms.info.title;
        artistText.text = "Artist: " + bms.info.artist;
        bpmText.text = "Bpm: " + bms.info.init_bpm;
        genreText.text = "Genre: " + bms.info.genre;

        LoadBga();

        LoadSound(bms.sound_channels);
    }
	
	// Update is called once per frame
	void Update () {

        //bpm change events;
        if(bms.bpm_events.Count != 0 && bpmIndex < eventLength-1 && pulse > bms.bpm_events[bpmIndex].y)
        {
            pulseConstant = bms.bpm_events[bpmIndex].bpm * bms.info.resolution / (60 * 4);
            bpmText.text = "Bpm: " + bms.bpm_events[bpmIndex].bpm;
            bpmIndex++;
        }

        pulse100000 += (ulong)(Time.deltaTime * 100000 * pulseConstant);

        pulse = pulse100000 / 100000;

        if(notePlayers == null)
        {
            return;
        }
        foreach (NotePlayer np in notePlayers)
        {
            np.pulse = pulse;
        }
        //change bga sprite
        if (bms.bga.bga_events.Count != 0 && bgIndex < eventLength && pulse > bms.bga.bga_events[bgIndex].y)
        {
            bgImage.sprite = bgaSprites[bms.bga.bga_events[bgIndex].id];
            bgaText.text = "BGA: " + bms.bga.bga_header.Find(x => x.id == bms.bga.bga_events[bgIndex].id).name;
            bgIndex++;
            if (bgImage.sprite == null)
            {
                Debug.Log("Sprite Missing: " + bms.bga.bga_header.Find(x => x.id == bms.bga.bga_events[layerIndex].id).name);
            }
            else
            {
                bga.transform.localScale = new Vector3(
                worldScreenHeight / bgImage.sprite.bounds.size.x,
                worldScreenHeight / bgImage.sprite.bounds.size.y, 1);
                
                Debug.Log(bms.bga.bga_header.Find(x => x.id == bms.bga.bga_events[layerIndex].id).name);
            }
        }

        //change layer sprite
        
        if (bms.bga.layer_events.Count != 0 && layerIndex < eventLength && pulse > bms.bga.layer_events[layerIndex].y)
        {
            layerImage.sprite = layerSprites[bms.bga.layer_events[layerIndex].id];
            layerIndex++;
            layerText.text = "Layer: " + bms.bga.bga_header.Find(x => x.id == bms.bga.layer_events[layerIndex].id).name;
            if (layerImage.sprite == null)
            {
                Debug.Log("Sprite Missing: " + bms.bga.bga_header.Find(x => x.id == bms.bga.layer_events[layerIndex].id).name);
            }
            else
            {
                bgaLayer.transform.localScale = new Vector3(
                worldScreenHeight / layerImage.sprite.bounds.size.x,
                worldScreenHeight / layerImage.sprite.bounds.size.y, 1);
                
            }
        }

        pulseText.text = "Pulse: " + pulse + "/192";

    }

    //doesn't support bmp
    public static Texture2D LoadImageFromPath(string filePath)
    {
        Texture2D tex = null;
        byte[] fileData;

        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            tex = new Texture2D(256, 256);
            tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }
        return tex;
    }
    
    //old bga used Black as Transparent Color
    public static Texture2D BlackToTransparent(Texture2D tex)
    {
        Color[] pix = tex.GetPixels(0, 0, tex.width, tex.height);
        Color black = new Color(0, 0, 0);
        for(int p = 0; p<pix.Length; p++)
        {
            if(pix[p] == black)
                pix[p].a = 0;            
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
            string path = bms.path.Substring(0, bms.path.LastIndexOf("/") + 1) + bh.name;
            bgaTexture = LoadImageFromPath(path);
            if(bgaTexture == null)
            {
                Debug.Log("BGA File Missing!: " + path);
            }
            else
            {
                bgaSprites[bh.id] = Sprite.Create(bgaTexture, new Rect(0, 0, bgaTexture.width, bgaTexture.height), new Vector2(0, 0));
                layerSprites[bh.id] = Sprite.Create(BlackToTransparent(bgaTexture), new Rect(0, 0, bgaTexture.width, bgaTexture.height), new Vector2(0, 0));
            }
        }
    }

    void LoadSound(List<SoundChannel> sound_channels)
    {
        Transform soundTrans = GameObject.Find("Sound").transform;

        Debug.Log("Sound_Channel Count: " + sound_channels.Count);
        foreach (SoundChannel sc in sound_channels)
        {
            if (sc.name != "")
            {
                string path = bms.path.Substring(0, bms.path.LastIndexOf("/") + 1) + sc.name;

                if (!File.Exists(path) && String.Equals(Path.GetExtension(path).ToLower(), ".wav"))
                {
                    path = Path.ChangeExtension(path, ".ogg");
                }

                if (File.Exists(path))
                {
                    GameObject scPre = Instantiate(soundchannelpre) as GameObject;
                    scPre.name = sc.name;
                    scPre.transform.SetParent(soundTrans);

                    WWW www = new WWW("file:///" + path);

                    AudioClip clip = www.GetAudioClip(false, false);
                    /*
                    while (!www.isDone)
                    {
                        Debug.Log("not Done "+ sc.name);
                        yield return null;
                    }
                    */
                    clip.name = Path.GetFileName(path);

                    scPre.GetComponent<AudioSource>().clip = clip;
                    scPre.GetComponent<NotePlayer>().notes = sc.notes;
                }
                
            }
        }
        notePlayers = GameObject.Find("Sound").GetComponentsInChildren<NotePlayer>();

    }
}

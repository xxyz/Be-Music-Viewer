using UnityEngine;
using System.Collections;
using BMSParser;
using System.Collections.Generic;
using UnityEngine.UI;

public class DrawBMSGraph : MonoBehaviour {
    
    public GameObject graphBarWhite;
    public GameObject graphBarRed;
    public GameObject progressBar;

    public int playingMeausure = 0;

    public float paddingLeft = 0;
    public float paddingRight = 0;
    public float paddingTop = 0;
    public float paddingBottom = 0;

    public float timeOffset;

    private float left, right, bottom, top;

    private List<GameObject> bars = new List<GameObject>();
    private RectTransform progressBarRect;
    private int progressMeasure = 0;
    private double endTime;
    private float width, height;

    private bool isLoaded = false;

    public void DrawGraph(List<BmsEvent> bmsEvents)
    {
        RectTransform rect = GetComponent<RectTransform>();
        progressBarRect = progressBar.GetComponent<RectTransform>();

        endTime = bmsEvents[bmsEvents.Count - 1].time;

        left = rect.offsetMin.x;
        right = rect.offsetMax.x;
        bottom = rect.offsetMin.y;
        top = rect.offsetMax.y;

        width = right - left - paddingLeft - paddingRight;
        height = top - bottom - paddingBottom - paddingTop;

        progressBarRect.sizeDelta = new Vector2(1.0f, height - paddingTop - paddingBottom);
        progressBarRect.localPosition = new Vector3(paddingLeft, paddingBottom);

        int currMeasure = 0;
        double currMeasureLength = 0;
        double currMeasureTime = 0;
        int currMeasureNote = 0, currScratchNote = 0;
        int maxNote = 30;

        foreach (BmsEvent be in bmsEvents)
        {
            if(be.eventType == BMSParser.EventType.LineEvent)
            {
                
                currMeasureLength = be.time - currMeasureTime;
                
                GameObject bar = Instantiate(graphBarWhite) as GameObject;
                bar.name = "Bar Measure:" + currMeasure;
                bar.transform.SetParent(transform);
                RectTransform barRectTrans = bar.GetComponent<RectTransform>();
                barRectTrans.localPosition = new Vector3((float)(paddingLeft + width * currMeasureTime / endTime), paddingBottom);
                barRectTrans.sizeDelta = new Vector2((float)(currMeasureLength / endTime * width) - 0, (float)currMeasureNote / (float)currMeasureLength / maxNote  * height);
                bar.transform.localScale = new Vector3(1.0f, 1.0f);
                bars.Add(bar);

                if(currScratchNote > 0)
                {
                    GameObject scratchBar = Instantiate(graphBarRed) as GameObject;
                    scratchBar.name = "Scratch Measure: " + currMeasure;
                    scratchBar.transform.SetParent(transform);
                    RectTransform scratchRectTrans = scratchBar.GetComponent<RectTransform>();
                    scratchRectTrans.localPosition = new Vector3((float)(paddingLeft + width * currMeasureTime / endTime), paddingBottom);
                    scratchRectTrans.sizeDelta = new Vector2((float)(currMeasureLength / endTime * width) - 0, (float)currScratchNote / (float)currMeasureLength / maxNote * height);
                    scratchBar.transform.localScale = new Vector3(1.0f, 1.0f);
                }

                //set tooltip
                GraphBarTooltip tooltip = bar.GetComponent<GraphBarTooltip>();
                tooltip.measureNum = currMeasure;
                tooltip.notes = currMeasureNote;
                tooltip.time = currMeasureLength;
                tooltip.density = currMeasureNote / currMeasureLength;
                tooltip.scratch = currScratchNote;

                currMeasureNote = 0; currScratchNote = 0;
                currMeasure++;
                currMeasureTime = be.time;

            }
            else if(be.eventType == BMSParser.EventType.NoteEvent)
            {
                int x = ((NoteEvent)be).x;
                if (BMSUtil.GetNoteType(x) != NoteType.BGM)
                {
                    currMeasureNote++;
                    if (x == 8 || x == 16)
                        currScratchNote++;

                }
                    
            }

        }
        isLoaded = true;
    }

    void Update()
    {
        while (!isLoaded)
            return;

        float xPos = Mathf.Clamp(paddingLeft + (Time.time + timeOffset) / (float)endTime * width, paddingLeft, width + paddingLeft);
        progressBarRect.localPosition = new Vector3(xPos, paddingBottom);
        if(progressMeasure < bars.Count && bars[progressMeasure] != null && 
            bars[progressMeasure].GetComponent<RectTransform>().localPosition.x <= progressBarRect.localPosition.x)
        {
            bars[progressMeasure].GetComponent<Image>().color = Color.red;
            if(progressMeasure > 0)
                bars[progressMeasure-1].GetComponent<Image>().color = new Color(0.949f, 0.498f, 0.156f);
            progressMeasure++;
        }
    }

}

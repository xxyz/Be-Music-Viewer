using UnityEngine;
using System.Collections;
using BMSParser;
using System.Collections.Generic;
using UnityEngine.UI;

public class DrawBMSGraph : MonoBehaviour {
    
    public GameObject graphBarWhite;
    public GameObject graphBarRed;
    public GameObject graphBarBlue;
    public GameObject progressBar;
    public GameObject tooltipBar;

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
        int currMeasureNote = 0, currScratchNote = 0, currLnNote = 0;
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

                float scartchNoteBarHeight = 0f;
                if(currScratchNote > 0)
                {
                    GameObject scratchBar = Instantiate(graphBarRed) as GameObject;
                    scratchBar.name = "Scratch Measure: " + currMeasure;
                    scratchBar.transform.SetParent(transform);
                    RectTransform scratchRectTrans = scratchBar.GetComponent<RectTransform>();
                    scratchRectTrans.localPosition = new Vector3((float)(paddingLeft + width * currMeasureTime / endTime), paddingBottom);

                    scartchNoteBarHeight = currScratchNote / (float)currMeasureLength / maxNote * height;
                    scratchRectTrans.sizeDelta = new Vector2((float)(currMeasureLength / endTime * width), scartchNoteBarHeight);
                    scratchBar.transform.localScale = new Vector3(1.0f, 1.0f);
                }
                if(currLnNote > 0)
                {
                    GameObject lnBar = Instantiate(graphBarBlue) as GameObject;
                    lnBar.name = "LN Measure: " + currMeasure;
                    lnBar.transform.SetParent(transform);
                    RectTransform lnRectTrans = lnBar.GetComponent<RectTransform>();
                    lnRectTrans.localPosition = new Vector3((float)(paddingLeft + width * currMeasureTime / endTime), paddingBottom + scartchNoteBarHeight);

                    lnRectTrans.sizeDelta = new Vector2((float)(currMeasureLength / endTime * width), currLnNote / (float)currMeasureLength / maxNote * height);
                    lnBar.transform.localScale = new Vector3(1.0f, 1.0f);
                }

                //set tooltip

                GameObject tooltipbar = Instantiate(tooltipBar) as GameObject;
                tooltipbar.name = "Tooltip Measure:" + currMeasure;
                tooltipbar.transform.SetParent(transform);
                RectTransform tooltipbarRectTrans = tooltipbar.GetComponent<RectTransform>();
                tooltipbarRectTrans.localPosition = new Vector3((float)(paddingLeft + width * currMeasureTime / endTime), paddingBottom);
                tooltipbarRectTrans.sizeDelta = new Vector2((float)(currMeasureLength / endTime * width), height);
                tooltipbar.transform.localScale = new Vector3(1.0f, 1.0f);
                

                GraphBarTooltip tooltip = tooltipbar.GetComponent<GraphBarTooltip>();
                tooltip.measureNum = currMeasure;
                tooltip.notes = currMeasureNote;
                tooltip.time = currMeasureLength;
                tooltip.density = currMeasureNote / currMeasureLength;
                tooltip.scratch = currScratchNote;
                tooltip.ln = currLnNote;

                currMeasureNote = 0; currScratchNote = 0; currLnNote = 0;
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
                    else if (((NoteEvent)be).l > 0)
                        currLnNote++;
                }
                    
            }

        }
        isLoaded = true;
    }

    //move progress bar
    void Update()
    {
        while (!isLoaded)
            return;

        float xPos = Mathf.Clamp(paddingLeft + (Time.time - timeOffset) / (float)endTime * width, paddingLeft, width + paddingLeft);
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

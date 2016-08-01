using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class GraphBarTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    public int measureNum;
    public int notes;
    public double time;
    public double density;
    public int ln;
    public int scratch;

    private GameObject tooltipPanel;
    private Text measureText, noteText, timeText, densityText, lnText, scText;
    private Image barImage;
    private Color prevColor;

    void Awake()
    {
        barImage = GetComponent<Image>();
        tooltipPanel = GameObject.Find("MeasureInfoPanel");
        measureText = GameObject.Find("MeasureNumberText").GetComponent<Text>();
        noteText = GameObject.Find("MeasureNoteText").GetComponent<Text>();
        timeText = GameObject.Find("MeasureTimeText").GetComponent<Text>();
        densityText = GameObject.Find("MeasureDensityText").GetComponent<Text>();
        lnText = GameObject.Find("MeasureLnText").GetComponent<Text>();
        scText = GameObject.Find("MeasureScratchText").GetComponent<Text>();
    }

    void Start()
    {
        tooltipPanel.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        prevColor = barImage.color;
        barImage.color = Color.red;
        measureText.text = "Measure #" + measureNum;
        noteText.text = "Notes: " + notes;
        timeText.text = "Length: " + Math.Round(time, 2) + "s";
        densityText.text = "Density: " + Math.Round(density, 2) + "notes/s";
        scText.text = "Scratch: " + scratch;
        lnText.text = "LN: " + ln;
        tooltipPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        barImage.color = prevColor;
        tooltipPanel.SetActive(false);
    }
}

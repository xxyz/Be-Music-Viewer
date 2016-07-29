using UnityEngine;
using System.Collections;
using BMSParser;
using System.IO;
using System;
using System.Windows.Forms;
using UnityEngine.UI;

public class BMSLoader : MonoBehaviour {

    public string path;
    public GameObject titleText, subtitleText, artistText, subartistText, genreText;
    public GameObject eyecatchImage;
    public GameObject loadingBar;

    public BMS bms;

	public void loadBMS () {

        if (!File.Exists(path))
        {
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length != 0)
                path = args[args.Length - 1];
        }

        //Open without argument
        while (!File.Exists(path))
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMS Files|*.bms;*.bme;*.bml;*.pms";
            openFileDialog.Title = "Select a BMS File";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }
        }

        BMSParser.BMSParser bmsParser = new BMSParser.BMSParser();
        bms = bmsParser.Parse(path);
        titleText.GetComponent<Text>().text = bms.info.title;
        subtitleText.GetComponent<Text>().text = bms.info.subtitle;
        artistText.GetComponent<Text>().text = bms.info.artist;
        genreText.GetComponent<Text>().text = bms.info.genre;
        if (bms.info.subartists.Count != 0)
            subartistText.GetComponent<Text>().text = bms.info.subartists[0];
        else
            subartistText.SetActive(false);

        if (File.Exists(bms.path + "\\" + bms.info.eyecatch_image))
        {
            Texture2D bgaTex = Util.LoadImageFromPath(bms.path + "\\" + bms.info.eyecatch_image);
            eyecatchImage.GetComponent<Image>().sprite = Sprite.Create(bgaTex, new Rect(0, 0, bgaTex.width, bgaTex.height), new Vector2(0, 0));
        }
    }
}

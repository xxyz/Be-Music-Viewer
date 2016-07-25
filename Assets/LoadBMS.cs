using UnityEngine;
using System.Collections;
using System.Windows.Forms;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.IO;

public class LoadBMS : MonoBehaviour {

    public static string path;
    public Text loadingText;

    // Use this for initialization
    void Start() {

        loadingText = GameObject.Find("LoadingText").GetComponent<Text>();

        //BMSE Argument
        string[] args = Environment.GetCommandLineArgs();
        if(args.Length != 0)
        {
            Debug.Log(args[args.Length - 1]);
            path = args[args.Length - 1];

            if (File.Exists(path))
            {
                StartCoroutine("LoadMain");
                return;
            }
                
        }
        //Open without argument
        while (!File.Exists(path))
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "BMS Files|*.bms;*.bme;*.bml;*.pms";
            openFileDialog.Title = "Select a BMS File";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }
        }
        StartCoroutine("LoadMain");
    }

    IEnumerator LoadMain()
    {
        AsyncOperation async = SceneManager.LoadSceneAsync("main");

        while (!async.isDone)
        {
            int progress = Mathf.RoundToInt(async.progress * 100.0f);
            Debug.Log(progress);
            loadingText.text = progress + "%";

            yield return null;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}

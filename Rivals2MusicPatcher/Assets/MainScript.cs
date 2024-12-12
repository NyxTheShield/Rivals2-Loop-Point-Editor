using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using NaughtyAttributes;
using SFB;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScript : MonoBehaviour
{
    public static string path;
    
    public List<MusicData> songData;
    
    //UI Stuff
    public TMP_Dropdown songDropdown;
    public Button folderSelectButton;
    public TMP_Text folderPathUI;
    public TMP_Text trackTextUI;

    public GameObject songSelectView;
    public GameObject songDataView;
    public TMP_InputField loopStartInput;
    public TMP_InputField loopEndInput;

    public TMP_Text messageTemplate;
    public AnimationCurve messageCurve;
    private void Start()
    {
        //Populate Dropdown
        songDropdown.options = new List<TMP_Dropdown.OptionData>();
        
        foreach (var song in songData)
        {
            songDropdown.options.Add(new TMP_Dropdown.OptionData(song.songName));
        }
        
        //Populate Path
        if (PlayerPrefs.HasKey("BankPath"))
        {
            path = PlayerPrefs.GetString("BankPath");
            folderPathUI.text = "Current Path:" + path;
            OnPathChanged();
        }
    }

    public void SelectPath()
    {
        var returnArray = StandaloneFileBrowser.OpenFilePanel("Select Music.bank", "", "bank", false);
        if (returnArray.Length <= 0)
        {
            //LogError
            return;
            
        }

        var returnPath = returnArray[0];
        path = returnPath;
        folderPathUI.text = "Current Path:" + returnPath;
        PlayerPrefs.SetString("BankPath", path);
        OnPathChanged();
    }


    public void OnSongChanged()
    {
        trackTextUI.text = songDropdown.options[songDropdown.value].text;
        songDataView.SetActive(true);
        
        loopStartInput.text = (songData[songDropdown.value].ReadLoopStart()/48000.0f).ToString(CultureInfo.InvariantCulture);
        loopEndInput.text = (songData[songDropdown.value].ReadLoopEnd()/48000.0f).ToString(CultureInfo.InvariantCulture);
    }

    public void OnPatchButtonPressed()
    {
        try
        {
            int loopStartValue = (int)float.Parse(loopStartInput.text)*48000;
            int loopEndValue = (int)float.Parse(loopStartInput.text)*48000;
            PatchIntValue(songData[songDropdown.value].loopStartOffset1, loopStartValue);
            PatchIntValue(songData[songDropdown.value].loopStartOffset2, loopStartValue);
            PatchIntValue(songData[songDropdown.value].loopStartOffset3, loopStartValue);
            PatchIntValue(songData[songDropdown.value].loopEndOffset1, loopEndValue);
            PatchIntValue(songData[songDropdown.value].loopEndOffset2, loopEndValue);

            CreateMessage("Patched Sucessfully", Color.green);
        }
        catch (Exception e)
        {
            CreateMessage("Patching Failed!", Color.red);
            throw;
        }
        
    }

    public void CreateMessage(string message, Color color, float time = 2)
    {
        messageTemplate.text = message;
        messageTemplate.color = color;
        var confirmation = GameObject.Instantiate(messageTemplate.gameObject, messageTemplate.transform.parent);
        confirmation.SetActive(true);
            
        LeanTween.value(0, 1, time).setOnUpdateRatio((float val, float ratio) =>
        {
            confirmation.GetComponent<TMP_Text>().alpha = messageCurve.Evaluate(ratio);
        }).setOnComplete(() =>
        {
            GameObject.Destroy(confirmation);
        });
    }

    public void OnPathChanged()
    {
        songSelectView.SetActive(true);
    }

    [Button("Read Data")]
    public void ReadData()
    {
        var content = File.ReadAllText("C:\\Users\\pc\\Documents\\rivalsmusicdata");
        foreach (var line in content.Split(
                     new string[] { Environment.NewLine },
                     StringSplitOptions.None
                 ))
        {
            
            var auxData = new MusicData();
            var separated = line.Split(",");
            print(line);
            auxData.songName = separated[0];
            auxData.loopStartOffset1 = Int32.Parse(separated[3]);
            auxData.loopStartOffset2 = Int32.Parse(separated[4]);
            auxData.loopStartOffset3 =Int32.Parse(separated[5]);
            auxData.loopEndOffset1 = Int32.Parse(separated[6]);
            auxData.loopEndOffset2 =Int32.Parse(separated[7]);
            songData.Add(auxData);
        }
    }
    
    public static void PatchIntValue(int offset, int value)
    {
        using (Stream stream = File.Open(MainScript.path, FileMode.Open))
        {
            stream.Position = offset;
            var data = BitConverter.GetBytes(value);
            stream.Write(data, 0, data.Length);
        }
    }

    public static byte[] ReadAtOffset(byte[] content, int offset, int length)
    {
        return content.Skip(offset).Take(length).ToArray();
    }

    public static string IntToHexString(int value)
    {
        return BitConverter.ToString(BitConverter.GetBytes(value));
    }
    
    [Serializable]
    public class MusicData
    {
        public string songName = "Song Name";
        public int loopStartOffset1 = -1;
        public int loopStartOffset2 = -1;
        public int loopStartOffset3 = -1;
        public int loopEndOffset1 = -1;
        public int loopEndOffset2 = -1;

        public int ReadLoopStart()
        {
            var content = File.ReadAllBytes(MainScript.path);
            var value = ReadAtOffset(content, loopStartOffset1, 4);
            return BitConverter.ToInt32(value);
        }
        
        public int ReadLoopEnd()
        {
            var content = File.ReadAllBytes(MainScript.path);
            var value = ReadAtOffset(content, loopEndOffset1, 4);
            return BitConverter.ToInt32(value);
        }

        public void PatchValues(int loopStart, int loopEnd)
        {
            PatchIntValue(loopStartOffset1, loopStart);
            PatchIntValue(loopStartOffset2, loopStart);
            PatchIntValue(loopStartOffset3, loopStart);
            PatchIntValue(loopEndOffset1, loopEnd);
            PatchIntValue(loopEndOffset2, loopEnd);
        }
    }

}


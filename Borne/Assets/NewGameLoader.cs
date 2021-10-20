// Decompiled with JetBrains decompiler
// Type: GameLoader
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 38147E8E-80C5-4F37-B26A-9756989EB72D
// Assembly location: F:\Menu\ArcaBoxGameMenu_Data\Managed\Assembly-CSharp.dll

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.UI;

public class NewGameLoader : MonoBehaviour
{
    public Button btnLeft;
    public Button btnRight;
    public GameObject slides;
    public GameObject gameExample;
    public AudioSource audioSource;
    public Text errorText;
    private Sprite DEFAULT_IMG;
    private string GAME_DIRECTORY;
    private static List<string> SUPPORTED_IMAGE_TYPES = new List<string>()
      {
        ".jpg",
        ".png"
      };
    private Dictionary<string, string> Games;
    private List<string> Videos;
    private List<GameObject> Slides;

    private Process currentGameApp; // Contains the game that is active (if there is one)

    #region Timer
    private bool RASPBERRY_ENABLED;
    private string RASPBERRY_URL;
    private System.Timers.Timer timer;
    private int nbSeconds = 10;
    #endregion Timer

    #region Video
    private bool VIDEO_ENABLED;
    private string VIDEO_DIRECTORY;
    private string VLC_EXECUTABLE_PATH;
    #endregion Video

    private void Start()
    {
        this.DEFAULT_IMG = UnityEngine.Resources.Load<Sprite>("defaultImage");
        this.Games = new Dictionary<string, string>();
        this.Videos = new List<string>();
        this.Slides = GameObject.Find("Main Camera").GetComponent<SliderMenu>().Slides = new List<GameObject>();
        Cursor.visible = false;
        this.LoadConfigurationVar(); // Get the variables defined in the "App.config" file
        this.LoadGames();
        this.LoadVideos();

        if (VIDEO_ENABLED || RASPBERRY_ENABLED)
        {
            timer = new System.Timers.Timer();
            timer.Enabled = false;
            timer.Interval = (nbSeconds + 3) * 1000;
            timer.Elapsed += Timer_ElapsedEvent;
        }
    }

    private void Timer_ElapsedEvent(object sender, System.Timers.ElapsedEventArgs e)
    {
        currentGameApp.CloseMainWindow();
        timer.Stop();
        
    }

    private void Update()
    {
        
        if (currentGameApp != null && currentGameApp.HasExited) // It append when the game is closing
        {
            currentGameApp = null;
            if(RASPBERRY_ENABLED)
                setTimerStatus(false);
            timer.Stop();

            // Lauch video
            if (VIDEO_ENABLED)
            {
                try
                {
                    Process currentVideoApp = new Process(); // Contains the video player process
                    currentVideoApp.StartInfo.FileName = VLC_EXECUTABLE_PATH; // Use VLC media player to show the videos
                    currentVideoApp.StartInfo.Arguments = VIDEO_DIRECTORY + Videos[UnityEngine.Random.Range(0, Videos.Count)] + " --play-and-exit --fullscreen"; // The 2 arguments are to put the video in full screen and to close the program automatically at the end of the video
                    currentVideoApp.Start();
                }
                catch
                {
                    this.DisplayError("Lecture de la vidéo impossible (ER:03)", 2f);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            this.btnLeft.onClick.Invoke();
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            this.btnRight.onClick.Invoke();
        if (!Input.GetKeyDown(KeyCode.Alpha8))
            return;
        Transform child = this.slides.transform.GetChild(this.slides.transform.childCount - 1);
        try
        {
            string[] files = Directory.GetFiles(Directory.GetDirectories(this.Games[child.name] ?? "")[0], "*.exe");
            this.audioSource.volume = 0.0f;
            if (files.Length != 0)
            {
                string str = files[0];
                currentGameApp = new Process();
                currentGameApp.StartInfo.FileName = str;
                currentGameApp.Start();
                setTimerStatus(true);
                timer.Start();
                //System.Threading.Timer timer = new System.Threading.Timer(new System.Threading.TimerCallback(new System.Object()));
            }
            else
                this.DisplayError("Le fichier .exe n'a pas été trouvé (ER:04)", 2f);
        }
        catch
        {
            this.DisplayError("Le dossier du jeu est introuvable (ER:05)", 2f);
        }
    }

    private void DisplayError(string text, float fadoutTime)
    {
        if (!((System.Object)this.errorText != (System.Object)null))
            return;
        this.errorText.CrossFadeAlpha(1f, 0.0f, false);
        this.errorText.text = text;
        this.errorText.CrossFadeAlpha(0.0f, fadoutTime, false);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            return;
        this.audioSource.volume = 100f;
    }

    private void LoadGames()
    {
        DirectoryInfo[] directoryInfo;
        try
        {
            directoryInfo = new DirectoryInfo(GAME_DIRECTORY).GetDirectories();
        }
        catch
        {
            this.DisplayError("Jeux introuvables (ER:06)", 5f);
            return;
        }

        foreach (DirectoryInfo directory in directoryInfo)
            this.Games.Add(directory.Name, directory.FullName);

        foreach (KeyValuePair<string, string> game in this.Games)
        {
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.gameExample, this.transform.position, this.transform.rotation);
            Text componentInChildren1 = gameObject.GetComponentInChildren<Text>();
            Image componentInChildren2 = gameObject.GetComponentInChildren<Image>();
            gameObject.transform.SetParent(this.slides.transform);
            gameObject.name = game.Key;
            string key = game.Key;
            componentInChildren1.text = key;
            string[] files = Directory.GetFiles(game.Value, "*.*");
            string path1 = "";
            foreach (string path2 in files)
            {
                string extension = Path.GetExtension(path2);
                if (NewGameLoader.SUPPORTED_IMAGE_TYPES.Contains(extension))
                    path1 = path2;
            }
            byte[] numArray = new byte[0];
            Texture2D texture2D = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            try
            {
                byte[] data = File.ReadAllBytes(path1);
                texture2D.LoadImage(data);
                componentInChildren2.sprite = Sprite.Create(texture2D, new Rect(0.0f, 0.0f, (float)texture2D.width, (float)texture2D.height), new Vector2(0.5f, 0.5f), 100f);
            }
            catch
            {
                try
                {
                    componentInChildren2.sprite = this.DEFAULT_IMG;
                }
                catch
                {
                    componentInChildren2.sprite = (Sprite)null;
                }
            }
            this.Slides.Add(gameObject);
        }
    }

    private void LoadVideos()
    {
        FileInfo[] directoryInfo;
        try
        {
            directoryInfo = new DirectoryInfo(VIDEO_DIRECTORY).GetFiles();
        }
        catch
        {
            this.DisplayError("Vidéos introuvables (ER:07)", 5f);
            return;
        }

        foreach (FileInfo files in directoryInfo)
            this.Videos.Add(files.Name);
    }

    private void setTimerStatus(bool status)
    {
        string strStatus;
        if (status)
            strStatus = "start";
        else
            strStatus = "stop";

        WebRequest wrGETURL;
        wrGETURL = WebRequest.Create(RASPBERRY_URL + "/timerStatus?a=" + strStatus);
        try
        {
            wrGETURL.GetResponse();
        }
        catch
        {
            this.DisplayError("Communication avec le timer distant impossible (ER:08)", 3f);
        }
    }

    /// <summary>
    ///  Read the content of the config file "App.config" and set all the app var
    /// </summary>
    private void LoadConfigurationVar()
    {
        Dictionary<string, string> dictConfig;
        try
        {
            string[] lines = File.ReadAllLines("./App.config");
            dictConfig = lines.Select(l => l.Split('=')).ToDictionary(a => a[0], a => a[1]); // Put each param of file in a dictionary
        }
        catch
        {
            this.DisplayError("Fichier de config introuvable ou illisible (ER:01)", 10f);
            return;
        }

        if (dictConfig.Values.Contains(String.Empty)) // Check that all the variables are defined in the file
        {
            this.DisplayError("Valeur vide dans le fichier de config (ER:02)", 10f);
            return;
        }

        GAME_DIRECTORY = dictConfig["GAME_DIRECTORY"];

        TIMER_ENABLED = checkBoolConfigVar(dictConfig["TIME_LIMIT_ENABLED"]);
        if (TIMER_ENABLED) // The time is required to use the raspberry and/or the video features
        {
            NB_SECONDS = Convert.ToInt32(dictConfig["SECONDS"]);

            RASPBERRY_ENABLED = checkBoolConfigVar(dictConfig["RASPBERRY_ENABLED"]);
            if (RASPBERRY_ENABLED)
            {
                RASPBERRY_URL = dictConfig["RASPBERRY_URL"];
            }

            VIDEO_ENABLED = checkBoolConfigVar(dictConfig["VIDEO_ENABLED"]);
            if (VIDEO_ENABLED)
            {
                VIDEO_DIRECTORY = dictConfig["VIDEO_DIRECTORY"];
                VLC_EXECUTABLE_PATH = dictConfig["VLC_EXECUTABLE_PATH"];
            }
        }
        else
        {
            RASPBERRY_ENABLED = false;
            VIDEO_ENABLED = false;
        }
    }

    /// <summary>
    ///  Check the input string and determine if it's mean True or False
    /// </summary>
    /// <param name="configString">The string retrieve in the config file</param>
    /// <returns></returns>
    private bool checkBoolConfigVar(string configString)
    {
        configString = configString.ToLower();
        return configString == "on" ||
            configString == "true" ||
            configString == "1";
    }
}
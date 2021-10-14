// Decompiled with JetBrains decompiler
// Type: GameLoader
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 38147E8E-80C5-4F37-B26A-9756989EB72D
// Assembly location: F:\Menu\ArcaBoxGameMenu_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Configuration;

public class NewGameLoader : MonoBehaviour
{
    public Button btnLeft;
    public Button btnRight;
    public GameObject slides;
    public GameObject gameExample;
    public AudioSource audioSource;
    public Text errorText;
    private Sprite DEFAULT_IMG;
    private const string GAME_DIRECTORY = "./Games/";
    private const string VIDEO_DIRECTORY = "./Videos/";
    private static List<string> SUPPORTED_IMAGE_TYPES = new List<string>()
      {
        ".jpg",
        ".png"
      };
    private Dictionary<string, string> Games;
    private List<string> Videos;
    private List<GameObject> Slides;

    private Process currentGameApp; // Contains the game that is active (if there is one)

    #region Raspberry Timer

    // Timer raspberry with physical 7-segment
    private string raspberryURL = "http://10.5.42.89:80";

    #endregion Raspberry Timer

    private void Start()
    {
        this.DEFAULT_IMG = UnityEngine.Resources.Load<Sprite>("defaultImage");
        this.Games = new Dictionary<string, string>();
        this.Videos = new List<string>();
        this.Slides = GameObject.Find("Main Camera").GetComponent<SliderMenu>().Slides = new List<GameObject>();
        Cursor.visible = false;
        this.LoadGames();
        this.LoadVideos();
        //this.LoadConfigurationVar(); // Get the variables defined in the "App.config" file
    }

    private void Update()
    {
        if (currentGameApp != null && currentGameApp.HasExited) // It append when the game is closing
        {
            currentGameApp = null;
            setTimerStatus(false);

            // Lauch video
            Process currentVideoApp = new Process();
            currentVideoApp.StartInfo.FileName = @"C:\Program Files\VideoLAN\VLC\vlc.exe";
            currentVideoApp.StartInfo.Arguments = @".\Videos\" + Videos[UnityEngine.Random.Range(0, Videos.Count)] + " --play-and-exit --fullscreen";
            //Process.Start(@"videos\shoot_2.mp4");
            currentVideoApp.Start();
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
            }
            else
                this.DisplayError("Le fichier .exe n'a pas été trouvé", 1f);
        }
        catch
        {
            this.DisplayError("Le dossier du jeu est introuvable", 1f);
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
        foreach (DirectoryInfo directory in new DirectoryInfo("./Games/").GetDirectories())
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
        foreach (FileInfo files in new DirectoryInfo(VIDEO_DIRECTORY).GetFiles())
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
        wrGETURL = WebRequest.Create(raspberryURL + "/timerStatus?a=" + strStatus);
        wrGETURL.GetResponse();
    }

    //private void LoadConfigurationVar()
    //{
    //    raspberryURL = "";
    //    raspberryURL = ConfigurationManager.AppSettings.Get("RASPBERRRY_URL");
    //}
}
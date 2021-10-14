using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour
{
    public Button btnLeft;
    public Button btnRight;
    public GameObject slides;
    public GameObject gameExample;
    public AudioSource audioSource;

    private const string GAME_DIRECTORY = "./Games/";

    private Dictionary<string, string> Games;
    private List<GameObject> Slides;

    private Process gameApp;

    #region Raspberry Timer
    // Timer raspberry with physical 7-segment
    const string RASPBERRY_URL = "http://10.5.42.89:80";
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        this.Games = new Dictionary<string, string>();

        GameObject camera = GameObject.Find("Main Camera");
        SliderMenu script = camera.GetComponent<SliderMenu>();
        Slides = script.Slides = new List<GameObject>();

		Cursor.visible = false;

        LoadGames();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            btnLeft.onClick.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            btnRight.onClick.Invoke();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            audioSource.volume = 0f;

            string selectedGame = slides.transform.GetChild(slides.transform.childCount - 1).name;

            UnityEngine.Debug.Log(audioSource.volume);

            string path = @"" + Games[selectedGame] + "\\App\\game.exe";

            //eventHandled = new TaskCompletionSource<bool>();

            gameApp = new Process();
            gameApp.StartInfo.FileName = path;
            //gameApp.Exited += new System.EventHandler(Game_Exited);
            gameApp.Start();
            

            setTimerStatus(true);
        }

        if (gameApp != null && gameApp.HasExited)
        {
            setTimerStatus(false);
            gameApp = null;
        }
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
            audioSource.volume = 100f;
    }

    private void LoadGames()
    {
        DirectoryInfo dir = new DirectoryInfo(GAME_DIRECTORY);

        foreach (DirectoryInfo d in dir.GetDirectories())
        {
            this.Games.Add(d.Name, d.FullName);
        }

        foreach (var game in this.Games)
        {

            GameObject gmObj = Instantiate(gameExample, transform.position, transform.rotation);
            Text gmObjText = gmObj.GetComponentInChildren<Text>();
            Image gmObjImage = gmObj.GetComponentInChildren<Image>();
            
            //Load values
            gmObj.transform.SetParent(slides.transform);
            gmObj.name = game.Key;
            gmObjText.text = game.Key;

            //Load image
            byte[] data = File.ReadAllBytes(game.Value + "\\image.jpg");
            Texture2D texture = new Texture2D(64, 64, TextureFormat.ARGB32, false);
            texture.LoadImage(data);
            gmObjImage.sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);

            Slides.Add(gmObj);
        }
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
        wrGETURL.GetResponse();
    }
}

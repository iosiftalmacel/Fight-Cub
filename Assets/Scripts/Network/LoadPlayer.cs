using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LoadPlayer : MonoBehaviour {

    public GameObject scroll1;
    public GameObject scroll2;
    public Text text;

    public GameObject tierPage;
    public GameObject loadingPage;

    bool startedLoading;
    bool retriedLoading;
    float startedLoadingTime;

    void OnEnable()
    {
        scroll1.SetActive(true);
        scroll2.SetActive(false);
        tierPage.SetActive(true);
        loadingPage.SetActive(false);
        startedLoading = false;
        text.text = "SELECT CHARACTER";

        AudioManager.Instance.StopAll();
        AudioManager.Instance.Play("off_game", 1, true);
    }


    void Start()
    {
        if (MultiplayerManager.singleton == null)
        {
            Debug.Log("Cazzooooooo");
            return;
        }

        ((MultiplayerManager)MultiplayerManager.singleton).playerPrefabList.Add((GameObject)Resources.Load("TRex"));
        ((MultiplayerManager)MultiplayerManager.singleton).playerPrefabList.Add((GameObject)Resources.Load("Cat"));
        ((MultiplayerManager)MultiplayerManager.singleton).playerPrefabList.Add((GameObject)Resources.Load("Platy"));
    }

    void Update()
    {
        if (startedLoading)
        {
            if(Time.realtimeSinceStartup - startedLoadingTime > 15 && !retriedLoading)
            {
                ((MultiplayerManager)MultiplayerManager.singleton).StartMultiplayer();
                retriedLoading = true;
            }
            else if(Time.realtimeSinceStartup - startedLoadingTime > 30 && retriedLoading)
            {
                startedLoading = false;
                tierPage.SetActive(true);
                loadingPage.SetActive(false);
            }
        }
    }

    public void ChoosePlayer(int value)
    {

        ((MultiplayerManager)MultiplayerManager.singleton).index = value;

        text.text = "SELECT TIER";
        scroll1.SetActive(false);
        scroll2.SetActive(true);
        AudioManager.Instance.Play("ui_button", 4f);
    }

    public void ChooseTier(string value)
    {

        ((MultiplayerManager)MultiplayerManager.singleton).onlineScene = value;
        ((MultiplayerManager)MultiplayerManager.singleton).ROOM = value;

        tierPage.SetActive(false);
        loadingPage.SetActive(true);
        startedLoading = true;
        retriedLoading = false;
        startedLoadingTime = Time.realtimeSinceStartup;
        ((MultiplayerManager)MultiplayerManager.singleton).StartMultiplayer();
        AudioManager.Instance.Play("ui_swish");
    }

    public void GoBack()
    {
        if (scroll2.activeInHierarchy)
        {
            scroll1.SetActive(true);
            scroll2.SetActive(false);
            text.text = "SELECT CHARACTER";
            AudioManager.Instance.Play("ui_button", 4f);
        }
        else
        {
            AudioManager.Instance.Play("ui_button", 4f);
            Application.Quit();
        }
    }
}

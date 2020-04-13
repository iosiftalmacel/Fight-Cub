using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButton : MonoBehaviour {

	public void StartGame()
    {
        AudioManager.Instance.Play("ui_button", 4f);
        SceneManager.LoadScene("ChoosePlayer");
    }
}

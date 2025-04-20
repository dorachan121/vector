using UnityEngine;
using UnityEngine.SceneManagement;

public class sceneManager : MonoBehaviour
{
    public void GoTutorial(){
        SceneManager.LoadScene("tutorial");

    }

    public void GoPlayGame(){
        SceneManager.LoadScene("game");
    }
}

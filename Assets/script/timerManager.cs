using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class timerManager : MonoBehaviour
{
    public float Counter;
    public Text timer;
    public GameObject timerImage;
    public void CountDown()
    {
        betManager seeBetManager = GetComponent<betManager>();
        gameManager seeGameManager = GetComponent<gameManager>();
        timer.text = string.Format("{0:00.00}", Counter);
        Counter -= Time.deltaTime;
        if (Counter <= 0.0f)
        {
            Counter = 0.0f;
            for (int i = 0; i < seeBetManager.betMoneySelectButton.Length; i++)
            {
                seeBetManager.betMoneySelectButton[i].enabled = false;

            }
        }
    }


    public void BetTimer()
    {
        timerManager timerManage = GetComponent<timerManager>();
        timer.gameObject.SetActive(true);
        timerImage.SetActive(true);
        timerManage.enabled = true;
    }
}

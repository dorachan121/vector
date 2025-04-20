using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using Random = UnityEngine.Random;

public class gameManager : MonoBehaviour
{
    [Header("セレクターの管理")]
    public Text playerCRS;
    public Text dealerCRS;
    public GameObject[] playerSelectedCRS;
    public GameObject[] dealerSelectedCRS;
    [Header("リザルト数値")]
    public Text[] WinText;
    public Text[] LoseText;
    public Text[] DrawText;

    enum RESULT_DISPLAY_TEXT_ELEMENT
    {
        GET_MONEY,
        PLAYER_RCS,
        DEALER_RCS,
        PLAYER_MAX_NUM,
        DEALER_MAX_NUM,

    }
    RESULT_DISPLAY_TEXT_ELEMENT _ELEMENT;
    public GameObject[] WinLose;
    private int playerAudience;
    public float resultDisplayTimer;
    private float displayTimer;
    public List<int> row = new List<int>();
    public List<int> column = new List<int>();
    public List<int> slash = new List<int>();
    public List<int> maxRCS = new List<int>();
    public int resultCounter;
    public Text[] row_result;
    public Text[] column_result;
    public Text[] slash_result;
    public GameObject row_gameObject;
    public GameObject column_gameObject;
    public GameObject slash_gameObject;

    [Header("試合中に更新するゲームオブジェクト")]
    public List<GameObject> usedCards = new List<GameObject>();
    //private Vector3 defaultCardSize;
    public List<GameObject> playerSelectedCards = new List<GameObject>();
    public List<int> usedStageNumber = new List<int>();

    [Header("プレイヤーのボタン表示をするためのボタン")]
    public GameObject[] distributeCardPlayer;


    [Header("使用するカードのプレハブ")]

    public List<GameObject> cards = new List<GameObject>();

    [Header("カードのボタン")]
    public Button[] playerHandButton;
    public Button[] CardOnStageButton;

    [Header("表示するカードの座標をGameObjectで管理")]
    public GameObject[] displayPlayerHand;
    public GameObject[] displayCardOnStage;

    [Header("セレクターの管理")]
    public GameObject PlayerAudienceSelector;
    public GameObject RowColumnSlashSelector;
    public GameObject PlayerDealerSelector;

    private int counter = 0;
    float second = 0f;
    private int displayCard;
    private int stageNumber;
    private int playerCardNumber;

    enum MAGNIFICATION
    {
        LOSE,
        DRAW,
        TWO_TIMES,
        THREE_TIMES,
        MAX
    }
    MAGNIFICATION magnification;

    enum DP
    {
        DEALER,
        PLAYER
    }
    DP dp;

    enum PLAYER_SELECT
    {
        PLAYER_AUDIENCE_PHASE,
        PLAYER_DEALER_PHASE,
    }
    PLAYER_SELECT playerSelect;
    enum PHASE
    {
        FIRST_CARD,
        DISTRIBUTE_PLAYER_CARD,
        DEALER_HAND,
        PLAYER_AUDIENCE_SELECT,
        BET_TIME,
        COLUMN_ROW_SLASH_SELECT,
        PLAYER_DEALER_SELECT,
        DEALER_AUDIENCE,
        PLAYER_AUDIENCE,
        RESULT,
        RESULT_JUDGEMENT,
        RESULT_JUDGEMENT_AUDIENCE,
        RESULT_DISPLAY,

        TMP,
    };
    PHASE phase;


    enum RESULT
    {
        ROW,
        COLUMN,
        SLASH,
    }
    RESULT result;
    enum RCS
    {
        ROW,
        COLUMN,
        SLASH
    }
    RCS rcs;


    // Start is called before the first frame update
    void Start()
    {
        // for (int i = 0; i < cards.Count; i++)
        // {
        //     defaultCardSize = cards[i].transform.localScale;
        // }

        displayCard = Random.Range(0, cards.Count);
        playerAudience = Random.Range(0, playerSelectedCRS.Length);
        displayTimer = resultDisplayTimer;
        // 各ボタンに対してリスナーを追加
        for (int i = 0; i < playerHandButton.Length; i++)
        {
            int index = i; // ボタンの番号
            playerHandButton[i].onClick.AddListener(() => PLAYER_CARD_BUTTON(index));
        }
        for (int i = 0; i < CardOnStageButton.Length; i++)
        {
            int index = i; // ボタンの番号
            CardOnStageButton[i].onClick.AddListener(() => STAGE_CARD_BUTTON(index));
        }
        playerCardNumber = 9;
        phase = PHASE.FIRST_CARD;
        playerSelect = PLAYER_SELECT.PLAYER_DEALER_PHASE;
        stageNumber = 4;
    }

    private int counterOfPlayerSelectedCardsArrays = 0;
    // Update is called once per frame
    void Update()
    {
        betManager seeBetManager = GetComponent<betManager>();
        timerManager seeTimerManager = GetComponent<timerManager>();

        switch (phase)
        {
            case PHASE.FIRST_CARD:
                if (!usedCards.Contains(cards[displayCard]) && !usedStageNumber.Contains(stageNumber))
                {

                    usedStageNumber.Add(stageNumber);
                    Instantiate(cards[displayCard], new Vector2(displayCardOnStage[stageNumber].transform.position.x, displayCardOnStage[stageNumber].transform.position.y), Quaternion.identity);
                    phase = PHASE.PLAYER_AUDIENCE_SELECT;
                    usedCards[4] = cards[displayCard];
                }
                else
                {
                    displayCard = Random.Range(0, cards.Count);
                    stageNumber = Random.Range(0, displayCardOnStage.Length);
                }
                break;
            case PHASE.DISTRIBUTE_PLAYER_CARD:

                second += Time.deltaTime;
                if (!usedCards.Contains(cards[displayCard]))
                {
                    if (second >= 0.5f)
                    {
                        second = 0f;
                        //cards[displayCard].transform.localScale = new Vector3(0.2f, 0.2f, 0.1f);
                        Instantiate(cards[displayCard], new Vector2(displayPlayerHand[counter].transform.position.x, displayPlayerHand[counter].transform.position.y), Quaternion.identity);
                        usedCards[playerCardNumber] = cards[displayCard];
                        if (playerSelect == PLAYER_SELECT.PLAYER_AUDIENCE_PHASE)
                        {
                            playerSelectedCards[counterOfPlayerSelectedCardsArrays] = usedCards[9 + counterOfPlayerSelectedCardsArrays];
                            counterOfPlayerSelectedCardsArrays++;
                        }
                        playerCardNumber++;

                        counter++;
                    }
                    if (counter >= displayPlayerHand.Length)
                    {
                        switch (playerSelect)
                        {
                            case PLAYER_SELECT.PLAYER_DEALER_PHASE:
                                // for (int i = 0; i < cards.Count; i++)
                                // {
                                //     cards[i].transform.localScale = defaultCardSize;
                                // }
                                phase = PHASE.COLUMN_ROW_SLASH_SELECT;
                                break;
                            case PLAYER_SELECT.PLAYER_AUDIENCE_PHASE:
                                // for (int i = 0; i < cards.Count; i++)
                                // {
                                //     cards[i].transform.localScale = defaultCardSize;
                                // }

                                phase = PHASE.PLAYER_DEALER_SELECT;
                                break;
                        }
                        second = 0f;
                    }
                }
                else
                {
                    displayCard = Random.Range(0, cards.Count);
                }
                break;

            case PHASE.PLAYER_DEALER_SELECT:
                second += Time.deltaTime;
                if (second >= 0.5f)
                {
                    PlayerDealerSelector.SetActive(true);
                    phase = PHASE.TMP;
                    second = 0f;
                }
                break;

            case PHASE.COLUMN_ROW_SLASH_SELECT:
                second += Time.deltaTime;
                if (second >= 0.5f)
                {
                    RowColumnSlashSelector.SetActive(true);
                    phase = PHASE.BET_TIME;
                    second = 0f;
                }
                break;

            case PHASE.DEALER_HAND:

                second += Time.deltaTime;
                if (!usedCards.Contains(cards[displayCard]) && !usedStageNumber.Contains(stageNumber))
                {
                    if (second >= 1.5f)
                    {
                        second = 0f;
                        usedStageNumber.Add(stageNumber);
                        CardOnStageButton[stageNumber].transform.position = new Vector3(1000000, 0, 0);
                        Instantiate(cards[displayCard], new Vector2(displayCardOnStage[stageNumber].transform.position.x, displayCardOnStage[stageNumber].transform.position.y), Quaternion.identity);
                        usedCards[stageNumber] = cards[displayCard];
                        // counter++;
                        phase = PHASE.TMP;
                        for (int i = 0; i < distributeCardPlayer.Length; i++)
                        {
                            distributeCardPlayer[i].SetActive(true);
                        }
                        resultCounter++;
                        if (resultCounter >= CardOnStageButton.Length)
                        {
                            phase = PHASE.RESULT;
                        }
                    }
                }
                else
                {
                    displayCard = Random.Range(0, cards.Count);
                    stageNumber = Random.Range(0, displayCardOnStage.Length);
                }
                break;

            case PHASE.PLAYER_AUDIENCE_SELECT:
                second += Time.deltaTime;
                if (second >= 2.5f)
                {
                    PlayerAudienceSelector.SetActive(true);
                    phase = PHASE.TMP;
                    second = 0f;
                }
                break;

            case PHASE.BET_TIME:
                seeTimerManager.BetTimer();
                seeTimerManager.CountDown();
                if (seeTimerManager.Counter <= 0)
                {
                    seeTimerManager.Counter = 0;
                    switch (playerSelect)
                    {
                        case PLAYER_SELECT.PLAYER_DEALER_PHASE:
                            seeBetManager.betMoneySelectButton[3].gameObject.SetActive(false);
                            phase = PHASE.DISTRIBUTE_PLAYER_CARD;
                            break;
                        case PLAYER_SELECT.PLAYER_AUDIENCE_PHASE:
                            while (true)
                            {
                                playerSelectedCRS[playerAudience].SetActive(true);
                                playerCRS.text = playerSelectedCRS[playerAudience].name;
                                switch (playerCRS.text)
                                {
                                    case "row":
                                        rcs = RCS.ROW;
                                        break;
                                    case "column":
                                        rcs = RCS.COLUMN;
                                        break;
                                    case "slash":
                                        rcs = RCS.SLASH;
                                        break;
                                }

                                int dealer = Random.Range(0, dealerSelectedCRS.Length);
                                if (playerAudience == dealer)
                                {
                                    continue;
                                }
                                else
                                {
                                    seeBetManager.betMoneySelectButton[3].gameObject.SetActive(false);
                                    phase = PHASE.PLAYER_AUDIENCE;
                                    dealerSelectedCRS[dealer].SetActive(true);
                                    dealerCRS.text = dealerSelectedCRS[dealer].name;

                                    break;
                                }
                            }

                            break;
                    }
                }
                break;

            case PHASE.PLAYER_AUDIENCE:

                second += Time.deltaTime;
                if (!usedCards.Contains(cards[displayCard]) && !usedStageNumber.Contains(stageNumber))
                {
                    if (second >= 1.5f)
                    {
                        second = 0f;
                        usedStageNumber.Add(stageNumber);
                        //CardOnStageButton[stageNumber].transform.position = new Vector3(1000000, 0, 0);
                        Instantiate(playerSelectedCards[counter], new Vector2(displayCardOnStage[stageNumber].transform.position.x, displayCardOnStage[stageNumber].transform.position.y), Quaternion.identity);
                        usedCards[stageNumber] = playerSelectedCards[counter];
                        counter++;
                        resultCounter++;
                        phase = PHASE.DEALER_AUDIENCE;

                    }
                }
                else
                {
                    displayCard = Random.Range(0, playerSelectedCards.Count);
                    stageNumber = Random.Range(0, displayCardOnStage.Length);
                }
                break;

            case PHASE.DEALER_AUDIENCE:

                second += Time.deltaTime;
                if (!usedCards.Contains(cards[displayCard]) && !usedStageNumber.Contains(stageNumber))
                {
                    if (second >= 1.5f)
                    {
                        second = 0f;
                        usedStageNumber.Add(stageNumber);
                        //CardOnStageButton[stageNumber].transform.position = new Vector3(1000000, 0, 0);
                        Instantiate(cards[displayCard], new Vector2(displayCardOnStage[stageNumber].transform.position.x, displayCardOnStage[stageNumber].transform.position.y), Quaternion.identity);

                        usedCards[stageNumber] = cards[displayCard];
                        // counter++;
                        phase = PHASE.PLAYER_AUDIENCE;

                        resultCounter++;
                        if (resultCounter >= CardOnStageButton.Length)
                        {
                            phase = PHASE.RESULT;
                        }
                    }
                }
                else
                {
                    displayCard = Random.Range(0, cards.Count);
                    stageNumber = Random.Range(0, displayCardOnStage.Length);
                }
                break;

            case PHASE.RESULT:
                Debug.Log("リザルト画面表示");
                RESULT_CALCULATION(usedCards);
                switch (result)
                {
                    case RESULT.ROW:
                        row_gameObject.SetActive(true);
                        for (int i = 0; i < row_result.Length; i++)
                        {
                            row_result[i].text = row[i].ToString();
                        }
                        resultDisplayTimer -= Time.deltaTime;
                        if (resultDisplayTimer <= 0.0f)
                        {
                            Debug.Log("ROWのリザルト");
                            resultDisplayTimer = displayTimer;
                            result = RESULT.COLUMN;
                            row_gameObject.SetActive(false);
                        }
                        maxRCS[0] = Math.Max(int.Parse(row_result[0].text), Math.Max(int.Parse(row_result[1].text), int.Parse(row_result[2].text)));
                        break;
                    case RESULT.COLUMN:

                        column_gameObject.SetActive(true);
                        for (int i = 0; i < column_result.Length; i++)
                        {
                            column_result[i].text = column[i].ToString();
                        }
                        resultDisplayTimer -= Time.deltaTime;
                        if (resultDisplayTimer <= 0.0f)
                        {
                            Debug.Log("COLUMNのリザルト");
                            resultDisplayTimer = displayTimer;
                            result = RESULT.SLASH;
                            column_gameObject.SetActive(false);
                        }
                        maxRCS[1] = Math.Max(int.Parse(column_result[0].text), Math.Max(int.Parse(column_result[1].text), int.Parse(column_result[2].text)));

                        break;
                    case RESULT.SLASH:
                        slash_gameObject.SetActive(true);
                        for (int i = 0; i < slash_result.Length; i++)
                        {
                            slash_result[i].text = slash[i].ToString();
                        }
                        resultDisplayTimer -= Time.deltaTime;
                        if (resultDisplayTimer <= 0.0f)
                        {
                            Debug.Log("SLASHのリザルト");
                            resultDisplayTimer = displayTimer;
                            if (dp == DP.DEALER || dp == DP.PLAYER)
                            {
                                phase = PHASE.RESULT_JUDGEMENT_AUDIENCE;

                            }
                            else
                            {

                                phase = PHASE.RESULT_JUDGEMENT;
                            }
                            slash_gameObject.SetActive(false);
                        }
                        maxRCS[2] = Math.Max(int.Parse(slash_result[0].text), int.Parse(slash_result[1].text));

                        break;
                }
                break;

            case PHASE.RESULT_JUDGEMENT_AUDIENCE:
                switch (dp)
                {
                    case DP.DEALER:
                        switch (rcs)
                        {
                            case RCS.ROW:
                                switch (dealerCRS.text)
                                {
                                    case "row":
                                        //nop
                                        break;
                                    case "column":
                                        if (maxRCS[0] < maxRCS[1])
                                        {
                                            Debug.Log("ディーラーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[0] > maxRCS[1])
                                        {
                                            Debug.Log("ディーラーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[0] == maxRCS[1])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                    case "slash":
                                        if (maxRCS[0] < maxRCS[2])
                                        {
                                            Debug.Log("ディーラーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[0] > maxRCS[2])
                                        {
                                            Debug.Log("ディーラーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[0] == maxRCS[2])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                }

                                break;
                            case RCS.COLUMN:
                                switch (dealerCRS.text)
                                {
                                    case "row":
                                        if (maxRCS[1] < maxRCS[0])
                                        {
                                            Debug.Log("ディーラーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[1] > maxRCS[0])
                                        {
                                            Debug.Log("ディーラーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[1] == maxRCS[0])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                    case "column":
                                        //nop
                                        break;
                                    case "slash":
                                        if (maxRCS[1] < maxRCS[2])
                                        {
                                            Debug.Log("ディーラーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[1] > maxRCS[2])
                                        {
                                            Debug.Log("ディーラーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[1] == maxRCS[2])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                }

                                break;
                            case RCS.SLASH:
                                switch (dealerCRS.text)
                                {
                                    case "row":
                                        if (maxRCS[2] < maxRCS[0])
                                        {
                                            Debug.Log("ディーラーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[2] > maxRCS[0])
                                        {
                                            Debug.Log("ディーラーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[2] == maxRCS[0])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                    case "column":
                                        if (maxRCS[2] < maxRCS[1])
                                        {
                                            Debug.Log("ディーラーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[2] > maxRCS[1])
                                        {
                                            Debug.Log("ディーラーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[2] == maxRCS[1])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                    case "slash":
                                        //nop
                                        break;
                                }

                                break;
                        }

                        break;
                    case DP.PLAYER:
                        switch (rcs)
                        {
                            case RCS.ROW:
                                switch (dealerCRS.text)
                                {
                                    case "row":
                                        //nop
                                        break;
                                    case "column":
                                        if (maxRCS[0] > maxRCS[1])
                                        {
                                            Debug.Log("プレイヤーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[0] < maxRCS[1])
                                        {
                                            Debug.Log("プレイヤーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[0] == maxRCS[1])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                    case "slash":
                                        if (maxRCS[0] > maxRCS[2])
                                        {
                                            Debug.Log("プレイヤーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[0] < maxRCS[2])
                                        {
                                            Debug.Log("プレイヤーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[0] == maxRCS[2])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                }

                                break;
                            case RCS.COLUMN:
                                switch (dealerCRS.text)
                                {
                                    case "row":
                                        if (maxRCS[1] > maxRCS[0])
                                        {
                                            Debug.Log("プレイヤーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[1] < maxRCS[0])
                                        {
                                            Debug.Log("プレイヤーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[1] == maxRCS[0])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                    case "column":
                                        //nop
                                        break;
                                    case "slash":
                                        if (maxRCS[1] > maxRCS[2])
                                        {
                                            Debug.Log("プレイヤーの勝利");
                                            magnification = MAGNIFICATION.TWO_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[1] < maxRCS[2])
                                        {
                                            Debug.Log("プレイヤーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[1] == maxRCS[2])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                }

                                break;
                            case RCS.SLASH:
                                switch (dealerCRS.text)
                                {
                                    case "row":
                                        if (maxRCS[2] > maxRCS[0])
                                        {
                                            Debug.Log("プレイヤーの勝利");
                                            magnification = MAGNIFICATION.THREE_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[2] < maxRCS[0])
                                        {
                                            Debug.Log("プレイヤーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[2] == maxRCS[0])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                    case "column":
                                        if (maxRCS[2] > maxRCS[1])
                                        {
                                            Debug.Log("プレイヤーの勝利");
                                            magnification = MAGNIFICATION.THREE_TIMES;
                                            WinLose[0].SetActive(true);
                                        }
                                        else if (maxRCS[2] < maxRCS[1])
                                        {
                                            Debug.Log("プレイヤーの敗北");
                                            magnification = MAGNIFICATION.LOSE;
                                            WinLose[1].SetActive(true);
                                        }
                                        else if (maxRCS[2] == maxRCS[1])
                                        {
                                            Debug.Log("ドロー");
                                            magnification = MAGNIFICATION.DRAW;
                                            WinLose[2].SetActive(true);
                                        }
                                        break;
                                    case "slash":
                                        //nop
                                        break;
                                }

                                break;
                        }

                        break;
                    default:
                        break;
                }

                phase = PHASE.RESULT_DISPLAY;
                break;

            case PHASE.RESULT_JUDGEMENT:

                switch (rcs)
                {
                    case RCS.ROW:
                        switch (dealerCRS.text)
                        {
                            case "row":
                                //nop
                                break;
                            case "column":
                                if (maxRCS[0] > maxRCS[1])
                                {
                                    Debug.Log("プレイヤーの勝利");
                                    magnification = MAGNIFICATION.TWO_TIMES;
                                    WinLose[0].SetActive(true);
                                }
                                else if (maxRCS[0] < maxRCS[1])
                                {
                                    Debug.Log("プレイヤーの敗北");
                                    magnification = MAGNIFICATION.LOSE;
                                    WinLose[1].SetActive(true);
                                }
                                else if (maxRCS[0] == maxRCS[1])
                                {
                                    Debug.Log("ドロー");
                                    magnification = MAGNIFICATION.DRAW;
                                    WinLose[2].SetActive(true);
                                }
                                break;
                            case "slash":
                                if (maxRCS[0] > maxRCS[2])
                                {
                                    Debug.Log("プレイヤーの勝利");
                                    magnification = MAGNIFICATION.TWO_TIMES;
                                    WinLose[0].SetActive(true);
                                }
                                else if (maxRCS[0] < maxRCS[2])
                                {
                                    Debug.Log("プレイヤーの敗北");
                                    magnification = MAGNIFICATION.LOSE;
                                    WinLose[1].SetActive(true);
                                }
                                else if (maxRCS[0] == maxRCS[2])
                                {
                                    Debug.Log("ドロー");
                                    magnification = MAGNIFICATION.DRAW;
                                    WinLose[2].SetActive(true);
                                }
                                break;
                        }

                        break;
                    case RCS.COLUMN:
                        switch (dealerCRS.text)
                        {
                            case "row":
                                if (maxRCS[1] > maxRCS[0])
                                {
                                    Debug.Log("プレイヤーの勝利");
                                    magnification = MAGNIFICATION.TWO_TIMES;
                                    WinLose[0].SetActive(true);
                                }
                                else if (maxRCS[1] < maxRCS[0])
                                {
                                    Debug.Log("プレイヤーの敗北");
                                    magnification = MAGNIFICATION.LOSE;
                                    WinLose[1].SetActive(true);
                                }
                                else if (maxRCS[1] == maxRCS[0])
                                {
                                    Debug.Log("ドロー");
                                    magnification = MAGNIFICATION.DRAW;
                                    WinLose[2].SetActive(true);
                                }
                                break;
                            case "column":
                                //nop
                                break;
                            case "slash":
                                if (maxRCS[1] > maxRCS[2])
                                {
                                    Debug.Log("プレイヤーの勝利");
                                    magnification = MAGNIFICATION.TWO_TIMES;
                                    WinLose[0].SetActive(true);
                                }
                                else if (maxRCS[1] < maxRCS[2])
                                {
                                    Debug.Log("プレイヤーの敗北");
                                    magnification = MAGNIFICATION.LOSE;
                                    WinLose[1].SetActive(true);
                                }
                                else if (maxRCS[1] == maxRCS[2])
                                {
                                    Debug.Log("ドロー");
                                    magnification = MAGNIFICATION.DRAW;
                                    WinLose[2].SetActive(true);
                                }
                                break;
                        }

                        break;
                    case RCS.SLASH:
                        switch (dealerCRS.text)
                        {
                            case "row":
                                if (maxRCS[2] > maxRCS[0])
                                {
                                    Debug.Log("プレイヤーの勝利");
                                    magnification = MAGNIFICATION.THREE_TIMES;
                                    WinLose[0].SetActive(true);
                                }
                                else if (maxRCS[2] < maxRCS[0])
                                {
                                    Debug.Log("プレイヤーの敗北");
                                    magnification = MAGNIFICATION.LOSE;
                                    WinLose[1].SetActive(true);
                                }
                                else if (maxRCS[2] == maxRCS[0])
                                {
                                    Debug.Log("ドロー");
                                    magnification = MAGNIFICATION.DRAW;
                                    WinLose[2].SetActive(true);
                                }
                                break;
                            case "column":
                                if (maxRCS[2] > maxRCS[1])
                                {
                                    Debug.Log("プレイヤーの勝利");
                                    magnification = MAGNIFICATION.THREE_TIMES;
                                    WinLose[0].SetActive(true);
                                }
                                else if (maxRCS[2] < maxRCS[1])
                                {
                                    Debug.Log("プレイヤーの敗北");
                                    magnification = MAGNIFICATION.LOSE;
                                    WinLose[1].SetActive(true);
                                }
                                else if (maxRCS[2] == maxRCS[1])
                                {
                                    Debug.Log("ドロー");
                                    magnification = MAGNIFICATION.DRAW;
                                    WinLose[2].SetActive(true);
                                }
                                break;
                            case "slash":
                                //nop
                                break;
                        }

                        break;
                }

                phase = PHASE.RESULT_DISPLAY;
                break;

            case PHASE.RESULT_DISPLAY:
                switch (magnification)
                {
                    case MAGNIFICATION.LOSE:
                        LoseText[0].text = "-$" + seeBetManager.nowBetMoney;
                        seeBetManager.data.lastMoney -= seeBetManager.nowBetMoney;
                        LoseText[1].text = playerCRS.text;
                        LoseText[2].text = dealerCRS.text;
                        switch (rcs)
                        {
                            case RCS.ROW:
                                LoseText[3].text = maxRCS[0].ToString();
                                break;
                            case RCS.COLUMN:
                                LoseText[3].text = maxRCS[1].ToString();
                                break;
                            case RCS.SLASH:
                                LoseText[3].text = maxRCS[2].ToString();
                                break;
                        }

                        switch (dealerCRS.text)
                        {
                            case "row":
                                LoseText[4].text = maxRCS[0].ToString();
                                break;
                            case "column":
                                LoseText[4].text = maxRCS[1].ToString();
                                break;
                            case "slash":
                                LoseText[4].text = maxRCS[2].ToString();
                                break;
                        }
                        break;
                    case MAGNIFICATION.DRAW:
                        DrawText[0].text = "+$00";
                        seeBetManager.data.lastMoney -= seeBetManager.remainingMoney;
                        DrawText[1].text = playerCRS.text;
                        DrawText[2].text = dealerCRS.text;
                        switch (rcs)
                        {
                            case RCS.ROW:
                                DrawText[3].text = maxRCS[0].ToString();
                                break;
                            case RCS.COLUMN:
                                DrawText[3].text = maxRCS[1].ToString();
                                break;
                            case RCS.SLASH:
                                DrawText[3].text = maxRCS[2].ToString();
                                break;
                        }

                        switch (dealerCRS.text)
                        {
                            case "row":
                                DrawText[4].text = maxRCS[0].ToString();
                                break;
                            case "column":
                                DrawText[4].text = maxRCS[1].ToString();
                                break;
                            case "slash":
                                DrawText[4].text = maxRCS[2].ToString();
                                break;
                        }
                        break;

                    case MAGNIFICATION.TWO_TIMES:
                        WinText[0].text = "+$" + seeBetManager.nowBetMoney + "x2";
                        seeBetManager.data.lastMoney = seeBetManager.remainingMoney + seeBetManager.nowBetMoney * 2;
                        WinText[1].text = playerCRS.text;
                        WinText[2].text = dealerCRS.text;
                        switch (rcs)
                        {
                            case RCS.ROW:
                                WinText[3].text = maxRCS[0].ToString();
                                break;
                            case RCS.COLUMN:
                                WinText[3].text = maxRCS[1].ToString();
                                break;
                            case RCS.SLASH:
                                WinText[3].text = maxRCS[2].ToString();
                                break;
                        }
                        switch (dealerCRS.text)
                        {
                            case "row":
                                WinText[4].text = maxRCS[0].ToString();
                                break;
                            case "column":
                                WinText[4].text = maxRCS[1].ToString();
                                break;
                            case "slash":
                                WinText[4].text = maxRCS[2].ToString();
                                break;
                        }
                        break;

                    case MAGNIFICATION.THREE_TIMES:
                        WinText[0].text = "+$" + seeBetManager.nowBetMoney + "x3";
                        seeBetManager.data.lastMoney = seeBetManager.remainingMoney + seeBetManager.nowBetMoney * 3;
                        WinText[1].text = playerCRS.text;
                        WinText[2].text = dealerCRS.text;
                        switch (rcs)
                        {
                            case RCS.ROW:
                                WinText[3].text = maxRCS[0].ToString();
                                break;
                            case RCS.COLUMN:
                                WinText[3].text = maxRCS[1].ToString();
                                break;
                            case RCS.SLASH:
                                WinText[3].text = maxRCS[2].ToString();
                                break;
                        }
                        switch (dealerCRS.text)
                        {
                            case "row":
                                WinText[4].text = maxRCS[0].ToString();
                                break;
                            case "column":
                                WinText[4].text = maxRCS[1].ToString();
                                break;
                            case "slash":
                                WinText[4].text = maxRCS[2].ToString();
                                break;
                        }
                        break;

                    default:
                        break;
                }
                phase = PHASE.TMP;
                break;

            case PHASE.TMP:
                counter = 0;
                Debug.Log("にゃああああああ");
                break;

            default:
                break;
        }

    }
    void PLAYER_CARD_BUTTON(int num)
    {
        Debug.Log(num); // ボタンの番号を表示
        playerSelectedCards[counterOfPlayerSelectedCardsArrays] = usedCards[9 + num];
        playerHandButton[num].transform.position = new Vector3(1000000, 0, 0);
    }
    void STAGE_CARD_BUTTON(int num)
    {
        timerManager seeTimerManager = GetComponent<timerManager>();

        usedStageNumber.Add(num);
        Debug.Log(num); // ボタンの番号を表示
        Instantiate(playerSelectedCards[counterOfPlayerSelectedCardsArrays], new Vector2(displayCardOnStage[num].transform.position.x, displayCardOnStage[num].transform.position.y), Quaternion.identity);
        CardOnStageButton[num].transform.position = new Vector3(1000000, 0, 0);
        usedCards[num] = playerSelectedCards[counterOfPlayerSelectedCardsArrays];

        for (int i = 0; i < distributeCardPlayer.Length; i++)
        {
            distributeCardPlayer[i].SetActive(false);
        }
        counterOfPlayerSelectedCardsArrays++;
        phase = PHASE.DEALER_HAND;

        resultCounter++;
    }

    public void PLAYER_SELECT_BUTTON()
    {
        betManager seeBetManager = GetComponent<betManager>();
        switch (playerSelect)
        {
            case PLAYER_SELECT.PLAYER_DEALER_PHASE:
                PlayerAudienceSelector.SetActive(false);
                //playerSelect = PLAYER_SELECT.PLAYER_DEALER_PHASE;
                phase = PHASE.BET_TIME;
                seeBetManager.betMoneySelectButton[3].gameObject.SetActive(true);
                for (int i = 0; i < seeBetManager.betMoneySelectButton.Length; i++)
                {
                    seeBetManager.betMoneySelectButton[i].enabled = true;

                }
                break;
            case PLAYER_SELECT.PLAYER_AUDIENCE_PHASE:
                dp = DP.PLAYER;
                PlayerDealerSelector.SetActive(false);
                Shuffle(playerSelectedCards);

                phase = PHASE.BET_TIME;
                seeBetManager.betMoneySelectButton[3].gameObject.SetActive(true);
                for (int i = 0; i < seeBetManager.betMoneySelectButton.Length; i++)
                {
                    seeBetManager.betMoneySelectButton[i].enabled = true;

                }
                break;


            default:
                break;
        }
    }

    void RESULT_CALCULATION(List<GameObject> obj)
    {
        //rowの計算
        row[0] = int.Parse(obj[0].name.Substring(1)) + int.Parse(obj[1].name.Substring(1)) + int.Parse(obj[2].name.Substring(1));
        row[1] = int.Parse(obj[3].name.Substring(1)) + int.Parse(obj[4].name.Substring(1)) + int.Parse(obj[5].name.Substring(1));
        row[2] = int.Parse(obj[6].name.Substring(1)) + int.Parse(obj[7].name.Substring(1)) + int.Parse(obj[8].name.Substring(1));

        //columnの計算
        column[0] = int.Parse(obj[0].name.Substring(1)) + int.Parse(obj[3].name.Substring(1)) + int.Parse(obj[6].name.Substring(1));
        column[1] = int.Parse(obj[1].name.Substring(1)) + int.Parse(obj[4].name.Substring(1)) + int.Parse(obj[7].name.Substring(1));
        column[2] = int.Parse(obj[2].name.Substring(1)) + int.Parse(obj[5].name.Substring(1)) + int.Parse(obj[8].name.Substring(1));

        slash[0] = int.Parse(obj[0].name.Substring(1)) + int.Parse(obj[4].name.Substring(1)) + int.Parse(obj[8].name.Substring(1));
        slash[1] = int.Parse(obj[2].name.Substring(1)) + int.Parse(obj[4].name.Substring(1)) + int.Parse(obj[6].name.Substring(1));

        Debug.Log("slash0:" + slash[0]);
        Debug.Log("slash1:" + slash[1]);
    }
    void Shuffle(List<GameObject> obj)
    {
        for (int i = 0; i < obj.Count; i++)
        {
            //（説明１）現在の要素を預けておく
            GameObject temp = obj[i];
            //（説明２）入れ替える先をランダムに選ぶ
            int randomIndex = Random.Range(0, obj.Count);
            //（説明３）現在の要素に上書き
            obj[i] = obj[randomIndex];
            //（説明４）入れ替え元に預けておいた要素を与える
            obj[randomIndex] = temp;
        }
    }
    public void DEALER_SELECT_BUTTON()
    {
        betManager seeBetManager = GetComponent<betManager>();

        dp = DP.DEALER;
        PlayerDealerSelector.SetActive(false);
        Shuffle(playerSelectedCards);
        phase = PHASE.BET_TIME;
        seeBetManager.betMoneySelectButton[3].gameObject.SetActive(true);
        for (int i = 0; i < seeBetManager.betMoneySelectButton.Length; i++)
        {
            seeBetManager.betMoneySelectButton[i].enabled = true;

        }
    }
    public void AUDIENCE_SELECT_BUTTON()
    {
        PlayerAudienceSelector.SetActive(false);
        playerSelect = PLAYER_SELECT.PLAYER_AUDIENCE_PHASE;
        phase = PHASE.DISTRIBUTE_PLAYER_CARD;
    }
    public void SLASH_SELECT_BUTTON()
    {
        rcs = RCS.SLASH;
        RowColumnSlashSelector.SetActive(false);
        playerSelectedCRS[2].SetActive(true);
        playerCRS.text = playerSelectedCRS[2].name;
        while (true)
        {
            int rand = Random.Range(0, playerSelectedCRS.Length);
            if (rand == 2)
            {
                continue;
            }
            else
            {
                dealerSelectedCRS[rand].SetActive(true);
                dealerCRS.text = dealerSelectedCRS[rand].name;
                break;
            }
        }

        for (int i = 0; i < distributeCardPlayer.Length; i++)
        {
            distributeCardPlayer[i].SetActive(true);
        }
        phase = PHASE.TMP;
    }
    public void ROW_SELECT_BUTTON()
    {
        rcs = RCS.ROW;
        RowColumnSlashSelector.SetActive(false);
        playerSelectedCRS[0].SetActive(true);
        playerCRS.text = playerSelectedCRS[0].name;
        while (true)
        {
            int rand = Random.Range(0, playerSelectedCRS.Length);
            if (rand == 0)
            {
                continue;
            }
            else
            {
                dealerSelectedCRS[rand].SetActive(true);
                dealerCRS.text = dealerSelectedCRS[rand].name;
                break;
            }
        }
        for (int i = 0; i < distributeCardPlayer.Length; i++)
        {
            distributeCardPlayer[i].SetActive(true);
        }
        phase = PHASE.TMP;
    }
    public void COLUMN_SELECT_BUTTON()
    {
        rcs = RCS.COLUMN;
        RowColumnSlashSelector.SetActive(false);
        playerSelectedCRS[1].SetActive(true);
        playerCRS.text = playerSelectedCRS[1].name;
        while (true)
        {
            int rand = Random.Range(0, playerSelectedCRS.Length);
            if (rand == 1)
            {
                continue;
            }
            else
            {
                dealerSelectedCRS[rand].SetActive(true);
                dealerCRS.text = dealerSelectedCRS[rand].name;
                break;
            }
        }
        for (int i = 0; i < distributeCardPlayer.Length; i++)
        {
            distributeCardPlayer[i].SetActive(true);
        }
        phase = PHASE.TMP;
    }

    public void GO_GAME()
    {
        SceneManager.LoadScene("game");

    }

}

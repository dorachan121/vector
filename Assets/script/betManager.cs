using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SaveData
{
    public int lastMoney;
}


public class betManager : MonoBehaviour
{
    [HideInInspector] public SaveData data;     // json変換するデータのクラス
    string filepath;                            // jsonファイルのパス
    string fileName = "Data.json";              // jsonファイル名

    //-------------------------------------------------------------------
    // 開始時にファイルチェック、読み込み
    void Awake()
    {
        // パス名取得
        filepath = Application.dataPath + "/" + fileName;

        // ファイルがないとき、ファイル作成
        if (!File.Exists(filepath))
        {
            //data.lastMoney=0;
            Save(data);
        }

        // ファイルを読み込んでdataに格納
        data = Load(filepath);
    }

    //-------------------------------------------------------------------
    // jsonとしてデータを保存
    void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data);                 // jsonとして変換
        StreamWriter wr = new StreamWriter(filepath, false);    // ファイル書き込み指定
        wr.WriteLine(json);                                     // json変換した情報を書き込み
        wr.Close();                                             // ファイル閉じる
    }

    // jsonファイル読み込み
    SaveData Load(string path)
    {
        StreamReader rd = new StreamReader(path);               // ファイル読み込み指定
        string json = rd.ReadToEnd();                           // ファイル内容全て読み込む
        rd.Close();                                             // ファイル閉じる

        return JsonUtility.FromJson<SaveData>(json);            // jsonファイルを型に戻して返す
    }

    //標示するテキストに関するインスタンス
    public Text remainingMoneyText;
    public Text nowBetMoneyText;
    public int remainingMoney;
    public int nowBetMoney;

    //掛け金に関するインスタンス
    public UnityEngine.UI.Button[] betMoneySelectButton;
    public GameObject[] betMoney;
    public GameObject sideOfBetMoney;
    private float moveDistance = 1.5f;//移動範囲
    private int selectBetMoney = 1;//ベット金額の選択部分の変数
    private const int maxVisibilityPoint = 7;
    private const int minVisibilityPoint = 0;
    private int visibilityPoint = 0; //掛け金の見えない部分


    // Start is called before the first frame update
    void Start()
    {
        remainingMoney = data.lastMoney;
        remainingMoneyText.text = string.Format("remainder:\n{0}$", data.lastMoney);// カウントダウンが0秒になったらタイマーを止める
        nowBetMoneyText.text = string.Format("bet:{0}$", nowBetMoney);// カウントダウンが0秒になったらタイマーを止める
        for (int i = 0; i < betMoneySelectButton.Length; i++)
        {
            betMoneySelectButton[i].enabled = false;

        }
        UpdateVisibility();
        for (int i = 0; i < betMoney.Length; i++)
        {
            betMoney[i].transform.position = new Vector3(
                betMoney[i].transform.position.x,
                betMoney[i].transform.position.y,
                betMoney[i].transform.position.z);
        }
    }

    // Update is called once per frame
    void Update()
    {
        Save(data);
        timerManager seeTimerManager = GetComponent<timerManager>();
        gameManager seeGameManager = GetComponent<gameManager>();

        Debug.Log("nowBetMoney:" + nowBetMoney);
    }
    public void betMoneySelectButton_Cancel()
    {
        remainingMoney+=nowBetMoney;
        remainingMoneyText.text = string.Format("remainder:\n{0}$", remainingMoney);// カウントダウンが0秒になったらタイマーを止める
        nowBetMoney = 0;
        nowBetMoneyText.text = string.Format("{0}", nowBetMoney);
    }
    public void betMoneySelectButton_Bet()
    {
        nowBetMoney += int.Parse(betMoney[selectBetMoney].name);
        remainingMoney -= int.Parse(betMoney[selectBetMoney].name);
        data.lastMoney=remainingMoney;
        if (nowBetMoney <= remainingMoney)
        {
            nowBetMoneyText.text = string.Format("bet:{0}$", nowBetMoney);// カウントダウンが0秒になったらタイマーを止める
            remainingMoneyText.text = string.Format("remainder:\n{0}$", remainingMoney);// カウントダウンが0秒になったらタイマーを止める
        }
        else
        {
            nowBetMoney -= int.Parse(betMoney[selectBetMoney].name);
        }
    }

    public void betMoneySelectButton_Left()
    {
        if (visibilityPoint > minVisibilityPoint)
        {
            visibilityPoint--;
            selectBetMoney--;
            //Debug.Log(betMoney[selectBetMoney]);
            if (visibilityPoint > betMoney.Length - 3)
            {
                visibilityPoint = betMoney.Length - 3;
            }
            for (int i = 0; i < betMoney.Length; i++)
            {
                betMoney[i].transform.position = new Vector3(
                    betMoney[i].transform.position.x,
                    betMoney[i].transform.position.y - moveDistance,
                    betMoney[i].transform.position.z);
            }
            UpdateVisibility();
        }
    }
    public void betMoneySelectButton_Right()
    {
        if (visibilityPoint < maxVisibilityPoint)
        {
            visibilityPoint++;
            selectBetMoney++;
            //Debug.Log(betMoney[selectBetMoney]);
            if (visibilityPoint > betMoney.Length - 3)
            {
                visibilityPoint = betMoney.Length - 3;
            }
            for (int i = 0; i < betMoney.Length; i++)
            {
                betMoney[i].transform.position = new Vector3(
                    betMoney[i].transform.position.x,
                    betMoney[i].transform.position.y + moveDistance,
                    betMoney[i].transform.position.z);
            }

            UpdateVisibility();
        }
    }
    private void UpdateVisibility()
    {
        for (int i = 0; i < betMoney.Length; i++)
        {
            // iが表示範囲内なら表示、それ以外は非表示
            betMoney[i].SetActive(i >= visibilityPoint && i < visibilityPoint + 3);
        }
    }
}

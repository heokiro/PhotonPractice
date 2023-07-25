using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Xml.Linq;

public class GameManager : MonoBehaviourPunCallbacks
{
    #region Singleton
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            if (!_instance)
            {
                _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                if (_instance == null)
                    Debug.Log("no Singleton obj");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }

        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    public QuizBank quizBank;

    [Header("PlayerUI")]
    public Text[] OneChat;
    public GameObject[] ShowPanel;
    public Image[] ShowPanelImage;
    public Text[] ShowPanelText;
    public GameObject[] MissHintX;
    public Color[] playerColor;
    private PlayerController[] playerArr;
    public int readyCount = 0;

    [Header("GameUI")]
    public Text expText;
    public Text quizText;
    public GameObject ShowPanels;
    public GameObject AnswerPanel;
    public InputField AnswerInputField;
    public GameObject WaitPanel;
    public Text AnswerText;
    public GameObject RoundPanel;
    public Text RoundText;
    private bool isShowHint;
    private bool isRounding;

    [Header("GameLogic")]
    public PhotonView PV;
    int rounds = 13;
    int curRound = 1;

    string curQuiz = "";

    private void Start()
    {
        expText.text = "모두 준비 하면 시작됩니다.";
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        CheckAllReady();
    }

    void CheckAllReady()
    {
        if (PhotonNetwork.InRoom)
        {
            if (readyCount == PhotonNetwork.CurrentRoom.PlayerCount && PhotonNetwork.CurrentRoom.PlayerCount >= 2 && curRound == 1)
            {
                if(!isRounding)
                {
                    isRounding = true;
                    RoundPanelOpen();
                    Debug.Log("StartGame");
                    Debug.Log("curRound : " + curRound);
                }
            }
            else if (readyCount == PhotonNetwork.CurrentRoom.PlayerCount && curRound > 1 && curRound <= 13)
            {
                if(!isRounding)
                {
                    isRounding = true;
                    if (!isShowHint)
                    {
                        PV.RPC("ShowHint", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
                        isShowHint = true;
                        Debug.Log("NextRound");
                        Debug.Log("curRound : " + curRound);
                        Invoke("TurnOffWaitPanel", 5f);

                    }
                }
            }
            else if (curRound > 13)
            {
                //EndGame();
            }
        }
    }

    [PunRPC]
    void AllReadyButton(string name)
    {
        playerArr = FindObjectsOfType<PlayerController>();
        foreach (var player in playerArr)
        {
            player.TurnOffRPCNoButton();
            player.RPCReadyButton(name);
            player.hintText.text = "";
            player.hintInputField.text = "";

            if (curRound > 1)
            {
                player.HintPlaceholder.text = "힌트를 입력해주세요";
            }
            if (PhotonNetwork.CurrentRoom.Players[(curRound) % (PhotonNetwork.CurrentRoom.PlayerCount) + 1].NickName == player.PV.Owner.NickName)
            {
                player.HintPlaceholder.text = "정답을 맞힐 차례입니다";
            }
            
        }
        isRounding = false;
        if (name == PhotonNetwork.LocalPlayer.NickName)
        {
            curRound++;
        }
    }

    void TurnOffWaitPanel()
    {
        WaitPanel.SetActive(false);
    }

    void RoundPlay()
    {
        expText.text = $"{PhotonNetwork.CurrentRoom.Players[curRound % (PhotonNetwork.CurrentRoom.PlayerCount) + 1].NickName}" + "님이 맞출단어";
        GetAnswerPanel(PhotonNetwork.LocalPlayer.NickName);
        quizText.text = quizBank.QuizBankArr[curRound];
    }

    void RoundPanelOpen()
    {
        RoundText.text = "ROUND " + $"{curRound}";
        RoundPanel.SetActive(true);
        Invoke("RoundPanelClose", 2f);
    }

    void RoundPanelClose()
    {
        RoundPanel.SetActive(false);
        PV.RPC("AllReadyButton", RpcTarget.All, PhotonNetwork.NickName);
        RoundPlay();
    }

    void GetAnswerPanel(string name)
    {
        if( name == PhotonNetwork.CurrentRoom.Players[curRound % (PhotonNetwork.CurrentRoom.PlayerCount) + 1].NickName)
        {
            AnswerPanel.SetActive(true);
            WaitPanel.SetActive(true);
        }
    }

    //Answer Button Connect Method
    public void SendAnswer()
    {
        if (quizText.text == AnswerText.text)
        {
           PV.RPC("CorrectAnswer", RpcTarget.All);
        }
        else
        {
          PV.RPC("WrongAnswer", RpcTarget.All);
        }

        AnswerPanel.SetActive(false);
    }

    [PunRPC]
    private void CorrectAnswer()
    {
        PV.RPC("HideHint", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        AnswerText.text = "";
        AnswerInputField.text = "";
        Debug.Log("Correct!");

    }

    [PunRPC]
    void WrongAnswer()
    {
        PV.RPC("HideHint", RpcTarget.All, PhotonNetwork.LocalPlayer.NickName);
        AnswerText.text = "";
        AnswerInputField.text = "";
        Debug.Log("Wrong!!");
    }

    [PunRPC]
    void ShowHint(string name)
    {
        if(name == PhotonNetwork.LocalPlayer.NickName)
        {
            Debug.Log("ShowHint!!");
            ShowPanels.transform.localPosition -= new Vector3(0, 1500, 0);
        }
    }

    [PunRPC]
    void HideHint(string name)
    {
        if (name == PhotonNetwork.LocalPlayer.NickName)
        {
            Debug.Log("HideHint!!");
            ShowPanels.transform.localPosition += new Vector3(0, 1500, 0);
            isShowHint = false;
            RoundPanelOpen();
        }
    }
}
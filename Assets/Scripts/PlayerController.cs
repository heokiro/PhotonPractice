using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks
{
    [Header("PlayerProp")]
    public Text userName;
    private Text chatText;
    public InputField hintInputField;
    public Text HintPlaceholder;
    public Text hintText;
    private Text showText;
    private GameObject missHintX;
    public RawImage[] IconImages;
    public SpriteRenderer Square;

    [Header("ReadyPanel")]
    public GameObject ReadyPanel;
    public Image ReadyPanelImage;
    public Text ReadyText;
    public bool isReady;

    public PhotonView PV;
    public int playerRoomId;

    private void Start()
    {
        userName.text = PV.Owner.NickName;
        playerRoomId = PV.OwnerActorNr - 1;
        PV.RPC("UpdateUserData", RpcTarget.All);
    }

    void Update()
    {

    }

    [PunRPC]
    public void UpdateUserData()
    {
        chatText = GameManager.Instance.OneChat[playerRoomId];
        showText = GameManager.Instance.ShowPanelText[playerRoomId];
        missHintX = GameManager.Instance.MissHintX[playerRoomId];
        Square.color = GameManager.Instance.playerColor[playerRoomId];
        ReadyPanelImage.color = GameManager.Instance.playerColor[playerRoomId];

        GameManager.Instance.ShowPanel[playerRoomId].SetActive(true);
        GameManager.Instance.ShowPanelImage[playerRoomId].color = GameManager.Instance.playerColor[playerRoomId];
    }

    public void ReadyButton()
    {
        PV.RPC("RPCReadyButton", RpcTarget.All, PhotonNetwork.NickName);
        PV.RPC("RPCUpdateHintButton", RpcTarget.All, hintText.text);
    }

    [PunRPC]
    void RPCUpdateHintButton(string hintText)
    {
        showText.text = hintText;
    }

    [PunRPC]
    public void RPCReadyButton(string name)
    {
        if (name == PV.Owner.NickName && !isReady)
        {
            var pos = ReadyPanel.transform.localPosition;
            ReadyPanel.transform.localPosition = pos - new Vector3(0, 1010);
            ReadyText.text = "취소!";
            isReady = true;
            GameManager.Instance.readyCount++;
        }
        else if (name == PV.Owner.NickName && isReady)
        {
            var pos = ReadyPanel.transform.localPosition;
            ReadyPanel.transform.localPosition = pos + new Vector3(0, 1010);
            ReadyText.text = "준비!";
            isReady = false;
            GameManager.Instance.readyCount--;
        }
    }

    public void TurnOnRPCNoButton()
    {
        PV.RPC("RPCTONMButton", RpcTarget.All, PhotonNetwork.NickName);
    }

    public void TurnOffRPCNoButton()
    {
        PV.RPC("RPCTOFFMButton", RpcTarget.All, PhotonNetwork.NickName);
    }

    [PunRPC]
    public void RPCTONMButton(string name)
    {
        if (name == PV.Owner.NickName)
        {
            missHintX.SetActive(true);
        }
    }

    [PunRPC]
    public void RPCTOFFMButton(string name)
    {
        if (name == PV.Owner.NickName)
        {
            missHintX.SetActive(false);
        }
    }
}

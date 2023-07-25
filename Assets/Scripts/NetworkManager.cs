using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Photon.Realtime;
using Unity.VisualScripting;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("DisconnectPanel")]
    public GameObject DisconnectPanel;
    public InputField NameInput;

    [Header("LobbyPanel")]
    public GameObject LobbyPanel;
    public InputField RoomNameInput;
    public Text WelcomeText;
    public Text LobbyInfoText;
    public Button[] RoomBtn;

    [Header("GamePanel")]
    public GameObject GamePanel;
    public Text RoomInfoText;
    public Text[] ChatText;
    public InputField ChatInput;
    public GameObject[] ResponPoint;
    public GameObject LeftUserPanel;
    public GameObject RightUserPanel;

    [Header("ETC")]
    public GameObject LoadingPanel;
    public Text StatusText;
    public PhotonView PV;

    List<RoomInfo> myList = new List<RoomInfo>();

    private void Update()
    {
        StatusText.text = PhotonNetwork.NetworkClientState.ToString() + "\n하는 중 입니다.";
        LobbyInfoText.text = "현재 " + (PhotonNetwork.CountOfPlayers) + "명이 접속중입니다.\n" +
            "즐거운 시간 되세요.";
        EnterSend();
    }

    #region ConnectServer
    public void Connect()
    {
        PhotonNetwork.ConnectUsingSettings();
        LoadingPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        LoadingPanel.SetActive(false);
        LobbyPanel.SetActive(true);
        DisconnectPanel.SetActive(false);
        GamePanel.SetActive(false);
        PhotonNetwork.LocalPlayer.NickName = NameInput.text;
        WelcomeText.text = PhotonNetwork.LocalPlayer.NickName + "님 환영합니다.";
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        LobbyPanel.SetActive(false);
        GamePanel.SetActive(false);
    }
    #endregion

    #region Room

    public void MyListClick(int num)
    {
        LoadingPanel.SetActive(true);
        PhotonNetwork.JoinRoom(myList[num].Name);
        MyListRenewal();
    }

    private void MyListRenewal()
    {
        for (int i = 0; i < RoomBtn.Length; i++)
        {
            RoomBtn[i].interactable = ( i < myList.Count) ? true : false;
            RoomBtn[i].transform.GetChild(0).GetComponent<Text>().text = (i < myList.Count) ? myList[i].Name : "";
            RoomBtn[i].transform.GetChild(1).GetComponent<Text>().text = (i < myList.Count) ? myList[i].PlayerCount + "/" + myList[i].MaxPlayers : "";
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        int roomCount = roomList.Count;
        for (int i = 0; i < roomCount; i++)
        {
            if (!roomList[i].RemovedFromList)
            {
                if (!myList.Contains(roomList[i])) myList.Add(roomList[i]);
                else myList[myList.IndexOf(roomList[i])] = roomList[i];
            }
            else if (myList.IndexOf(roomList[i]) != -1) myList.RemoveAt(myList.IndexOf(roomList[i]));
        }
        MyListRenewal();
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(RoomNameInput.text == "" ? "Room" + Random.Range(0, 100) :
            RoomNameInput.text, new RoomOptions { MaxPlayers = 6 });
        LoadingPanel.SetActive(true);
    }

    public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    public override void OnJoinedRoom()
    {
        LoadingPanel.SetActive(false);
        LobbyPanel.SetActive(false);
        GamePanel.SetActive(true);
        ChatInput.text = "";
        for (int i = 0; i < ChatText.Length; i++)
        {
            ChatText[i].text = "";
        }
        InstantiatePlayerPrefap();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        RoomNameInput.text = ""; CreateRoom();
        LoadingPanel.SetActive(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        LoadingPanel.SetActive(false);
        RoomRenewal();
        ChatRPC("<color=orange>" + newPlayer.NickName + "님이 참가하셨습니다</color>");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        RoomRenewal();
        ChatRPC("<color=orange>" + otherPlayer.NickName + "님이 퇴장하셨습니다</color>");
    }

    void RoomRenewal()
    {
        RoomInfoText.text = PhotonNetwork.CurrentRoom.Name + " / " + PhotonNetwork.CurrentRoom.PlayerCount + "명 / " + PhotonNetwork.CurrentRoom.MaxPlayers + "최대";
    }
    #endregion

    #region Chat
    public void Send()
    {
        PV.RPC("ChatRPC", RpcTarget.All, PhotonNetwork.NickName + " : " + ChatInput.text);
        ChatInput.text = "";
    }

    void EnterSend()
    {
        if ( ChatInput.text != "" && Input.GetKeyDown(KeyCode.Return))
        {
            Send();
        }
    }

    [PunRPC]
    void ChatRPC(string msg)
    {
        bool isInput = false;
        for (int i = 0; i < ChatText.Length; i++)
            if (ChatText[i].text == "")
            {
                isInput = true;
                ChatText[i].text = msg;
                break;
            }
        if (!isInput)
        {
            for (int i = 1; i < ChatText.Length; i++) ChatText[i - 1].text = ChatText[i].text;
            ChatText[ChatText.Length - 1].text = msg;
        }
    }
    #endregion

    #region InstantiatePlayer

    void InstantiatePlayerPrefap()
    {
        var pos = ResponPoint[PhotonNetwork.CurrentRoom.PlayerCount - 1].transform.position;
        PhotonNetwork.Instantiate("Player", pos, Quaternion.identity);
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

public class UserPanelController : MonoBehaviourPunCallbacks
{
    public Text userName;
    public Text chatText;
    public InputField hintInputField;
    public RawImage[] IconImages;

    private void Start()
    {
        userName.text = PhotonNetwork.NickName;
        chatText.text = "";
    }

    void Update()
    {

    }
}

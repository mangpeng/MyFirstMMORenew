using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class ChatScroll : UI_Base
{
    public bool IsChat = false;

    [SerializeField]
    ScrollRect _scrollRect;

    [SerializeField]
    RectTransform _content;

    [SerializeField]
    public TMP_InputField inputField;

    bool _canScrollbarToBottom = false;

    public override void Init()
    {
        _scrollRect.verticalScrollbar.onValueChanged.AddListener(OnScroll);
        inputField.onEndEdit.AddListener(OnEndEdit);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return))
        {
            if(!inputField.gameObject.activeSelf)
            {
                IsChat = true;
                inputField.gameObject.SetActive(true);
                inputField.ActivateInputField();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            inputField.gameObject.SetActive(false);
            IsChat = false;
        }
    }

    public void AddChat(string msg)
    {
        GameObject go = Managers.Resource.Instantiate("UI/Chat/ChatLine");
        TextMeshProUGUI chatObj = go.GetComponent<TextMeshProUGUI>();
        chatObj.text = msg;

        chatObj.transform.SetParent(_content);
        chatObj.transform.localScale = Vector3.one;

        // 화면 꽉 차면 스크롤 하단으로 강제로 내림
        float textTotalHeight = _content.GetComponentsInChildren<Text>().Sum((t) => t.rectTransform.sizeDelta.y);
        if(textTotalHeight >= _scrollRect.GetComponent<RectTransform>().sizeDelta.y)
        {
            _canScrollbarToBottom = true;
            _scrollRect.verticalScrollbar.value = 0;
        }
    }


    #region callback
    void OnEndEdit(string inputMsg)
    {
        if (string.IsNullOrEmpty(inputMsg))
            return;

        inputField.text = string.Empty;
        inputField.ActivateInputField();

        C_Chat chatPacket = new C_Chat();
        chatPacket.ObjectId = Managers.Object.MyPlayer.Id;
        chatPacket.Message = inputMsg;

        Managers.Network.Send(chatPacket);

        Debug.Log($"send message : {chatPacket.Message}");
    }

    void OnScroll(float arg0)
    {
        if(_canScrollbarToBottom)
        {
            _scrollRect.verticalScrollbar.value = 0;
            _canScrollbarToBottom = false;
        }
    }



    #endregion
}

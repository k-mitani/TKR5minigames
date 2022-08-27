using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageBox : MonoBehaviour
{
    public TextMeshProUGUI text;
    public Button oKButton;
    public Button yesButton;
    public Button noButton;
    public Button cancelButton;
    private bool isShowing;
    private MessageBoxResult result;

    private System.Action<MessageBoxResult> callback;

    // Start is called before the first frame update
    void Start()
    {
        oKButton.onClick.AddListener(OnOkClick);
        yesButton.onClick.AddListener(OnYesClick);
        noButton.onClick.AddListener(OnNoClick);
        cancelButton.onClick.AddListener(OnCancelClick);
        gameObject.SetActive(false);
        isShowing = false;
    }

    public void Show(string text, MessageBoxType type = MessageBoxType.Ok, System.Action<MessageBoxResult> callback = null)
    {
        this.text.text = text;
        gameObject.SetActive(true);
        isShowing = true;
        this.callback = callback;

        switch (type)
        {
            case MessageBoxType.Ok:
                oKButton.gameObject.SetActive(true);
                yesButton.gameObject.SetActive(false);
                noButton.gameObject.SetActive(false);
                cancelButton.gameObject.SetActive(false);
                oKButton.Select();
                break;
            case MessageBoxType.OkCancel:
                oKButton.gameObject.SetActive(true);
                yesButton.gameObject.SetActive(false);
                noButton.gameObject.SetActive(false);
                cancelButton.gameObject.SetActive(true);
                oKButton.Select();
                break;
            case MessageBoxType.YesNo:
                oKButton.gameObject.SetActive(false);
                yesButton.gameObject.SetActive(true);
                noButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(false);
                yesButton.Select();
                break;
            case MessageBoxType.YesNoCancel:
                oKButton.gameObject.SetActive(false);
                yesButton.gameObject.SetActive(true);
                noButton.gameObject.SetActive(true);
                cancelButton.gameObject.SetActive(true);
                yesButton.Select();
                break;
        }

    }

    public IEnumerator ShowAsync(string text, MessageBoxType type = MessageBoxType.Ok, System.Action<MessageBoxResult> callback = null)
    {
        var res = default(MessageBoxResult?);
        Show(text, type, result => res = result);
        while (!res.HasValue) yield return new WaitForSeconds(0.05f);
        callback?.Invoke(result);
    }


    private void OnOkClick()
    {
        result = MessageBoxResult.Ok;
        OnClick();
    }
    private void OnYesClick()
    {
        result = MessageBoxResult.Yes;
        OnClick();
    }
    private void OnNoClick()
    {
        result = MessageBoxResult.No;
        OnClick();
    }
    private void OnCancelClick()
    {
        result = MessageBoxResult.Cancel;
        OnClick();
    }
    private void OnClick()
    {
        gameObject.SetActive(false);
        isShowing = false;
        callback?.Invoke(result);
    }
}

public enum MessageBoxType
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel,
}

public enum MessageBoxResult
{
    Ok,
    Yes,
    No,
    Cancel,
}

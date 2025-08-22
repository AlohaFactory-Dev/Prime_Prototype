using Aloha.Coconut;
using Aloha.Coconut.UI;
using TMPro;
using UnityEngine;

public class NicknameFilteringTester : UISlice
{
    [SerializeField] private TMP_InputField nicknameInputField;
    [SerializeField] private TMP_Text resultText;

    private void Start()
    {
        nicknameInputField.onEndEdit.AddListener(OnEndEdit);
    }

    private async void OnEndEdit(string nickname)
    {
        if (string.IsNullOrEmpty(nickname))
        {
            resultText.text = "닉네임을 입력해주세요.";
            return;
        }

        (bool isSuccess, string failureMessage) = await TextFilteringManager.IsValid(nickname);
        resultText.text = isSuccess == false ? failureMessage : "사용 가능한 닉네임입니다.";
    }
}
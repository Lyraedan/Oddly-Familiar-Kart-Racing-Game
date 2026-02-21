using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ModIOAuthUI : MonoBehaviour
{
    public InputField emailInputField;
    public InputField codeInputField;

    public Button requestCode;
    public Button submitCode;

    public async void OnClick_RequestCode()
    {
        ModIOManager.Instance.Email = emailInputField.text;

        bool success = await ModIOManager.Instance.Authenticate();

        if (success)
        {
            emailInputField.gameObject.SetActive(false);
            codeInputField.gameObject.SetActive(true);
            submitCode.gameObject.SetActive(true);
            requestCode.gameObject.SetActive(false);
        }
    }

    public void OnClick_SubmitCode()
    {
        ModIOManager.Instance.SubmitCode(codeInputField.text);
    }

    private void Start()
    {
        emailInputField.gameObject.SetActive(true);
        codeInputField.gameObject.SetActive(false);
        submitCode.gameObject.SetActive(false);
        requestCode.gameObject.SetActive(true);

        requestCode.onClick.AddListener(OnClick_RequestCode);
        submitCode.onClick.AddListener(OnClick_SubmitCode);
    }
}

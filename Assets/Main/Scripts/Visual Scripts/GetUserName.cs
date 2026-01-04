using System;
using TMPro;
using UnityEngine;

public class GetUserName : MonoBehaviour
{
    private TextMeshPro _userNameText;
    void Awake()
    {
        _userNameText = GetComponent<TextMeshPro>();
    }

    private void Start()
    {
        string userName = Environment.UserName;

        if(userName != null)
        _userNameText.text = userName + "\'s Flower Shop";
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.SetInt("Points", 0);
    }
}

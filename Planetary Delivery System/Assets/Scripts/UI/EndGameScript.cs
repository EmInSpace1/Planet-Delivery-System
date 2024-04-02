using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndGameScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI points;

    private void Start()
    {
        int finalPoints = PlayerPrefs.GetInt("Points");
        points.text = finalPoints.ToString();
    }
}

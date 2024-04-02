using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndLevelScript : MonoBehaviour
{
    [SerializeField] private GameObject levelFinishMenu;
    [SerializeField] private TextMeshProUGUI endLevelTime;
    [SerializeField] private TextMeshProUGUI levelPoints;

    private bool finishLevel;

    private float levelTimer;

    private void Update()
    {
        levelTimer += Time.deltaTime;
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "Package" && !finishLevel)
        {
            Debug.Log(other.gameObject.GetComponent<PackageBehaviour>().FinishConditionsMet());
            finishLevel = other.gameObject.GetComponent<PackageBehaviour>().FinishConditionsMet();

            if (finishLevel)
            {
                levelFinishMenu.SetActive(true);
                endLevelTime.text = string.Format("{0:00}:{1:00}", (int)levelTimer / 60, (int)levelTimer % 60);
                levelPoints.text = ((int)(200 - levelTimer)).ToString();

                int currentPoints = PlayerPrefs.GetInt("Points");
                PlayerPrefs.SetInt("Points", (int)(200 - levelTimer) + currentPoints);
            }
        }
    }
}

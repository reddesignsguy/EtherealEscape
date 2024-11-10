using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class IncrementCounter : MonoBehaviour
{

    public TextMeshProUGUI scoreText;

    int score = 0;

    public void Click()
    {
        score = score + 1;
        scoreText.text = score.ToString();
    }

}

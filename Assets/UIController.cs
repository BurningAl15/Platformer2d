using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController _instance;

    [SerializeField] private Image heart1, heart2, heart3;

    [SerializeField] private Sprite heartFull, heartEmpty;
    
    void Awake()
    {
        _instance = this;
        heart1.sprite = heartFull;
        heart2.sprite = heartFull;
        heart3.sprite = heartFull;
    }

    public void UpdateHealthDisplay_Damage(int index)
    {
        switch (index)
        {
            default:
            case 0:
                heart1.sprite = heartEmpty;
                heart2.sprite = heartEmpty;
                heart3.sprite = heartEmpty;
                break;
            case 1:
                heart1.sprite = heartFull;
                heart2.sprite = heartEmpty;
                heart3.sprite = heartEmpty;
                break;
            case 2:
                heart1.sprite = heartFull;
                heart2.sprite = heartFull;
                heart3.sprite = heartEmpty;
                break;
            case 3:
                heart1.sprite = heartFull;
                heart2.sprite = heartFull;
                heart3.sprite = heartFull;
                break;
        }
    }
    public void UpdateHealthDisplay_Cure(int index)
    {
        switch (index)
        {
            case 1:
                heart1.sprite = heartFull;
                break;
            case 2:
                heart2.sprite = heartFull;
                break;
            case 3:
                heart3.sprite = heartFull;
                break;
        }
    }
}

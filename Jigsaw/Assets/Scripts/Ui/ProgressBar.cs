using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image _fillImage;

    public void SetFillAmount(float amount)
    {
        if (amount < 0) amount = 0;
        if (amount > 1) amount = 1;
        _fillImage.fillAmount = amount;

    }
}

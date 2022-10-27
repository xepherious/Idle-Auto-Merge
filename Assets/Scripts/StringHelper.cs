using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LargeNumbers;

public class StringHelper : MonoBehaviour
{
    public static string FormatTime(int time)
    {
        int minutes = time / 60;
        int seconds = time - 60 * minutes;
        return string.Format("{0:0}:{1:00}", minutes, seconds);
    }

    public static string FormatCurrency(AlphabeticNotation currency)
    {
        if (currency.magnitude > 1)
        {
            return currency.ToString();
        }
        else if (currency >= 1000)
        {
            // magic number of 3 is used to get the place for the comma
            return ((int)currency).ToString().Insert(((int)currency).ToString().Length - 3, ",");

        }
        else
        {
            return ((int)currency).ToString();
        }
    }
}

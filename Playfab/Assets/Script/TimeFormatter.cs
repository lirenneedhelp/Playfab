using System;
using UnityEngine;

public class TimeFormatter : MonoBehaviour
{
    public static string FormatTimeDifference(TimeSpan difference)
    {
        if (difference.TotalMinutes < 1)
        {
            return $"{(int)difference.TotalSeconds}s";
        }
        else if (difference.TotalHours < 1)
        {
            return $"{(int)difference.TotalMinutes}m";
        }
        else if (difference.TotalDays < 1)
        {
            return $"{(int)difference.TotalHours}H";
        }
        else
        {
            return $"{(int)difference.TotalDays}D";
        }
    }
}

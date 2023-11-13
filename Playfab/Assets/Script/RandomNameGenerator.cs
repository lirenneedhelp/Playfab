using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomNameGenerator : MonoBehaviour
{
   public static string GenerateName()
   {
        string first = "Guest";
        int randomNumber = Random.Range(10000, 99999);
        string guestName = first + "_" + randomNumber;

        Debug.Log(guestName);

        return guestName;
   }
}

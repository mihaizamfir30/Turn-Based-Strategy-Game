using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameUI
{
    public static class Utils
    {

        public static string IntToString(int number)
        {
            if (number < 1000)
            {
                return number.ToString();
            }

            if (number < 1000 * 1000)
            {
                return ((number / 1000).ToString() + "k");
            }

            return ((number / (1000 * 1000)).ToString() + "kk");
        }
    }
}

/*
================================
Assets for Unity by Makaka Games
================================
 
[Online  Docs -> Updated]: https://makaka.org/unity-assets
[Offline Docs - PDF file]: find it in the package folder.

[Support]: https://makaka.org/support

Copyright © 2025 Andrey Sirota (Makaka Games)
*/

namespace MakakaGames.Publisher.Scoring
{
    public class ScoreBaseControl
    {
        public static string thousandLiteral = "K";
        public static string millionLiteral = "M";

        public static float Truncate(float value, int digits)
        {
            double mult = System.Math.Pow(10.0d, digits);
            double result = System.Math.Truncate(mult * value) / mult;

            return (float) result;
        }

        public static string Round(float value, int digits)
        {   
            if (value >= 1000000)
            {
                return Truncate((value / 1000000f), digits).ToString()
                    + millionLiteral;
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
using System;
using System.Globalization;

namespace GameTimeClient.Tracking.Utility
{
    public static class Util
    {


        /// <summary>
        ///     A case insensitive String.Contains()
        ///     Source: http://stackoverflow.com/a/15464440/846655
        /// </summary>
        /// <param name="paragraph">String to search through</param>
        /// <param name="word">String to search for</param>
        /// <returns>True if the word was found in paragraph</returns>
        public static bool ContainsCaseInsensitive(String paragraph, String word)
        {
            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(
                paragraph, word, CompareOptions.IgnoreCase) >= 0;
        }
    }
}

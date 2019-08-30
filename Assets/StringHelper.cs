namespace Assets
{
    public static class StringHelper
    {
        /// <summary>
        /// get the string between the two given strings
        /// </summary>
        /// <param name="strSource">source you are interested in extract something</param>
        /// <param name="strStart">first string</param>
        /// <param name="strEnd">second string</param>
        /// <returns></returns>
        public static string GetBetween(string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        } // credit https://stackoverflow.com/questions/10709821/find-text-in-string-with-c-sharp

    }
}

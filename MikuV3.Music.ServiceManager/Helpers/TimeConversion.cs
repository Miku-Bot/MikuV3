using System;

namespace MikuV3.Music.ServiceManager.Helpers
{
    public class TimeConversion
    {

        /// <summary>
        /// Convert YTDL's "interesting" upload date string to an actual DateTime object
        /// </summary>
        /// <param name="ytDlDate"></param>
        /// <returns></returns>
        public static DateTime ParseYTDLDate(string ytDlDate)
        {
            if (ytDlDate == null) return DateTime.UtcNow;
            var character = ytDlDate.ToCharArray();
            var date = DateTime.UtcNow;
            //if the upload date is incorrect (might be for a direct link or livestream) it just gets the current time
            try
            {
                date = new DateTime(Convert.ToInt32($"{character[0]}{character[1]}{character[2]}{character[3]}"),
                    Convert.ToInt32($"{character[4]}{character[5]}"),
                    Convert.ToInt32($"{character[6]}{character[7]}"));
            }
            catch { }
            return date;
        }
    }
}

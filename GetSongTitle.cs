using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Av.Utils;

namespace NowPlaying
{

    class GetSongTitle
    {

        #region " Class data "
        protected string m_sUrl = "http://www.gerasfm.lt/muzika/grojarastis";

        /// <summary>
        /// String to find before song title
        /// </summary>
        protected string m_findStarter = "Dabar eteryje";

        /// <summary>
        /// Regex mask to specify song mask
        /// </summary>
        protected string _titleMask = "(<span class=\"mintys\">)([^<]+)(</span>)";

        /// <summary>
        /// Position in regex match
        /// </summary>
        protected int m_songPos = 2;

        protected string m_page = "<th style=\"width: 180px; vertical-align: top; text-align: left;\">\n<br><span class=\"draugai\">&nbsp;&nbsp;Dabar eteryje&nbsp;</span><br>\n<center><span class=\"mintys\">&nbsp;The Cardigans\n - Erase And Rewind\n&nbsp;</span>";

        #endregion

        /// <summary>
        /// Parses and returns Song object for specifi radio station or NULL!
        /// </summary>
        /// <param name="radio"></param>
        public static Song Parse(RadioStationItem radio)
        {
            Song song = new Song();
            song.PlayedAt = DateTime.Now;

            try
            {
                //Log4cs.Log(Importance.Debug, "Radio [{1}]{0}{2}", Environment.NewLine, radio.RadioName, radio.RawStationResponse);
                if(!string.IsNullOrEmpty(radio.RawStationResponse))
                {
                    int offset = 0;
                    if( !string.IsNullOrEmpty(radio.beforeTitle) )
                    {
                        // Use to place song title and artist position
                        offset = radio.RawStationResponse.IndexOf(radio.beforeTitle, StringComparison.CurrentCultureIgnoreCase);
                    }

                    // Get song title
                    Match m = radio.TitleRegex.Match(radio.RawStationResponse, offset);
                    if(m.Success)
                    {
                        song.Title = m.Groups[radio.TitlePositon].ToString();
                        song.Title = StripTags(song.Title).Trim();
                        // TODO: clean HTML entities method
                        song.Title = song.Title.Replace("#039;", "'");
                        
                    }

                    // Get song artist
                    m = radio.ArtistRegex.Match(radio.RawStationResponse, offset);
                    if(m.Success)
                    {
                        song.Artist = m.Groups[radio.ArtistPositon].ToString();
                        song.Artist = StripTags(song.Artist).Trim();
                        song.Artist = song.Artist.Replace("#039;", "'");
                    }
                }

            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error parsing response from radio station!");
                Log4cs.Log(Importance.Debug, ex.ToString());
                song = null;
            }
            return song;
        }

        private static string StripTags( string s )
        {
            StringBuilder sb = new StringBuilder();
            // '<'
            int nOpened = 0;
            // '>'
            int nClosed = 0;
            bool isSim;

            

            for( int i = 0; i < s.Length; i++ )
            {
                switch( s[i] )
                {
                    case '<':
                        nOpened++;
                        break;
                    case '>':
                        // First is not counting
                        if( i != 0 )
                        {
                            nClosed++;
                        }
                        break;
                    case '&':
                        if( s.Substring(i).ToLower().StartsWith("&nbsp;") )
                            i += 5;
                        break;
                    // Check and for \r\n
                    case '\r':
                        if( (i + 1 < s.Length) && (s[i + 1] == '\n') )
                            i++;
                        break;
                    case '\n':
                        break;
                    default:
                        if( nOpened == nClosed )
                        {
                            sb.Append(s[i]);
                        }
                        break;
                }
            }  // END FOR

            return sb.ToString();
        }
        
    }  // END CLASS
}

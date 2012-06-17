using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NowPlaying
{

    public class RadioStationItem
    {
        /// <summary>
        /// Whether we should fetch information from this radio
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// To display in popup
        /// </summary>
        public string RadioName = "";

        private string _url = "";
        /// <summary>
        /// Gets/sets URL to now playing song
        /// </summary>
        public string Url
        {
            get
            {
                int r = new Random().Next(9999);
                return _url.Replace("%RANDOM%", r.ToString());
            }

            set
            {
                _url = value;
            }
        }

        /// <summary>
        /// Regexp mask to extract song title
        /// </summary>
        public string TitleMask = "";

        private Regex _titleRx = null;
        public Regex TitleRegex
        {
            get
            {
                if( _titleRx == null )
                {
                    _titleRx = new Regex(TitleMask);
                }

                return _titleRx;
            }
        }

        /// <summary>
        /// Position of song title in regexp match
        /// </summary>
        public int TitlePositon = 0;

        public string ArtistMask { get; set; }

        public int ArtistPositon { get; set; }

        private Regex _artistRx = null;
        public Regex ArtistRegex
        {
            get
            {
                if(_artistRx == null)
                {
                    _artistRx = new Regex(ArtistMask);
                }

                return _artistRx;
            }
        }

        /// <summary>
        /// Text to locate song title
        /// </summary>
        public string beforeTitle = "";

        /// <summary>
        /// Gets time of last successful check of this station
        /// </summary>
        public DateTime LastCheckedAt { get; set; }

        /// <summary>
        /// Gets/sets raw response from station (just to ease references)
        /// </summary>
        public string RawStationResponse { get; set; }

        private Song _song = null;
        /// <summary>
        /// Gets Song object for current song or NULL!
        /// </summary>
        public Song Song
        {
            get
            {
                if(_song == null)
                {
                    if(!string.IsNullOrEmpty(RawStationResponse))
                    {

                    }
                }

                return _song;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", RadioName, Url);
        }
    }
}

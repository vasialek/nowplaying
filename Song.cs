using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NowPlaying
{

    public class Song
    {
        /// <summary>
        /// Which radio station played this song
        /// </summary>
        public RadioStationItem Radio { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        /// <summary>
        /// ID of track on radio station
        /// </summary>
        public string TrackId { get; set; }

        public string StationUrl { get; set; }

        /// <summary>
        /// Time of song was on radio
        /// </summary>
        public DateTime PlayedAt { get; set; }

        public string FullTitle { get { return string.Format("{0} - {1}", Artist, Title); } }
    }

}

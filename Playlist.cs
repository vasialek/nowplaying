using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Av.Utils;

namespace NowPlaying
{
    class Playlist
    {

        protected List<Song> _arPlayedSongs = new List<Song>();

        /// <summary>
        /// Get last Song on any station
        /// </summary>
        public Song GetLast()
        {
            return _arPlayedSongs.Last();
        }

        /// <summary>
        /// Get last song played on specified radio station
        /// </summary>
        /// <param name="radio">Name of station to get song</param>
        public Song GetLast(string radio)
        {
            return _arPlayedSongs.FindLast(s => s.Radio.RadioName == radio);
        }


        /// <summary>
        /// Adds song to playlist, returns whether song is new
        /// </summary>
        /// <param name="song"></param>
        public bool Add(Song song)
        {
            Song lastSong = GetLast(song.Radio.RadioName);
            if((lastSong == null) || !lastSong.Artist.Equals(song.Artist) || !lastSong.Title.Equals(song.Title))
            {
                Log4cs.Log(Importance.Debug, "Adding new song on {0}: {1} - {2}", song.Radio.RadioName, song.Artist, song.Title);
                _arPlayedSongs.Add(song);
                return true;
            } else
            {
                Log4cs.Log(Importance.Debug, "Song {0} - {1} is currently playing on {2}", song.Artist, song.Title, song.Radio.RadioName);
            }
            return false;
        }

        public Song[] GetListOfLast(string radioName, int limit)
        {
            var songs = (from s in _arPlayedSongs.Cast<Song>()
                         where s.Radio.RadioName.Equals(radioName)
                         select s).Take(limit);
            return songs == null ? null : songs.ToArray();
        }
    }
}

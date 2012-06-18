using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net;
using Av.Utils;

namespace NowPlaying
{
    internal class PageDownloaderThread
    {

        /// <summary>
        /// New song is parsed
        /// </summary>
        public event SongParsedDelegate SongParsed = null;

        /// <summary>
        /// Should we run thread and download
        /// </summary>
        protected bool _isRunning = false;

        protected Thread _thread = null;

        /// <summary>
        /// List of stations to get titles
        /// </summary>
        protected RadioStationItem[] _arRadios = null;

        /// <summary>
        /// Gets whether thread is running (downloading titles)
        /// </summary>
        public bool IsRunning { get { return _isRunning; } }

        /// <summary>
        /// Starts thread, which loads pages and fires event on completion
        /// </summary>
        public void Start( RadioStationItem[] ar )
        {
            _isRunning = true;
            _thread = new Thread(new ParameterizedThreadStart(DownloadThread));
            _thread.Start(ar);
        }

        /// <summary>
        /// Stops thread
        /// </summary>
        public void Stop()
        {
            Log4cs.Log("Stopping downloader thread...");
            _isRunning = false;
        }

        /// <summary>
        /// Dowloads pages which are specified and fires event
        /// </summary>
        protected void DownloadThread( object obj )
        {
            // Use this to stop in case there are too many errors
            int errors = 0;

            int[] arEventsToWait = {
                (int)AEvents.EventsId.GotData
                , (int)AEvents.EventsId.Quit
            };
            //WebClient[] arWeb = null;
            WebClient wc = new WebClient();
            //wc.DownloadDataCompleted += new DownloadDataCompletedEventHandler(OnPageDownloaded);


            if( obj != null )
            {
                _arRadios = (RadioStationItem[])obj;
                Log4cs.Log("Got {0} radio stations to proceed...", _arRadios.Length);
            }

            // Which station to proceed
            int currentStation = 0;
            while( _isRunning )
            {
                try
                {
                    if( currentStation >= _arRadios.Length )
                    {
                        currentStation = 0;
                    }
                    Log4cs.Log(Importance.Debug, "Downloading station #{0} from {1}...", currentStation, _arRadios[currentStation].Url);
                    //try
                    //{
                    _arRadios[currentStation].RawStationResponse = wc.DownloadString(_arRadios[currentStation].Url);
                    _arRadios[currentStation].LastCheckedAt = DateTime.Now;
                    Song song = GetSongTitle.Parse(_arRadios[currentStation]);
                        if(song != null)
                        {
                            // Set station where song was played
                            song.Radio = _arRadios[currentStation];

                            // Check if we need to notify about new song
                            if(SongParsed != null)
                            {
                                SongParsed(_arRadios[currentStation], song);
                            }
                        }

                    //} catch(Exception ex)
                    //{
                    //    Log4cs.Log(Importance.Error, "Error downloading song from {0}!", _arRadios[i].Url);
                    //    Log4cs.Log(Importance.Debug, ex.ToString());
                    //}
                // Everything goes ok
                errors = 0;

                } catch(Exception ex)
                {
                    Log4cs.Log(Importance.Error, "Error downloading song title!");
                    Log4cs.Log(Importance.Debug, ex.ToString());
                    errors++;

                    if(errors > 5)
                    {
                        // 5 errors continuosly, sleep for 30 second and wait for Quit
                        if(Common.WaitMyEvents(ref AEvents.arEvents, arEventsToWait, 30000, true) == (int)AEvents.EventsId.Quit)
                        {
                            Log4cs.Log(Importance.Warning, "Got 5 continuosly errors and then Quit signal!");
                            _isRunning = false;
                        }
                    }else if( errors > 10 )
                    {
                        // 5 errors continuosly, sleep for 120 second and wait for Quit
                        if(Common.WaitMyEvents(ref AEvents.arEvents, arEventsToWait, 120000, true) == (int)AEvents.EventsId.Quit)
                        {
                            Log4cs.Log(Importance.Warning, "Got 10 continuosly errors and then Quit signal!");
                            _isRunning = false;
                        }
                    }
                } finally
                {
                    // Wait for exit signal or timeout if all radios are fetched
                    if( currentStation == _arRadios.Length - 1 )
                    {
                        switch(Common.WaitMyEvents(ref AEvents.arEvents, arEventsToWait, 30000, true))
                        {
                            //case (int)AEvents.EventsId.GotData:
                            //    Log4cs.Log(Importance.Debug, "Got data from radio station");
                            //    break;
                            case (int)AEvents.EventsId.Quit:
                                Log4cs.Log("Got signal to stop, exiting downloder thread...");
                                _isRunning = false;
                                break;
                        }
                    }

                    // Go to next station anyway
                    currentStation++;

                }
            }  // END WHILE ( continue download )
        }

        void OnPageDownloaded( object sender, DownloadDataCompletedEventArgs e )
        {
            try
            {
                Log4cs.Log(e.UserState.GetType().ToString());
            } catch( Exception ex )
            {
                Log4cs.Log(Importance.Debug, ex.ToString());
            }
        }

    }
}

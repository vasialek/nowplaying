using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Threading;
using Av;
using Av.Utils;

namespace NowPlaying
{
    class Settings
    {

        #region " Name / Version "
        public static string Name = "iNowPlaying";
        public static string Version = "0.7.14";
        public static string NameVersion
        {
            get { return string.Format("{0} (v{1})", Name, Version); }
        }
        #endregion


        /// <summary>
        /// Events which need attention in whole program, i.e. Exit
        /// </summary>
        public class Events
        {
            /// <summary>
            /// How many events are in list
            /// </summary>
            public const int Count = 1;

            /// <summary>
            /// Named ID of events, to help manage them
            /// </summary>
            public class EventId
            {
                public const int Exit = 0;
            }
            //public enum EventId { Exit = 0 };

            /// <summary>
            /// Array of events which could be raised
            /// </summary>
            public static ManualResetEvent[] ArEvents = null;

            static Events()
            {
                ArEvents = new ManualResetEvent[Events.Count];
                ArEvents[EventId.Exit] = new ManualResetEvent(false);
            }

            /// <summary>
            /// Converts ID of event to string name
            /// </summary>
            public static string IdToName(int id)
            {
                switch(id)
                {
                    case EventId.Exit:
                        return "Exit";
                }

                return id.ToString();
            }
        }

        private static RadioStationItem[] _arRadioStations = null;

        /// <summary>
        /// Gets array of radio stations or NULL!
        /// </summary>
        public static RadioStationItem[] RadioStations { get { return _arRadioStations; } }

        public static void Load()
        {
            Log4cs.Log("Loading settings...");
            _arRadioStations = LoadRadioList(Common.GetPath() + "stations.xml");
        }

        /// <summary>
        /// Reads and parses config file and creates task for each radio
        /// </summary>
        /// <param name="xmlConfigFile"></param>
        private static RadioStationItem[] LoadRadioList(string xmlConfigFile)
        {
            List<RadioStationItem> arStationsItems = new List<RadioStationItem>();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlConfigFile);
                XmlElement xml = xmlDoc.DocumentElement;
                //XmlNode node = xml.ChildNodes[0];

                if(xml.ChildNodes.Count > 0)
                {
                    //arStationsItems = new RadioStationItem[xml.ChildNodes.Count];
                    bool isActive = false;

                    //Log4cs.Log("There are {0}",);
                    foreach(XmlNode radioNode in xml.ChildNodes)
                    {
                        RadioStationItem radio = new RadioStationItem();
                        //Log4cs.Log(radioNode.Name + ": " + radioNode.ChildNodes[0].Value);
                        foreach(XmlNode var in radioNode.ChildNodes)
                        {
                            //Log4cs.Log(var.Name + ": " + var.ChildNodes[0].Value);
                            switch(var.Name.ToLower())
                            {
                                case "isactive":
                                    bool.TryParse(var.ChildNodes[0].Value, out isActive);
                                    radio.IsActive = isActive;
                                    break;
                                case "title":
                                    radio.RadioName = var.ChildNodes[0].Value;
                                    break;
                                case "titlemask":
                                    radio.TitleMask = var.ChildNodes[0].Value;
                                    break;
                                case "titleposition":
                                    radio.ArtistPositon = Int32.Parse(var.ChildNodes[0].Value);
                                    break;
                                case "artistemask":
                                    radio.ArtistMask = var.ChildNodes[0].Value;
                                    break;
                                case "artistposition":
                                    radio.TitlePositon = Int32.Parse(var.ChildNodes[0].Value);
                                    break;
                                case "url":
                                    radio.Url = var.ChildNodes[0].Value;
                                    break;
                                case "beforetitle":
                                    if( var.HasChildNodes )
                                    {
                                        radio.beforeTitle = var.ChildNodes[0].Value;
                                    }
                                    break;
                                default:
                                    break;
                            }  // END SWITCH
                        }  // END FOREACH ( list all item parameter )

                        if( radio.IsActive )
                        {
                            arStationsItems.Add(radio);
                        }

                    }  // END FOREACH ( list all items )
                }  // END IF

                if(arStationsItems != null)
                {
                    Log4cs.Log("Radio station list:");
                    for(int i = 0; i < arStationsItems.Count; i++)
                    {
                        Log4cs.Log("\t{0}", arStationsItems[i].ToString());
                    }
                }
            } catch(Exception ex)
            {
                Log4cs.Log("Error parsing " + xmlConfigFile, Importance.Error);
                Log4cs.Log(ex.ToString(), Importance.Debug);
            }

            return arStationsItems.Count > 0 ? arStationsItems.ToArray() : null;
        }

        public static string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("Settings for {0} (v{1})", Name, Version);
            sb.AppendLine();

            return sb.ToString();
        }

    }
}

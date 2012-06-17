using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NowPlaying
{

    /// <summary>
    /// Class containing public events to control events: data arrived, operation completes, program quits, ...
    /// </summary>
    class AEvents
    {
        public enum EventsId { Quit = 0, GotData }

        public static ManualResetEvent[] arEvents = 
        { 
            new ManualResetEvent(false)
            , new ManualResetEvent(false)
            //, new ManualResetEvent(false)
            //, new ManualResetEvent(false)
            //, new ManualResetEvent(false) 
        };

        public static string ToString(int eventId)
        {
            switch(eventId)
            {
                case (int)AEvents.EventsId.Quit:
                    return "Quit (" + AEvents.EventsId.Quit + ")";
                case (int)AEvents.EventsId.GotData:
                    return "GotData (" + AEvents.EventsId.GotData + ")";
                case ManualResetEvent.WaitTimeout:
                    return string.Format("WaitTimeout ({0})", eventId);
                //case 2:
                //    return "ProxyCommand (2)";
                //case 3:
                //    return "OperationComplete (3)";
                //case 4:
                //    return "Terminate (4)";
            }

            return string.Format("Unknown ({0})", eventId);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NowPlaying
{

    /// <summary>
    /// Class to display context menu
    /// </summary>
    class MyMenu
    {
        /// <summary>
        /// Contains names of menu items
        /// </summary>
        protected static string[] m_arNames = null;

        /// <summary>
        /// How many menu item
        /// </summary>
        public const int Size = 5;

        /// <summary>
        /// Position in menu list
        /// </summary>
        public class Position
        {
            public const int Start = 0;
            public const int Stop = 1;
            //public const int LastErrors = 2;
            public const int Version = 2;
            public const int Reload = 3;
            public const int Exit = 4;
        }

        public static int[] GetMenuItems()
        {
            return new[] { Position.Version, Position.Exit };
        }

        /// <summary>
        /// Returns name of item by position
        /// </summary>
        /// <param name="pos">Position in menu. Should be [0; Size)</param>
        /// <returns></returns>
        public static string ToName(int pos)
        {
            // Like constructor - creates array on the first use
            if(m_arNames == null)
            {
                m_arNames = new string[Size];
                m_arNames[Position.Start] = "Start";
                m_arNames[Position.Stop] = "Stop";
                //m_arNames[Position.LastErrors] = "Last errors";
                m_arNames[Position.Version] = "Version";
                m_arNames[Position.Reload] = "Reload settings";
                m_arNames[Position.Exit] = "Exit";
            }

            if((pos >= 0) && (pos < Size))
            {
                return m_arNames[pos];
            }

            return pos.ToString();
        }

    }

}

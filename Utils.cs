using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.IO;
using System.Threading;

namespace Av.Utils
{

    public class Common
    {
        /// <summary>
        /// Removes any spaces or zerroes from beggining of IP, i.e. "192.168.048.011" -> "192.168.48.11"
        /// </summary>
        /// <param name="sIp">Dotted IP address</param>
        /// <returns></returns>
        public static string NormalizeIp(string sIp)
        {
            string[] arIp = sIp.Split(new char[] { '.' });
            if(arIp != null)
            {
                sIp = "";
                // Remove all spaces and zerroes
                for(int i = 0; i < arIp.Length; i++)
                    arIp[i] = arIp[i].TrimStart(new char[] { '0', ' ' });

                sIp = string.Join(".", arIp);
            }

            return sIp;
        }

        /// <summary>
        /// Gets path of executable and ensures its ends w/ '\'
        /// </summary>
        public static string GetPath()
        {
            string aPath = "";
            try
            {
                System.Reflection.Module[] modules = System.Reflection.Assembly.GetExecutingAssembly().GetModules();
                aPath = System.IO.Path.GetDirectoryName(modules[0].FullyQualifiedName);
                if((aPath != "") && (aPath[aPath.Length - 1] != '\\')) aPath += '\\';
            } catch(Exception) { }

            return aPath;
        }

        /// <summary>
        /// Replace and returns '{' w/ '{{' and '}' w/ '}}'
        /// </summary>
        /// <returns></returns>
        public static string EscapeBraces(string str)
        {
            if(!string.IsNullOrEmpty(str))
            {
                str = str.Replace("{", "{{");
                str = str.Replace("}", "}}");
            }

            return str;
        }


        /// <summary>
        /// Waits for any specified event
        /// </summary>
        /// <param name="arEvents">Events that could be</param>
        /// <param name="myEvents">One of events to wait</param>
        /// <param name="timeoutMs">Timeout</param>
        /// <returns>Id of event or WaitTimeout</returns>
        public static int WaitMyEvents(ref ManualResetEvent[] arEvents, int[] arEventsToWait, int timeoutMs, bool resetEvent)
        {
            int nEventId = WaitHandle.WaitTimeout;
            // How long we are waiting
            int waitingTimeMs = 0;
            DateTime start;

            while( waitingTimeMs < timeoutMs )
            {
                start = DateTime.Now;
                //Log4cs.Log("Waiting for " + (timeoutMs - waitingTimeMs) + " ms.");
                nEventId = WaitHandle.WaitAny(arEvents, timeoutMs - waitingTimeMs, false);
                if( nEventId != WaitHandle.WaitTimeout )
                {
                    // Check if receive desired event
                    if(Array.IndexOf(arEventsToWait, nEventId) >= 0)
                    {

                        if( resetEvent )
                        {
                            Log4cs.Log(Importance.Debug, "Reseting event with ID: {0}", nEventId);
                            arEvents[nEventId].Reset();
                        }
                        return nEventId;
                    } else
                    {
                        waitingTimeMs += ((TimeSpan)(DateTime.Now - start)).Milliseconds;
                    }
                } else
                {
                    // Stop waiting and return
                    waitingTimeMs = timeoutMs;
                }
            }  // END WHILE

            return nEventId;
        }
    }

    #region " RegHelper - to get, delete and update values and keys in Registry "
    class RegHelper
    {

        public static object GetValue(string path, string name)
        {
            return GetValue(Registry.CurrentUser, path, name);
        }

        public static object GetValue(RegistryKey rootKey, string path, string name)
        {
            RegistryKey key = null;
            object value = "";

            try
            {
                //Log4cs.Log("Getting {0} from {1}", path + name, rootKey);
                key = rootKey.OpenSubKey(path);
                value = key.GetValue(name);
            } catch(Exception ex)
            {
                Log4cs.Log("Error getting " + name + " from " + path, Importance.Error);
                Log4cs.Log(ex.ToString(), Importance.Debug);
            }

            try
            {
                key.Close();
                rootKey.Close();
            } catch(Exception) {/* Log4cs.Log(ex.ToString()); */}

            return value;
        }

        /// <summary>
        /// Returns all values from registry path, using specified Registry key
        /// </summary>
        /// <param name="path">path to registry key beginning </param>
        /// <returns>Dictionary w/ values or empty</returns>
        public static Dictionary<string, object> GetAllValues(RegistryKey rootKey, string path)
        {
            Dictionary<string, object> arValues = new Dictionary<string, object>();
            //KeyValuePair<string, object>[] arValues1 = null;
            string[] arKeys = null;
            //string subPath = "";
            RegistryKey key = null;
            //RegistryKey rootKey = Registry.CurrentUser;

            if(path == null)
            {
                Log4cs.Log("No path specified for Registry!", Importance.Error);
                return arValues;
            }

            if(path.StartsWith("\\"))
                path = path.Substring(1);

            Log4cs.Log("Get values from: {0}\\{1}", rootKey.ToString(), path);

            try
            {
                key = rootKey.OpenSubKey(path);
                arKeys = key.GetValueNames();
                Log4cs.Log(Importance.Debug, "Got " + arKeys.Length + " values in {0}\\{1}", rootKey.ToString(), path);

                if(arKeys.Length > 0)
                {
                    for(int i = 0; i < arKeys.Length; i++)
                    {
                        try
                        {
                            arValues[arKeys[i]] = key.GetValue(arKeys[i]).ToString();
                        } catch(Exception)
                        {
                            Log4cs.Log(Importance.Warning, "Duplicate key [" + arKeys[i] + "]");
                        }

                        //Log4cs.Log("\t" + arKeys[i] + "->" + key.GetValue( arKeys[i] ).ToString() );
                    }
                }  // END IF

            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error listing " + rootKey.ToString() + "\\" + path);
                Log4cs.Log(Importance.Debug, ex.ToString());
                //return m_arValues;
            }

            try
            {
                key.Close();
                rootKey.Close();
            } catch(Exception) { }

            return arValues;
        }

        public static Dictionary<string, Dictionary<string, object>> GetAllSubKeysValues(RegistryKey rootKey, string path)
        {
            Dictionary<string, Dictionary<string, object>> arDict = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, object> arValues = new Dictionary<string, object>();
            //string[] arKeys = null;
            //string subPath = "";
            RegistryKey key = null;

            if(path == null)
            {
                Log4cs.Log("No path specified for Registry!", Importance.Error);
                return null;
            }

            if(path.StartsWith("\\"))
                path = path.Substring(1);

            Log4cs.Log("Get subkeys from: {0}\\{1}", rootKey.ToString(), path);

            try
            {
                key = rootKey.OpenSubKey(path);
                string[] arSubKeys = key.GetSubKeyNames();
                for(int i = 0; (arSubKeys != null) && (i < arSubKeys.Length); i++)
                {
                    //Log4cs.Log(Importance.Debug, "\t" + arSubKeys[i]);
                    arDict[arSubKeys[i]] = RegHelper.GetAllValues(rootKey, path + "\\" + arSubKeys[i]);
                }

            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error listing " + rootKey.ToString() + "\\" + path);
                Log4cs.Log(Importance.Debug, ex.ToString());
                arDict = null;
            }

            try
            {
                key.Close();
                rootKey.Close();
            } catch(Exception) { }

            return arDict;
        }


        public static void Save(string path, string name, string value)
        {
            Save(Registry.CurrentUser, path, name, value);
        }

        public static void Save(RegistryKey rootKey, string path, string name, string value)
        {
            RegistryKey key = null;

            try
            {
                //Log4cs.Log("Getting {0} from {1}", path + name, rootKey);
                key = rootKey.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
                key.SetValue(name, value);
            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error saving {0} in [{1}]{2}!", name, rootKey, path);
                Log4cs.Log(ex.ToString(), Importance.Debug);
            }

            try
            {
                key.Close();
                rootKey.Close();
            } catch(Exception) { }

        }

        public static void Save(RegistryKey rootKey, string path, string name, int value)
        {
            RegistryKey key = null;
            object obj = (object)value;

            try
            {
                //Log4cs.Log("Getting {0} from {1}", path + name, rootKey);
                key = rootKey.CreateSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
                key.SetValue(name, value);
            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error saving {0} in [{1}]{2}!", name, rootKey, path);
                Log4cs.Log(ex.ToString(), Importance.Debug);
            }

            try
            {
                key.Close();
                rootKey.Close();
            } catch(Exception) { }

        }


        public static void Delete(RegistryKey rootKey, string path, string name)
        {
            RegistryKey key = null;

            try
            {
                //Log4cs.Log("Getting {0} from {1}", path + name, rootKey);
                key = rootKey.OpenSubKey(path, RegistryKeyPermissionCheck.ReadWriteSubTree);
                key.DeleteValue(name, true);
            } catch(Exception ex)
            {
                Log4cs.Log(Importance.Error, "Error deleting {0} in [{1}]{2}!", name, rootKey, path);
                Log4cs.Log(ex.ToString(), Importance.Debug);
            }

            try
            {
                key.Close();
                rootKey.Close();
            } catch(Exception) { }
        }
    }  // END CLASS RegHelper

    #endregion


    #region " Log4cs - class for logging "

    public enum Importance { Debug, Info, Warning, Error, No, HideThis };

    /// <summary>
    /// Class for simple logging
    /// </summary>
    public class Log4cs
    {

        protected static string m_fileName = "Log_{0}.log";
        protected static string m_logsDir = @"c:\logs\";

        protected static object m_monitor = "xx";

        /// <summary>
        /// Name of log file. Use {0} to place date in file name, i.e. logs_{0}.log
        /// </summary>
        public static string FileName
        {
            get { return m_fileName; }
            set
            {
                if(!value.Contains("{0}"))
                    throw new FormatException("Log filename must contains '{0}'!");

                m_fileName = value;
            }
        }

        /// <summary>
        /// Gets/sets directory where logs are
        /// </summary>
        public static string Dir
        {
            get { return m_logsDir; }
            set
            {
                m_logsDir = value;
                if(!m_logsDir.EndsWith("\\"))
                    m_logsDir += "\\";
            }
        }


        /// <summary>
        /// Time in log file, set to "" if no time is needed
        /// </summary>
        public static string TimeFmt = "HH:mm:ss";
        /// <summary>
        /// Do we need to output to console
        /// </summary>
        public static bool OutputToConsole = false;

        public static void Log(string msg, params object[] Args)
        {
            Log(Importance.Info, msg, Args);
        }


        public static void Log(Importance logLevel, string msg, params object[] Args)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(Dir);
            StringBuilder sb = new StringBuilder();
            StringBuilder strMsg = new StringBuilder();
            string filename = "";
            StreamWriter streamW = null;

            if(!dirInfo.Exists)
            {
                try
                {
                    dirInfo.Create();
                } catch(Exception)
                {
                    System.Console.WriteLine("Error creating '{0}' directory!", Dir);
                    return;
                }
            }  // END IF

            try
            {
                sb.AppendFormat(FileName, DateTime.Now.ToString("yyyyMMdd"));
                filename = dirInfo.FullName;

                // Ensures that ends w/ '\'
                if(!filename.EndsWith("\\"))
                    filename += "\\";

                filename += sb.ToString();
                //            System.Console.WriteLine( "Log file name: " + filename );

            } catch(Exception)
            {
                System.Console.WriteLine("Bad log file name '{0}'!", FileName);
            }

            if(TimeFmt.Length > 0)
                strMsg.Append(DateTime.Now.ToString(TimeFmt)).Append(" ");

            if(logLevel != Importance.No)
            {
                if(logLevel == Importance.HideThis)
                {
                    strMsg.Append("[*** ").Append(logLevel.ToString().ToUpper()).Append(" ***] ");
                } else
                {
                    strMsg.Append("[").Append(logLevel.ToString().ToUpper()).Append("] ");
                }
            }

            //strMsg += " " + msg + Environment.NewLine;
            strMsg.AppendFormat(msg, Args).Append(Environment.NewLine);

            if(OutputToConsole)
                System.Console.Write(strMsg.ToString());

            try
            {
                lock(m_monitor)
                {
                    streamW = new StreamWriter(filename, true);
                    streamW.AutoFlush = true;
                    streamW.Write(strMsg);
                }
            } catch(Exception ex)
            {
                System.Console.WriteLine("Error writing to '{0}' file!", filename);
                System.Console.WriteLine(ex.ToString());
            }

            if(streamW != null)
                streamW.Close();
        }

        /*
         *  Log levels for events log 
         */
        private static System.Diagnostics.EventLogEntryType[] m_arLogLevels = 
        { 
            System.Diagnostics.EventLogEntryType.Information,
            System.Diagnostics.EventLogEntryType.Warning,
            System.Diagnostics.EventLogEntryType.Error 
        };


        /// <summary>
        /// Logs event in Windows application events
        /// </summary>
        public static void LogEvent(string title, string text, int level, int code)
        {
            try
            {
                if(!System.Diagnostics.EventLog.SourceExists(title))
                    System.Diagnostics.EventLog.CreateEventSource(title, "Application");
                System.Diagnostics.EventLog.WriteEntry(title, text, m_arLogLevels[level], code);
            } catch
            {

            }
        }

    }  // END CLASS Log4cs 
    #endregion

}

# shLog
use log4net

# How to use
 **** Download log4net from Nugget.
 
using shLog;
public enum Log

{

    first = 0,
    
    second,
    
    third,
    
}
 
LogProcessor m_Log = new LogProcessor();

m_Log.Init("LogPath", "MyLog", 1, "firstLogName", "second", "third");

// Write Log

m_Log.Write((int)Log.first, "Log Message");

// Close
m_Log.Close();

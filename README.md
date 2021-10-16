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

m_Log.Init("LogPath", "MyLog", 1, System.Enum.GetNames( typeof( Log ) ););

// Write Log

m_Log.Write((int)Log.first, "Log Message");

m_Log.Write((int)Log.third, "333 log");

// Close
m_Log.Close();

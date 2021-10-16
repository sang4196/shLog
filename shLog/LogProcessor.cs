/*using Common.Logging;*/

using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using System.Text;
using System.IO;

namespace shLog
{
    public class LogProcessor
    {
        #region Member

        private static int m_RefCount = 0;

        private static readonly object m_InstanceLock = new object();

        // Log 데이터를 가공할 Text 버퍼
        private StringBuilder m_LogToTextBuffer = new StringBuilder(1500);

        private static bool m_IsInited = false;

        public List<string> m_ListLogName = new List<string>();
        private ILog[] m_ArrLog = null;

        private string m_LogPath = "";
        private string m_LogName = "";
        private int m_Period = 0;

        private Thread m_ThreadMain = null;
        private bool m_bThared = false;

        public LogProcessor()
        {
        }

        //--------------------------------------------------------------------------------
        ~LogProcessor()
        {
            // ReleaseInstance(); 소멸자로 호출 되면 Release 시킬려고 했더니 WaitOne 에걸림
        }

        #endregion Member


        #region Method
        /// <summary>
        /// Init
        /// </summary>
        /// <param name="_sLogPath">로그 경로 Log Path</param>
        /// <param name="_sSystemName">LogProcessor Name</param>
        /// <param name="_nPeriod">보관 주기 storage period day</param>
        /// <param name="args">로그 이름들 Logs name</param>
        /// <returns></returns>
        public bool Init(string _sLogPath, string _sSystemName, int _nPeriod, params object[] args)
        {
            if (m_IsInited == true)
                return false;

            string LogPath = _sLogPath;
            string LogName = _sSystemName;


            if (string.IsNullOrWhiteSpace(LogPath))
                LogPath = "DefaultLogPath\\";

            if (string.IsNullOrWhiteSpace(LogName))
                LogName = "DefaultSystemName";

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is string)
                    m_ListLogName.Add(args[i] as string);
            
            }

            m_ArrLog = new ILog[m_ListLogName.Count];

            m_bThared = true;
            m_ThreadMain = new Thread(() => ThreadScheduler());
            m_ThreadMain.Start();

            return LoggerInit(LogPath, LogName, _nPeriod);
        }

        public void Close()
        {
            m_bThared = false;
            if (m_ThreadMain != null)
            {
                m_ThreadMain.Join();
                m_ThreadMain = null;
            }
        }

        public void ChangePeriod(int _nPeriod)
        {
            m_Period = _nPeriod;
        }

        private void ThreadScheduler()
        {
            while (m_bThared)
            {
                try
                {
                    Thread.Sleep(100);

                    if (m_IsInited)
                        DeleteFile(m_LogPath, m_Period);
                }
                catch (ThreadAbortException ex)
                {
                    Write(0, "[ERROR] [EXCEPTION] LogProcessor : ThreadScheduler(ThreadAbortException)\r\n" + ex.ToString());

                    Thread.ResetAbort();
                    break;
                }
                catch (Exception ex)
                {
                    Write(0, "[ERROR] [EXCEPTION] LogProcessor : ThreadScheduler()\r\n" + ex.ToString());
                }
            }
        }

        private void DeleteFile(string sPath, int nDayPeriod)
        {
            DirectoryInfo TargetDirectory = new DirectoryInfo(sPath);
            if (TargetDirectory.Exists == true)
            {
                DirectoryInfo[] directories = TargetDirectory.GetDirectories();

                DateTime dtTarget = DateTime.UtcNow.AddDays(-nDayPeriod);

                foreach (DirectoryInfo di in directories)
                {
                    if (di.CreationTimeUtc < dtTarget)
                    {
                        DeleteDirectory(di);
                    }
                }
            }
        }

        private void DeleteDirectory(DirectoryInfo Direc)
        {
            foreach (DirectoryInfo di in Direc.GetDirectories())
            {
                DeleteDirectory(di);
            }

            foreach (FileInfo fi in Direc.GetFiles())
            {
                fi.Delete();
            }

            Directory.Delete(Direc.FullName);
        }

        //--------------------------------------------------------------------------------
        private bool LoggerInit(string _Path, string _SystemName, int _nPeriod)
        {
            for (int i = 0; i < m_ArrLog.Length; i++)
            {
                m_ArrLog[i] = Tracer.GetDyamicLogger(m_ListLogName[i], _Path);
            }

            m_LogPath = _Path;
            m_LogName = _SystemName;
            m_Period = _nPeriod;

            m_IsInited = true;

            return true;
        }

        //--------------------------------------------------------------------------------
        public string GetLogPath()
        {
            return m_LogPath;
        }

        //--------------------------------------------------------------------------------
        public string GetLogName()
        {
            return m_LogName;
        }

        //--------------------------------------------------------------------------------
        public void ReleaseInstance()
        {
            lock (m_InstanceLock)
            {
                if (m_RefCount > 0)
                    m_RefCount--;

                if (m_RefCount == 0)
                    Terminate();
            }
        }

        //--------------------------------------------------------------------------------
        public void Terminate()
        {
            lock (m_InstanceLock)
            {
            }
        }

        //--------------------------------------------------------------------------------

        public void Write(int _nIdx, string _sMsg)
        {
            if (m_ArrLog != null && m_ArrLog.Length >= _nIdx)
            {
                m_ArrLog[_nIdx].Info(_sMsg);
            }
        }
        #endregion Method
    }
}
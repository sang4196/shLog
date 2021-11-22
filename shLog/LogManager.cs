/*using Common.Logging;*/

using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using System.Text;
using System.IO;

namespace shLog
{
    public class LogManager
    {
        #region Member

        private static readonly object m_InstanceLock = new object();
        
        private static bool m_IsInited = false;

        public List<string> m_ListLogName = new List<string>();
        private ILog[] m_ArrLog = null;

        private string m_sLogPath = "";
        private string m_sLogName = "";
        private int m_nPeriod = 0;

        private Thread m_ThreadMain = null;
        private bool m_bThared = false;

        public LogManager()
        {
        }

        //--------------------------------------------------------------------------------
        ~LogManager()
        {
        }

        #endregion Member


        #region Method
        /// <summary>
        /// Init
        /// </summary>
        /// <param name="_sLogPath">로그 경로 Log Path</param>
        /// <param name="_sProcessName">LogProcessor Name</param>
        /// <param name="_nPeriod">보관 주기 storage period day</param>
        /// <param name="args">로그 이름들 Logs name</param>
        /// <returns></returns>
        public bool Init(string _sLogPath, string _sProcessName, int _nPeriod, params string[] args)
        {
            if (m_IsInited == true)
                return false;
            
            if (string.IsNullOrWhiteSpace(_sLogPath))
                _sLogPath = "DefaultLogPath\\";

            if (string.IsNullOrWhiteSpace(_sProcessName))
                _sProcessName = "DefaultProcessName";

            for (int i = 0; i < args.Length; i++)
            {
                m_ListLogName.Add(args[i]);
            }

            m_ArrLog = new ILog[m_ListLogName.Count];

            m_bThared = true;
            m_ThreadMain = new Thread(() => ThreadDeleteFile());
            m_ThreadMain.Start();

            return LoggerInit(_sLogPath, _sProcessName, _nPeriod);
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
            m_nPeriod = _nPeriod;
        }

        private void ThreadDeleteFile()
        {
            while (m_bThared)
            {
                try
                {
                    Thread.Sleep(100);

                    if (m_IsInited)
                        DeleteFile(m_sLogPath, m_nPeriod);
                }
                catch (ThreadAbortException ex)
                {
                    Write(0, "[ERROR] LogProcessor : ThreadDeleteFile(ThreadAbortException)\r\n" + ex.ToString());

                    Thread.ResetAbort();
                    break;
                }
                catch (Exception ex)
                {
                    Write(0, "[ERROR] LogProcessor : ThreadDeleteFile()\r\n" + ex.ToString());
                }
            }
        }

        private void DeleteFile(string _sPath, int _nDayPeriod)
        {
            DirectoryInfo TargetDirectory = new DirectoryInfo(_sPath);
            if (TargetDirectory.Exists == true)
            {
                DirectoryInfo[] directories = TargetDirectory.GetDirectories();

                DateTime dtTarget = DateTime.UtcNow.AddDays(-_nDayPeriod);

                foreach (DirectoryInfo di in directories)
                {
                    if (di.CreationTimeUtc < dtTarget)
                    {
                        DeleteDirectory(di);
                    }
                }
            }
        }

        private void DeleteDirectory(DirectoryInfo _Direc)
        {
            foreach (DirectoryInfo di in _Direc.GetDirectories())
            {
                DeleteDirectory(di);
            }

            foreach (FileInfo fi in _Direc.GetFiles())
            {
                fi.Delete();
            }

            Directory.Delete(_Direc.FullName);
        }

        private bool LoggerInit(string _sPath, string _sProcessName, int _nPeriod)
        {
            for (int i = 0; i < m_ArrLog.Length; i++)
            {
                m_ArrLog[i] = Tracer.GetDyamicLogger(m_ListLogName[i], _sPath);
            }

            m_sLogPath = _sPath;
            m_sLogName = _sProcessName;
            m_nPeriod = _nPeriod;

            m_IsInited = true;

            return true;
        }

        public string GetLogPath()
        {
            return m_sLogPath;
        }

        public string GetLogName()
        {
            return m_sLogName;
        }
        
        public void Terminate()
        {
            lock (m_InstanceLock)
            {
            }
        }

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
    using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analysis_Server.Manager.AnalysisManager;
using Analysis_Server.Manager.DBManager;
using Analysis_Server.Structure.Analysis;
using Analysis_Server.THD;

namespace Analysis_Server.Manager.SenderManager
{
    public class SenderThreadManagerClass : ISenderThreadManagerClass
    {
        private ConnectThreadClass m_connectThreadClass;
        private List<SenderThreadClass> m_senderThreadClasses;
        private Thread m_Thread;
        public SenderThreadManagerClass(IDBManagerClass dBManagerClass)
        {
            m_connectThreadClass = new ConnectThreadClass(dBManagerClass.GetServerInfosClass().serverPort);
            m_connectThreadClass.SetCallback(AddSenderSession);
            m_senderThreadClasses = new List<SenderThreadClass>();

            m_Thread = new Thread(CheckSenderThread);
            m_Thread.Start();

            m_connectThreadClass.Run();
        }

        private void CheckSenderThread()
        {
            int length = m_senderThreadClasses.Count;
            while (true)
            {
                Thread.Sleep(10000);
                for (int i=0;i< length;i++)
                {
                    if (m_senderThreadClasses[i].IsRunningFinished())
                    {
                        m_senderThreadClasses.RemoveAt(i);
                        length--;
                        i--;
                    }
                }
            }
            
        }
        public void AddAnlysisResult(int cameraId, List<AnalysisReultClass> result)
        {
            foreach (SenderThreadClass senderClass in m_senderThreadClasses)
            {
                if (senderClass.CompairCameraId(cameraId))
                {
                    senderClass.addResult(result);
                }
            }
        }
        public void AddSenderSession(TcpClient tcpClient, int cameraId)
        {
            m_senderThreadClasses.Add(new SenderThreadClass(tcpClient, cameraId));
            m_senderThreadClasses.Last<SenderThreadClass>().Run();
        }




    }
}

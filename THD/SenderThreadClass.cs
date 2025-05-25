using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Analysis_Server.Structure.Analysis;
using Newtonsoft.Json;

namespace Analysis_Server.THD
{
    public class SenderThreadClass
    {
        private Thread m_Thread;
        private bool m_Running;
        private bool m_pause;
        private TcpClient m_tcpClient;
        private string m_videoSourceId;
        private List<AnalysisReultClass> m_analysisReultClasses;
        private ManualResetEvent m_event;
        private bool m_isThreadFinished;

        public SenderThreadClass(int idx,TcpClient tcpClient)
        {
            m_tcpClient = tcpClient;
            m_Thread = new Thread(DoWork);
            m_Running = false;
            m_pause = false;
            m_videoSourceId = "";
            m_event = new ManualResetEvent(false);
            m_isThreadFinished = false;
        }
        public bool IsRunningFinished()
        {
            return m_isThreadFinished;
        }
        public void addResult(List<AnalysisReultClass> result)
        {
            m_analysisReultClasses = null;
            m_analysisReultClasses = result;
            m_event.Set();
        }
        public bool CompairVideoSourceId(string id)
        {
            return m_videoSourceId.Equals(id);
        }
        public void Run()
        {
            m_Running = true;
            m_pause = true;
            m_Thread.Start();
        }

        public void pause()
        {
            m_pause = false;
        }

        public void restart()
        {
            m_pause = true;
        }

        public void quit()
        {
            m_Running = false;
            m_pause = false;
            m_Thread.Join();
            m_Thread = null;
        }
        private void DoWork()
        {
            NetworkStream stream = m_tcpClient.GetStream();

            //VideoSourceId 받기
            byte[] lengthBuffer = new byte[4];
            stream.Read(lengthBuffer, 0, 4);
            int dataLength = BitConverter.ToInt32(lengthBuffer, 0);

            // 그 길이만큼 데이터 받기
            byte[] buffer = new byte[dataLength];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            m_videoSourceId = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            while (m_Running)
            {
                while (m_pause)
                {
                    try
                    {
                        m_event.WaitOne();
                        string json = JsonConvert.SerializeObject(m_analysisReultClasses);

                        // 문자열을 바이트 배열로 변환
                        buffer = new byte[1024];
                        buffer = Encoding.UTF8.GetBytes(json);

                        //JSON 데이터 크기 부터 먼저 전송
                        byte[] lengthPrefix = BitConverter.GetBytes(buffer.Length);
                        stream.Write(lengthPrefix, 0, lengthPrefix.Length);

                        // JSON 데이터 전송
                        stream.Write(buffer, 0, buffer.Length);

                        Console.WriteLine("데이터 전송 완료");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(m_tcpClient.Client.RemoteEndPoint + "통신 종료");
                        m_Running = false;
                        m_pause = false;
                    }
                }
            }
            m_tcpClient.Close();
            m_isThreadFinished = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analysis_Server.Structure.Analysis;
using System.IO;
using System.Windows.Interop;

namespace Analysis_Server.THD
{
    public class ConnectThreadClass
    {
        private Thread m_Thread;
        private bool m_Running;
        private bool m_pause;
        private TcpListener m_listener;
        private int m_port;

        public delegate void SendResultDelegate(TcpClient result, int cameraId);
        private SendResultDelegate m_callback;
        public ConnectThreadClass(int serverPort)
        {
            m_Thread = new Thread(DoWork);
            m_Running = false;
            m_pause = false;
            m_port = serverPort;
        }

        public void SetCallback(SendResultDelegate callback)
        {
            m_callback = callback;
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
            m_listener = new TcpListener(IPAddress.Any, m_port);
            m_listener.Start();

            Console.WriteLine($" 서버 시작됨 (포트: {m_port})");
            while (m_Running)
            {
                while (m_pause)
                {
                    try
                    {
                        // {클라이언트 접속 대기 (비동기 아님, 쓰레드로 처리함)
                        TcpClient client = m_listener.AcceptTcpClient();
                        Console.WriteLine($"클라이언트 접속: {client.Client.RemoteEndPoint}");

                        string msg = "";
                        NetworkStream ns = client.GetStream();
                        StreamReader reader = new StreamReader(ns);
                        msg = reader.ReadLine();  // 클라이언트에서 \n 으로 끝나는 문자열 전송해야 읽힘
                        Console.WriteLine($"받은 메시지: {msg}");
                        if (!msg.Equals(""))
                        {
                            m_callback.Invoke(client, Convert.ToInt32(msg));
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }
                }
            }
            m_listener.Stop();
        }
    }
}

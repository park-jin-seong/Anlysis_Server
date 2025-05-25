using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analysis_Server.Structure
{
    public class DataBaseClass
    {
        public int m_DBType { get; set; }
        public string m_Ip { get; set; }
        public string m_Port { get; set; }
        public string m_Id { get; set; }
        public string m_Pw { get; set; }

        public DataBaseClass() 
        {
            m_DBType = 0;
            m_Ip = "";
            m_Port = "";
            m_Id = "";
            m_Pw = "";
        }

        public DataBaseClass(int DBType, string ip, string port, string id, string pw) 
        {
            m_DBType = DBType;
            m_Ip = ip;
            m_Port = port;
            m_Id = id;
            m_Pw = pw;
        }

        public string getCoonectString()
        {
            string conn = "";

            if (m_DBType == 1)
            {
                conn = "HOST="+ m_Ip + ";PORT="+ m_Port + ";USERNAME="+ m_Id + ";PASSWORD="+ m_Pw + ";DATABASE=";
            }
            return conn;
        }


    }
}

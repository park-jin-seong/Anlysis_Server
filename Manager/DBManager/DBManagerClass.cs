using System;
using System.Collections.Generic;
using System.Net;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Analysis_Server.Manager.SystemInfoManager;
using Analysis_Server.Structure.DB;
using Analysis_Server.THD;
using System.Net.Sockets;

namespace Analysis_Server.Manager.DBManager
{
    public class DBManagerClass : IDBManagerClass
    {
        private SystemInfoClass m_SystemInfoClass;
        private ServerInfosClass m_ServerInfosClass;
        private List<CameraInfoClass> m_CameraInfosClasses;

        public DBManagerClass(ISystemInfoManagerClass systemInfoManagerClass)
        {
            m_SystemInfoClass = systemInfoManagerClass.GetSystemInfoClass();
            m_CameraInfosClasses = new List<CameraInfoClass>();

            InsertServerInfo();
            GetServerInfosDB();
        }

        public ServerInfosClass GetServerInfosClass() => m_ServerInfosClass;
        public List<CameraInfoClass> GetCameraInfosClasses() => m_CameraInfosClasses;

        private void InsertServerInfo()
        {
            var host = m_SystemInfoClass.m_dataBaseClass.m_Ip;
            var port = m_SystemInfoClass.m_dataBaseClass.m_Port;
            var user = m_SystemInfoClass.m_dataBaseClass.m_Id;
            var password = m_SystemInfoClass.m_dataBaseClass.m_Pw;
            var analysisDatabaseName = "sentry_server";


            var dbConnString = $"Server={host};Port={port};Uid={user};Pwd={password};Database={analysisDatabaseName};";
            using (var conn = new MySqlConnection(dbConnString))
            {
                conn.Open();

                string targetIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
                string targetPort = "20000";
                string targetJson = JsonConvert.SerializeObject(new List<string>());

                using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM serverInfos WHERE serverIp = @ip;", conn))
                {
                    checkCmd.Parameters.AddWithValue("@ip", targetIp);
                    long count = Convert.ToInt64(checkCmd.ExecuteScalar());

                    if (count == 0)
                    {
                        using (var insertCmd = new MySqlCommand("INSERT INTO serverInfos (serverIp, serverPort, serverType, osId, osPw) VALUES (@ip, @port, @type, @osId, @osPw);", conn))
                        {
                            insertCmd.Parameters.AddWithValue("@ip", targetIp);
                            insertCmd.Parameters.AddWithValue("@port", int.Parse(targetPort)); // int로 변환
                            insertCmd.Parameters.AddWithValue("@type", "Analysis");
                            insertCmd.Parameters.AddWithValue("@osId", "");
                            insertCmd.Parameters.AddWithValue("@osPw", "");

                            insertCmd.ExecuteNonQuery();
                            Console.WriteLine("새 서버 데이터 삽입 완료");
                        }
                    }
                    else
                    {
                        Console.WriteLine("해당 IP는 이미 존재합니다.");
                    }
                }
            }
        }

        private void MakeCameraInfoDB()
        {
            var host = m_SystemInfoClass.m_dataBaseClass.m_Ip;
            var port = m_SystemInfoClass.m_dataBaseClass.m_Port;
            var user = m_SystemInfoClass.m_dataBaseClass.m_Id;
            var password = m_SystemInfoClass.m_dataBaseClass.m_Pw;
            var databaseName = "Analysis";
            var dbConnString = $"Server={host};Port={port};Uid={user};Pwd={password};Database={databaseName};";

            using (var conn = new MySqlConnection(dbConnString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(
                    @"CREATE TABLE IF NOT EXISTS camera_info (
                        m_idx INT AUTO_INCREMENT PRIMARY KEY,
                        m_videoSourceId VARCHAR(100) NOT NULL,
                        m_cameraName VARCHAR(100) NOT NULL,
                        m_rtspUrl TEXT NOT NULL,
                        m_coordx DOUBLE NOT NULL,
                        m_coordy DOUBLE NOT NULL
                    );", conn))
                {
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✅ 테이블 'camera_info' 생성 완료");
                }
            }
        }

        private void GetServerInfosDB()
        {
            string targetIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
            var host = m_SystemInfoClass.m_dataBaseClass.m_Ip;
            var port = m_SystemInfoClass.m_dataBaseClass.m_Port;
            var user = m_SystemInfoClass.m_dataBaseClass.m_Id;
            var password = m_SystemInfoClass.m_dataBaseClass.m_Pw;
            var databaseName = "sentry_server";
            var dbConnString = $"Server={host};Port={port};Uid={user};Pwd={password};Database={databaseName};";

            using (var conn = new MySqlConnection(dbConnString))
            {
                conn.Open();

                using (var cmd = new MySqlCommand(
                    "SELECT cameraId, cameraName, cctvUrl, coordx, coordy, isAnalisis FROM camerainfos WHERE analysisServerId = (SELECT serverId FROM serverinfos WHERE serverIp = @ip) AND isAnalisis = 1;", conn))
                {
                    cmd.Parameters.AddWithValue("@ip", targetIp);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            m_CameraInfosClasses.Add(new CameraInfoClass(
                                reader.GetInt32(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetFloat(3),
                                reader.GetFloat(4),
                                reader.GetBoolean(5)));
                        }
                    }
                }
            }
        }


        //private void GetCameraInfoDB()
        //{
        //    var host = m_SystemInfoClass.m_dataBaseClass.m_Ip;
        //    var port = m_SystemInfoClass.m_dataBaseClass.m_Port;
        //    var user = m_SystemInfoClass.m_dataBaseClass.m_Id;
        //    var password = m_SystemInfoClass.m_dataBaseClass.m_Pw;
        //    var databaseName = "Analysis";
        //    var dbConnString = $"Server={host};Port={port};Uid={user};Pwd={password};Database={databaseName};";

        //    using (var conn = new MySqlConnection(dbConnString))
        //    {
        //        conn.Open();

        //        var parameterNames = new List<string>();
        //        for (int i = 0; i < m_SetupClass.m_assignedVideoSourceIds.Count; i++)
        //        {
        //            string paramName = $"@id{i}";
        //            parameterNames.Add(paramName);
        //        }

        //        string inClause = string.Join(", ", parameterNames);
        //        string query = $"SELECT * FROM camera_info WHERE m_videoSourceId IN ({inClause})";

        //        using (var cmd = new MySqlCommand(query, conn))
        //        {
        //            for (int i = 0; i < m_SetupClass.m_assignedVideoSourceIds.Count; i++)
        //            {
        //                cmd.Parameters.AddWithValue(parameterNames[i], m_SetupClass.m_assignedVideoSourceIds[i]);
        //            }

        //            using (var reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    m_CameraInfosClasses.Add(new CameraInfoClass(
        //                        reader.GetInt32(0),
        //                        reader.GetString(1),
        //                        reader.GetString(2),
        //                        reader.GetString(3),
        //                        reader.GetDouble(4),
        //                        reader.GetDouble(5)
        //                    ));
        //                }
        //            }
        //        }
        //    }
        //}
    }
}

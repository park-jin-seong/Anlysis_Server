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
            GetCameraInfosDB();
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
                string targetPort = "10554";
                string targetJson = JsonConvert.SerializeObject(new List<string>());

                using (var checkCmd = new MySqlCommand("SELECT COUNT(*) FROM serverInfos WHERE serverIp = @ip AND serverType = 'Analysis';", conn))
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
                        using (var selectCmd = new MySqlCommand("SELECT serverId, serverIp, serverPort, serverType, osId, osPw FROM serverInfos WHERE serverIp = @ip AND serverType = 'Analysis';", conn))
                        {
                            selectCmd.Parameters.AddWithValue("@ip", targetIp);

                            using (var reader = selectCmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    m_ServerInfosClass = new ServerInfosClass(
                                        reader.GetInt32("serverId"),
                                        reader.GetString("serverIp"),
                                        reader.GetInt32("serverPort"),
                                        reader.GetString("serverType"),
                                        reader.IsDBNull(reader.GetOrdinal("osId")) ? "" : reader.GetString("osId"),
                                        reader.IsDBNull(reader.GetOrdinal("osPw")) ? "" : reader.GetString("osPw")
                                    );

                                    Console.WriteLine("이미 존재하는 서버 정보:");
                                    Console.WriteLine($"ID: {m_ServerInfosClass.serverId}, IP: {m_ServerInfosClass.serverIp}, Port: {m_ServerInfosClass.serverPort}, Type: {m_ServerInfosClass.serverType}");
                                }
                            }
                        }
                    }

                }
            }
        }

        private void GetCameraInfosDB()
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Analysis_Server.Manager.SystemInfoManager;
using Analysis_Server.Structure.DB;
using Analysis_Server.THD;
using Newtonsoft.Json;
using Npgsql;

namespace Analysis_Server.Manager.DBManager
{
    public class DBManagerClass : IDBManagerClass
    {
        private SystemInfoClass m_SystemInfoClass;

        private SetupClass m_SetupClass;
        private List<CameraInfoClass> m_CameraInfosClasses;

        public DBManagerClass(ISystemInfoManagerClass systemInfoManagerClass)
        {
            m_SystemInfoClass = systemInfoManagerClass.GetSystemInfoClass();
            m_CameraInfosClasses = new List<CameraInfoClass>();

            MakeSetupDB();
            MakeCameraInfoDB();
            GetSetupDB();
            GetCameraInfoDB();
        }

        public SetupClass GetSetupClass()
        {
            return m_SetupClass;
        }
        public List<CameraInfoClass> GetCameraInfosClasses()
        {
            return m_CameraInfosClasses;
        }
        private void MakeSetupDB()
        {
            var host = "localhost";
            var port = m_SystemInfoClass.m_dataBaseClass.m_Port;
            var user = m_SystemInfoClass.m_dataBaseClass.m_Id;
            var password = m_SystemInfoClass.m_dataBaseClass.m_Pw;
            var analysisDatabaseName = "Analysis";

            // Step 1: postgres DB에 연결하여 Analysis DB 생성
            var adminConnString = $"Host={host};Port={port};Username={user};Password={password};Database=postgres";

            using (var adminConn = new NpgsqlConnection(adminConnString))
            {
                adminConn.Open();

                using (var cmd = new NpgsqlCommand($"CREATE DATABASE \"{analysisDatabaseName}\"", adminConn))
                {
                    try
                    {
                        cmd.ExecuteNonQuery();
                        Console.WriteLine($"✅ 데이터베이스 '{analysisDatabaseName}' 생성 완료");
                    }
                    catch (PostgresException ex) when (ex.SqlState == "42P04")
                    {
                        Console.WriteLine($"⚠️ 데이터베이스 '{analysisDatabaseName}'는 이미 존재합니다.");
                    }
                }
            }

            // Step 2: 새로 만든 Analysis DB에 연결
            var dbConnString = $"Host={host};Port={port};Username={user};Password={password};Database={analysisDatabaseName}";

            using (var conn = new NpgsqlConnection(dbConnString))
            {
                conn.Open();

                // Step 3: setup 테이블 생성
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS setup (
                                        m_idx SERIAL PRIMARY KEY,
                                        m_serverIp VARCHAR(100) NOT NULL,
                                        m_serverPort VARCHAR(10) NOT NULL,
                                        m_assignedVideoSourceIds JSON NOT NULL);
                                        ";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✅ 테이블 'setup' 생성 완료");
                }

                using (var checkCmd = new NpgsqlCommand())
                {
                    string targetIp = Dns.GetHostEntry(Dns.GetHostName()).AddressList[1].ToString();
                    string targetPort = "20000";
                    List<string> aa = new List<string>();
                    aa.Add("video1");
                    aa.Add("video2");
                    string targetJson = JsonConvert.SerializeObject(new List<string>());

                    checkCmd.Connection = conn;
                    checkCmd.CommandText = @"SELECT COUNT(*) FROM setup WHERE m_serverIp = @ip;";
                    checkCmd.Parameters.AddWithValue("ip", targetIp);

                    long count = (long)checkCmd.ExecuteScalar();

                    if (count == 0)
                    {
                        // IP가 없으면 삽입
                        using (var insertCmd = new NpgsqlCommand())
                        {
                            insertCmd.Connection = conn;
                            insertCmd.CommandText = @"INSERT INTO setup (m_serverIp, m_serverPort, m_assignedVideoSourceIds) VALUES (@ip, @port, @json);";

                            insertCmd.Parameters.AddWithValue("ip", targetIp);
                            insertCmd.Parameters.AddWithValue("port", targetPort);

                            var jsonParam = new NpgsqlParameter("json", NpgsqlTypes.NpgsqlDbType.Json);
                            jsonParam.Value = targetJson;
                            insertCmd.Parameters.Add(jsonParam);

                            insertCmd.ExecuteNonQuery();
                            Console.WriteLine("✅ 새 IP 데이터 삽입 완료");
                        }
                    }
                    else
                    {
                        Console.WriteLine("⚠️ 해당 IP는 이미 존재합니다.");
                    }
                }
            }

        }
        private void MakeCameraInfoDB()
        {
            var host = "localhost";
            var port = m_SystemInfoClass.m_dataBaseClass.m_Port;
            var user = m_SystemInfoClass.m_dataBaseClass.m_Id;
            var password = m_SystemInfoClass.m_dataBaseClass.m_Pw;
            var analysisDatabaseName = "Analysis";
            var dbConnString = $"Host={host};Port={port};Username={user};Password={password};Database={analysisDatabaseName}";
            using (var conn = new NpgsqlConnection(dbConnString))
            {
                conn.Open();

                // Step 3: setup 테이블 생성
                using (var cmd = new NpgsqlCommand())
                {
                    cmd.Connection = conn;
                    cmd.CommandText = @"CREATE TABLE IF NOT EXISTS camera_info (
                                        m_idx SERIAL PRIMARY KEY,
                                        m_videoSourceId VARCHAR(100) NOT NULL,
                                        m_cameraName VARCHAR(100) NOT NULL,
                                        m_rtspUrl TEXT NOT NULL,
                                        m_coordx DOUBLE PRECISION NOT NULL,
                                        m_coordy DOUBLE PRECISION NOT NULL);
                                        ";
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("✅ 테이블 'setup' 생성 완료");
                }
            }



            // Step 2: 새로 만든 Analysis DB에 연결

            //using (var conn = new NpgsqlConnection(dbConnString))
            //{
            //    conn.Open();

            //    var insertQuery = @"INSERT INTO camera_info 
            //                        (m_videoSourceId, m_cameraName, m_rtspUrl, m_coordx, m_coordy)
            //                        VALUES (@videoSourceId, @cameraName, @rtspUrl, @coordx, @coordy)";

            //    using (var cmd = new NpgsqlCommand(insertQuery, conn))
            //    {
            //        // 파라미터 바인딩
            //        cmd.Parameters.AddWithValue("@videoSourceId", "video1");
            //        cmd.Parameters.AddWithValue("@cameraName", "Main Gate");
            //        cmd.Parameters.AddWithValue("@rtspUrl", "rtsp://192.168.0.10/live");
            //        cmd.Parameters.AddWithValue("@coordx", 127.05672);
            //        cmd.Parameters.AddWithValue("@coordy", 37.49292);

            //        int affected = cmd.ExecuteNonQuery();
            //        Console.WriteLine($"✅ {affected}개 행 INSERT 성공");
            //    }
            //}
            //using (var conn = new NpgsqlConnection(dbConnString))
            //{
            //    conn.Open();

            //    var insertQuery = @"INSERT INTO camera_info 
            //                        (m_videoSourceId, m_cameraName, m_rtspUrl, m_coordx, m_coordy)
            //                        VALUES (@videoSourceId, @cameraName, @rtspUrl, @coordx, @coordy)";

            //    using (var cmd = new NpgsqlCommand(insertQuery, conn))
            //    {
            //        // 파라미터 바인딩
            //        cmd.Parameters.AddWithValue("@videoSourceId", "video2");
            //        cmd.Parameters.AddWithValue("@cameraName", "Main Gate");
            //        cmd.Parameters.AddWithValue("@rtspUrl", "rtsp://192.168.0.10/live");
            //        cmd.Parameters.AddWithValue("@coordx", 127.05672);
            //        cmd.Parameters.AddWithValue("@coordy", 37.49292);

            //        int affected = cmd.ExecuteNonQuery();
            //        Console.WriteLine($"✅ {affected}개 행 INSERT 성공");
            //    }
            //}
        }
        private void GetSetupDB()
        {
            string targetIp = "192.168.0.2";
            var host = "localhost";
            var port = m_SystemInfoClass.m_dataBaseClass.m_Port;
            var user = m_SystemInfoClass.m_dataBaseClass.m_Id;
            var password = m_SystemInfoClass.m_dataBaseClass.m_Pw;
            var databaseName = "Analysis";

            var dbConnString = $"Host={host};Port={port};Username={user};Password={password};Database={databaseName}";

            using (var conn = new NpgsqlConnection(dbConnString))
            {
                conn.Open();

                using (var cmd = new NpgsqlCommand("SELECT m_idx, m_serverIp, m_serverPort, m_assignedVideoSourceIds FROM setup WHERE m_serverIp = @ip", conn))
                {
                    cmd.Parameters.AddWithValue("ip", targetIp);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            m_SetupClass = new SetupClass(reader.GetInt32(0),
                                                        reader.GetString(1),
                                                        reader.GetString(2),
                                                        reader.GetString(3));

                        }
                    }
                }
            }
        }

        private void GetCameraInfoDB()
        {
            var host = "localhost";
            var port = m_SystemInfoClass.m_dataBaseClass.m_Port;
            var user = m_SystemInfoClass.m_dataBaseClass.m_Id;
            var password = m_SystemInfoClass.m_dataBaseClass.m_Pw;
            var databaseName = "Analysis";
            var dbConnString = $"Host={host};Port={port};Username={user};Password={password};Database={databaseName}";

            using (var conn = new NpgsqlConnection(dbConnString))
            {
                conn.Open();

                // 동적으로 IN 절 구성
                var parameterNames = new List<string>();
                for (int i = 0; i < m_SetupClass.m_assignedVideoSourceIds.Count; i++)
                {
                    string paramName = $"@id{i}";
                    parameterNames.Add(paramName);
                }

                string inClause = string.Join(", ", parameterNames);
                string query = $"SELECT * FROM camera_info WHERE m_videoSourceId IN ({inClause})";

                using (var cmd = new NpgsqlCommand(query, conn))
                {
                    // 파라미터 바인딩
                    for (int i = 0; i < m_SetupClass.m_assignedVideoSourceIds.Count; i++)
                    {
                        cmd.Parameters.AddWithValue(parameterNames[i], m_SetupClass.m_assignedVideoSourceIds[i]);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            m_CameraInfosClasses.Add(new CameraInfoClass(reader.GetInt32(0),
                                                                        reader.GetString(1),
                                                                        reader.GetString(2),
                                                                        reader.GetString(3),
                                                                        reader.GetDouble(4),
                                                                        reader.GetDouble(5)));

                        }
                    }
                }
            }
        }
    }
}

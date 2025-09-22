using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Analysis_Server.Structure.Analysis;
using Analysis_Server.Structure.DB;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;

namespace Analysis_Server.THD
{
    public class AnalysisThreadClass
    {
        private Thread m_Thread;
        private CameraInfoClass m_CameraInfo;
        private string m_ModelPath;

        private bool m_Running;
        private bool m_pause;

        private InferenceSession _session;
        private List<RectangleF> boundingBoxes;
        private List<string> labels;

        public delegate void SendResultDelegate(int cameraId, List<AnalysisReultClass> result);
        private SendResultDelegate m_callback;
        
        public AnalysisThreadClass(string modelPath, int cameraId, string cameraName, string cctvUrl, float coordx, float coordy, bool isAnalisis)
        {
            m_ModelPath = modelPath;
            m_CameraInfo = new CameraInfoClass(cameraId, cameraName, cctvUrl, coordx, coordy, isAnalisis);
            m_Thread = new Thread(DoWork);
            m_Running = false;
            m_pause = false;

            _session = new InferenceSession(modelPath);
            boundingBoxes = new List<RectangleF>();
            labels = new List<string>();
        }
        public void SetCallback(SendResultDelegate callback)
        {
            m_callback = callback;
        }

        public bool CheckVideoSourceId(string id)
        {
            if (id.Equals(m_CameraInfo.cameraId.ToString()))
            {
                return true;
            }
            return false;
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
            VideoCapture capture = new VideoCapture();

            capture.Open(m_CameraInfo.cctvUrl);
            Mat image = new Mat();  // 프로토콜로 받을 이미지
            Bitmap bitmap;  // 받은 이미지 분석용으로 변환
            Stopwatch stopwatch = new Stopwatch();  //분석 속도 측정
            while (m_Running)
            {
                while (m_pause)
                {
                    stopwatch.Reset();
                    stopwatch.Start();
                    if (!capture.Read(image)) //이미지 수신
                    {
                        Cv2.WaitKey();
                    }
                    if (image.Size().Width > 0 && image.Size().Height > 0)
                    {
                        bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(image); // 분석용 이미지로 변환
                        var input = PreprocessImage(bitmap);    //텐서로 변환

                        //영상 분석
                        var results = _session.Run(new[] { NamedOnnxValue.CreateFromTensor("images", input) });     //분석 및 결과 반환
                        // 결과 처리
                        PostprocessResults(results);
                    }

                    if (Cv2.WaitKey(1) >= 0)
                        break;
                    //Cv2.WaitKey(sleepTime);
                    stopwatch.Stop(); //시간측정 끝

                    System.Console.WriteLine(m_CameraInfo.cameraId + " : " + stopwatch.ElapsedMilliseconds + "ms");
                    Thread.Sleep(1);

                }
            }
        }
        private Tensor<float> PreprocessImage(Bitmap bit)
        {

            Bitmap resized = new Bitmap(bit, new System.Drawing.Size(640, 640));

            // 텐서로 변환
            float[] imageData = new float[3 * 640 * 640];
            for (int y = 0; y < 640; y++)
            {
                for (int x = 0; x < 640; x++)
                {
                    Color pixel = resized.GetPixel(x, y);
                    imageData[(0 * 640 + y) * 640 + x] = pixel.R / 255.0f;
                    imageData[(1 * 640 + y) * 640 + x] = pixel.G / 255.0f;
                    imageData[(2 * 640 + y) * 640 + x] = pixel.B / 255.0f;
                }
            }

            return new DenseTensor<float>(imageData, new[] { 1, 3, 640, 640 });
        }
        private void PostprocessResults(IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results)
        {
            // 결과 데이터 가져오기
            var output = results.First().AsEnumerable<float>().ToArray();
            List<AnalysisReultClass> analysisReultClasses = new List<AnalysisReultClass>();

            // 각 객체에 대한 정보는 [x, y, width, height, score, class_id] 형태로 저장되어 있다고 가정
            const int numAttributes = 6; // x, y, width, height, score, class_id
            for (int i = 0; i < output.Length / numAttributes; i++)
            {
                float x = output[i * numAttributes + 0]/640;
                float y = output[i * numAttributes + 1]/640;
                float width = output[i * numAttributes + 2]/640;
                float height = output[i * numAttributes + 3]/640;
                float score = output[i * numAttributes + 4];
                int classId = (int)output[i * numAttributes + 5];

                if (score > 0.3) // 신뢰도 필터링
                {
                    analysisReultClasses.Add(new AnalysisReultClass(x, y, width, height, score, classId));
                }
            }
            if (analysisReultClasses.Count != 0)
            {
                m_callback.Invoke(m_CameraInfo.cameraId, analysisReultClasses);
            }
        }
    }
}

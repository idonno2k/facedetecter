using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;

using OpenCvSharp;
using OpenCvSharp.Test;


namespace CSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //return;
        }

        class Face : IDisposable
        {
            IplImage FindFace;
            public IplImage FaceDetect( string filepath)
            {
                // CvHaarClassifierCascade, cvHaarDetectObjects
                // 얼굴을 검출하기 위해서 Haar 분류기의 캐스케이드를 이용한다
                CvColor[] colors = new CvColor[]{
                new CvColor(0,0,255),
                new CvColor(0,128,255),
                new CvColor(0,255,255),
                new CvColor(0,255,0),
                new CvColor(255,128,0),
                new CvColor(255,255,0),
                new CvColor(255,0,0),
                new CvColor(255,0,255),
            };
                const double scale = 1.04;
                const double scaleFactor = 1.139;
                const int minNeighbors = 2;

                IplImage src = new IplImage(filepath, LoadMode.Color);

                using (IplImage img = src.Clone())
                using (IplImage smallImg = new IplImage(new CvSize(Cv.Round(img.Width / scale), Cv.Round(img.Height / scale)), BitDepth.U8, 1))
                {
                    // 얼굴 검출용의 화상의 생성
                    using (IplImage gray = new IplImage(img.Size, BitDepth.U8, 1))
                    {
                        Cv.CvtColor(img, gray, ColorConversion.BgrToGray);
                        Cv.Resize(gray, smallImg, Interpolation.Linear);
                        Cv.EqualizeHist(smallImg, smallImg);
                    }
                    using (CvHaarClassifierCascade cascade = CvHaarClassifierCascade.FromFile(Application.StartupPath + "\\" + "haarcascade_frontalface_alt.xml"))

                    using (CvMemStorage storage = new CvMemStorage())
                    {
                        storage.Clear();
                        // 얼굴의 검출
                        CvSeq<CvAvgComp> faces = Cv.HaarDetectObjects(smallImg, cascade, storage, scaleFactor, minNeighbors, 0, new CvSize(120, 120));
                        // 검출한 얼굴에사각형을 그린다
                        for (int i = 0; i < faces.Total; i++)
                        {
                            CvRect r = faces[i].Value.Rect;
                            CvPoint center = new CvPoint
                            {
                                X = Cv.Round((r.X + r.Width * 0.5) * scale),
                                Y = Cv.Round((r.Y + r.Height * 0.5) * scale)
                            };
                            int radius = Cv.Round((r.Width + r.Height)*1.0);
                           // img.Circle(center, radius, colors[i % 8], 15, LineType.AntiAlias, 0);

                            r.X = center.X - Cv.Round(radius * 0.5);
                            r.Y = center.Y - Cv.Round(radius * 0.5);

                            r.Width = radius;
                            r.Height = radius; 

                           // img.Rectangle(r, colors[i % 8],1,LineType.AntiAlias, 0);

                            using (IplImage crobImg = src.Clone())
                            {
                                Cv.Resize(img, crobImg, Interpolation.Linear);
                                Cv.SetImageROI(crobImg, r);
                                // Cv.ShowImage("crop",crobImg);
                           
                                string savePath = Path.GetDirectoryName(filepath);
                                string savefile = Path.GetFileNameWithoutExtension(filepath);

                               // savePath = savePath + "\\crobimg";
                               // DirectoryInfo di = new DirectoryInfo(savePath);
                               // if (di.Exists == false)
                               // {
                               //     di.Create();
                               // }

                                string savefilename = savePath + "\\crobimg\\" + savefile + "_" + i + ".jpg";
                                Cv.SaveImage(savefilename, crobImg); 
                            }
                        }
                    }
                    IplImage testFindFace = img.Clone();
                    return testFindFace;
                }
            }

            public void Dispose()
            {
                if (FindFace != null) FindFace.Dispose();
            }
        }

        private string m_folder_path;
        static string[] m_Files;
        private void button1_Click(object sender, EventArgs e)
        {

            FolderBrowserDialog forderbrowser = new FolderBrowserDialog();
            if (forderbrowser.ShowDialog() == DialogResult.OK)
            {
                m_folder_path = forderbrowser.SelectedPath;
                m_Files = Directory.GetFiles(m_folder_path,"*.jpg");

                if (m_Files != null)
                {
                    string sDirPath;
                    sDirPath = m_folder_path + "\\crobimg";
                    DirectoryInfo di = new DirectoryInfo(sDirPath);
                    if (di.Exists == false)
                    {
                        di.Create();
                    }

                    Thread t1 = new Thread(new ThreadStart(threadFaceDetectionRun));
                    t1.Start();
                }
            }

        }


        private void threadFaceDetectionRun()
        {
            
            for( int i=0; i < m_Files.Length ;i++)
            {
                //IplImage img = new IplImage(m_folder_path + m_Files[i], LoadMode.Color);

                Face fac = new Face();
                IplImage imgHarr = fac.FaceDetect( m_Files[i]);

                pictureBoxIpl2.ImageIpl = imgHarr;
            }
        }


    }
}

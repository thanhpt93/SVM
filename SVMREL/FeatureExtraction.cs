using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.IO;

namespace SVMREL
{
    public class FeatureExtraction
    {
        // Read list Image
        public static void LoadImage(string pathfile, List<Mat> imagelist)
        {
            Size sizes = new Size(40, 40);
            Mat img;
            string[] files = Directory.GetFiles(pathfile);
            for (int i = 0; i < files.Length; i++)
            {
                img = Cv2.ImRead(files[i]);
                Cv2.Resize(img, img, sizes);
                imagelist.Add(img);
            }
        }
        // Mean value function calculation
        public static double CalMeanValue(Mat img)
        {
            double mean = 0;
            SortedDictionary<int, int> ANALISYS = new SortedDictionary<int, int>();
            int count = 1;
            int value = 0;
            int Sum = 0;
            int SumValue = 0;
            byte[] data = new byte[img.Rows * img.Cols];
            img.GetArray(0, 0, data);
            for(int i=0;i<data.Length;i++)
            {
                count = 1;
                value = data[i];
                if (ANALISYS.ContainsKey(value))
                {
                    ANALISYS[value]++;
                    count = ANALISYS[value];
                    ANALISYS.Remove(value);
                }
                ANALISYS.Add(value, count);
            }
            foreach(var temp in ANALISYS)
            {
                Sum += temp.Key * temp.Value;
                SumValue += temp.Value;
            }
            mean = (double)Sum / SumValue;
            return mean;
        }
       
        // Calculation HoG value then write to the file:
        public static void compute_hog(List<Mat> img_lst, OpenCvSharp.Size size, int lables, string path)
        {
            HOGDescriptor hog = new HOGDescriptor();
            hog.WinSize = size;
            Mat gray = new Mat();
            //Mat Mat_descriptor = new Mat();
            float[] descriptors;
            int descriptors_size = 0;
            StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8);
            for (int i = 0; i < img_lst.Count; i++)// vong lap duyet tung anh
            {
                string line = lables.ToString();
                sw.Write(line);
                Cv2.CvtColor(img_lst[i], gray, ColorConversionCodes.RGB2GRAY);
                //canny
                // Cv2.Canny(gray, gray, 100, 200);
                descriptors = hog.Compute(gray);
                descriptors_size = descriptors.Length;
                Mat Mat_descriptor = new Mat(descriptors_size, 1, MatType.CV_8UC1);
                for (int a = 0; a < descriptors.Length; a++)
                {
                    Mat_descriptor.Set<float>(a, 0, descriptors[a]);
                    float value = Mat_descriptor.Get<float>(a, 0);
                    string lines = " " + (a + 1) + ":" + value;
                    sw.Write(lines);
                }
                sw.WriteLine();
            }
            sw.Close();
        }
        
        public static void compute_hog_test(List<Mat> img_lst, OpenCvSharp.Size size, int lables, string path)
        {        
            HOGDescriptor hog = new HOGDescriptor();
            hog.WinSize = size;
            Mat gray = new Mat();

            float[] descriptors;
            int descriptors_size = 0;
            StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8);
          
            for (int i = 0; i < img_lst.Count; i++)// vong lap duyet tung anh
            {
                
                string line = lables.ToString();
                sw.Write(line);
                
                Cv2.CvtColor(img_lst[i], gray, ColorConversionCodes.RGB2GRAY);               
                descriptors = hog.Compute(gray);
                
                descriptors_size = descriptors.Length;
                Mat Mat_descriptor = new Mat(descriptors_size, 1, MatType.CV_8UC1);
                
                for (int a = 0; a < descriptors.Length; a++)
                {
                    Mat_descriptor.Set<float>(a, 0, descriptors[a]);
                    float value = Mat_descriptor.Get<float>(a, 0);
                    string lines = " " + (a + 1) + ":" + value;
                    sw.Write(lines);
                }            
                sw.WriteLine();               
            }          
            sw.Close();      
        }
        public static void MeanValueCompute(List<Mat> img_lst, OpenCvSharp.Size size, int lables, string path)
        {
            Mat[] RGB = new Mat[3];
            StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8);
            for (int i = 0; i < img_lst.Count; i++)// vong lap duyet tung anh
            {
                RGB = Cv2.Split(img_lst[i]);
                string line = lables.ToString();
                sw.Write(line);
               
                //if (InforPublic.canny == true)
                //{
                //    Cv2.Canny(gray, gray, InforPublic.Th1, InforPublic.Th2);
                //}
                //if (InforPublic.median == true)
                //{
                //    Cv2.MedianBlur(gray, gray, InforPublic.Kernel_median);
                //    // ListImage.Add(img);
                //}              
                for (int a = 0; a < RGB.Length; a++)
                {
                    double mean = CalMeanValue(RGB[a]);                   
                    string lines = " " + (a + 1) + ":" + mean;
                    sw.Write(lines);
                }
                sw.WriteLine();
            }
            sw.Close();
        }
        public static void MeanValueCompute_test(List<Mat> img_lst, OpenCvSharp.Size size, int lables, string path)
        {
            Mat[] RGB = new Mat[3];
            StreamWriter sw2 = new StreamWriter(path, false, Encoding.UTF8);
            for (int i = 0; i < img_lst.Count; i++)// vong lap duyet tung anh
            {
                RGB = Cv2.Split(img_lst[i]);
                string line = lables.ToString();
                sw2.Write(line);

                //if (InforPublic.canny == true)
                //{
                //    Cv2.Canny(gray, gray, InforPublic.Th1, InforPublic.Th2);
                //}
                //if (InforPublic.median == true)
                //{
                //    Cv2.MedianBlur(gray, gray, InforPublic.Kernel_median);
                //    // ListImage.Add(img);
                //}              
                for (int a = 0; a < RGB.Length; a++)
                {

                    double mean = CalMeanValue(RGB[a]);
                    string lines = " " + (a + 1) + ":" + mean;
                    sw2.Write(lines);
                }
                sw2.WriteLine();
            }
            sw2.Close();
        }
    }
}


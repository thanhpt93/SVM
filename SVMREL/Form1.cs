using LibSVMsharp;
using LibSVMsharp.Extensions;
using LibSVMsharp.Helpers;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SVMREL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        SVMModel model = new SVMModel();
        string parameter_file = "param.json";
        Dictionary<string, string> parameter = new Dictionary<string, string>();

        // get information from json file
        private Dictionary<string, string> load_json_file(string file_path)
        {
            Dictionary<string, string> content = new Dictionary<string, string>();
            var text = File.ReadAllText(file_path);
            content = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
            return content;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //load parameter from setting file
            parameter = load_json_file(parameter_file);

            if (!Directory.Exists(parameter["path_extraction"]))
            {
                Directory.CreateDirectory(parameter["path_extraction"]);
            }
            if (!Directory.Exists(parameter["path_model"]))
            {
                Directory.CreateDirectory(parameter["path_model"]);
            }
            if (!Directory.Exists(parameter["path_result"]))
            {
                Directory.CreateDirectory(parameter["path_result"]);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd1 = new FolderBrowserDialog();
            if (fbd1.ShowDialog() == DialogResult.OK)
            {
                string path = fbd1.SelectedPath;
                textBox1.Text = path;
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd2 = new FolderBrowserDialog();
            if (fbd2.ShowDialog() == DialogResult.OK)
            {
                string path = fbd2.SelectedPath;
                textBox2.Text = path;
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            FolderBrowserDialog fbd1 = new FolderBrowserDialog();
            if (fbd1.ShowDialog() == DialogResult.OK)
            {
                string path = fbd1.SelectedPath;
                string[] filename = Directory.GetFiles(path);
                foreach (var temp in filename)
                {
                    listBox1.Items.Add(temp);
                }
            }
        }

        List<string> Class_Train = new List<string>();
        List<string> Class_Validation = new List<string>();

        Dictionary<string, string[]> TRAIN_DATA = new Dictionary<string, string[]>();
        Dictionary<string, string[]> VALIDATION_DATA = new Dictionary<string, string[]>();

        Dictionary<int, string> MAP = new Dictionary<int, string>();

        private void get_data(string path_train, string path_validation)
        {
            try
            {
                MAP.Clear();
                string[] allfiles = Directory.GetFiles(path_train, "*", SearchOption.AllDirectories);
                // get class
                foreach (var tmp in allfiles)
                {
                    string[] value = tmp.Split('\\');
                    string class_ = value[value.Length - 2];
                    if (!Class_Train.Contains(class_))
                    {
                        Class_Train.Add(class_);
                    }
                }

                // get image each class         
                foreach (var tmp in Class_Train)
                {
                    string folder_class = path_train + "\\" + tmp;
                    string[] image = Directory.GetFiles(folder_class);
                    TRAIN_DATA.Add(tmp, image);
                }

                // get data validation

                string[] allfiles2 = Directory.GetFiles(path_validation, "*", SearchOption.AllDirectories);
                // get class
                foreach (var tmp in allfiles2)
                {
                    string[] value = tmp.Split('\\');
                    string class_ = value[value.Length - 2];
                    if (!Class_Validation.Contains(class_))
                    {
                        Class_Validation.Add(class_);
                    }
                }

                // get image each class           
                foreach (var tmp in Class_Validation)
                {
                    string folder_class = path_validation + "\\" + tmp;
                    string[] image = Directory.GetFiles(folder_class);
                    VALIDATION_DATA.Add(tmp, image);
                }

                //mapping index with class
                for(int i=0; i < Class_Train.Count; i++)
                {
                    MAP.Add(i, Class_Train[i]);
                }

                // write mapping to file
                string mapping_file = parameter["mapping_file"];
                StreamWriter map = new StreamWriter(mapping_file, false, Encoding.UTF8);
                foreach(var tmp in MAP)
                {
                    string line = tmp.Key + " " + tmp.Value;
                    map.WriteLine(line);
                }
                map.Close();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        bool flag = false;
        private void btnShowInfor_Click(object sender, EventArgs e)
        {
            try
            {
                flag = true;
                TRAIN_DATA.Clear();
                VALIDATION_DATA.Clear();
                // get data traain
                string path_train = textBox1.Text;
                string path_validation = textBox2.Text;

                // get data to show
                get_data(path_train, path_validation);

                string title_train = "Train data information \n";
                string title_val = "Validation data information \n";
                string msg1 = "";
                string msg2 = "";
                foreach (var tmp in TRAIN_DATA)
                {
                    msg1 += "class: " + tmp.Key + "   count: " + tmp.Value.Length + "\n";
                }
                foreach (var tmp in VALIDATION_DATA)
                {
                    msg2 += "class: " + tmp.Key + "   count: " + tmp.Value.Length + "\n";
                }

                MessageBox.Show(title_train + msg1 + "\n\n" + title_val + msg2);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex < 0) return;
            else
            {
                Mat img_display = new Mat();
                string dir = listBox1.SelectedItem.ToString();
                img_display = Cv2.ImRead(dir);
                pictureBox1.Image = img_display.ToBitmap();
            }
        }
        OpenCvSharp.Size sizes = new OpenCvSharp.Size();
        private void btn_FeatureExtraction_Click(object sender, EventArgs e)
        {
            try
            {
                if (!flag)
                {
                    string path_train = textBox1.Text;
                    string path_validation = textBox2.Text;
                    get_data(path_train, path_validation);
                }

                string size = parameter["resize"];
                sizes = new OpenCvSharp.Size(Convert.ToInt32(size.Split(',')[0]), Convert.ToInt32(size.Split(',')[1]));
                File.Create(parameter["hog_train_file"]).Dispose();
                File.Create(parameter["hog_val_file"]).Dispose();
                List<Mat> ListImageTrain = new List<Mat>();
                List<Mat> ListImageVal = new List<Mat>();

                // train
                int index_train = 0;
                foreach (var tmp in TRAIN_DATA)
                {
                    ListImageTrain.Clear();
                    foreach (var tmp2 in tmp.Value)
                    {
                        try
                        {
                            Mat img = Cv2.ImRead(tmp2);
                            Cv2.Resize(img, img, sizes);
                            ListImageTrain.Add(img);
                        }
                        // if read image error
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    FeatureExtraction.compute_hog(ListImageTrain, sizes, index_train, parameter["hog_train_file"]);
                    index_train += 1;
                }

                // val
                int index_val = 0;
                foreach (var tmp in VALIDATION_DATA)
                {
                    ListImageVal.Clear();
                    foreach (var tmp2 in tmp.Value)
                    {
                        try
                        {
                            Mat img = Cv2.ImRead(tmp2);
                            Cv2.Resize(img, img, sizes);
                            ListImageVal.Add(img);
                        }
                        // if read image error
                        catch (Exception ex)
                        {
                            continue;
                        }
                    }
                    FeatureExtraction.compute_hog(ListImageVal, sizes, index_val, parameter["hog_val_file"]);
                    index_val += 1;
                }

                MessageBox.Show("Completed");
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        double Accuracy = 0;
        private void btnTrain_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Stopwatch time = new System.Diagnostics.Stopwatch();
            parameter = load_json_file(parameter_file);
            try
            {

                File.Create(parameter["path_model"] + textBox3.Text + ".txt").Dispose();
                File.Create(parameter["result_file"]).Dispose();
                if (textBox3.Text == "")
                {
                    MessageBox.Show("create model name");
                }
                //if(SHOWRESULT.ContainsKey(txtModelName.Text))
                //{
                //    MessageBox.Show("Model name already exits");
                //}
                else
                {
                    time.Start();
                    #region Creat file Model
                    // Creat param SVM
                    SVMProblem FileTrain = SVMProblemHelper.Load(parameter["hog_train_file"]);
                    SVMParameter param = new SVMParameter();
                    param.Type = SVMType.C_SVC;
                    if (parameter["kernel_svm"] == "RBF")
                        param.Kernel = SVMKernelType.RBF;
                    if (parameter["kernel_svm"] == "Linear")
                        param.Kernel = SVMKernelType.LINEAR;
                    if (parameter["kernel_svm"] == "Poly")
                        param.Kernel = SVMKernelType.POLY;
                    if (parameter["kernel_svm"] == "Sigmoid")
                        param.Kernel = SVMKernelType.SIGMOID;
                    // param.C = Convert.ToDouble(parameter["c"]);
                    param.C = double.Parse(parameter["c"], CultureInfo.InvariantCulture);
                    param.P = double.Parse(parameter["p"], CultureInfo.InvariantCulture);
                    param.Gamma = double.Parse(parameter["gamma"], CultureInfo.InvariantCulture);
                    param.Degree = Convert.ToInt16(parameter["degree"]);
                    param.Nu = double.Parse(parameter["nu"], CultureInfo.InvariantCulture);
                    param.Coef0 = double.Parse(parameter["coef0"], CultureInfo.InvariantCulture);
                    param.Eps = double.Parse(parameter["eps"], CultureInfo.InvariantCulture);
                    //Train model
                    model = LibSVMsharp.SVM.Train(FileTrain, param);
                    LibSVMsharp.SVM.SaveModel(model, parameter["path_model"] + textBox3.Text + ".txt");

                    time.Stop();
                    double train_time = time.ElapsedMilliseconds;
                    #endregion

                    #region Validation data
                    SVMProblem Validation = SVMProblemHelper.Load(parameter["hog_val_file"]);
                    double[] Target_validation = Validation.Predict(model);
                    StreamWriter sw = new StreamWriter(parameter["result_file"], true, Encoding.UTF8);
                    for (int i = 0; i < Target_validation.Length; i++)
                    {
                        string lines = Target_validation[i].ToString();
                        sw.WriteLine(lines);
                    }
                    sw.Close();
                    Accuracy = SVMHelper.EvaluateClassificationProblem(Validation, Target_validation);
                    Accuracy = Math.Round(Accuracy, 3);


                    // show result training
                    textBox4.Text = (train_time / 1000).ToString();
                    textBox5.Text = Accuracy.ToString();
                    MessageBox.Show("Trainning sucessful");

                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        // tabPage Testing
        SVMModel model_load = new SVMModel();
        OpenCvSharp.Size sizes2 = new OpenCvSharp.Size();
        private void btnLoadModel_Click(object sender, EventArgs e)
        {
            parameter = load_json_file(parameter_file);
            string size = parameter["resize"];
            sizes2 = new OpenCvSharp.Size(Convert.ToInt32(size.Split(',')[0]), Convert.ToInt32(size.Split(',')[1]));
            model_load = SVM.LoadModel(parameter["path_model"] + comboBox1.Text);
            MessageBox.Show("Load model completed");

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_DropDown(object sender, EventArgs e)
        {
            try
            {
                comboBox1.Items.Clear();
                string[] model = Directory.GetFiles(parameter["path_model"]);
                foreach (var tmp in model)
                {
                    string[] sub = tmp.Split('/');
                    comboBox1.Items.Add(sub[sub.Length - 1]);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            File.Create(parameter["hog_test_file"]).Dispose();
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            if (listBox2.SelectedIndex < 0) return;
            else
            {
                try
                {
                    List<Mat> ListImage = new List<Mat>();
                    Mat img_display = new Mat();
                    string dir = listBox2.SelectedItem.ToString();
                    img_display = Cv2.ImRead(dir);

                    pictureBox2.Image = img_display.ToBitmap();
                    // Process predict
                    sw.Start();
                    Cv2.Resize(img_display, img_display, sizes2);
                    ListImage.Add(img_display);
                    FeatureExtraction.compute_hog_test(ListImage, sizes2, 100, parameter["hog_test_file"]);
                    SVMProblem Test = SVMProblemHelper.Load(parameter["hog_test_file"]);
                    double[] Target = Test.Predict(model_load);
                    sw.Stop();
                    label13.Text = MAPPING[(int)(Target[0])].ToString();
                    label10.Text = sw.ElapsedMilliseconds.ToString() + " (ms)";
                    string time = sw.ElapsedMilliseconds.ToString();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                string path = fbd.SelectedPath;
                string[] filename = Directory.GetFiles(path);
                foreach (var temp in filename)
                {
                    listBox2.Items.Add(temp);
                }
            }
        }

        Dictionary<int, string> MAPPING = new Dictionary<int, string>();
        private void button5_Click(object sender, EventArgs e)
        {
            MAPPING.Clear();
            try
            {
                using (StreamReader sr = new StreamReader(parameter["mapping_file"]))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] value = line.Split(' ');
                        MAPPING.Add(Convert.ToInt16(value[0]), value[1]);
                    }
                }
                MessageBox.Show("Loaded mapping");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}

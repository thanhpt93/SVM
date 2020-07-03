# SVM Platform for UET Training

This is demo platform about SVM algorithm with HOG feature is used. It also support multi class to classify

Design: thanhpt93.hd@gmail.com

## Requirement
1. Visual Studio 2017 or higher install
2. Window form C# application
3. OpenCV, LibSVM library install

## OpenCV, LibSVM install guide

We use OpenCVSharp and LibSVMSharp library to create small application

Step to install OpenCVSharp:

1. Right click in References -> Manage Nuget Package

![Install Guide](ImgShow\InstallGuide.JPG)

2. Search library which you want to install then install

3. when you coding, remember using library to use

![Install Guide](ImgShow\InstallGuide2.JPG)

## Some function importance use in project

### OpenCV

1. Read image

```
Mat img = new Mat();
string dir = "path to image file";
img = Cv2.ImRead(dir);
```

2. Resize image

```
Cv2.Resize(img, img, (40,40));
```

3. Convert image color to gray
```
Cv2.CvtColor(img, gray, ColorConversionCodes.BGR2GRAY);
```

### LibSVM

1. Create model
```
                    SVMModel model = new SVMModel();
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
                    param.C = double.Parse(parameter["c"], CultureInfo.InvariantCulture);
                    param.P = double.Parse(parameter["p"], CultureInfo.InvariantCulture);
                    param.Gamma = double.Parse(parameter["gamma"], CultureInfo.InvariantCulture);
                    param.Degree = Convert.ToInt16(parameter["degree"]);
                    param.Nu = double.Parse(parameter["nu"], CultureInfo.InvariantCulture);
                    param.Coef0 = double.Parse(parameter["coef0"], CultureInfo.InvariantCulture);
                    //Train model
                    model = LibSVMsharp.SVM.Train(FileTrain, param);
```

2. Save model
```
LibSVMsharp.SVM.SaveModel(model, "name of model");
```

3. Load model
```
SVMModel model_load = new SVMModel();
model_load = SVM.LoadModel(parameter["path_model"] + comboBox1.Text);
```

## Usage

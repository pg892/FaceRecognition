using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace FaceDetectionSample
{
    public partial class Form1 : Form
    {
        // Declare variables
        MCvFont _font = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_TRIPLEX, 0.6d, 0.6d);
        HaarCascade _faceDetected;
        Image<Bgr, byte> _frame;
        Capture _camera;
        Image<Gray, byte> _result;
        Image<Gray, byte> _trainedFace;
        Image<Gray, byte> _grayFace;
        List<Image<Gray, byte>> _trainingImages = new List<Image<Gray, byte>>();
        List<string> _labels = new List<string>();
        List<string> _users = new List<string>();
        int _count, _numLabels;
        string _name;

        private void Save_Click(object sender, EventArgs e)
        {
            _count++;
            _grayFace = _camera.QueryGrayFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            var detectedFaces = _grayFace.DetectHaarCascade(_faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach(var f in detectedFaces[0])
            {
                _trainedFace = _frame.Copy(f.rect).Convert<Gray, byte>();
                break;
            }
            _trainedFace = _result.Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            _trainingImages.Add(_trainedFace);
            _labels.Add(textName.Text);
            File.WriteAllText($"{Application.StartupPath}/Faces/Faces.txt", $"{_trainingImages.ToArray().Length.ToString()},");
            for(var i = 1; i < _trainingImages.ToArray().Length + 1; i++)
            {
                _trainingImages.ToArray()[i - 1].Save($"{Application.StartupPath}/Faces/Face{i}.bmp");
                File.AppendAllText($"{Application.StartupPath}/Faces/Faces.txt", $"{_labels.ToArray()[i - 1]},");
            }
            MessageBox.Show($"{textName.Text} Added Successfully");
        }

        public Form1()
        {
            InitializeComponent();
            _faceDetected = new HaarCascade("haarcascade_frontalface_default.xml");

            try
            {
                var LabelsInf = File.ReadAllText($"{Application.StartupPath}/Faces/Faces.txt");
                var labels = LabelsInf.Split(',');
                _numLabels = Convert.ToInt16(labels[0]);
                _count = _numLabels;
                string FacesLoad;
                for(var i = 1; i < _numLabels + 1; i++)
                {
                    FacesLoad = $"Face{i}.bmp";
                    _trainingImages.Add(new Image<Gray, byte>($"{Application.StartupPath}/Faces/{FacesLoad}"));
                    _labels.Add(labels[i]);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Nothing in the database");
            }
        }

        private void Start_Click(object sender, EventArgs e)
        {
            _camera = new Capture();
            _camera.QueryFrame();
            Application.Idle += new EventHandler(FrameProcedure);
        }

        private void FrameProcedure(object sender, EventArgs e)
        {
            _users.Add("");
            _frame = _camera.QueryFrame().Resize(320, 240, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
            _grayFace = _frame.Convert<Gray, byte>();
            var facesDetectedNow = _grayFace.DetectHaarCascade(_faceDetected, 1.2, 10, Emgu.CV.CvEnum.HAAR_DETECTION_TYPE.DO_CANNY_PRUNING, new Size(20, 20));
            foreach(var f in facesDetectedNow[0])
            {
                _result = _frame.Copy(f.rect).Convert<Gray, byte>().Resize(100, 100, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);
                _frame.Draw(f.rect, new Bgr(Color.Green), 3);
                if(_trainingImages.ToArray().Length != 0)
                {
                    var termCriteria = new MCvTermCriteria(_count, 0.0019);
                    var recogizer = new EigenObjectRecognizer(_trainingImages.ToArray(), _labels.ToArray(), 1500, ref termCriteria);
                    _name = recogizer.Recognize(_result);
                    _frame.Draw(_name, ref _font, new Point(f.rect.X - 2, f.rect.Y - 2), new Bgr(Color.Red));
                }
                _users.Add("");
            }
            cameraBox.Image = _frame;
            _users.Clear();
        }
    }
}
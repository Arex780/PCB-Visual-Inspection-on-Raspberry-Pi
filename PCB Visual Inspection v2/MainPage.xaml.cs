using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Devices.Enumeration;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;

namespace PCB_Visual_Inspection_v2
{

    public sealed partial class MainPage : Page
    {
        // 2D arrays are declared with 0 size because they will be overwrited by right resolution
        // so it doesn't use memory if they won't be used
        bool algorithm1 = false;
        bool algorithm2 = false;
        bool algorithm3 = false;
        bool algorithm4 = false;
        byte how_much = 0;
        byte[,] r_test = new byte[0, 0];
        byte[,] g_test = new byte[0, 0];
        byte[,] b_test = new byte[0, 0];
        byte[,] r_pcb1 = new byte[0, 0];
        byte[,] g_pcb1 = new byte[0, 0];
        byte[,] b_pcb1 = new byte[0, 0];
        byte[,] r_pcb2 = new byte[0, 0];
        byte[,] g_pcb2 = new byte[0, 0];
        byte[,] b_pcb2 = new byte[0, 0];
        byte[,] r_pcb3 = new byte[0, 0];
        byte[,] g_pcb3 = new byte[0, 0];
        byte[,] b_pcb3 = new byte[0, 0];
        byte[,] r_master_simply = new byte[0, 0];
        byte[,] g_master_simply = new byte[0, 0];
        byte[,] b_master_simply = new byte[0, 0];
        byte[,] r_slave_simply = new byte[0, 0];
        byte[,] g_slave_simply = new byte[0, 0];
        byte[,] b_slave_simply = new byte[0, 0];
        byte[,] r_master_edge = new byte[0, 0];
        byte[,] g_master_edge = new byte[0, 0];
        byte[,] b_master_edge = new byte[0, 0];
        byte[,] r_slave_edge = new byte[0, 0];
        byte[,] g_slave_edge = new byte[0, 0];
        byte[,] b_slave_edge = new byte[0, 0];
        byte[] tolerance_settings = new byte[10];
        public bool flag_package;
        byte screen_rotation = 180;
        StorageFolder storage_pictures = KnownFolders.PicturesLibrary;
        StorageFolder storage_package = Package.Current.InstalledLocation;
        DeviceInformationCollection device_info;
        double score_algorithm1 = 0;
        double score_algorithm2 = 0;
        double score_algorithm3 = 0;
        ColorTolerance tolerance = new ColorTolerance();
        Pictures images = new Pictures();
        ImageProcessing ProcessImage = new ImageProcessing();
        CameraControl Camera = new CameraControl();
        EdgeDetection EdgeDetector = new EdgeDetection();

        public MainPage()
        {
            InitializeComponent();
            Camera.Preview = PreviewControl;

            // taken from: https://msdn.microsoft.com/en-gb/library/windows/apps/hh465088.aspx
            Application.Current.Resuming += Application_Resuming;
            Application.Current.Suspending += Application_Suspending;
        }
 
        private void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // disposing recources from the page and turning off camera preview 
            var deferral = e.SuspendingOperation.GetDeferral();
            Camera.Dispose();
            deferral.Complete();
        }
 
        private async void Application_Resuming(object sender, object o)
        {
            TextAlgorithms.Text = "Algorithms:";
            CB1.IsEnabled = false;
            CB2.IsEnabled = false;
            CB3.IsEnabled = false;
            CB1.IsChecked = false;
            CB2.IsChecked = false;
            CB3.IsChecked = false;
            Btn_Test.IsEnabled = false;
            await Camera.Initialize(screen_rotation);
            CB2.IsEnabled = true;
            CB1.IsEnabled = true;
            CB3.IsEnabled = true;
        }
 
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            tolerance = (ColorTolerance)e.Parameter;

            TakenPhoto.Visibility = Visibility.Collapsed;
            TextAlgorithms.Text = "Algorithms:";
            CB1.IsEnabled = false;
            CB2.IsEnabled = false;
            CB3.IsEnabled = false;
            CB1.IsChecked = false;
            CB2.IsChecked = false;
            CB3.IsChecked = false;
            Btn_Test.IsEnabled = false;
            device_info = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            if (device_info.Count < 1)
            {
                TextUp.Foreground = new SolidColorBrush(Colors.Red);
                TextUp.Text = "NO CAMERA";
                TextUp.Visibility = Visibility.Visible;
                await Task.Delay(1500);
                TextUp.Visibility = Visibility.Collapsed;
            }
            await Camera.Initialize(screen_rotation);
            CB2.IsEnabled = true;
            CB1.IsEnabled = true;
            CB3.IsEnabled = true;
        }
 
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (tolerance != null)
            {
                tolerance_settings[0] = tolerance.Red;
                tolerance_settings[1] = tolerance.Green;
                tolerance_settings[2] = tolerance.Blue;
                tolerance_settings[3] = tolerance.PCB;
                tolerance_settings[4] = tolerance.Threshold;
                tolerance_settings[5] = tolerance.Simplifier;
                tolerance_settings[6] = tolerance.EdgeMethod;
                tolerance_settings[7] = tolerance.StrongEdges;
                tolerance_settings[8] = tolerance.FilterBackground;
                tolerance_settings[9] = tolerance.Scale;
            }
            Camera.Dispose();
        }

        public async void Button_Click_4(object sender, RoutedEventArgs e) // Save PCB3 Button
        {
            if (Camera.Capture != null)
            {
                SwitchControls(false);
                await Camera.TakePicture("PCB3.bmp");
                SwitchControls(true);

                TextUp.Foreground = new SolidColorBrush(Colors.Green);
                TextUp.Text = "PCB3 TEMPLATE PICTURE SAVED";
                TextUp.Visibility = Visibility.Visible;
                await Task.Delay(1500);
                TextUp.Visibility = Visibility.Collapsed;
            }
            else
            {
                TextUp.Foreground = new SolidColorBrush(Colors.Red);
                TextUp.Text = "PCB3 FILE SAVING ERROR";
                TextUp.Visibility = Visibility.Visible;
                await Task.Delay(1500);
                TextUp.Visibility = Visibility.Collapsed;
            }
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e) // Start test Button
        {
            // set 0 on algorithms scores
            score_algorithm1 = 0;
            score_algorithm2 = 0;
            score_algorithm3 = 0;

            // set last taken parameters from SecondPage
            byte red_tolerance = tolerance_settings[0];
            byte green_tolerance = tolerance_settings[1];
            byte blue_tolerance = tolerance_settings[2];
            byte which_pcb = tolerance_settings[3];
            byte threshold = tolerance_settings[4];
            byte simplify_ratio = tolerance_settings[5];
            byte edge_method = tolerance_settings[6];
            byte strong_edges = tolerance_settings[7];
            byte filter_background = tolerance_settings[8];
            byte scale = tolerance_settings[9];

            // disable any controls during the test
            SwitchControls(false);

            // set new parameters if they were sent from SecondPage
            // value could be null if user will move from ThirdPage to MainPage
            // that's why there is tolerance_settings array that stores last saved parameters 
            if (tolerance != null)
            {
                which_pcb = tolerance.PCB;
                threshold = tolerance.Threshold;
                simplify_ratio = tolerance.Simplifier;
                red_tolerance = tolerance.Red;
                green_tolerance = tolerance.Green;
                blue_tolerance = tolerance.Blue;
                edge_method = tolerance.EdgeMethod;
                strong_edges = tolerance.StrongEdges;
                filter_background = tolerance.FilterBackground;
                scale = tolerance.Scale;
            }

            // if camera preview is turned on, a picture is taken and while doing visual inspection the preview 
            // is disabled and changed with an image so there is less processor usage 
            if (Camera.Capture != null)
            {
                await Camera.TakePicture("TEST.bmp");
                Camera.Dispose();
                if (which_pcb == 1)
                    TakenPhoto.Source = new BitmapImage(new Uri("ms-appx:///Assets/ArduinoUNO.png"));
                if (which_pcb == 2)
                    TakenPhoto.Source = new BitmapImage(new Uri("ms-appx:///Assets/BoardUNO.png"));
                if (which_pcb == 3)
                    TakenPhoto.Source = new BitmapImage(new Uri("ms-appx:///Assets/CustomBoard.png"));
                TakenPhoto.Visibility = Visibility.Visible;
            }

            // GetPixel function is called and values are written to tested pcb color arrays
            await ProcessImage.GetPixel(storage_pictures, "TEST.BMP", scale, "TEST-resized.BMP");
            r_test = ProcessImage.RedColorArray;
            g_test = ProcessImage.GreenColorArray;
            b_test = ProcessImage.BlueColorArray;

            // depending on choosen PCB and rescalling the right values are assigned
            // to color arrays of choosen PCB and compared to tested pcb 
            if (which_pcb == 1)
            {
                if (scale == 25)
                    await ProcessImage.GetPixel(storage_package, @"Assets\PCB1-resized25.BMP");
                else if (scale == 50)
                    await ProcessImage.GetPixel(storage_package, @"Assets\PCB1-resized50.BMP");
                else
                    await ProcessImage.GetPixel(storage_package, @"Assets\PCB1.BMP");
                r_pcb1 = ProcessImage.RedColorArray;
                g_pcb1 = ProcessImage.GreenColorArray;
                b_pcb1 = ProcessImage.BlueColorArray;
                await ProcessImage.ComparePictures(r_pcb1, g_pcb1, b_pcb1, r_test, g_test, b_test, "HeatMap.BMP", "ScoreMap.BMP", red_tolerance, green_tolerance, blue_tolerance, filter_background);
            }
            if (which_pcb == 2)
            {
                if (scale == 25)
                    await ProcessImage.GetPixel(storage_package, @"Assets\PCB2-resized25.BMP");
                else if (scale == 50)
                    await ProcessImage.GetPixel(storage_package, @"Assets\PCB2-resized50.BMP");
                else
                    await ProcessImage.GetPixel(storage_package, @"Assets\PCB2.BMP");
                r_pcb2 = ProcessImage.RedColorArray;
                g_pcb2 = ProcessImage.GreenColorArray;
                b_pcb2 = ProcessImage.BlueColorArray;
                await ProcessImage.ComparePictures(r_pcb2, g_pcb2, b_pcb2, r_test, g_test, b_test, "HeatMap.BMP", "ScoreMap.BMP", red_tolerance, green_tolerance, blue_tolerance, filter_background);
            }
            if (which_pcb == 3)
            {
                await ProcessImage.GetPixel(storage_pictures, "PCB3.BMP", scale, "PCB3-resized.BMP");
                r_pcb3 = ProcessImage.RedColorArray;
                g_pcb3 = ProcessImage.GreenColorArray;
                b_pcb3 = ProcessImage.BlueColorArray;
                await ProcessImage.ComparePictures(r_pcb3, g_pcb3, b_pcb3, r_test, g_test, b_test, "HeatMap.BMP", "ScoreMap.BMP", red_tolerance, green_tolerance, blue_tolerance, filter_background);
            }

            // writting score values from ProccessImage object (from ImageProccessing class) 
            // to auxiliary values and ThirdPage parameters 
            score_algorithm1 = ProcessImage.Score;
            images.ResultBackgroundRaw = ProcessImage.BackgroundPixelCount;
            images.ResultGoodPixelRaw = ProcessImage.GoodPixelCount;
            images.ResultBadPixelRaw = ProcessImage.BadPixelCount;
            
            // if 2nd algorithm is choosen, execute SetPixel function for tested pcb and template pcb and save to variables
            if (algorithm2 == true)
            {
                await ProcessImage.SetPixel(r_test, g_test, b_test, "Slave_Simplified.BMP", simplify_ratio);

                if (which_pcb == 1)
                    await ProcessImage.SetPixel(r_pcb1, g_pcb1, b_pcb1, "Master_Simplified.BMP", simplify_ratio);
                if (which_pcb == 2)
                    await ProcessImage.SetPixel(r_pcb2, g_pcb2, b_pcb2, "Master_Simplified.BMP", simplify_ratio);
                if (which_pcb == 3)
                    await ProcessImage.SetPixel(r_pcb3, g_pcb3, b_pcb3, "Master_Simplified.BMP", simplify_ratio );

                await ProcessImage.GetPixel(storage_pictures, "Slave_Simplified.BMP");
                r_slave_simply = ProcessImage.RedColorArray;
                g_slave_simply = ProcessImage.GreenColorArray;
                b_slave_simply = ProcessImage.BlueColorArray;

                await ProcessImage.GetPixel(storage_pictures, "Master_Simplified.BMP");
                r_master_simply = ProcessImage.RedColorArray;
                g_master_simply = ProcessImage.GreenColorArray;
                b_master_simply = ProcessImage.BlueColorArray;

                await ProcessImage.ComparePictures(r_master_simply, g_master_simply, b_master_simply, r_slave_simply, g_slave_simply, b_slave_simply, "SimplifiedHeatMap.BMP", "SimplifiedScoreMap.BMP", 0, 0, 0, filter_background, 7, true);

                score_algorithm2 = ProcessImage.Score;
                images.ResultBackgroundSimplified = ProcessImage.BackgroundPixelCount;
                images.ResultGoodPixelSimplified = ProcessImage.GoodPixelCount;
                images.ResultBadPixelSimplified = ProcessImage.BadPixelCount;
            }

            // if 3rd algorithm is choosen, execute ConvolutionFilter for tested and template pcbs with right scale
            if (algorithm3 == true)
            {
                if (which_pcb == 1)
                {
                    if (scale == 25)
                        await EdgeDetector.ConvolutionFilter(storage_package, @"Assets\PCB1-resized25.bmp", edge_method, strong_edges, "Master_Edge.bmp");
                    else if (scale == 50)
                        await EdgeDetector.ConvolutionFilter(storage_package, @"Assets\PCB1-resized50.bmp", edge_method, strong_edges, "Master_Edge.bmp");
                    else
                        await EdgeDetector.ConvolutionFilter(storage_package, @"Assets\PCB1.bmp", edge_method, strong_edges, "Master_Edge.bmp");
                }

                if (which_pcb == 2)
                {
                    if (scale == 25)
                        await EdgeDetector.ConvolutionFilter(storage_package, @"Assets\PCB2-resized25.bmp", edge_method, strong_edges, "Master_Edge.bmp");
                    else if (scale == 50)
                        await EdgeDetector.ConvolutionFilter(storage_package, @"Assets\PCB2-resized50.bmp", edge_method, strong_edges, "Master_Edge.bmp");
                    else
                        await EdgeDetector.ConvolutionFilter(storage_package, @"Assets\PCB2.bmp", edge_method, strong_edges, "Master_Edge.bmp");
                }

                if (which_pcb == 3)
                {
                    if (scale != 100)
                        await EdgeDetector.ConvolutionFilter(storage_pictures, "PCB3-resized.bmp", edge_method, strong_edges, "Master_Edge.bmp");
                    else
                        await EdgeDetector.ConvolutionFilter(storage_pictures, "PCB3.bmp", edge_method, strong_edges, "Master_Edge.bmp");
                }

                // writting values to color arrays of template pcb
                await ProcessImage.GetPixel(storage_pictures, "Master_Edge.BMP");
                r_master_edge = ProcessImage.RedColorArray;
                g_master_edge = ProcessImage.GreenColorArray;
                b_master_edge = ProcessImage.BlueColorArray;

                // writting values to color arrays of template pcb
                if (scale != 100)
                    await EdgeDetector.ConvolutionFilter(storage_pictures, "TEST-resized.bmp", edge_method, strong_edges, "Slave_Edge.bmp");
                else
                    await EdgeDetector.ConvolutionFilter(storage_pictures, "TEST.bmp", edge_method, strong_edges, "Slave_Edge.bmp");

                await ProcessImage.GetPixel(storage_pictures, "Slave_Edge.BMP");
                r_slave_edge = ProcessImage.RedColorArray;
                g_slave_edge = ProcessImage.GreenColorArray;
                b_slave_edge = ProcessImage.BlueColorArray;

                // comparing color arrays of tested pcb and template pcb.
                await ProcessImage.ComparePictures(r_master_edge, g_master_edge, b_master_edge, r_slave_edge, g_slave_edge, b_slave_edge, "EdgeHeatMap.BMP", "EdgeScoreMap.BMP", red_tolerance, green_tolerance, blue_tolerance, filter_background, 1, false, true);

                // save score and parameters
                score_algorithm3 = ProcessImage.Score;
                images.ResultBackgroundEdge = ProcessImage.BackgroundPixelCount;
                images.ResultGoodPixelEdge = ProcessImage.GoodPixelCount;
                images.ResultBadPixelEdge = ProcessImage.BadPixelCount;
            }

            // sending parameters to Images object (from Pictures class) 
            // which later will be used on ThirdPage 
            images.PCB = which_pcb;

            if (algorithm1 == true)
            {
                images.ResultRaw = score_algorithm1;
                images.AlgorithmRaw = true;
            }
            else images.AlgorithmRaw = false;

            if (algorithm2 == true)
            {
                images.AlgorithmSimplified = true;
                images.ResultSimplified = score_algorithm2;
            }
            else images.AlgorithmSimplified = false;

            if (algorithm3 == true)
            {
                images.AlgorithmEdge = true;
                images.ResultEdge = score_algorithm3;
            }
            else images.AlgorithmEdge = false;

            images.IsTested = true;

            // execute function that show results of used algoritms and unlock controls 
            ShowResult(score_algorithm1, score_algorithm2, score_algorithm3, threshold);
            SwitchControls(true);

            // restart camera preview
            if (Camera.Capture == null && device_info.Count > 0)
            {
                TakenPhoto.Visibility = Visibility.Collapsed;
                TakenPhoto.Source = null;
                await Camera.Initialize(screen_rotation);
            }
        }

        private async void ShowResult(double score1, double score2, double score3, uint score_threshold)
        {
            byte passed = 0;
            byte failed = 0;

            if (score1 == 0)
                TextUp.Text = "";
            else if (score1 > 0 && score1 <score_threshold)
            {
                TextUp.Text = "1: FAIL   ";
                failed++;
            }
            else
            {
                TextUp.Text = "1: PASS   ";
                passed++;
            }

            if (score2 > 0 && score2 < score_threshold)
            {
                failed++;
                TextUp.Text += "2: FAIL   ";
            }
            else if (score2 >= score_threshold)
            {
                passed++;
                TextUp.Text += "2: PASS   ";
            }

            if (score3 > 0 && score3 < score_threshold)
            {
                failed++;
                TextUp.Text += "3: FAIL   ";
            }
            else if (score3 >= score_threshold)
            {
                passed++;
                TextUp.Text += "3: PASS   ";
            }

            if (passed == 0)
                TextUp.Foreground = new SolidColorBrush(Colors.Red);
            else if (failed == 0)
                TextUp.Foreground = new SolidColorBrush(Colors.Green);
            else
                TextUp.Foreground = new SolidColorBrush(Colors.Yellow);

            TextUp.Visibility = Visibility.Visible;
            await Task.Delay(3000);
            TextUp.Visibility = Visibility.Collapsed;

        }

        public void SwitchControls(bool enable)
        {
            if (enable == true)
            {
                CB1.IsEnabled = true;
                CB2.IsEnabled = true;
                CB3.IsEnabled = true;
                Btn_Test.IsEnabled = true;
                Btn_Pic.IsEnabled = true;
                Btn_Exit.IsEnabled = true;
                settings_hyperlink.IsEnabled = true;
                report_hyperlink.IsEnabled = true;
            }
            else
            {
                CB1.IsEnabled = false;
                CB2.IsEnabled = false;
                CB3.IsEnabled = false;
                Btn_Test.IsEnabled = false;
                Btn_Pic.IsEnabled = false;
                Btn_Exit.IsEnabled = false;
                settings_hyperlink.IsEnabled = false;
                report_hyperlink.IsEnabled = false;
            }
        }

        public void How_much_algorithms()
        {
            how_much = 0;
            if (CB1.IsChecked == true)
            {
                how_much++;
                algorithm1 = true;
            }
            else algorithm1 = false;
            if (CB2.IsChecked == true)
            {
                how_much++;
                algorithm2 = true;
            }
            else algorithm2 = false;
            if (CB3.IsChecked == true)
            {
                how_much++;
                algorithm3 = true;
            }
            else algorithm3 = false;
            if (CB4.IsChecked == true)
            {
                how_much++;
                algorithm4 = true;
            }
            else algorithm4 = false;
            if (how_much == 0)
            {
                TextAlgorithms.Text = "Algorithms:";
                Btn_Test.IsEnabled = false;
            }
            else
            {
                TextAlgorithms.Text = "Algorithms(" + Convert.ToString(how_much) + "):";
                Btn_Test.IsEnabled = true;
            }
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e) // Settings
        {
            this.Frame.Navigate(typeof(SecondPage));
        }

        public void Button_Click(object sender, RoutedEventArgs e) // Exit Button
        {
            Application.Current.Exit();
        }

        private void CB1_CheckedChanged(Object sender, RoutedEventArgs e) // Algorithm 1 CheckBox
        {
            How_much_algorithms();
        }

        private void CB2_CheckedChanged(Object sender, RoutedEventArgs e) // Algorithm 2 CheckBox
        {
            How_much_algorithms();
        }

        private void CB3_CheckedChanged(Object sender, RoutedEventArgs e) // Algorithm 3 CheckBox
        {
            How_much_algorithms();
        }

        private void CB4_CheckedChanged(Object sender, RoutedEventArgs e) // Algorithm 4 CheckBox
        {
            How_much_algorithms();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) // Report
        {
            this.Frame.Navigate(typeof(ThirdPage), images);
        }
    }
    
    public class Pictures
    {
        // it's used to send parameters to REPORT page
        public byte PCB { get; set; }
        public bool AlgorithmRaw { get; set; }
        public double ResultRaw { get; set; }
        public uint ResultGoodPixelRaw { get; set; }
        public uint ResultBadPixelRaw { get; set; }
        public uint ResultBackgroundRaw { get; set; }
        public uint ResultGoodPixelSimplified { get; set; }
        public uint ResultBadPixelSimplified { get; set; }
        public uint ResultBackgroundSimplified { get; set; }
        public double ResultSimplified { get; set; }
        public bool AlgorithmSimplified { get; set; }
        public bool AlgorithmEdge { get; set; }
        public double ResultEdge { get; set; }
        public uint ResultGoodPixelEdge { get; set; }
        public uint ResultBadPixelEdge { get; set; }
        public uint ResultBackgroundEdge { get; set; }
        public bool IsTested { get; set; }
    }

}





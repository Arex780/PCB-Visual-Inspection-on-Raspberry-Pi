using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace PCB_Visual_Inspection_v2
{

    public sealed partial class SecondPage : Page
    {
        byte which_pcb = 1;
        public byte rt = 255*10/100;
        public byte gt = 255*10/100;
        public byte bt = 255*10/100;
        ColorTolerance tolerance = new ColorTolerance();
        public byte threshold = 70;
        public byte simplify_ratio = 85;
        public byte edge_method = 2;
        public byte strong_edges = 1;
        public byte filter_background = 1;
        public byte scale = 100;

        public SecondPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += Application_Suspending;

            PCB1.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
            PCB2.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            PCB2.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            Btn_Pic.IsEnabled = false;
            CB_Edge.IsChecked = true;
            CB_Background.IsChecked = true;
            PCB1_Image.Visibility = Visibility.Visible;
            PCB2_Image.Visibility = Visibility.Collapsed;
            PCB3_Image.Visibility = Visibility.Collapsed;
            ScoreComboBox.SelectedIndex = 2;
            RedComboBox.SelectedIndex = 1;
            GreenComboBox.SelectedIndex = 1;
            BlueComboBox.SelectedIndex = 1;
            EdgeComboBox.SelectedIndex = 2;
            SimplifyComboBox.SelectedIndex = 0;
            ScaleComboBox.SelectedIndex = 0;
        }

        private void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        public void ChangePicture(byte pcb_nr)
        {
            switch(pcb_nr)
            {
                case 1:
                    PCB1_Image.Visibility = Visibility.Visible;
                    PCB2_Image.Visibility = Visibility.Collapsed;
                    PCB3_Image.Visibility = Visibility.Collapsed; break;
                case 2:
                    PCB1_Image.Visibility = Visibility.Collapsed;
                    PCB2_Image.Visibility = Visibility.Visible;
                    PCB3_Image.Visibility = Visibility.Collapsed; break;
                case 3:
                    PCB1_Image.Visibility = Visibility.Collapsed;
                    PCB2_Image.Visibility = Visibility.Collapsed;
                    PCB3_Image.Visibility = Visibility.Visible; break;
            }
        }

        public void SetTolerance()
        {
            tolerance.Red = rt;
            tolerance.Green = gt;
            tolerance.Blue = bt;
            tolerance.PCB = which_pcb;
            tolerance.Threshold = threshold;
            tolerance.Simplifier = simplify_ratio;
            tolerance.EdgeMethod = edge_method;
            tolerance.StrongEdges = strong_edges;
            tolerance.FilterBackground = filter_background;
            tolerance.Scale = scale;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e) // PCB1
        {
            PCB1.Background = new SolidColorBrush(Color.FromArgb(255,0,120,215));
            PCB2.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            PCB3.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            Btn_Pic.IsEnabled = false;
            which_pcb = 1;
            ChangePicture(which_pcb);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e) // PCB2
        {
            PCB2.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
            PCB1.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            PCB3.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            Btn_Pic.IsEnabled = false;
            which_pcb = 2;
            ChangePicture(which_pcb);
        }

        private void Button_Click_3(object sender, RoutedEventArgs e) // PCB3
        {
            PCB3.Background = new SolidColorBrush(Color.FromArgb(255, 0, 120, 215));
            PCB2.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            PCB1.Background = new SolidColorBrush(Color.FromArgb(255, 51, 51, 51));
            Btn_Pic.IsEnabled = true;
            which_pcb = 3;
            ChangePicture(which_pcb);
        }

        private void RedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Red color tolerance
        {
            string selected_item = e.AddedItems[0].ToString();
            switch (selected_item)
            {
                case "±5%":
                    rt = 255*5/100;
                    break;
                case "±10%":
                    rt = 255*10/100;
                    break;
                case "±15%":
                    rt = 255*15/100;
                    break;
                case "±20%":
                    rt = 255*20/100;
                    break;
            }
        }


        private void GreenComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Green color tolerance
        {
            string selected_item = e.AddedItems[0].ToString();
            switch (selected_item)
            {
                case "±5%":
                    gt = 255*5/100;
                    break;
                case "±10%":
                    gt = 255*10/100;
                    break;
                case "±15%":
                    gt = 255*15/100;
                    break;
                case "±20%":
                    gt = 255*20/100;
                    break;
            }
        }

        private void BlueComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Blue color tolerance
        {
            string selected_item = e.AddedItems[0].ToString();
            switch (selected_item)
            {
                case "±5%":
                    bt = 255*5/100;
                    break;
                case "±10%":
                    bt = 255*10/100;
                    break;
                case "±15%":
                    bt = 255*15/100;
                    break;
                case "±20%":
                    bt = 255*20/100;
                    break;
            }
        }

        private void ScoreComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Test threshold
        {
            string selected_item = e.AddedItems[0].ToString();
            switch (selected_item)
            {
                case "50%":
                    threshold = 50;
                    break;
                case "60%":
                    threshold = 60;
                    break;
                case "70%":
                    threshold = 70;
                    break;
                case "80%":
                    threshold = 80;
                    break;
                case "90%":
                    threshold = 90;
                    break;
            }
        }

        private void SimplifyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Simplify colors
        {
            string selected_item = e.AddedItems[0].ToString();
            switch (selected_item)
            {
                case "to 3³":
                    simplify_ratio = 85;
                    break;
                case "to 4³":
                    simplify_ratio = 64;
                    break;
                case "to 5³":
                    simplify_ratio = 51;
                    break;
            }
        }

        private void EdgeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Edge detection method
        {
            string selected_item = e.AddedItems[0].ToString();
            switch (selected_item)
            {
                case "with Sobel operator":
                    edge_method = 0;
                    break;
                case "with Prewitt operator":
                    edge_method = 1;
                    break;
                case "with Kirsch operator":
                    edge_method = 2;
                    break;
            }
        }

        private void ScaleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) // Image scalling
        {
            string selected_item = e.AddedItems[0].ToString();
            switch (selected_item)
            {
                case "100%":
                    scale = 100;
                    break;
                case "50%":
                    scale = 50;
                    break;
                case "25%":
                    scale = 25;
                    break;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // Exit
        {
            Application.Current.Exit();
        }

        private void Button_Click_4(object sender, RoutedEventArgs e) // Save PCB3
        {
            SetTolerance();
            this.Frame.Navigate(typeof(MainPage), tolerance);
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) // Camera preview
        {
            SetTolerance();
            this.Frame.Navigate(typeof(MainPage), tolerance);
        }

        private void CB_Edge_Click(object sender, RoutedEventArgs e) // Only strong edges
        {
            if (CB_Edge.IsChecked == true)
                strong_edges = 1;
            else
                strong_edges = 0;
        }

        private void CB_Background_Click(object sender, RoutedEventArgs e) // Background filtration
        {
            if (CB_Background.IsChecked == true)
                filter_background = 1;
            else
                filter_background = 0;
        }

    }

    public class ColorTolerance
    {
        // class which sends parameters to CAMERA PREVIEW page
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }
        public byte PCB { get; set; }
        public byte Threshold { get; set; }
        public byte Simplifier { get; set; }
        public byte EdgeMethod { get; set; }
        public byte StrongEdges { get; set; }
        public byte FilterBackground { get; set; }
        public byte Scale { get; set; }
    }
}

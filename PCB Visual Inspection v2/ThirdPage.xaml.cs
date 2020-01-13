using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.ApplicationModel;
using Windows.Storage;

namespace PCB_Visual_Inspection_v2
{
 
    public sealed partial class ThirdPage : Page
    {
        Pictures images = new Pictures();
        double score1_true_counter;
        double score1_false_counter;
        double score1_background_counter;
        double score2_true_counter;
        double score2_false_counter;
        double score2_background_counter;
        double score3_true_counter;
        double score3_false_counter;
        double score3_background_counter;
        bool zoom_map = false;
        bool zoom_pcb_raw = false;
        bool zoom_pcb_simply = false;
        bool zoom_pcb_edge = false;
        bool zoom_test_raw = false;
        bool zoom_test_simply = false;
        bool zoom_test_edge = false;
        string image_location;
        StorageFolder storage_location;
        
        public ThirdPage()
        {
            this.InitializeComponent();
            Application.Current.Suspending += Application_Suspending;
        }

        private void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            images = (Pictures)e.Parameter;
            MapComboBox.SelectedIndex = 0;
            SetImages();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            TEST_simply.Source = null;
            TEST_raw.Source = null;
            TEST_edge.Source = null;
            PCB_raw.Source = null;
            PCB_simply.Source = null;
            PCB_edge.Source = null;
            image_map.Source = null;
            score1_text.Visibility = Visibility.Collapsed;
            score2_text.Visibility = Visibility.Collapsed;
            score3_text.Visibility = Visibility.Collapsed;
            map_true_text.Visibility = Visibility.Collapsed;
            map_false_text.Visibility = Visibility.Collapsed;
            map_background_text.Visibility = Visibility.Collapsed;
            MapComboBox.SelectedIndex = 0;
            ErrorText.Visibility = Visibility.Collapsed;
        }

        public async void DisplayImage(byte pcb_number)
        {
            switch (pcb_number)
            {
                case 0: image_location = "TEST.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 1: image_location = @"Assets\PCB1.BMP"; storage_location = Package.Current.InstalledLocation; break;
                case 2: image_location = @"Assets\PCB2.BMP"; storage_location = Package.Current.InstalledLocation; break;
                case 3: image_location = "PCB3.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 4: image_location = "Master_Simplified.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 5: image_location = "Slave_Simplified.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 6: image_location = "HeatMap.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 7: image_location = "ScoreMap.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 8: image_location = "SimplifiedScoreMap.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 9: image_location = "Master_Edge.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 10: image_location = "Slave_Edge.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
                case 11: image_location = "EdgeScoreMap.BMP"; storage_location = KnownFolders.PicturesLibrary; break;
            }
            StorageFile file = await storage_location.GetFileAsync(image_location);
            if (file != null)
            {
                // Open a stream for the selected file. The 'using' block ensures the stream is disposed after the image is loaded.
                using (Windows.Storage.Streams.IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read))
                {
                    // Set the image source to the selected bitmap.
                    BitmapImage image = new BitmapImage();
                    image.SetSource(fileStream);
                    switch (pcb_number)
                    {
                        case 0: TEST_raw.Source = image; break; // Test image
                        case 1: PCB_raw.Source = image; break; // PCB1
                        case 2: PCB_raw.Source = image; break; // PCB2
                        case 3: PCB_raw.Source = image; break; // PCB3
                        case 4: PCB_simply.Source = image; break; // Algorithm2 - Master
                        case 5: TEST_simply.Source = image; break; // Algorithm2 - Slave
                        case 6: image_map.Source = image; break; // HeatMap
                        case 7: image_map.Source = image; break; // ScoreMap
                        case 8: image_map.Source = image; break; // SimplifiedScoreMap
                        case 9: PCB_edge.Source = image; break; // Algorithm3 - Master
                        case 10: TEST_edge.Source = image; break; // Algorithm3 - Slave
                        case 11: image_map.Source = image; break; // EdgeScoreMap
                    }
                }
            }
        }

        public void SetImages()
        {

            switch (images.PCB)
            {
                case 1: DisplayImage(1); break;
                case 2: DisplayImage(2); break;
                case 3: DisplayImage(3); break;
            }
            if (images.IsTested == true)
            {
                DisplayImage(6); // HeatMap
                MapComboBox.Visibility = Visibility.Visible;
                DisplayImage(0); // Test image

                if (images.AlgorithmRaw == true)
                {
                    score1_text.Text = "SCORE (ALGORITHM 1): " + Convert.ToString(images.ResultRaw)+"%";
                    score1_text.Visibility = Visibility.Visible;
                }
                else score1_text.Visibility = Visibility.Collapsed;

                if (images.AlgorithmSimplified == true)
                {
                    score2_text.Text = "SCORE (ALGORITHM 2): " + Convert.ToString(images.ResultSimplified) + "%";
                    score2_text.Visibility = Visibility.Visible;
                    DisplayImage(5); // Slave Image
                    DisplayImage(4); // Master Image
                }
                else
                {
                    score2_text.Visibility = Visibility.Collapsed;
                    TEST_simply.Source = null;
                    PCB_simply.Source = null;
                }

                if (images.AlgorithmEdge == true)
                {
                    score3_text.Text = "SCORE (ALGORITHM 3): " + Convert.ToString(images.ResultEdge) + "%";
                    score3_text.Visibility = Visibility.Visible;
                    DisplayImage(10); // Slave Image
                    DisplayImage(9); // Master Image
                }
                else
                {
                    score3_text.Visibility = Visibility.Collapsed;
                    TEST_edge.Source = null;
                    PCB_edge.Source = null;
                }
            }
        }

        private void MapComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selected_item = e.AddedItems[0].ToString();
            switch (selected_item)
            {
                case "heat map":
                    if (images.IsTested == true)
                    {
                        DisplayImage(6); // HeatMap
                        image_map.Visibility = Visibility.Visible;
                        ErrorText.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ErrorText.Visibility = Visibility.Visible;
                        image_map.Visibility = Visibility.Collapsed;
                    }
                    map_true_text.Visibility = Visibility.Collapsed;
                    map_false_text.Visibility = Visibility.Collapsed;
                    map_background_text.Visibility = Visibility.Collapsed;
                    break;

                case "score map (algorithm 1)":
                    if (images.IsTested == true && images.AlgorithmRaw == true)
                    {
                        DisplayImage(7); // ScoreMap
                        image_map.Visibility = Visibility.Visible;
                        ErrorText.Visibility = Visibility.Collapsed;
                        score1_true_counter = images.ResultGoodPixelRaw;
                        score1_false_counter = images.ResultBadPixelRaw;
                        score1_background_counter = images.ResultBackgroundRaw;
                        map_true_text.Text = "Good pixel count: " + Convert.ToString(score1_true_counter);
                        map_false_text.Text = "Bad pixel count: " + Convert.ToString(score1_false_counter);
                        map_background_text.Text = "Background pixel count: " + Convert.ToString(score1_background_counter);
                        map_true_text.Visibility = Visibility.Visible;
                        map_false_text.Visibility = Visibility.Visible;
                        map_background_text.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ErrorText.Visibility = Visibility.Visible;
                        image_map.Visibility = Visibility.Collapsed;
                        map_true_text.Visibility = Visibility.Collapsed;
                        map_false_text.Visibility = Visibility.Collapsed;
                        map_background_text.Visibility = Visibility.Collapsed;
                    }
                    break;

                case "score map (algorithm 2)":
                    if (images.IsTested == true && images.AlgorithmSimplified == true)
                    {
                        DisplayImage(8); // SimplifiedScoreMap
                        image_map.Visibility = Visibility.Visible;
                        ErrorText.Visibility = Visibility.Collapsed;
                        score2_true_counter = images.ResultGoodPixelSimplified;
                        score2_false_counter = images.ResultBadPixelSimplified;
                        score2_background_counter = images.ResultBackgroundSimplified;
                        map_true_text.Text = "Good pixel count: " + Convert.ToString(score2_true_counter);
                        map_false_text.Text = "Bad pixel count: " + Convert.ToString(score2_false_counter);
                        map_background_text.Text = "Background pixel count: " + Convert.ToString(score2_background_counter);
                        map_true_text.Visibility = Visibility.Visible;
                        map_false_text.Visibility = Visibility.Visible;
                        map_background_text.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ErrorText.Visibility = Visibility.Visible;
                        image_map.Visibility = Visibility.Collapsed;
                        map_true_text.Visibility = Visibility.Collapsed;
                        map_false_text.Visibility = Visibility.Collapsed;
                        map_background_text.Visibility = Visibility.Collapsed;
                    }
                    break;

                case "score map (algorithm 3)":
                    if (images.IsTested == true && images.AlgorithmEdge == true)
                    {
                        DisplayImage(11); // EdgeScoreMap
                        image_map.Visibility = Visibility.Visible;
                        ErrorText.Visibility = Visibility.Collapsed;
                        score3_true_counter = images.ResultGoodPixelEdge;
                        score3_false_counter = images.ResultBadPixelEdge;
                        score3_background_counter = images.ResultBackgroundEdge;
                        map_true_text.Text = "Good pixel count: " + Convert.ToString(score3_true_counter);
                        map_false_text.Text = "Bad pixel count: " + Convert.ToString(score3_false_counter);
                        map_background_text.Text = "Background pixel count: " + Convert.ToString(score3_background_counter);
                        map_true_text.Visibility = Visibility.Visible;
                        map_false_text.Visibility = Visibility.Visible;
                        map_background_text.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        ErrorText.Visibility = Visibility.Visible;
                        image_map.Visibility = Visibility.Collapsed;
                        map_true_text.Visibility = Visibility.Collapsed;
                        map_false_text.Visibility = Visibility.Collapsed;
                        map_background_text.Visibility = Visibility.Collapsed;
                    }
                    break;
            }
        }

        private void Image_map_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (zoom_map == false)
                zoom_map = true;
            else zoom_map = false;
            if (zoom_map == true)
            {
                image_map.Height = 400;
                image_map.Width = 640;
                image_map.Margin = new Thickness(152, 5, 0, 0);
                PCB_raw.IsTapEnabled = false;
                PCB_simply.IsTapEnabled = false;
                PCB_edge.IsTapEnabled = false;
                TEST_raw.IsTapEnabled = false;
                TEST_simply.IsTapEnabled = false;
                TEST_edge.IsTapEnabled = false;
                PCB_raw.Visibility = Visibility.Collapsed;
                PCB_simply.Visibility = Visibility.Collapsed;
                PCB_edge.Visibility = Visibility.Collapsed;
                TEST_raw.Visibility = Visibility.Collapsed;
                TEST_simply.Visibility = Visibility.Collapsed;
                TEST_edge.Visibility = Visibility.Collapsed;
                MasterText.Visibility = Visibility.Collapsed;
                SlaveText.Visibility = Visibility.Collapsed;
            }
            else
            {
                image_map.Height = 267;
                image_map.Width = 426;
                image_map.Margin = new Thickness(352, 10, 0, 0);
                PCB_raw.IsTapEnabled = true;
                PCB_simply.IsTapEnabled = true;
                PCB_edge.IsTapEnabled = true;
                TEST_raw.IsTapEnabled = true;
                TEST_simply.IsTapEnabled = true;
                TEST_edge.IsTapEnabled = true;
                PCB_raw.Visibility = Visibility.Visible;
                PCB_simply.Visibility = Visibility.Visible;
                PCB_edge.Visibility = Visibility.Visible;
                TEST_raw.Visibility = Visibility.Visible;
                TEST_simply.Visibility = Visibility.Visible;
                TEST_edge.Visibility = Visibility.Visible;
                MasterText.Visibility = Visibility.Visible;
                SlaveText.Visibility = Visibility.Visible;
            }
        }

        private void PCB_raw_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (zoom_pcb_raw == false)
                zoom_pcb_raw = true;
            else zoom_pcb_raw = false;
            if (zoom_pcb_raw == true)
            {
                PCB_raw.Height = 400;
                PCB_raw.Width = 640;
                PCB_raw.Margin = new Thickness(152, 5, 0, 0);
                image_map.IsTapEnabled = false;
                PCB_simply.IsTapEnabled = false;
                PCB_edge.IsTapEnabled = false;
                TEST_raw.IsTapEnabled = false;
                TEST_simply.IsTapEnabled = false;
                TEST_edge.IsTapEnabled = false;
                image_map.Visibility = Visibility.Collapsed;
                PCB_simply.Visibility = Visibility.Collapsed;
                PCB_edge.Visibility = Visibility.Collapsed;
                TEST_raw.Visibility = Visibility.Collapsed;
                TEST_simply.Visibility = Visibility.Collapsed;
                TEST_edge.Visibility = Visibility.Collapsed;
                MasterText.Visibility = Visibility.Collapsed;
                SlaveText.Visibility = Visibility.Collapsed;
            }
            else
            {
                PCB_raw.Height = 100;
                PCB_raw.Width = 150;
                PCB_raw.Margin = new Thickness(19, 38, 0, 0);
                image_map.IsTapEnabled = true;
                PCB_simply.IsTapEnabled = true;
                PCB_edge.IsTapEnabled = true;
                TEST_raw.IsTapEnabled = true;
                TEST_simply.IsTapEnabled = true;
                TEST_edge.IsTapEnabled = true;
                image_map.Visibility = Visibility.Visible;
                PCB_simply.Visibility = Visibility.Visible;
                PCB_edge.Visibility = Visibility.Visible;
                TEST_raw.Visibility = Visibility.Visible;
                TEST_simply.Visibility = Visibility.Visible;
                TEST_edge.Visibility = Visibility.Visible;
                MasterText.Visibility = Visibility.Visible;
                SlaveText.Visibility = Visibility.Visible;
            }
        }

        private void PCB_simply_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (zoom_pcb_simply == false)
                zoom_pcb_simply = true;
            else zoom_pcb_simply = false;
            if (zoom_pcb_simply == true)
            {
                PCB_simply.Height = 400;
                PCB_simply.Width = 640;
                PCB_simply.Margin = new Thickness(152, 5, 0, 0);
                PCB_raw.IsTapEnabled = false;
                image_map.IsTapEnabled = false;
                PCB_edge.IsTapEnabled = false;
                TEST_raw.IsTapEnabled = false;
                TEST_simply.IsTapEnabled = false;
                TEST_edge.IsTapEnabled = false;
                PCB_raw.Visibility = Visibility.Collapsed;
                image_map.Visibility = Visibility.Collapsed;
                PCB_edge.Visibility = Visibility.Collapsed;
                TEST_raw.Visibility = Visibility.Collapsed;
                TEST_simply.Visibility = Visibility.Collapsed;
                TEST_edge.Visibility = Visibility.Collapsed;
                MasterText.Visibility = Visibility.Collapsed;
                SlaveText.Visibility = Visibility.Collapsed;
            }
            else
            {
                PCB_simply.Height = 100;
                PCB_simply.Width = 150;
                PCB_simply.Margin = new Thickness(19, 143, 0, 0);
                PCB_raw.IsTapEnabled = true;
                image_map.IsTapEnabled = true;
                PCB_edge.IsTapEnabled = true;
                TEST_raw.IsTapEnabled = true;
                TEST_simply.IsTapEnabled = true;
                TEST_edge.IsTapEnabled = true;
                PCB_raw.Visibility = Visibility.Visible;
                image_map.Visibility = Visibility.Visible;
                PCB_edge.Visibility = Visibility.Visible;
                TEST_raw.Visibility = Visibility.Visible;
                TEST_simply.Visibility = Visibility.Visible;
                TEST_edge.Visibility = Visibility.Visible;
                MasterText.Visibility = Visibility.Visible;
                SlaveText.Visibility = Visibility.Visible;
            }
        }

        private void PCB_edge_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (zoom_pcb_edge == false)
                zoom_pcb_edge = true;
            else zoom_pcb_edge = false;
            if (zoom_pcb_edge == true)
            {
                PCB_edge.Height = 400;
                PCB_edge.Width = 640;
                PCB_edge.Margin = new Thickness(152, 5, 0, 0);
                PCB_raw.IsTapEnabled = false;
                image_map.IsTapEnabled = false;
                PCB_simply.IsTapEnabled = false;
                TEST_raw.IsTapEnabled = false;
                TEST_simply.IsTapEnabled = false;
                TEST_edge.IsTapEnabled = false;
                PCB_raw.Visibility = Visibility.Collapsed;
                image_map.Visibility = Visibility.Collapsed;
                PCB_simply.Visibility = Visibility.Collapsed;
                TEST_raw.Visibility = Visibility.Collapsed;
                TEST_simply.Visibility = Visibility.Collapsed;
                TEST_edge.Visibility = Visibility.Collapsed;
                MasterText.Visibility = Visibility.Collapsed;
                SlaveText.Visibility = Visibility.Collapsed;
            }
            else
            {
                PCB_edge.Height = 100;
                PCB_edge.Width = 150;
                PCB_edge.Margin = new Thickness(19, 248, 0, 0);
                PCB_raw.IsTapEnabled = true;
                image_map.IsTapEnabled = true;
                PCB_simply.IsTapEnabled = true;
                TEST_raw.IsTapEnabled = true;
                TEST_simply.IsTapEnabled = true;
                TEST_edge.IsTapEnabled = true;
                PCB_raw.Visibility = Visibility.Visible;
                image_map.Visibility = Visibility.Visible;
                PCB_simply.Visibility = Visibility.Visible;
                TEST_raw.Visibility = Visibility.Visible;
                TEST_simply.Visibility = Visibility.Visible;
                TEST_edge.Visibility = Visibility.Visible;
                MasterText.Visibility = Visibility.Visible;
                SlaveText.Visibility = Visibility.Visible;
            }
        }

        private void TEST_raw_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (zoom_test_raw == false)
                zoom_test_raw = true;
            else zoom_test_raw = false;
            if (zoom_test_raw == true)
            {
                TEST_raw.Height = 400;
                TEST_raw.Width = 640;
                TEST_raw.Margin = new Thickness(152, 5, 0, 0);
                PCB_raw.IsTapEnabled = false;
                PCB_simply.IsTapEnabled = false;
                PCB_edge.IsTapEnabled = false;
                image_map.IsTapEnabled = false;
                TEST_simply.IsTapEnabled = false;
                TEST_edge.IsTapEnabled = false;
                PCB_raw.Visibility = Visibility.Collapsed;
                PCB_simply.Visibility = Visibility.Collapsed;
                PCB_edge.Visibility = Visibility.Collapsed;
                image_map.Visibility = Visibility.Collapsed;
                TEST_simply.Visibility = Visibility.Collapsed;
                TEST_edge.Visibility = Visibility.Collapsed;
                MasterText.Visibility = Visibility.Collapsed;
                SlaveText.Visibility = Visibility.Collapsed;
            }
            else
            {
                TEST_raw.Height = 100;
                TEST_raw.Width = 150;
                TEST_raw.Margin = new Thickness(174, 38, 0, 0);
                PCB_raw.IsTapEnabled = true;
                PCB_simply.IsTapEnabled = true;
                PCB_edge.IsTapEnabled = true;
                image_map.IsTapEnabled = true;
                TEST_simply.IsTapEnabled = true;
                TEST_edge.IsTapEnabled = true;
                PCB_raw.Visibility = Visibility.Visible;
                PCB_simply.Visibility = Visibility.Visible;
                PCB_edge.Visibility = Visibility.Visible;
                image_map.Visibility = Visibility.Visible;
                TEST_simply.Visibility = Visibility.Visible;
                TEST_edge.Visibility = Visibility.Visible;
                MasterText.Visibility = Visibility.Visible;
                SlaveText.Visibility = Visibility.Visible;
            }
        }

        private void TEST_simply_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (zoom_test_simply == false)
                zoom_test_simply = true;
            else zoom_test_simply = false;
            if (zoom_test_simply == true)
            {
                TEST_simply.Height = 400;
                TEST_simply.Width = 640;
                TEST_simply.Margin = new Thickness(152, 5, 0, 0);
                PCB_raw.IsTapEnabled = false;
                PCB_simply.IsTapEnabled = false;
                PCB_edge.IsTapEnabled = false;
                image_map.IsTapEnabled = false;
                image_map.IsTapEnabled = false;
                TEST_edge.IsTapEnabled = false;
                PCB_raw.Visibility = Visibility.Collapsed;
                PCB_simply.Visibility = Visibility.Collapsed;
                PCB_edge.Visibility = Visibility.Collapsed;
                image_map.Visibility = Visibility.Collapsed;
                image_map.Visibility = Visibility.Collapsed;
                TEST_edge.Visibility = Visibility.Collapsed;
                MasterText.Visibility = Visibility.Collapsed;
                SlaveText.Visibility = Visibility.Collapsed;
            }
            else
            {
                TEST_simply.Height = 100;
                TEST_simply.Width = 150;
                TEST_simply.Margin = new Thickness(174, 143, 0, 0);
                PCB_raw.IsTapEnabled = true;
                PCB_simply.IsTapEnabled = true;
                PCB_edge.IsTapEnabled = true;
                image_map.IsTapEnabled = true;
                image_map.IsTapEnabled = true;
                TEST_edge.IsTapEnabled = true;
                PCB_raw.Visibility = Visibility.Visible;
                PCB_simply.Visibility = Visibility.Visible;
                PCB_edge.Visibility = Visibility.Visible;
                image_map.Visibility = Visibility.Visible;
                image_map.Visibility = Visibility.Visible;
                TEST_edge.Visibility = Visibility.Visible;
                MasterText.Visibility = Visibility.Visible;
                SlaveText.Visibility = Visibility.Visible;
            } 
        }

        private void TEST_edge_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (zoom_test_edge == false)
                zoom_test_edge = true;
            else zoom_test_edge = false;
            if (zoom_test_edge == true)
            {
                TEST_edge.Height = 400;
                TEST_edge.Width = 640;
                TEST_edge.Margin = new Thickness(152, 5, 0, 0);
                PCB_raw.IsTapEnabled = false;
                PCB_simply.IsTapEnabled = false;
                PCB_edge.IsTapEnabled = false;
                image_map.IsTapEnabled = false;
                image_map.IsTapEnabled = false;
                TEST_simply.IsTapEnabled = false;
                PCB_raw.Visibility = Visibility.Collapsed;
                PCB_simply.Visibility = Visibility.Collapsed;
                PCB_edge.Visibility = Visibility.Collapsed;
                image_map.Visibility = Visibility.Collapsed;
                image_map.Visibility = Visibility.Collapsed;
                TEST_simply.Visibility = Visibility.Collapsed;
                MasterText.Visibility = Visibility.Collapsed;
                SlaveText.Visibility = Visibility.Collapsed;
            }
            else
            {
                TEST_edge.Height = 100;
                TEST_edge.Width = 150;
                TEST_edge.Margin = new Thickness(174, 248, 0, 0);
                PCB_raw.IsTapEnabled = true;
                PCB_simply.IsTapEnabled = true;
                PCB_edge.IsTapEnabled = true;
                image_map.IsTapEnabled = true;
                image_map.IsTapEnabled = true;
                TEST_simply.IsTapEnabled = true;
                PCB_raw.Visibility = Visibility.Visible;
                PCB_simply.Visibility = Visibility.Visible;
                PCB_edge.Visibility = Visibility.Visible;
                image_map.Visibility = Visibility.Visible;
                image_map.Visibility = Visibility.Visible;
                TEST_simply.Visibility = Visibility.Visible;
                MasterText.Visibility = Visibility.Visible;
                SlaveText.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) // Exit
        {
            Application.Current.Exit();
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e) // Camera Preview
        {
            this.Frame.Navigate(typeof(MainPage));
        }

    }
}


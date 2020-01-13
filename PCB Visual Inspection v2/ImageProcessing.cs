using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Diagnostics;


namespace PCB_Visual_Inspection_v2
{
    public class ImageProcessing
    {
        public byte[,] RedColorArray = new byte[0, 0];
        public byte[,] GreenColorArray = new byte[0, 0];
        public byte[,] BlueColorArray = new byte[0, 0];
        public byte[,] AlphaColorArray = new byte[0, 0];
        public uint GoodPixelCount = 0;
        public uint BadPixelCount = 0;
        public uint BackgroundPixelCount = 0;
        public double Score = 0;
        public string ErrorCode;
        public bool[,] BackgroundArray = new bool[0, 0];

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        public async Task GetPixel(StorageFolder storage_location, string image_location, byte scale = 100, string scaled_image_location = "Resized.BMP")
        {
            // if scalling is active, execute Resize function and change file location
            if (scale != 100)
            {
                await Resize(storage_location, image_location, scale, scaled_image_location);
                image_location = scaled_image_location;
            }

            // decoding data stream from file 
            StorageFile file = await storage_location.GetFileAsync(image_location);
            Stream image_stream = await file.OpenStreamForReadAsync();
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(image_stream.AsRandomAccessStream());

            // saving to variables resolution of the image
            ImageHeight = Convert.ToInt32(decoder.PixelHeight);
            ImageWidth = Convert.ToInt32(decoder.PixelWidth);

            // creating byte array to storage information about image's pixels 
            // 1 pixel = 4 bytes (for each channel - A,R,G,B)
            var data = await decoder.GetPixelDataAsync();
            byte[] pixel_array = data.DetachPixelData();

            RedColorArray = new byte[ImageWidth, ImageHeight];
            GreenColorArray = new byte[ImageWidth, ImageHeight];
            BlueColorArray = new byte[ImageWidth, ImageHeight];
            AlphaColorArray = new byte[ImageWidth, ImageHeight];

            // 1st byte is blue color, 2nd - green, 3rd - red, 4th - aplha channel 
            // this 4 byte array is equal to 1 pixel 
            for (int i = 0; i < ImageWidth; i++) 
            {
                for (int j = 0; j < ImageHeight; j++) 
                {
                    BlueColorArray[i, j] = pixel_array[4 * ((ImageWidth * j) + i) + 0]; // kolor niebieski
                    GreenColorArray[i, j] = pixel_array[4 * ((ImageWidth * j) + i) + 1]; // kolor zielony
                    RedColorArray[i, j] = pixel_array[4 * ((ImageWidth * j) + i) + 2]; // kolor czerwony
                    AlphaColorArray[i, j] = pixel_array[4 * ((ImageWidth * j) + i) + 3]; // kanał alfa
                }
            }
        }

        public async Task SetPixel(byte[,] r_color, byte[,] g_color, byte[,] b_color, string file_name, byte simplifier)
        {
            // saving to variables resolution of the image
            int width = r_color.GetLength(0);
            int height = r_color.GetLength(1);

            // simplify colors by dividing without remainder and then multipling by simplifier
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++) 
                    {
                        b_color[i, j] = Convert.ToByte(b_color[i, j] / simplifier);
                        b_color[i, j] *= simplifier;
                        g_color[i, j] = Convert.ToByte(g_color[i, j] / simplifier);
                        g_color[i, j] *= simplifier;
                        r_color[i, j] = Convert.ToByte(r_color[i, j] / simplifier);
                        r_color[i, j] *= simplifier;
                    }
                }
                byte t = 192;
                sbyte border=2; // distance from the central pixel to border of checking mask 
                byte threshold=55; 
                bool is_background = false;

                // setting up background detecting for each simplifier in the program
                if (simplifier == 85)
                    t = 170;
                else if (simplifier == 64)
                    t = 192;
                else 
                    t = 204;

                // algorithm has a deadzone - protection from getting out of array 
                for (int i = border; i < width-border; i=i+border+1) 
                {
                    for (int j = border; j < height- border; j=j+border+1) 
                    {

                            uint goodpixel_counter = 0, badpixel_counter = 0;

                            // checking if pixel is background-pixel 
                            if (r_color[i, j] == t && g_color[i, j] == t && b_color[i, j] == t)
                                is_background = true;
                            else
                                is_background = false;

                            // if pixel is not background-pixel, the algorithm executes cross-checking in 8 directions  
                            // and counts how many positive scores there are - above setpoint 
                            // if there is pixels above setpoint, then every neighbor pixel takes value of center pixel 
                            if (is_background == false)
                            {
                                for (int n = (-border); n <= border; n++) 
                                {
                                    if (n == 0)
                                        continue;
                                    if (r_color[i + n, j + n] == r_color[i, j] && g_color[i + n, j + n] == g_color[i, j] && b_color[i + n, j + n] == b_color[i, j])
                                        goodpixel_counter++;
                                    else badpixel_counter++;

                                    if (r_color[i + n, j - n] == r_color[i, j] && g_color[i + n, j - n] == g_color[i, j] && b_color[i + n, j - n] == b_color[i, j])
                                        goodpixel_counter++;
                                    else badpixel_counter++;

                                    if (r_color[i + n, j + 0] == r_color[i, j] && g_color[i + n, j + 0] == g_color[i, j] && b_color[i + n, j + 0] == b_color[i, j])
                                        goodpixel_counter++;
                                    else badpixel_counter++;

                                    if (r_color[i + n, j + n] == r_color[i, j] && g_color[i + n, j + n] == g_color[i, j] && b_color[i + n, j + n] == b_color[i, j])
                                        goodpixel_counter++;
                                    else badpixel_counter++;
                                }
                                if (100 * goodpixel_counter / (badpixel_counter + goodpixel_counter) > threshold)
                                {
                                    for (int x = -(border); x <= border; x++)
                                    {
                                        for (int y = -(border); y <= border; y++)
                                        {
                                            if (x == 0 & y == 0)
                                                continue;
                                            r_color[i + x, j + y] = r_color[i, j];
                                            g_color[i + x, j + y] = g_color[i, j];
                                            b_color[i + x, j + y] = b_color[i, j];
                                        }
                                    }
                                }
                            }
                    }
                    }
            }

            // WriteableBitmap uses BGRA format - 4 bytes per pixel
            byte[] image_array = new byte[width * height * 4];

            BitmapAlphaMode alpha_mode = BitmapAlphaMode.Ignore;

            // saving to byte array the 2D color arrays 
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image_array[4 * ((width * j) + i) + 0] = b_color[i,j]; // blue
                    image_array[4 * ((width * j) + i) + 1] = g_color[i,j]; // green
                    image_array[4 * ((width * j) + i) + 2] = r_color[i,j]; // red
                }
            }

            // saving data to file with sent image settings (parameters)
            await SaveToFile(image_array, file_name, CreationCollisionOption.ReplaceExisting, BitmapPixelFormat.Bgra8, alpha_mode);
        }

        public async Task ComparePictures(byte[,] r_master, byte[,] g_master, byte[,] b_master, byte[,] r_slave, byte[,] g_slave, byte[,] b_slave , string heatmap_file_name, string scoremap_file_name, byte rt, byte gt, byte bt, byte filter_background = 1, byte edge_threshold = 7, bool is_simplified = false, bool black_background = false)
        {
            // checking if arrays have the same length and if not then set an error code 
            if ((r_master.Length + b_master.Length + g_master.Length) == (b_slave.Length + g_slave.Length + r_slave.Length))
            {
                // saving to variables resolution of the image
                int width = r_master.GetLength(0);
                int height = r_master.GetLength(1);

                // declaration of new 2D arrays 
                BackgroundArray = new bool[width, height];
                byte[,] r_difference = new byte[width, height];
                byte[,] g_difference = new byte[width, height];
                byte[,] b_difference = new byte[width, height];
                byte[,] difference = new byte[width, height];
                byte[,] score = new byte[width, height];

                // reseting counters and score
                GoodPixelCount = 0;
                BadPixelCount = 0;
                BackgroundPixelCount = 0;
                Score = 0;

                // funtion that checks background
                if (filter_background == 1)
                    CheckBackground(r_master, g_master, b_master, r_slave, g_slave, b_slave, edge_threshold,is_simplified, black_background);

                // calculating an absolute value of difference in color between two images 
                    for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        r_difference[i, j] = (byte)Math.Abs(r_master[i, j] - r_slave[i, j]);
                        g_difference[i, j] = (byte)Math.Abs(g_master[i, j] - g_slave[i, j]);
                        b_difference[i, j] = (byte)Math.Abs(b_master[i, j] - b_slave[i, j]);
                        difference[i, j] = (byte)((r_difference[i, j] + g_difference[i, j] + b_difference[i, j]) / 3);

                        // if a pixel is background then set 2 in score array 
                    if (BackgroundArray[i,j] == true)
                        { score[i, j] = 2; BackgroundPixelCount++; }

                        // if a pixel difference is lesser than color difference then set 1 in score array
                        else if (r_difference[i, j] <= rt && g_difference[i, j] <= gt && b_difference[i, j] <= bt)
                        { score[i, j] = 1; GoodPixelCount++; }

                        // if a pixel difference is greater than color difference then set 0 in score array
                        else
                        { score[i, j] = 0; BadPixelCount++; }
                    }
                }

                // calculate the score 
                Score = ((100*GoodPixelCount) / (GoodPixelCount + BadPixelCount));

                // functions that create heatmap and scoremap
                await HeatMap(difference, heatmap_file_name);
                await ScoreMap(score, scoremap_file_name);
            }
            else
                ErrorCode = "Resolution of images are different"; // MessageBox can be used
        }

        private void CheckBackground(byte[,] r_master, byte[,] g_master, byte[,] b_master, byte[,] r_slave, byte[,] g_slave, byte[,] b_slave, byte edge_threshold, bool is_simplified, bool black_background) 
        {
            // saving to variables resolution of the image
            int width = r_master.GetLength(0);
            int height = r_master.GetLength(1);

            // setting background tolerance
            byte tr = 192;
            if (is_simplified == true)
                tr = 170;
            byte tg = 170;
            byte tb = 128;
            double threshold = 90; 

            // start of function that measures elapsed time
            var watch = new Stopwatch();
            watch.Start();

            // this part of function is executed when the background is black - used for edge detection 
            if (black_background == true)
            {
                byte t = 128;
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {

                        // algorithm has a deadzone - protection from getting out of array 
                        if (((i > 1 * edge_threshold) && (i < width - 1 * edge_threshold)) && ((j > 1 * edge_threshold) && (j < height - 1 * edge_threshold)))
                        {
                            uint background_counter = 0;
                            uint foreground_counter = 0;

                            // executing of an algoritms that cross-checks in 8 directions 
                            // and counts how many pixels there are below background tolerance
                            // and tags checked pixel as background or not
                            for (int k = (-edge_threshold); k <= edge_threshold; k++)
                            {
                                if (k == 0)
                                    continue;
                                if (r_master[i + k, j + k] < t && r_slave[i + k, j + k] < t && g_master[i + k, j + k] < t && g_slave[i + k, j + k] < t && b_master[i + k, j + k] < t && b_slave[i + k, j + k] < t)
                                    background_counter++;
                                else foreground_counter++;

                                if (r_master[i + k, j - k] < t && r_slave[i + k, j - k] < t && g_master[i + k, j - k] < t && g_slave[i + k, j - k] < t && b_master[i + k, j + k] < t && b_slave[i + k, j - k] < t)
                                    background_counter++;
                                else foreground_counter++;

                                if (r_master[i + k, j + 0] < t && r_slave[i + k, j + 0] < t && g_master[i + k, j + 0] < t && g_slave[i + k, j + 0] < t && b_master[i + k, j + 0] < t && b_slave[i + k, j + 0] < t)
                                    background_counter++;
                                else foreground_counter++;

                                if (r_master[i + 0, j + k] < t && r_slave[i + 0, j + k] < t && g_master[i + 0, j + k] < t && g_slave[i + 0, j + k] < t && b_master[i + 0, j + k] < t && b_slave[i + 0, j + k] < t)
                                    background_counter++;
                                else foreground_counter++;
                            }
                            if (100 * background_counter / (foreground_counter + background_counter) > threshold)
                                BackgroundArray[i, j] = true;
                            else
                                BackgroundArray[i, j] = false;
                        }
                        else
                            BackgroundArray[i, j] = true;
                    }
                }
            }

            // algorithm for white background - the  difference is in sign 
            // and counts how many pixels there are above background tolerance
            else
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {

                        if (((i > 1 * edge_threshold) && (i < width - 1 * edge_threshold)) && ((j > 1 * edge_threshold) && (j < height - 1 * edge_threshold)))
                        {
                            uint background_counter = 0;
                            uint foreground_counter = 0;

                            for (int k = (-edge_threshold); k <= edge_threshold; k++)
                            {
                                if (k == 0)
                                    continue;
                                if (r_master[i + k, j + k] >= tr && r_slave[i + k, j + k] >= tr && g_master[i + k, j + k] >= tg && g_slave[i + k, j + k] >= tg && b_master[i + k, j + k] >= tb && b_slave[i + k, j + k] >= tb)
                                    background_counter++;
                                else foreground_counter++;

                                if (r_master[i + k, j - k] >= tr && r_slave[i + k, j - k] >= tr && g_master[i + k, j - k] >= tg && g_slave[i + k, j - k] >= tg && b_master[i + k, j + k] >= tb && b_slave[i + k, j - k] >= tb)
                                    background_counter++;
                                else foreground_counter++;

                                if (r_master[i + k, j + 0] >= tr && r_slave[i + k, j + 0] >= tr && g_master[i + k, j + 0] >= tg && g_slave[i + k, j + 0] >= tg && b_master[i + k, j + 0] >= tb && b_slave[i + k, j + 0] >= tb)
                                    background_counter++;
                                else foreground_counter++;

                                if (r_master[i + 0, j + k] >= tr && r_slave[i + 0, j + k] >= tr && g_master[i + 0, j + k] >= tg && g_slave[i + 0, j + k] >= tg && b_master[i + 0, j + k] >= tb && b_slave[i + 0, j + k] >= tb)
                                    background_counter++;
                                else foreground_counter++;
                            }
                            if (100 * background_counter / (foreground_counter + background_counter) > threshold)
                                BackgroundArray[i, j] = true;
                            else
                                BackgroundArray[i, j] = false;
                        }
                        else
                            BackgroundArray[i, j] = true;
                    }
                }
            }

            // end of function that measures elapsed time and displays the value in debugger
            watch.Stop();
            System.Diagnostics.Debug.WriteLine("Time elapsed: " + watch.Elapsed);
        }

        private async Task HeatMap(byte[,] difference , string heatmap_file_name)
        {
            int width = difference.GetLength(0);
            int height = difference.GetLength(1);

            byte[] image_array = new byte[width * height * 4];

            // if difference between pixel colors is higher then the green color is substracted and red is added and multiplied by 5
            // TryToByte prevents from getting out of byte's range (0...255)
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    image_array[4 * ((width * j) + i) + 0] = 0;
                    image_array[4 * ((width * j) + i) + 1] = TryToByte(255, (difference[i, j]), 1, false);
                    image_array[4 * ((width * j) + i) + 2] = TryToByte(0, (difference[i, j]), 5, true);
                    image_array[4 * ((width * j) + i) + 3] = 255;
                }
            }

            await SaveToFile(image_array, heatmap_file_name, CreationCollisionOption.ReplaceExisting, BitmapPixelFormat.Bgra8,BitmapAlphaMode.Ignore);
        }

        private byte TryToByte(byte var1, byte var2, byte multiplier, bool is_addition)
        {
            // returning the value between 0 and 255 
            int val = 0;
            if (is_addition == false)
                val = var1 - multiplier * var2;
            else
                val = var1 + multiplier * var2;
            if (val < 0)
                return 0;
            else if (val > 255)
                return 255;
            else
                return Convert.ToByte(val);
        }

        public async Task ScoreMap(byte[,] score , string scoremap_file_name)
        {
            int width = score.GetLength(0);
            int height = score.GetLength(1);

            byte[] image_array = new byte[width * height * 4];

            // if score[i,j] = 1 (good pixel), set green color
            // if score[i,j] = 0 (bad pixel), set red color 
            // if score[i,j] = 2 (background pixel), set blue color
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (score[i, j] == 1)
                    {
                        image_array[4 * ((width * j) + i) + 0] = 0;
                        image_array[4 * ((width * j) + i) + 1] = 255;
                        image_array[4 * ((width * j) + i) + 2] = 0;
                        image_array[4 * ((width * j) + i) + 3] = 255;
                    }
                    else if (score[i, j] == 0)
                    {
                        image_array[4 * ((width * j) + i) + 0] = 0;
                        image_array[4 * ((width * j) + i) + 1] = 0;
                        image_array[4 * ((width * j) + i) + 2] = 255;
                        image_array[4 * ((width * j) + i) + 3] = 255;
                    }
                    else
                    {
                        image_array[4 * ((width * j) + i) + 0] = 255;
                        image_array[4 * ((width * j) + i) + 1] = 0;
                        image_array[4 * ((width * j) + i) + 2] = 0;
                        image_array[4 * ((width * j) + i) + 3] = 255;
                    }
                }
            }

            await SaveToFile(image_array, scoremap_file_name, CreationCollisionOption.ReplaceExisting, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
        }

        public async Task<StorageFile> SaveToFile(byte[] image_array, string file_name, CreationCollisionOption collision, BitmapPixelFormat image_format, BitmapAlphaMode alpha_mode)
        {
            //  create new bitmap 
            WriteableBitmap image = new WriteableBitmap(ImageWidth, ImageHeight);

            // 'using' ensures that the data stream will be disposed after operation is finished
            using (Stream image_stream = image.PixelBuffer.AsStream())
            {
                await image_stream.WriteAsync(image_array, 0, ImageHeight * ImageWidth * 4);
            }

            // create new file in 'Pictures' folder with sent name and collision setting
            var file = await KnownFolders.PicturesLibrary.CreateFileAsync(file_name, collision);

            // opening file by data stream 
            using (IRandomAccessStream image_stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                // encoding image from created data stream 
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, image_stream);
                Stream pixel_stream = image.PixelBuffer.AsStream();

                // creating an array with data stream's length 
                byte[] pixel_array = new byte[pixel_stream.Length];

                // reading the data 
                await pixel_stream.ReadAsync(pixel_array, 0, pixel_array.Length);

                // encoding the image with parameters below:
                encoder.SetPixelData(image_format, alpha_mode, // format and alpha channel
                                    (uint)image.PixelWidth, // image width
                                    (uint)image.PixelHeight, // image height
                                    96.0, // DPI in width
                                    96.0, // DPI in height
                                    pixel_array); // byte stream
                await encoder.FlushAsync(); // end of encoding
            }
            return file; // returning file
        }

        public async Task Resize(StorageFolder storage_location, string image_location, byte scale, string new_image_location)
        {
            // read file from sent location and name 
            StorageFile file = await storage_location.GetFileAsync(image_location);

            // opening file by data stream 
            using (IRandomAccessStream image_stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                // decoding the image to read its resolution
                var decoder = await BitmapDecoder.CreateAsync(image_stream);

                // requested resolution
                int target_width = (int)decoder.PixelWidth * scale / 100;
                int target_height = (int)decoder.PixelHeight * scale / 100;

                // create new memory stream
                var memory_stream = new InMemoryRandomAccessStream();

                // encoding image from created memory stream 
                BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(memory_stream, decoder);

                // calculate scale ratio
                double ratio_width = (double)target_width / decoder.PixelWidth;
                double ratio_height = (double)target_height / decoder.PixelHeight;
                double ratio_scale = Math.Min(ratio_width, ratio_height);

                // prevent from requested resolution of 0x0 px
                if (target_width == 0)
                    ratio_scale = ratio_height;

                if (target_height == 0)
                    ratio_scale = ratio_width;

                // scaled resolution
                uint scaled_height = (uint)Math.Floor(decoder.PixelHeight * ratio_scale);
                uint scaled_width = (uint)Math.Floor(decoder.PixelWidth * ratio_scale);

                // linear interpolation of bitmap to requested resolution
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                encoder.BitmapTransform.ScaledHeight = scaled_height;
                encoder.BitmapTransform.ScaledWidth = scaled_width;

                //  end of encoding
                await encoder.FlushAsync();

                // starting position of memory stream
                memory_stream.Seek(0);

                var pixel_array = new byte[memory_stream.Size];

                await memory_stream.ReadAsync(pixel_array.AsBuffer(), (uint)memory_stream.Size, InputStreamOptions.None);

                StorageFile new_file = await storage_location.CreateFileAsync(new_image_location, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteBytesAsync(new_file, pixel_array);
            }
        }

        public async Task Rotate(StorageFolder storage_location, string image_location, uint rotation) // unused
        {
            // function that rotates the image
            StorageFile file = await storage_location.GetFileAsync(image_location);
            using (IRandomAccessStream image_stream = await file.OpenAsync(FileAccessMode.ReadWrite),
                                               memory_stream = new InMemoryRandomAccessStream())
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(image_stream);
                BitmapEncoder encoder = await BitmapEncoder.CreateForTranscodingAsync(memory_stream, decoder);
                if (rotation == 90)
                    encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;
                else if (rotation == 180)
                    encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise180Degrees;
                else if (rotation == 270)
                    encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise270Degrees;
                else
                {
                    ErrorCode = "Picture can be rotated only by 90, 180 and 270 degrees";
                    goto end_method;
                }
                await encoder.FlushAsync();
                memory_stream.Seek(0);
                image_stream.Seek(0);
                image_stream.Size = 0;
                await RandomAccessStream.CopyAsync(memory_stream, image_stream);
            end_method:;
            }
        }

        private async Task<Windows.UI.Color> GetColor(string image_name, int x_cord, int y_cord) // unused
        {
            // function that read color value from sent coordinates 
            var file = await KnownFolders.PicturesLibrary.GetFileAsync(image_name);
            var image_stream = await file.OpenStreamForReadAsync();
            var decoder = await BitmapDecoder.CreateAsync(image_stream.AsRandomAccessStream());
            var data = await decoder.GetPixelDataAsync();
            byte[] pixel_array = data.DetachPixelData();

            var k = (x_cord * (int)decoder.PixelWidth + y_cord) * 3;
            return Windows.UI.Color.FromArgb(0, pixel_array[k + 2], pixel_array[k + 1], pixel_array[k + 0]);
        }



    }
}

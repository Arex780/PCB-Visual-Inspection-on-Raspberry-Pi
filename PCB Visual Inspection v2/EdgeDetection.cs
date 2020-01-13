using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Graphics.Imaging;
using System.IO;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace PCB_Visual_Inspection_v2
{
    class EdgeDetection
    {
        // matrixes for Sobel, Prewitt and Kirsch operators 
        private static  double[,] xSobel
        {
            get
            {
                return new double[,]
                {
                    { -1, 0, 1 },
                    { -2, 0, 2 },
                    { -1, 0, 1 }
                };
            }
        }

        private static double[,] ySobel
        {
            get
            {
                return new double[,]
                {
                    {  1,  2,  1 },
                    {  0,  0,  0 },
                    { -1, -2, -1 }
                };
            }
        }

        private static double[,] xPrewitt
        {
            get
            {
                return new double[,]
                { { -1,  0,  1, },
                  { -1,  0,  1, },
                  { -1,  0,  1, }, };
            }
        }

        private static double[,] yPrewitt
        {
            get
            {
                return new double[,]
                { {  1,  1,  1, },
                  {  0,  0,  0, },
                  { -1, -1, -1, }, };
            }
        }

        private static double[,] xKirsch
        {
            get
            {
                return new double[,]
                { {  5,  5,  5, },
                  { -3,  0, -3, },
                  { -3, -3, -3, }, };
            }
        }

        private static double[,] yKirsch
        {
            get
            {
                return new double[,]
                { {  5, -3, -3, },
                  {  5,  0, -3, },
                  {  5, -3, -3, }, };
            }
        }

        public async Task ConvolutionFilter(StorageFolder storage_location, string image_location, byte edge_method, byte only_strong_edges, string output_file_name, bool grayscale = true)
        {
            // convolution kernels for two directions 
            double[,] xkernel = new double[3, 3];
            double[,] ykernel = new double[3, 3];

            // depending on the method - set desired operator
            if (edge_method == 0)
            {
                xkernel = xSobel;
                ykernel = ySobel;
            }
            else if (edge_method == 1)
            {
                xkernel = xPrewitt;
                ykernel = yPrewitt;
            }
            else if (edge_method == 2)
            {
                xkernel = xKirsch;
                ykernel = yKirsch;
            }

            // decode the data stream from the file
            StorageFile file = await storage_location.GetFileAsync(image_location);
            Stream image_stream = await file.OpenStreamForReadAsync();
            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(image_stream.AsRandomAccessStream());

            // save image resolution to variables
            int height = Convert.ToInt32(decoder.PixelHeight);
            int width = Convert.ToInt32(decoder.PixelWidth);

            var data = await decoder.GetPixelDataAsync();
            byte[] pixel_array = data.DetachPixelData();
            byte[] result_array = new byte[height * width * 4];

            // convert the image to greyscale with ITU-R BT.709 parameters
            if (grayscale == true)
            {
                float rgb = 0;
                for (int i = 0; i < pixel_array.Length; i += 4)
                {
                    rgb = pixel_array[i] * 0.2126f;
                    rgb += pixel_array[i + 1] * 0.7152f;
                    rgb += pixel_array[i + 2] * 0.0722f;
                    pixel_array[i] = (byte)rgb;
                    pixel_array[i + 1] = pixel_array[i];
                    pixel_array[i + 2] = pixel_array[i];
                    pixel_array[i + 3] = 255;
                }
            }

            // creating the variables for each pixel color and each kernel's direction (2 directions in this algorithm)
            double xr = 0.0;
            double xg = 0.0;
            double xb = 0.0;
            double yr = 0.0;
            double yg = 0.0;
            double yb = 0.0;

            // final pixel color values
            double r = 0.0;
            double g = 0.0;
            double b = 0.0;

            // border is a distance from center pixel in a matrix to border of a kernel 
            // every operator is a 3x3 matrix so the distance is always equal 1 
            int border = 1;
            int n = 0; // 1 byte in kernel's matrix 
            int k = 0; // 1 byte of image's byte array

            // algorithm has a deadzone - protection from getting out of array when filtering with kernel
            for (int j = border; j < height - border; j++)
            {
                for (int i = border; i < width - border; i++) 
                {
                    // reset RGB values 
                    xr = xg = xb = yr = yg = yb = 0;
                    r = g = b = 0.0;

                    // position of the center pixel in convolution kernel in reference to 1D byte array of an image
                    k = j * width*4 + i * 4; 

                    // convolution of the kernel with an image (filtering)
                    for (int y = -border; y <= border; y++)
                    {
                        for (int x = -border; x <= border; x++)
                        {
                            n = k + x * 4 + y * width*4; 
                            xb += (double)(pixel_array[n]) * xkernel[y + border, x + border];
                            xg += (double)(pixel_array[n + 1]) * xkernel[y + border, x + border];
                            xr += (double)(pixel_array[n + 2]) * xkernel[y + border, x + border];
                            yb += (double)(pixel_array[n]) * ykernel[y + border, x + border];
                            yg += (double)(pixel_array[n + 1]) * ykernel[y + border, x + border];
                            yr += (double)(pixel_array[n + 2]) * ykernel[y + border, x + border];
                        }
                    }

                    // calculate the gradient for RGB values 
                    b = Math.Sqrt((xb * xb) + (yb * yb));
                    g = Math.Sqrt((xg * xg) + (yg * yg));
                    r = Math.Sqrt((xr * xr) + (yr * yr));

                    // prevent the byte of getting out of range 
                    if (b > 255) b = 255;
                    else if (b < 0) b = 0;
                    if (g > 255) g = 255;
                    else if (g < 0) g = 0;
                    if (r > 255) r = 255;
                    else if (r < 0) r = 0;

                    // save informations to new byte array and decide if the image will be with only strong edges (leave only pure white color) or not
                    if (only_strong_edges == 1)
                    {
                        if (b < 255 & g < 255 & r <255)
                        {
                            result_array[k] = 0;
                            result_array[k + 1] = 0;
                            result_array[k + 2] = 0;
                            result_array[k + 3] = 255;
                        }
                        else
                        {
                            result_array[k] = (byte)(b);
                            result_array[k + 1] = (byte)(g);
                            result_array[k + 2] = (byte)(r);
                            result_array[k + 3] = 255;
                        }
                    }
                    else
                    {
                        result_array[k] = (byte)(b);
                        result_array[k + 1] = (byte)(g);
                        result_array[k + 2] = (byte)(r);
                        result_array[k + 3] = 255;
                    }
                }
            }

            WriteableBitmap image = new WriteableBitmap(width, height);

            using (image_stream = image.PixelBuffer.AsStream())
            {
                await image_stream.WriteAsync(result_array, 0, width * height * 4);
            }

            await SaveToFile(image, output_file_name, CreationCollisionOption.ReplaceExisting, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore);
        }

        public async Task<StorageFile> SaveToFile(WriteableBitmap image, string file_name, Windows.Storage.CreationCollisionOption collision, BitmapPixelFormat image_format, BitmapAlphaMode alpha_mode)
        {
            var file = await KnownFolders.PicturesLibrary.CreateFileAsync(file_name, collision);

            using (IRandomAccessStream image_stream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, image_stream);
                Stream pixel_stream = image.PixelBuffer.AsStream();

                byte[] pixel_array = new byte[pixel_stream.Length];

                await pixel_stream.ReadAsync(pixel_array, 0, pixel_array.Length);

                encoder.SetPixelData(image_format, alpha_mode,
                                    (uint)image.PixelWidth, 
                                    (uint)image.PixelHeight, 
                                    96.0, 
                                    96.0, 
                                    pixel_array); 
                await encoder.FlushAsync(); 
            }
            return file;
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.System.Display;
using Windows.Media.Capture;
using Windows.Storage;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Controls;
using Windows.Media.Devices;

namespace PCB_Visual_Inspection_v2
{
    class CameraControl
    {
        // this object gives possibility to capture image from camera module
        public MediaCapture Capture;

        // this object decides whenever camera screen goes to sleep
        private readonly DisplayRequest RequestDisplay = new DisplayRequest();

        // taken from: https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh868174.aspx
        private static readonly Guid RotationKey = new Guid("C380465D-2271-428C-9B83-ECEA3B4A85C1");

        // information about avaible devices 
        DeviceInformationCollection Devices;

        public string ErrorCode;

        // this object is UI element 
        // and allows to preview camera screen in app 
        public CaptureElement Preview { get; set; }

        public async Task Initialize(int rotation_offset)
        {

            // it collects informations about avaible devices that can capture video 
            Devices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);

            // if there is no current preview running in program or and if there is at least 1 camera avaible 
            // then execute operations below - otherwise set error code 
            if (Capture == null && Devices.Count > 0)
            {
                // it chooses first avaible device to capture video 
                var preferredDevice = Devices.FirstOrDefault(); 

                // it creates new object from MediaCapture class to capture image 
                Capture = new MediaCapture();

                // it prevents display from going to sleep when is inactive
                RequestDisplay.RequestActive();

                // initialization of camera preview 
                await Capture.InitializeAsync(
                    new MediaCaptureInitializationSettings
                    {
                        VideoDeviceId = preferredDevice.Id
                    });

                // it saves all supported resolutions of the camera to 1D array 
                var resolutions = Capture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).ToList();

                // it sets the type of saved data as picture with choosen resolution 
                await Capture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, resolutions[47]); // 5 - 1280x720; 47 - 1280x800;

                // it sets the source of UI element as data stream from camera module 
                Preview.Source = Capture;

                // it starts previewing the screen in UI
                await Capture.StartPreviewAsync();

                // it sets rotation (variable is sent to method) of the preview 
                var rotation_settings = Capture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview);
                rotation_settings.Properties.Add(RotationKey, rotation_offset);
                await Capture.SetEncodingPropertiesAsync(MediaStreamType.VideoPreview, rotation_settings, null);
            }
            else
            {
                ErrorCode = "Camera not found";
            }
        }

        public async Task TakePicture(string image_name)
        {

            // it creates the file to which image will be saved to specific path
            var file = await KnownFolders.PicturesLibrary.CreateFileAsync(image_name, CreationCollisionOption.ReplaceExisting);

            // it sets the type of saved picture as a bitmap 
            ImageEncodingProperties Picture = ImageEncodingProperties.CreateBmp();

            // update the file with image data 
            await Capture.CapturePhotoToStorageFileAsync(Picture, file);
        }

        public void Dispose()
        {
            // funtion which turns off the camera preview
            // when on different page
            if (Capture != null)
            {
                Capture.Dispose();
                Capture = null;
            }

            if (Preview.Source != null)
            {
                Preview.Source.Dispose();
                Preview.Source = null;
            }
        }

        public async void Focus() // unused
        {
            // try set contrast
            Capture.VideoDeviceController.Contrast.TrySetAuto(true);

            // it gives information if the device supports focus 
            if (Capture.VideoDeviceController.FocusControl.Supported == true)
            {
                // gain control on focus in Capture object
                var focus_control = Capture.VideoDeviceController.FocusControl;

                // set focus range
                var focus_range = focus_control.SupportedFocusRanges.Contains(AutoFocusRange.FullRange) ? AutoFocusRange.FullRange : focus_control.SupportedFocusRanges.FirstOrDefault();

                // set focus mode
                var focus_mode = focus_control.SupportedFocusModes.Contains(FocusMode.Single) ? FocusMode.Single : focus_control.SupportedFocusModes.FirstOrDefault();

                // configuration with parameters above 
                focus_control.Configure(
                    new FocusSettings
                    {
                        Mode = focus_mode,
                        AutoFocusRange = focus_range
                    });

                // wait until finish focusing 
                await focus_control.FocusAsync();
            }
        }

    }
}

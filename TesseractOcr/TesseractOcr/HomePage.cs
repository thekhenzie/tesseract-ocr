using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Java.Lang;
using Tesseract;
using Xamarin.Forms;
using XLabs.Ioc;
using XLabs.Platform.Device;
using XLabs.Platform.Services.Media;

namespace TesseractOcr
{
    public class HomePage : ContentPage
    {
        private Button _takePictureButton;
        private Button _refreshButton;
        private Label _recognizedTextLabel;

        private Image _takenImage;

        private readonly ITesseractApi _tesseractApi;
        private readonly IDevice _device;

        public HomePage()
        {
            _tesseractApi = Resolver.Resolve<ITesseractApi>();
            _device = Resolver.Resolve<IDevice>();

            BuildUi();

            WireEvents();
        }

        private void BuildUi()
        {
            _takePictureButton = new Button
            {
                Text = "New scan"
            };
            _refreshButton = new Button
            {
                Text = "Refresh"
            };

            _recognizedTextLabel = new Label(){ Text= "Please take a picture", TextColor = Color.Black};

            _takenImage = new Image() { HeightRequest = 200 };

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Children =
                    {
                        _takePictureButton,
                        _refreshButton,
                        _takenImage,
                        _recognizedTextLabel
                    }
                }
            };

        }

        private void WireEvents()
        {
            _takePictureButton.Clicked += TakePictureButton_Clicked;
            _refreshButton.Clicked += RefreshButton_Clicked;

        }

        void RefreshButton_Clicked(object sender, EventArgs e)
        {
            _recognizedTextLabel.Text = "Please take a picture";
            _takenImage.Source = " ";
        }
        async void TakePictureButton_Clicked(object sender, EventArgs e)
        {
            if (!_tesseractApi.Initialized)
                await _tesseractApi.Init("eng");

            var photo = await TakePic();
            if (photo != null)
            {
                var imageBytes = new byte[photo.Source.Length];
                photo.Source.Position = 0;
                photo.Source.Read(imageBytes, 0, (int)photo.Source.Length);
                photo.Source.Position = 0;

                _takenImage.Source = ImageSource.FromStream(() => photo.Source);

                var tessResult = await _tesseractApi.SetImage(imageBytes);
                //var path = $"android.resource://{PackageName}/Resource";
                //var tessResult = await _tesseractApi.SetImage(App._file.Path);
                if (tessResult)
                {
                    _recognizedTextLabel.Text = _tesseractApi.Text;
                }
                else
                {
                    _recognizedTextLabel.Text = "No Data";
                }
            }
        }
        // ...
        //private byte[] ConvertYuvToJpeg(byte[] yuvData, CameraDevice camera)
        //{
        //    var cameraParameters = camera.;
        //    var width = cameraParameters.PreviewSize.Width;
        //    var height = cameraParameters.PreviewSize.Height;
        //    var yuv = new YuvImage(yuvData, cameraParameters.PreviewFormat, width, height, null);
        //    var ms = new MemoryStream();
        //    var quality = 80;   // adjust this as needed
        //    yuv.CompressToJpeg(new Rect(0, 0, width, height), quality, ms);
        //    var jpegData = ms.ToArray();

        //    return jpegData;
        //}
        private async Task<MediaFile> TakePic()
        {
            var mediaStorageOptions = new CameraMediaStorageOptions
            {
                DefaultCamera = CameraDevice.Rear
            };
            var mediaFile = await _device.MediaPicker.TakePhotoAsync(mediaStorageOptions);

            _recognizedTextLabel.Text = "Loading..";
            return mediaFile;
        }
    }
}

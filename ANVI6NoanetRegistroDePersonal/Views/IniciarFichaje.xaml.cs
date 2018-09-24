using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using ANVI6NoanetRegistroDePersonal.Controller;
using Windows.System.Threading;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml.Automation.Peers;
using System.Collections.ObjectModel;
using Windows.Graphics.Imaging;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media;
using Windows.AI.MachineLearning;
using Windows.Storage;



// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ANVI6NoanetRegistroDePersonal.Views
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class IniciarFichaje : Page
    {
        public Brush brush = new SolidColorBrush(Windows.UI.Colors.Green);
        private const string _kModelFileName = "viscoreitano.onnx";//"perros.onnx";//
        private const string _kLabelsFileName = "Labels.json";
        public LearningModel _model = null;
        public LearningModelSession _session;
        public string SeleccionCamara = string.Empty;
        private ScenarioState currentState;
        private ThreadPoolTimer frameProcessingTimer;
        public string SeleccionCamaraID = string.Empty;
        private MediaCapture mediaCapture;
        private VideoEncodingProperties videoProperties;
        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        private enum ScenarioState
        {

            Idle,
            Streaming
        }
        private async void ChangeScenarioState(ScenarioState newState)
        {
            // Disable UI while state change is in progress
            switch (newState)
            {
                case ScenarioState.Idle:


                    this.ShutdownWebCam();

                    this.currentState = newState;
                    break;

                case ScenarioState.Streaming:

                    if (!await this.StartWebcamStreaming())
                    {
                        this.ChangeScenarioState(ScenarioState.Idle);
                        break;
                    }

                    this.currentState = newState;

                    break;
            }
        }
        

        public async void ShutdownWebCam()
        {
            if (this.frameProcessingTimer != null)
            {
                this.frameProcessingTimer.Cancel();
            }

            if (this.mediaCapture != null)
            {
                if (this.mediaCapture.CameraStreamState == Windows.Media.Devices.CameraStreamState.Streaming)
                {
                    try
                    {
                        await this.mediaCapture.StopPreviewAsync();
                    }
                    catch (Exception)
                    {

                    }
                }
                this.mediaCapture.Dispose();
            }

            this.frameProcessingTimer = null;
            this.CamPreview.Source = null;
            this.mediaCapture = null;

        }
        private void MediaCapture_CameraStreamFailed(MediaCapture sender, object args)
        {
            // MediaCapture is not Agile and so we cannot invoke its methods on this caller's thread
            // and instead need to schedule the state change on the UI thread.
            var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                ChangeScenarioState(ScenarioState.Idle);
            });
        }
        private async Task<bool> StartWebcamStreaming()
        {
            var seleccioncamara = SeleccionCamaraID;


            bool successful = true;
            try
            {
                var camara = lstBoxCamaras.SelectedIndex;

                this.mediaCapture = new MediaCapture();
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = StreamingCaptureMode.Video;

                settings.VideoDeviceId = SeleccionCamaraID;




                await mediaCapture.InitializeAsync(settings);
                this.mediaCapture.Failed += this.MediaCapture_CameraStreamFailed;


                var deviceController = mediaCapture.VideoDeviceController;
                this.videoProperties = deviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;

                this.CamPreview.Source = this.mediaCapture;
                await mediaCapture.StartPreviewAsync();

                TimeSpan timerInterval = TimeSpan.FromMilliseconds(88);
               // this.frameProcessingTimer = Windows.System.Threading.ThreadPoolTimer.CreatePeriodicTimer(new Windows.System.Threading.TimerElapsedHandler(ProcessCurrentVideoFrame), timerInterval);
            }
            catch (System.UnauthorizedAccessException ex)
            {
                NotifyUser(ex.Message.ToString(), NotifyType.ErrorMessage);
                
                // If the user has disabled their webcam this exception is thrown; provide a descriptive message to inform the user of this fact.
                //this.rootPage.NotifyUser("Webcam is disabled or access to the webcam is disabled for this app.\nEnsure Privacy Settings allow webcam usage.", NotifyType.ErrorMessage);
                successful = false;
            }
            catch (Exception ex)
            {
                //this.rootPage.NotifyUser(ex.ToString(), NotifyType.ErrorMessage);
                successful = false;
            }
            return successful;

        }
        private void lstBoxCamaras_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SeleccionCamara = ((Windows.UI.Xaml.Controls.Primitives.Selector)sender).SelectedItem.ToString();
            SeleccionCamaraID = ((System.Tuple<string, string>)((Windows.UI.Xaml.Controls.Primitives.Selector)sender).SelectedItem).Item2.ToString();
            ChangeScenarioState(ScenarioState.Idle);
            ChangeScenarioState(ScenarioState.Streaming);




        }
        public void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            
                UpdateStatus(strMessage, type);
          
        }

        private void UpdateStatus(string strMessage, NotifyType type)
        {
            switch (type)
            {
                case NotifyType.StatusMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
                    break;
                case NotifyType.ErrorMessage:
                    StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
                    break;
            }

            StatusBlock.Text = strMessage;

            // Collapse the StatusBlock if it has no text to conserve real estate.
            StatusBorder.Visibility = (StatusBlock.Text != String.Empty) ? Visibility.Visible : Visibility.Collapsed;
            if (StatusBlock.Text != String.Empty)
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusPanel.Visibility = Visibility.Visible;
            }
            else
            {
                StatusBorder.Visibility = Visibility.Collapsed;
                StatusPanel.Visibility = Visibility.Collapsed;
            }

            // Raise an event if necessary to enable a screen reader to announce the status update.
            var peer = FrameworkElementAutomationPeer.FromElement(StatusBlock);
            if (peer != null)
            {
                peer.RaiseAutomationEvent(AutomationEvents.LiveRegionChanged);
            }
        }

        public async Task<bool> IniciarModelo()
        {
            LearningModelDeviceKind GetDeviceKind()
            {
                return LearningModelDeviceKind.Default;
            }

            var modelFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{_kModelFileName}"));
            _model = await LearningModel.LoadFromStorageFileAsync(modelFile);
            _session = new LearningModelSession(_model, new LearningModelDevice(GetDeviceKind()));

            return true;
        }

        public IniciarFichaje()
        {
            this.InitializeComponent();
            ObtenerVideoDevices();
            IniciarModelo().Wait(1000);


        }
        public List<Tuple<string, string>> listaCamaras;
        private async void ObtenerVideoDevices()
        {

            var camaras = await Camaras.ObtenerCamaras();
            listaCamaras = camaras.ToList<Tuple<string, string>>();
            lstBoxCamaras.ItemsSource = camaras;

        }

        private void btnIniciarCamar_Click(object sender, RoutedEventArgs e)
        {
            if (this.currentState == ScenarioState.Streaming)
            {
                //this.rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                this.ChangeScenarioState(ScenarioState.Idle);
                btnIniciarCamar.Content = "Iniciar Camara";
            }
            else
            {

                //this.rootPage.NotifyUser(string.Empty, NotifyType.StatusMessage);
                this.ChangeScenarioState(ScenarioState.Streaming);
                btnIniciarCamar.Content = "Parar Camara";
            }
        }
        public async Task<ObservableCollection<FotosCapttuple>> tuplesDevolver()
        {
            return fotosCapttuple;

        }
        public static ObservableCollection<FotosCapttuple> fotosCapttuple = new ObservableCollection<FotosCapttuple>();
        public ObservableCollection<Tuple<FotosCapttuple>> tuples = new ObservableCollection<Tuple<FotosCapttuple>>();
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var lowLagCapture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));
            var capturedPhoto = await lowLagCapture.CaptureAsync();
            var softwareBitmap = capturedPhoto.Frame.SoftwareBitmap;
            softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);

            VideoFrame videoFrame = VideoFrame.CreateWithSoftwareBitmap(softwareBitmap);
            ObtenerIdentidadONNX obtenerIdentidadONNX = new ObtenerIdentidadONNX();
            var nombre = await obtenerIdentidadONNX.ObtenerIdentidadOnnX(videoFrame, _session);

            List<Empleados> EmpAutorizados = new List<Empleados>();
            EmpAutorizados.Add(new Empleados() { Nombre = "Visco" });
            EmpAutorizados.Add(new Empleados() { Nombre = "Reitano" });

            var buscarEmpleado = EmpAutorizados.Find(x => x.Nombre == nombre);

            if (buscarEmpleado!=null)
            {
                brush = new SolidColorBrush(Windows.UI.Colors.Green);

            }
            else
            {
                brush = new SolidColorBrush(Windows.UI.Colors.Red);
                nombre = "No Autorizado";
            }           
            if (nombre!="")
            {
                var iniciarFichajeController = new Controller.IniciarFichaje();
                DateTime utcDate = DateTime.UtcNow;

                var fotosCapttupleLocal = await iniciarFichajeController.TomarFoto(source, utcDate.ToString(), nombre, softwareBitmap, brush);
                fotosCapttuple.Add(fotosCapttupleLocal);

                tuples.Add(new Tuple<FotosCapttuple>(fotosCapttupleLocal));
            }

            
            await lowLagCapture.FinishAsync();
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
            
        }
        public class Empleados
        {
            public string Nombre { get; set; }
        }
        
    }


}

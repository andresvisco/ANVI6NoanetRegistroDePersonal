using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using ANVI6NoanetRegistroDePersonal.Controller;
using Windows.Graphics.Imaging;
using System.IO;
using Microsoft.ProjectOxford.Face;
using System.Collections.ObjectModel;
using ANVI6NoanetRegistroDePersonal.Views;
using static ANVI6NoanetRegistroDePersonal.Views.SincronizarFichaje;
using Windows.Foundation;
using Windows.Storage.Streams;

namespace ANVI6NoanetRegistroDePersonal.Controller
{
    public class SincronizarAzure
    {
        public ObservableCollection<Empleado> empleadosObS = new ObservableCollection<Empleado>();
        public static async Task<SoftwareBitmap> CreateFromBitmap(SoftwareBitmap softwareBitmap, uint width, uint heigth)
        {
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, stream);


                encoder.SetSoftwareBitmap(softwareBitmap);
                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;



                var ancho = width * (0.2);
                var alto = heigth * (0.2);


                encoder.BitmapTransform.ScaledWidth = (uint)ancho;
                encoder.BitmapTransform.ScaledHeight = (uint)alto;

                await encoder.FlushAsync();

                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                return await decoder.GetSoftwareBitmapAsync(softwareBitmap.BitmapPixelFormat, softwareBitmap.BitmapAlphaMode);
            }
        }
        public async Task<ObservableCollection<Empleado>> ObtenerIdentidades(ObservableCollection<FotosCapttuple> listadoCaras)
        {
            string subscriptionKey = "{your apikey}";
            string subscriptionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/face/v1.0";
            var faceServiceClient = new FaceServiceClient(subscriptionKey, subscriptionEndpoint);

            byte[] arrayByteData;
            foreach (var item in listadoCaras)
            {
                var empleado = new Empleado();

                SoftwareBitmap softwareBitmap = item.TupleFotosCapturadas.Item4;
                SoftwareBitmap softwareBitmapCropped = await CreateFromBitmap(softwareBitmap, (uint)softwareBitmap.PixelWidth, (uint)softwareBitmap.PixelHeight);
                SoftwareBitmap displayableImage = SoftwareBitmap.Convert(softwareBitmapCropped, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);


                arrayByteData = await EncodedBytesClass.EncodedBytes(displayableImage, BitmapEncoder.JpegEncoderId);
                var nuevoStreamFace = new MemoryStream(arrayByteData);

                try
                {
                   
                    var faces = await faceServiceClient.DetectAsync(nuevoStreamFace, true, false);

                    var resultadoIdentifiacion = await faceServiceClient.IdentifyAsync(faces.Select(ff => ff.FaceId).ToArray(), largePersonGroupId: "1");

                        var res = resultadoIdentifiacion[0];

                        if(res.Candidates.Length > 0)
                        {
                            var nombrePersona = await faceServiceClient.GetPersonInLargePersonGroupAsync("1", res.Candidates[0].PersonId);
                            empleado.EmpleadoNombe = nombrePersona.Name.ToString();
                            empleado.Horario = item.TupleFotosCapturadas.Item2.ToString();

                            
                        

                            //var estadoAnimo = 

                        }
                        else
                        {
                            //txtResult.Text = "Unknown";
                        }
                    
                }
                catch (Exception ex)
                {
                    var error = ex.Message.ToString();
                }
                empleadosObS.Add(empleado);

            }


            return empleadosObS;

        }

    }
}

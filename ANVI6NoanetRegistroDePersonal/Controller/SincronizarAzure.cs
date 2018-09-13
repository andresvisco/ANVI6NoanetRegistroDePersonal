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

namespace ANVI6NoanetRegistroDePersonal.Controller
{
    public class SincronizarAzure
    {

        public async Task<bool> ObtenerIdentidades(ObservableCollection<FotosCapttuple> listadoCaras)
        {
            string subscriptionKey = "5ff19b57095a4d10bf64274ed9e6ef30";
            string subscriptionEndpoint = "https://southcentralus.api.cognitive.microsoft.com/face/v1.0";
            var faceServiceClient = new FaceServiceClient(subscriptionKey, subscriptionEndpoint);

            byte[] arrayByteData;
            foreach (var item in listadoCaras)
            {
                SoftwareBitmap softwareBitmap = item.TupleFotosCapturadas.Item4;
                arrayByteData = await EncodedBytesClass.EncodedBytes(softwareBitmap, BitmapEncoder.BmpEncoderId);
                var nuevoStreamFace = new MemoryStream(arrayByteData);

                try
                {
                    IEnumerable<FaceAttributeType> faceAttributes =
                        new FaceAttributeType[] {
                            FaceAttributeType.Gender,
                            FaceAttributeType.Age,
                            FaceAttributeType.Smile,
                            FaceAttributeType.Emotion,
                            FaceAttributeType.Glasses,
                            FaceAttributeType.Hair };
                    var faces = await faceServiceClient.DetectAsync(nuevoStreamFace, true, false, faceAttributes);

                    var resultadoIdentifiacion = await faceServiceClient.IdentifyAsync(faces.Select(ff => ff.FaceId).ToArray(), largePersonGroupId: "1");

                    for (int i = 0; i < faces.Length; i++)
                    {
                        var res = resultadoIdentifiacion[i];

                        if(res.Candidates.Length > 0)
                        {
                            var nombrePersona = await faceServiceClient.GetPersonInLargePersonGroupAsync("1", res.Candidates[0].PersonId);
                            

                            //var estadoAnimo = 

                        }
                        else
                        {
                            //txtResult.Text = "Unknown";
                        }
                    }
                }
                catch (Exception ex)
                {
                    var error = ex.Message.ToString();
                }
            }


            return true;

        }

    }
}

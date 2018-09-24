using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using ANVI6NoanetRegistroDePersonal.Views;
using System.ServiceModel.Dispatcher;
using Windows.Graphics.Imaging;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media;

namespace ANVI6NoanetRegistroDePersonal.Controller
{
    public class IniciarFichaje
    {

        public async Task<FotosCapttuple> TomarFoto(SoftwareBitmapSource imagenCapturada, string fechaHora, string Empleado, SoftwareBitmap softwareBitmapCapturada, Brush colorEllipse)
        {
            var photo = imagenCapturada;
            FotosCapttuple fotosCapt = new FotosCapttuple()
            {
                TupleFotosCapturadas = new Tuple<SoftwareBitmapSource, string, string, SoftwareBitmap, Brush>(imagenCapturada, fechaHora, Empleado, softwareBitmapCapturada, colorEllipse)
            };
            
            
            return fotosCapt;
        }
      
    }
    public class FotosCapttuple
    {
        public Tuple<SoftwareBitmapSource, string, string, SoftwareBitmap, Brush> TupleFotosCapturadas { get; set; }
        


    }
}

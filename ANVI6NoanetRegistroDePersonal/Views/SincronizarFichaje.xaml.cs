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
using ANVI6NoanetRegistroDePersonal.Views;
using ANVI6NoanetRegistroDePersonal.Controller;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ANVI6NoanetRegistroDePersonal.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SincronizarFichaje : Page
    {
        public ObservableCollection<FotosCapttuple> fotosCapttuplesLista = new ObservableCollection<FotosCapttuple>();
        public ObservableCollection<Tuple<FotosCapttuple>> tuplesViewSinc = new ObservableCollection<Tuple<FotosCapttuple>>();
        public SincronizarFichaje()
        {
            this.InitializeComponent();

            Views.IniciarFichaje iniciarFichaje = new IniciarFichaje();
            fotosCapttuplesLista = iniciarFichaje.tuplesDevolver().Result;
            foreach (var item in fotosCapttuplesLista)
            {
                tuplesViewSinc.Add(new Tuple<FotosCapttuple>(item));
            }
            
            
        }

        private void btnVolver_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void btnSincronizar_Click(object sender, RoutedEventArgs e)
        {
            SincronizarAzure sincronizarAzure = new SincronizarAzure();
            await sincronizarAzure.ObtenerIdentidades(fotosCapttuplesLista);


        }
    }
}

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
using Windows.UI.Xaml.Automation.Peers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ANVI6NoanetRegistroDePersonal.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>

    public sealed partial class SincronizarFichaje : Page
    {
        public ObservableCollection<Empleado> empleadosLista = new ObservableCollection<Empleado>();
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
            empleadosLista.Clear();
            SincronizarAzure sincronizarAzure = new SincronizarAzure();
            NotifyUser("Iniciando Sincronización", NotifyType.StatusMessage);
            var empleadosListaObservable = await sincronizarAzure.ObtenerIdentidades(fotosCapttuplesLista);
            empleadosLista = empleadosListaObservable;
            lstEmpleadosFichados.ItemsSource = empleadosLista;
            NotifyUser("Sincronización Finalizada", NotifyType.StatusMessage);
            tuplesViewSinc.Clear();


        }
        public void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.

            UpdateStatus(strMessage, type);

        }
        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };

        public void UpdateStatus(string strMessage, NotifyType type)
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
    
        public class Empleado
        {
            public string EmpleadoNombe { get; set; }
            public string Horario { get; set; }
            
                
        }
    }
    
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ANVI6NoanetRegistroDePersonal.Views;

namespace ANVI6NoanetRegistroDePersonal.Controller
{
    public class Notify
    {
        public string Message { get; set; }
        public async Task<bool> Notificacion()
        {
            
            var resultado = await (new SincronizarFichaje()).NotifyUser("error", SincronizarFichaje.NotifyType.ErrorMessage);
            return resultado;
        }
    }
}

using System;

namespace sistema_ventas.Models
{
    public class HistorialCompraViewModel
    {
        public int UsuarioId { get; set; }
        public string CorreoUsuario { get; set; }
        public int ConciertoId { get; set; }
        public string NombreConcierto { get; set; }
        public DateTime FechaConcierto { get; set; }
        public string LugarConcierto { get; set; }
        public string SeccionConcierto { get; set; }
        public int CantidadComprada { get; set; }
        public decimal TotalCompra { get; set; }
    }
}
using System;

namespace sistema_ventas.Models
{
    public class Concierto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }  
        public DateTime Fecha { get; set; }  
        public string Lugar { get; set; }    
        public string Seccion { get; set; }  
        public decimal Precio { get; set; }   
        public int CantidadDisponibles { get; set; } 
    }
}

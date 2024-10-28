using sistema_ventas.Models;
using sistema_ventas.Permisos;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Web.Mvc;
using System.Net;

namespace sistema_ventas.Controllers
{
    public class HomeController : Controller
    {
        static string cadena = @"Data Source=DESKTOP-JCJT4VD\SQLEXPRESS;Initial Catalog=servicio_ventas;Integrated Security=true";

        [ValidarSesion]
        public ActionResult Index()
        {
            var concerts = new List<Concierto>();
            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_ListarConciertos", cn);
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        concerts.Add(new Concierto
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Fecha = Convert.ToDateTime(reader["Fecha"]),
                            Lugar = reader["Lugar"].ToString(),
                            Seccion = reader["Seccion"].ToString(),
                            Precio = Convert.ToDecimal(reader["Precio"]),
                            CantidadDisponibles = Convert.ToInt32(reader["CantidadDisponibles"])
                        });
                    }
                }
            }
            return View(concerts);
        }

        [ValidarSesion]
        public ActionResult Comprar(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Concierto concert = null;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerConcierto", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("Id", id);

                cn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        concert = new Concierto
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Nombre = reader["Nombre"].ToString(),
                            Fecha = Convert.ToDateTime(reader["Fecha"]),
                            Lugar = reader["Lugar"].ToString(),
                            Seccion = reader["Seccion"].ToString(),
                            Precio = Convert.ToDecimal(reader["Precio"]),
                            CantidadDisponibles = Convert.ToInt32(reader["CantidadDisponibles"])
                        };
                    }
                }
            }

            if (concert == null)
            {
                return HttpNotFound();
            }

            return View(concert);
        }

        [HttpPost]
        [ValidarSesion]
        public ActionResult ConfirmarCompra(int? idConcierto, int? cantidad)
        {
            if (idConcierto == null || cantidad == null)
            {
                TempData["MensajeCompra"] = "Datos de compra inválidos.";
                return RedirectToAction("Comprar", "Home");
            }

            int usuarioId = (int)Session["usuarioId"]; 

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_GuardarCompraConcierto", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("UsuarioId", usuarioId);
                cmd.Parameters.AddWithValue("ConciertoId", idConcierto);
                cmd.Parameters.AddWithValue("CantidadEntradas", cantidad); 

                cn.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["MensajeCompra"] = "Compra realizada con éxito.";
            return RedirectToAction("Comprar", "Home", new { id = idConcierto });
        }



        public ActionResult Historial()
        {
            int usuarioId = (int)Session["usuarioId"];  
            var historialCompras = new List<HistorialCompraViewModel>();

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerHistorialCompras", cn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("UsuarioId", usuarioId);

                cn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        historialCompras.Add(new HistorialCompraViewModel
                        {
                            UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
                            CorreoUsuario = reader["CorreoUsuario"].ToString(),
                            ConciertoId = Convert.ToInt32(reader["ConciertoId"]),
                            NombreConcierto = reader["NombreConcierto"].ToString(),
                            FechaConcierto = Convert.ToDateTime(reader["FechaConcierto"]),
                            LugarConcierto = reader["LugarConcierto"].ToString(),
                            SeccionConcierto = reader["SeccionConcierto"].ToString(),
                            CantidadComprada = Convert.ToInt32(reader["CantidadComprada"]),
                            TotalCompra = Convert.ToDecimal(reader["TotalCompra"])
                        });
                    }
                }
            }

            return View(historialCompras);
        }

        public ActionResult CerrarSesion()
        {
            Session["usuario"] = null;
            return RedirectToAction("Login", "Acceso");
        }
    }
}

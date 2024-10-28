using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Web.Mvc;
using sistema_ventas.Models;
using sistema_ventas.Permisos;
namespace sistema_ventas.Controllers
{
    public class ConcertsController : Controller
    {
        static string cadena = @"Data Source=DESKTOP-JCJT4VD\SQLEXPRESS ;Initial Catalog=servicio_ventas;Integrated Security=true";

        [ValidarAdminAtribute]
        public ActionResult Listar()
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

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Concierto concert)
        {
            string mensaje;
            bool registrado;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_CrearConcierto", cn);
                cmd.Parameters.AddWithValue("Nombre", concert.Nombre);
                cmd.Parameters.AddWithValue("Fecha", concert.Fecha);
                cmd.Parameters.AddWithValue("Lugar", concert.Lugar);
                cmd.Parameters.AddWithValue("Seccion", concert.Seccion);
                cmd.Parameters.AddWithValue("Precio", concert.Precio);
                cmd.Parameters.AddWithValue("CantidadDisponibles", concert.CantidadDisponibles);
                cmd.Parameters.Add("Registrado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();
                cmd.ExecuteNonQuery();

                registrado = Convert.ToBoolean(cmd.Parameters["Registrado"].Value);
                mensaje = cmd.Parameters["Mensaje"].Value.ToString();
            }

            ViewData["Mensaje"] = mensaje;

            if (registrado)
            {
                return RedirectToAction("Listar", "Concerts"); 
            }
            else
            {
                return View(concert);
            }
        }


        public ActionResult Edit(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Concierto concert = null;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerConcierto", cn);
                cmd.Parameters.AddWithValue("Id", id);
                cmd.CommandType = CommandType.StoredProcedure;

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
        public ActionResult Edit(Concierto concert)
        {
            string mensaje;
            bool actualizado;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_ActualizarConcierto", cn);
                cmd.Parameters.AddWithValue("Id", concert.Id);
                cmd.Parameters.AddWithValue("Nombre", concert.Nombre); 
                cmd.Parameters.AddWithValue("Fecha", concert.Fecha); 
                cmd.Parameters.AddWithValue("Lugar", concert.Lugar); 
                cmd.Parameters.AddWithValue("Seccion", concert.Seccion); 
                cmd.Parameters.AddWithValue("Precio", concert.Precio); 
                cmd.Parameters.AddWithValue("CantidadDisponibles", concert.CantidadDisponibles); 
                cmd.Parameters.Add("Actualizado", SqlDbType.Bit).Direction = ParameterDirection.Output;
                cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();
                cmd.ExecuteNonQuery();

                actualizado = Convert.ToBoolean(cmd.Parameters["Actualizado"].Value);
                mensaje = cmd.Parameters["Mensaje"].Value.ToString();
            }

            ViewData["Mensaje"] = mensaje;

            if (actualizado)
            {
                return RedirectToAction("Listar", "Concerts");
            }
            else
            {
                return View(concert);
            }
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Concierto concert = null;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_ObtenerConcierto", cn);
                cmd.Parameters.AddWithValue("@Id", id); 
                cmd.CommandType = CommandType.StoredProcedure;

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

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            string mensaje;
            bool eliminado;

            using (SqlConnection cn = new SqlConnection(cadena))
            {
                SqlCommand cmd = new SqlCommand("sp_EliminarConcierto", cn);
                cmd.Parameters.AddWithValue("@Id", id); 
                cmd.Parameters.Add("Eliminado", SqlDbType.Bit).Direction = ParameterDirection.Output; 
                cmd.Parameters.Add("Mensaje", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output; 
                cmd.CommandType = CommandType.StoredProcedure;

                cn.Open();
                cmd.ExecuteNonQuery();

                eliminado = Convert.ToBoolean(cmd.Parameters["Eliminado"].Value);
                mensaje = cmd.Parameters["Mensaje"].Value.ToString();
            }

            ViewData["Mensaje"] = mensaje;

            return RedirectToAction("Listar", "Concerts");
        }

    }
}

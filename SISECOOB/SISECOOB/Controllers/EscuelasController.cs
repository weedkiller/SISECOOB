﻿using SISECOOB.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SISECOOB.Controllers
{
    public class EscuelasController : Controller
    {
        // GET: Escuelas
        public ActionResult Index()
        {
            SISECOOBEntities db = new SISECOOBEntities();
            ViewBag.Municipios = db.Municipios.Select(i => new { id = i.MunicipioId, nombre = i.Nombre }).OrderBy(i=> i.nombre).ToList();
            ViewBag.Niveles = db.NivelesEducativos.Select(i => new { id = i.NivelID, nombre = i.Nombre }).OrderBy(i => i.nombre).ToList();
            return View();
        }

        public JsonResult Buscar(string nombre, string clave, int niveles = 0, int municipio = 0 ,int localidad = 0, int page = 1, int pageSize = 15)
        {
            SISECOOBEntities db = new SISECOOBEntities();

            IQueryable<Escuelas> query = db.Escuelas;

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(i => (i.Nombre).Contains(nombre));
            if (!string.IsNullOrEmpty(clave))
                query = query.Where(i => (i.Clave).Contains(clave));
            if (municipio > 0)
                query = query.Where(i => i.Municipio_fk == municipio);
            if (localidad > 0)
                query = query.Where(i => i.Localidad_fk == localidad);

            return Json(new
            {
                total = query.Count(),
                datos = query.OrderBy(i => i.Nombre)
                             .Skip((page - 1) * pageSize)
                             .Take(pageSize)
                             .ToList()
                             .Select(i => new
                             {
                                 id = i.EscuelaID,
                                 nombre = i.Nombre,
                                 clave = i.Clave,
                                 localidad = i.Localidades.Nombre + ", " + i.Municipios.Nombre,
                             })
                             .ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Details(int id)
        {
            SISECOOBEntities db = new SISECOOBEntities();

            IQueryable<Escuelas> query = db.Escuelas.Where(i => i.EscuelaID == id);

            return Json(new
            {
                total = query.Count(),
                datos = query.OrderBy(i => i.Nombre)
                             .ToList()
                             .Select(i => new
                             {
                                 nombre = i.Nombre,
                                 clave = i.Clave,
                                 nivel = i.NivelesEducativos.Nombre,
                                 municipio = i.Municipios.Nombre,
                                 localidad = i.Localidades.Nombre,
                                 domicilio = i.Domicilio,
                                 alumnos = i.Alumnos,
                                 turno = db.Turnos.FirstOrDefault(j => j.TurnoID == i.Turno).Nombre, 
                                 director = i.Director,
                                 zona = i.Zona,
                                 sector = i.Sector,
                                 tipopredio = i.TipoPredio,
                                 telefonos = db.Telefonos.Where(j => j.Proveniente == "Escuela" && j.ProvenienteID == i.EscuelaID)
                                                         .Select( j => new {
                                                             tipotel = j.TipoTelefono.TipoTelefono1,
                                                             tel = j.Telefono
                                                         }).ToList()
                             })
                             .ToList()
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Formulario(int id)
        {

            SISECOOBEntities db = new SISECOOBEntities();
            Escuelas esc = db.Escuelas.FirstOrDefault(i => i.EscuelaID == id);

            ViewBag.Municipios = db.Municipios.Select(i => new { id = i.MunicipioId, nombre = i.Nombre }).OrderBy(i => i.nombre).ToList();
            ViewBag.Niveles = db.NivelesEducativos.Select(i => new { id = i.NivelID, nombre = i.Nombre }).OrderBy(i => i.nombre).ToList();
            ViewBag.Turnos = db.Turnos.Select(i => new { id = i.TurnoID, nombre = i.Nombre }).OrderBy(i => i.nombre).ToList();
            ViewBag.TipoTel = db.TipoTelefono.Select(i => new { id = i.TelefonoID, nombre = i.TipoTelefono1 }).OrderBy(i => i.nombre).ToList();

            Escuelas e = new Escuelas();
            ViewBag.Localidades = null;
            ViewBag.Telefonos = null;

            if (esc != null)
            {
                e.EscuelaID = esc.EscuelaID;
                e.Nombre = esc.Nombre;
                e.Clave = esc.Clave;
                e.Nivel_fk = esc.Nivel_fk;
                e.Domicilio = esc.Domicilio;
                e.Municipio_fk = esc.Municipio_fk;
                e.Alumnos = esc.Alumnos;
                e.Localidad_fk = esc.Localidad_fk;
                e.Turno = esc.Turno;
                e.Director = esc.Director;
                e.Zona = esc.Zona;
                e.Sector = esc.Sector;
                e.TipoPredio = esc.TipoPredio;

                ViewBag.Localidades = db.Localidades.Where(i => i.MunicipioId_FK == e.Municipio_fk).Select(i => new { id = i.LocalidadId, nombre = i.Nombre }).OrderBy(i => i.nombre).ToList();
                ViewBag.Telefonos = db.Telefonos.Where(i => i.Proveniente == "Escuela" && i.ProvenienteID == id).Select(i => new { tel = i.Telefono, tipo = i.TipoTelefono.TipoTelefono1 }).OrderBy(i => i.tel).ToList();

            }

            return PartialView("_Formulario", e);
        }


        [HttpPost]
        public JsonResult Create(Escuelas escuela)
        {
            try
            {
                var esc = escuela.Crear();
                for (var i = 1; i < escuela.telefonos.Count(); i++)
                {
                    Telefonos t = new Telefonos();
                    t.Proveniente = "Escuela";
                    t.ProvenienteID = esc;
                    t.TipoTelefono_fk = Convert.ToInt32(escuela.tipotelefono[i]);
                    t.Telefono = escuela.telefonos[i];

                    t.Crear();
                }

                return Json(new
                {
                    result = true
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    result = false,
                    message = e.Message
                });
            }
        }

        [HttpPost]
        public JsonResult Edicion(Escuelas escuela)
        {
            try
            {
                var esc = escuela.Editar();

                SISECOOBEntities db = new SISECOOBEntities();
                List<Telefonos> t = db.Telefonos.Where(i => i.Proveniente == "Escuela" && i.ProvenienteID == esc).ToList();

                foreach (var i in t) {
                    db.Telefonos.DeleteObject(i);
                    db.SaveChanges();
                }


                for (var i = 1; i < escuela.telefonos.Count(); i++)
                {
                    Telefonos tel = new Telefonos();
                    tel.Proveniente = "Escuela";
                    tel.ProvenienteID = esc;
                    tel.TipoTelefono_fk = Convert.ToInt32(escuela.tipotelefono[i]);
                    tel.Telefono = escuela.telefonos[i];

                    tel.Crear();
                }

                return Json(new
                {
                    result = true
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    result = false,
                    message = e.Message
                });
            }
        }

        [HttpPost]
        public JsonResult Elimina(int id = 0)
        {
            try
            {
                Escuelas esc = new Escuelas();
                esc.Eliminar(id);
                return Json(new
                {
                    result = true
                });
            }
            catch (Exception e)
            {
                return Json(new
                {
                    result = false,
                    message = e.Message
                });
            }
        }

        public JsonResult telefonos(int id)
        {
            SISECOOBEntities db = new SISECOOBEntities();

            IQueryable<Telefonos> query = db.Telefonos.Where(i => i.Proveniente == "Escuela" && i.ProvenienteID == id);

            return Json(new
            {
                total = query.Count(),
                op = db.TipoTelefono.Select( i => new { id=i.TelefonoID, tipo=i.TipoTelefono1 } ).ToList(),
                datos = query.Select(i => new
                             {
                                 tel = i.Telefono,
                                 tipo = i.TipoTelefono.TipoTelefono1,
                             })
                             .ToList()
            }, JsonRequestBehavior.AllowGet);
        }
    }
}
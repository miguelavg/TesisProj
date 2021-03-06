﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TesisProj.Areas.Modelo.Models;
using TesisProj.Areas.Plantilla.Models;
using TesisProj.Models.Storage;

namespace TesisProj.Areas.Plantilla.Controllers
{
    [Authorize(Roles = "nav")]
    public class PlantillaProyectoController : Controller
    {
        private TProjContext db = new TProjContext();

        //
        // GET: /Plantilla/PlantillaProyecto/

        public ActionResult Index()
        {
            return View(db.PlantillaProyectos.OrderBy(t => t.Nombre).ToList());
        }

        //
        // GET: /Plantilla/PlantillaProyecto/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Plantilla/PlantillaProyecto/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(PlantillaProyecto plantillaproyecto)
        {
            if (ModelState.IsValid)
            {
                db.PlantillaProyectosRequester.AddElement(plantillaproyecto);
                return RedirectToAction("Index");
            }

            return View(plantillaproyecto);
        }

        //
        // GET: /Plantilla/PlantillaProyecto/Edit/5

        public ActionResult Edit(int id = 0)
        {
            PlantillaProyecto plantillaproyecto = db.PlantillaProyectos.Find(id);
            if (plantillaproyecto == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            return View(plantillaproyecto);
        }

        //
        // POST: /Plantilla/PlantillaProyecto/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PlantillaProyecto plantillaproyecto)
        {
            if (ModelState.IsValid)
            {
                db.PlantillaProyectosRequester.ModifyElement(plantillaproyecto);
                return RedirectToAction("Index");
            }

            return View(plantillaproyecto);
        }

        //
        // GET: /Plantilla/PlantillaProyecto/Delete/5

        public ActionResult Delete(int id = 0)
        {
            PlantillaProyecto plantillaproyecto = db.PlantillaProyectos.Find(id);
            if (plantillaproyecto == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            var salidaoperaciones = db.PlantillaSalidaOperaciones.Include(s => s.Operacion).Where(p => p.Operacion.IdPlantillaProyecto == plantillaproyecto.Id).ToList();
            foreach (PlantillaSalidaOperacion salida in salidaoperaciones)
            {
                db.PlantillaSalidaOperacionesRequester.RemoveElementByID(salida.Id);
            }

            var salidas = db.PlantillaSalidaProyectos.Where(s => s.IdPlantillaProyecto == plantillaproyecto.Id).ToList();
            foreach (PlantillaSalidaProyecto salida in salidas)
            {
                db.PlantillaSalidaProyectosRequester.RemoveElementByID(salida.Id);
            }

            var operaciones = db.PlantillaOperaciones.Where(o => o.IdPlantillaProyecto == plantillaproyecto.Id).ToList();
            foreach (PlantillaOperacion operacion in operaciones)
            {
                db.PlantillaOperacionesRequester.RemoveElementByID(operacion.Id);
            }

            db.PlantillaProyectosRequester.RemoveElementByID(plantillaproyecto.Id);
            return RedirectToAction("Index");
        }

        //
        // GET: /Plantilla/PlantillaProyecto/DuplicarPlantilla/5

        public ActionResult DuplicarPlantilla(int id = 0)
        {
            PlantillaProyecto plantilla = db.PlantillaProyectos.Include(e => e.Salidas).Include(e => e.Operaciones).Include(e => e.Salidas.Select(s => s.Operaciones)).FirstOrDefault(e => e.Id == id);
            if (plantilla == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            // Begin: Get unique name

            string nombre = "Copia de " + plantilla.Nombre + " ";
            string nombreTest = nombre;
            int i = 1;

            while (db.PlantillaProyectos.Any(p => p.Nombre.Equals(nombreTest)))
            {
                nombreTest = nombre + i++;
            }

            // End: Get unique name

            int idDuplicado = db.PlantillaProyectosRequester.AddElement(new PlantillaProyecto { Nombre = nombreTest });
            foreach (PlantillaOperacion operacion in plantilla.Operaciones.OrderBy(o => o.Secuencia))
            {
                db.PlantillaOperacionesRequester.AddElement(new PlantillaOperacion(operacion, idDuplicado));
            }

            foreach (PlantillaSalidaProyecto salida in plantilla.Salidas)
            {
                int idSalida = db.PlantillaSalidaProyectosRequester.AddElement(new PlantillaSalidaProyecto(salida, idDuplicado));
                foreach (PlantillaSalidaOperacion cruce in salida.Operaciones)
                {
                    int idOperacion = db.PlantillaOperaciones.First(o => o.Referencia.Equals(cruce.Operacion.Referencia) && o.IdPlantillaProyecto == idDuplicado).Id;
                    db.PlantillaSalidaOperacionesRequester.AddElement(new PlantillaSalidaOperacion { IdSalida = idSalida, IdOperacion = idOperacion, Secuencia = cruce.Secuencia });
                }
            }

            return RedirectToAction("Edit", new { id = idDuplicado });
        }

        //
        // GET: /Plantilla/PlantillaProyecto/VolverPlantilla/5

        public ActionResult VolverPlantilla(int id = 0)
        {
            Proyecto proyecto = db.Proyectos.Include(e => e.Salidas).Include(e => e.Operaciones).Include(e => e.Salidas.Select(s => s.Operaciones)).FirstOrDefault(e => e.Id == id);
            if (proyecto == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            // Begin: Get unique name

            string nombre = "Plantilla de " + proyecto.Nombre + " ";
            string nombreTest = nombre;
            int i = 1;

            while (db.PlantillaProyectos.Any(p => p.Nombre.Equals(nombreTest)))
            {
                nombreTest = nombre + i++;
            }

            // End: Get unique name

            int idPlantilla = db.PlantillaProyectosRequester.AddElement(new PlantillaProyecto { Nombre = nombreTest });

            foreach (Operacion operacion in proyecto.Operaciones.OrderBy(o => o.Secuencia))
            {
                db.PlantillaOperacionesRequester.AddElement(new PlantillaOperacion(operacion, idPlantilla));
            }

            foreach (SalidaProyecto salida in proyecto.Salidas)
            {
                int idSalida = db.PlantillaSalidaProyectosRequester.AddElement(new PlantillaSalidaProyecto(salida, idPlantilla));
                foreach (SalidaOperacion cruce in salida.Operaciones)
                {
                    int idOperacion = db.PlantillaOperaciones.First(o => o.Referencia.Equals(cruce.Operacion.Referencia) && o.IdPlantillaProyecto == idPlantilla).Id;
                    db.PlantillaSalidaOperacionesRequester.AddElement(new PlantillaSalidaOperacion { IdSalida = idSalida, IdOperacion = idOperacion, Secuencia = cruce.Secuencia });
                }
            }

            return RedirectToAction("EditProyecto", "AnonPlantilla", new { id = idPlantilla, idProyecto = id });
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
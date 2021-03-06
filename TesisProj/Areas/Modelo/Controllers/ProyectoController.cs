﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;
using TesisProj.Areas.Modelo.Models;
using TesisProj.Areas.Plantilla.Models;
using TesisProj.Areas.Seguridad.Models;
using TesisProj.Models.Storage;

namespace TesisProj.Areas.Modelo.Controllers
{
    [Authorize(Roles = "nav")]
    public partial class ProyectoController : Controller
    {
        private TProjContext db = new TProjContext();
        private int userId = 0;

        // Permisos: Anon
        // GET: /Modelo/Proyecto/

        public ActionResult Index()
        {
            int idUser = getUserId();
            var creador = db.Proyectos.Where(p => p.IdCreador == idUser).Include(p => p.Creador).OrderByDescending(p => p.Creacion).ToList();
            var editor = db.Colaboradores.Where(c => c.IdUsuario == idUser && !c.Creador && !c.SoloLectura).Select(c => c.Proyecto).Include(p => p.Creador).OrderByDescending(p => p.Creacion).ToList();
            var revisor = db.Colaboradores.Where(c => c.IdUsuario == idUser && c.SoloLectura).Select(c => c.Proyecto).Include(p => p.Creador).OrderByDescending(p => p.Creacion).ToList();

            return View(creador.Union(editor.Union(revisor)));
        }

        // Permisos: Creador, Editor, Revisor
        // GET: /Modelo/Proyecto/Console/5

        public ActionResult Console(int id = 0)
        {
            Proyecto proyecto = db.Proyectos.Include(p => p.Creador).FirstOrDefault(p => p.Id == id);
            if (proyecto == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            // Get project and check user
            int currentId = getUserId();
            Colaborador current = db.Colaboradores.FirstOrDefault(c => c.IdUsuario == currentId && c.IdProyecto == proyecto.Id);
            if (current == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            bool IsCreador = current.Creador;
            bool IsEditor = !current.Creador && !current.SoloLectura;
            bool IsRevisor = current.SoloLectura;

            ViewBag.IsCreador = IsCreador;
            ViewBag.IsEditor = IsEditor;
            ViewBag.IsRevisor = IsRevisor;

            return View(proyecto);
        }

        //
        // GET: /Modelo/Proyecto/Create

        public ActionResult Create()
        {
            ViewBag.IdCreador = new SelectList(db.UserProfiles.Where(u => u.UserName == User.Identity.Name), "UserId", "UserName");
            ViewBag.IdModificador = new SelectList(db.UserProfiles.Where(u => u.UserName == User.Identity.Name), "UserId", "UserName");
            ViewBag.IdPlantilla = new SelectList(db.PlantillaProyectos.OrderBy(p => p.Nombre), "Id", "Nombre");
            ViewBag.Now = DateTime.Now.ToShortDateString();
            ViewBag.Version = 0;
            return View();
        }

        //
        // POST: /Modelo/Proyecto/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Proyecto proyecto, int IdPlantilla = 0)
        {
            proyecto.Creacion = DateTime.Now;
            if (ModelState.IsValid)
            {
                proyecto.Creacion = DateTime.Now;
                proyecto.Calculado = DateTime.Now;
                db.ProyectosRequester.AddElement(proyecto);

                if (IdPlantilla > 0)
                {

                    //
                    // Copiar operaciones
                    var operaciones = db.PlantillaOperaciones.Where(p => p.IdPlantillaProyecto == IdPlantilla).ToList();

                    foreach (PlantillaOperacion plantilla in operaciones)
                    {
                        db.OperacionesRequester.AddElement(new Operacion(plantilla, proyecto.Id));
                    }

                    //
                    // Copiar salidas y su asociación con operaciones
                    var salidas = db.PlantillaSalidaProyectos.Include("Operaciones").Where(p => p.IdPlantillaProyecto == IdPlantilla).ToList();

                    foreach (PlantillaSalidaProyecto plantilla in salidas)
                    {
                        int idSalida = db.SalidaProyectosRequester.AddElement(new SalidaProyecto(plantilla, proyecto.Id));

                        foreach (PlantillaSalidaOperacion cruce in plantilla.Operaciones)
                        {
                            int idOperacion = db.Operaciones.First(o => o.IdProyecto == proyecto.Id && o.Referencia == cruce.Operacion.Referencia).Id;
                            db.SalidaOperacionesRequester.AddElement(new SalidaOperacion { IdSalida = idSalida, IdOperacion = idOperacion, Secuencia = cruce.Secuencia });
                        }
                    }
                }
                db.ColaboradoresRequester.AddElement(new Colaborador { IdProyecto = proyecto.Id, Creador = true, SoloLectura = false, IdUsuario = getUserId() });
                db.AuditsRequester.AddElement(new Audit { IdProyecto = proyecto.Id, Fecha = DateTime.Now, IdUsuario = getUserId(), Transaccion = "Crear", TipoObjeto = proyecto.GetType().ToString(), Original = proyecto.LogValues() });

                return RedirectToAction("Console", new { id = proyecto.Id });
            }

            ViewBag.IdPlantilla = new SelectList(db.PlantillaProyectos.OrderBy(p => p.Nombre), "Id", "Nombre", IdPlantilla);
            ViewBag.IdCreador = new SelectList(db.UserProfiles.Where(u => u.UserName == User.Identity.Name), "UserId", "UserName", proyecto.IdCreador);
            ViewBag.Version = 0;

            return View(proyecto);
        }

        // Permisos: Creador, Editor
        // GET: /Modelo/Proyecto/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Proyecto proyecto = db.Proyectos.Find(id);
            if (proyecto == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            // Get project and check user
            int currentId = getUserId();
            Colaborador current = db.Colaboradores.FirstOrDefault(c => c.IdUsuario == currentId && c.IdProyecto == proyecto.Id);
            if (current == null || current.SoloLectura)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            ViewBag.IdCreador = new SelectList(db.UserProfiles.Where(u => u.UserId == proyecto.IdCreador), "UserId", "UserName", proyecto.IdCreador);
            ViewBag.PreHorizonte = proyecto.Horizonte;
            return View(proyecto);
        }

        //
        // POST: /Modelo/Proyecto/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Proyecto proyecto, int PreHorizonte)
        {
            if (ModelState.IsValid)
            {
                db.ProyectosRequester.ModifyElement(proyecto, true, proyecto.Id, getUserId());

                if (PreHorizonte < proyecto.Horizonte)
                {
                    var parametros = db.Parametros.Include(p => p.Elemento).Where(p => p.Elemento.IdProyecto == proyecto.Id).ToList();
                    foreach (Parametro parametro in parametros)
                    {
                        Celda celda = db.Celdas.Where(c => c.IdParametro == parametro.Id).OrderByDescending(c => c.Periodo).FirstOrDefault();
                        if(celda == null) continue;
                        int deltaPeriodos =  proyecto.Horizonte - celda.Periodo;
                        decimal valor = celda.Valor;
                        for (int i = 1; i <= deltaPeriodos; i++)
                        {
                            db.CeldasRequester.AddElement(new Celda { IdParametro = celda.IdParametro, Periodo = celda.Periodo + i, Valor = celda.Valor });
                        }
                    } 
                }
                
                return RedirectToAction("Console", new { id = proyecto.Id });
            }
            ViewBag.IdCreador = new SelectList(db.UserProfiles.Where(u => u.UserId == proyecto.IdCreador), "UserId", "UserName", proyecto.IdCreador);
            return View(proyecto);
        }

        // Permisos: Creador
        // GET: /Modelo/Proyecto/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Proyecto proyecto = db.Proyectos.Find(id);
            if (proyecto == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            // Get project and check user
            int currentId = getUserId();
            Colaborador current = db.Colaboradores.FirstOrDefault(c => c.IdUsuario == currentId && c.IdProyecto == proyecto.Id);
            if (current == null || !current.Creador)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }
            
            var colaboradores = db.Colaboradores.Where(c => c.IdProyecto == proyecto.Id).ToList();
            foreach (Colaborador colaborador in colaboradores)
            {
                db.ColaboradoresRequester.RemoveElementByID(colaborador.Id);
            }

            var versions = db.DbVersions.Where(v => v.IdProyecto == proyecto.Id).ToList();
            foreach (DbVersion version in versions)
            {
                db.DbVersionsRequester.RemoveElementByID(version.Id);
            }

            var audits = db.Audits.Where(a => a.IdProyecto == proyecto.Id).ToList();
            foreach (Audit audit in audits)
            {
                db.AuditsRequester.RemoveElementByID(audit.Id);
            }

            var salidaoperaciones = db.SalidaOperaciones.Where(sxp => sxp.Operacion.IdProyecto == proyecto.Id).ToList();
            foreach (SalidaOperacion salidaoperacion in salidaoperaciones)
            {
                db.SalidaOperacionesRequester.RemoveElementByID(salidaoperacion.Id);
            }

            var operaciones = db.Operaciones.Where(o => o.IdProyecto == proyecto.Id).ToList();
            foreach (Operacion operacion in operaciones)
            {
                db.OperacionesRequester.RemoveElementByID(operacion.Id);
            }

            var salidas = db.SalidaProyectos.Where(s => s.IdProyecto == proyecto.Id).ToList();
            foreach (SalidaProyecto salida in salidas)
            {
                db.SalidaProyectosRequester.RemoveElementByID(salida.Id);
            }

            var formulas = db.Formulas.Where(f => f.Elemento.IdProyecto == proyecto.Id).ToList();
            foreach (Formula formula in formulas)
            {
                db.FormulasRequester.RemoveElementByID(formula.Id);
            }

            var celdas = db.Celdas.Where(c => c.Parametro.Elemento.IdProyecto == proyecto.Id).ToList();
            foreach (Celda celda in celdas)
            {
                db.CeldasRequester.RemoveElementByID(celda.Id);
            }

            var parametros = db.Parametros.Where(p => p.Elemento.IdProyecto == proyecto.Id).ToList();
            foreach (Parametro parametro in parametros)
            {
                db.ParametrosRequester.RemoveElementByID(parametro.Id);
            }

            var elementos = db.Elementos.Where(e => e.IdProyecto == proyecto.Id).ToList();
            foreach (Elemento elemento in elementos)
            {
                db.ElementosRequester.RemoveElementByID(elemento.Id);
            }

            db.ProyectosRequester.RemoveElementByID(id);
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        } 

        private int getUserId()
        {
            if (userId < 1)
            {
                try
                {
                    userId = db.UserProfiles.First(u => u.UserName == User.Identity.Name).UserId;
                }
                catch (Exception)
                {
                    return 0;
                }
            }

            return userId;
        }

    }
}
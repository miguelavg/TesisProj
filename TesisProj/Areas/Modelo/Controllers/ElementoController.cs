﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TesisProj.Areas.Modelo.Models;
using TesisProj.Areas.Plantilla.Models;
using TesisProj.Areas.Seguridad.Models;
using TesisProj.Models.Storage;

namespace TesisProj.Areas.Modelo.Controllers
{
    public partial class ProyectoController : Controller
    {
        // Permissions: Creador, Editor, Revisor
        // GET: /Modelo/Proyecto/Journal/5

        public ActionResult Journal(int id = 0)
        {
            Proyecto proyecto = db.Proyectos.Find(id);
            if (proyecto == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }
            // Check user
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

            ViewBag.Proyecto = proyecto.Nombre;
            ViewBag.ProyectoId = proyecto.Id;
            ViewBag.TipoElementos = db.TipoElementos.OrderBy(t => t.NombrePlural).ToList();

            var elementos = db.Elementos.Where(e => e.IdProyecto == proyecto.Id);
            return View(elementos.ToList());
        }

        // Permissions: Creador, Editor
        // GET: /Modelo/Proyecto/CreateElemento?idProyecto=5&idTipoElemento=1

        public ActionResult CreateElemento(int idProyecto = 0, int idTipoElemento = 0)
        {
            Proyecto proyecto = db.Proyectos.Find(idProyecto);
            TipoElemento tipo = db.TipoElementos.Find(idTipoElemento);
            if (proyecto == null || tipo == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }
            // Check user
            int currentId = getUserId();
            Colaborador current = db.Colaboradores.FirstOrDefault(c => c.IdUsuario == currentId && c.IdProyecto == proyecto.Id);
            if (current == null || current.SoloLectura)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            ViewBag.IdPlantilla = new SelectList(db.PlantillaElementos.Where(p => p.IdTipoElemento == tipo.Id).OrderBy(p => p.Nombre), "Id", "Nombre");
            ViewBag.IdTipoElemento = new SelectList(db.TipoElementos.Where(t => t.Id == tipo.Id), "Id", "Nombre", tipo.Id);
            ViewBag.IdProyecto = new SelectList(db.Proyectos.Where(p => p.Id == proyecto.Id), "Id", "Nombre", proyecto.Id);
            ViewBag.IdProyectoReturn = proyecto.Id;
            ViewBag.Proyecto = proyecto.Nombre;

            return View();
        }

        //
        // POST: /Modelo/Proyecto/CreateElemento

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateElemento(Elemento elemento, int IdPlantilla = 0)
        {

        //  Valida que no ingrese una fórmula única que ya exista en el proyecto

            if (IdPlantilla > 0)
            {
                var formulasUnicasProyecto = db.Formulas.Include(f => f.Elemento).Include(f => f.TipoFormula).Where(f => f.Elemento.IdTipoElemento == elemento.IdTipoElemento && f.Elemento.IdProyecto == elemento.IdProyecto && f.TipoFormula.Unico).ToList();
                var formulasUnicasPlantilla = db.PlantillaFormulas.Include(p => p.TipoFormula).Where(p => p.IdPlantillaElemento == IdPlantilla && p.TipoFormula.Unico).ToList();

                foreach (Formula formula in formulasUnicasProyecto)
                {
                    if (formulasUnicasPlantilla.Any(f => f.IdTipoFormula == formula.IdTipoFormula))
                    {
                        ModelState.AddModelError("IdPlantilla", "Esta plantilla contiene una fórmula cuyo tipo es único por proyecto y ya existe en el proyecto actual.");
                        break;
                    }
                }
            }

            if (ModelState.IsValid)
            {
                elemento.TipoElemento = db.TipoElementos.Find(elemento.IdTipoElemento);
                db.ElementosRequester.AddElement(elemento, true, elemento.IdProyecto, getUserId());

                if (IdPlantilla > 0)
                {
                    var parametros = db.PlantillaParametros.Where(p => p.IdPlantillaElemento == IdPlantilla).ToList();

                    foreach (PlantillaParametro plantilla in parametros)
                    {
                        db.ParametrosRequester.AddElement(new Parametro(plantilla, elemento.Id));
                    }

                    var formulas = db.PlantillaFormulas.Where(f => f.IdPlantillaElemento == IdPlantilla).OrderBy(f => f.Secuencia).ToList();

                    foreach (PlantillaFormula plantilla in formulas)
                    {
                        db.FormulasRequester.AddElement(new Formula(plantilla, elemento.Id));
                    }
                    
                    return RedirectToAction("PutParametros", new { id = elemento.Id });
                }

                return RedirectToAction("Catalog", new { id = elemento.Id });
            }

            ViewBag.IdPlantilla = new SelectList(db.PlantillaElementos.Where(p => p.IdTipoElemento == elemento.IdTipoElemento).OrderBy(p => p.Nombre), "Id", "Nombre", IdPlantilla);
            ViewBag.IdTipoElemento = new SelectList(db.TipoElementos.Where(t => t.Id == elemento.IdTipoElemento), "Id", "Nombre", elemento.IdTipoElemento);
            ViewBag.IdProyecto = new SelectList(db.Proyectos.Where(p => p.Id == elemento.IdProyecto), "Id", "Nombre", elemento.IdProyecto);
            ViewBag.IdProyectoReturn = elemento.IdProyecto;
            ViewBag.Proyecto = db.Proyectos.Find(elemento.IdProyecto).Nombre;

            return View(elemento);
        }

        // Permisos: Creador, Editor
        // GET: /Modelo/Proyecto/EditElemento/5

        public ActionResult EditElemento(int id = 0)
        {
            Elemento elemento = db.Elementos.Find(id);
            if (elemento == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }
            
            // Check user
            Proyecto proyecto = db.Proyectos.Find(elemento.IdProyecto); int currentId = getUserId();
            Colaborador current = db.Colaboradores.FirstOrDefault(c => c.IdUsuario == currentId && c.IdProyecto == proyecto.Id);
            if (current == null || current.SoloLectura)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            ViewBag.IdTipoElemento = new SelectList(db.TipoElementos.Where(t => t.Id == elemento.IdTipoElemento), "Id", "Nombre", elemento.IdTipoElemento);
            ViewBag.IdProyecto = new SelectList(db.Proyectos.Where(p => p.Id == elemento.IdProyecto), "Id", "Nombre", elemento.IdProyecto);
            ViewBag.Proyecto = db.Proyectos.Find(elemento.IdProyecto).Nombre;

            return View(elemento);
        }

        //
        // POST: /Modelo/Proyecto/EditElemento/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditElemento(Elemento elemento)
        {
            if (ModelState.IsValid)
            {
                elemento.TipoElemento = db.TipoElementos.Find(elemento.IdTipoElemento);
                db.ElementosRequester.ModifyElement(elemento, true, elemento.IdProyecto, getUserId());
                return RedirectToAction("Journal", new { id = elemento.IdProyecto });
            }

            ViewBag.IdTipoElemento = new SelectList(db.TipoElementos.Where(t => t.Id == elemento.IdTipoElemento), "Id", "Nombre", elemento.IdTipoElemento);
            ViewBag.IdProyecto = new SelectList(db.Proyectos.Where(p => p.Id == elemento.IdProyecto), "Id", "Nombre", elemento.IdProyecto);
            ViewBag.Proyecto = db.Proyectos.Find(elemento.IdProyecto).Nombre;

            return View(elemento);
        }

        // Permisos: Creador, Editor
        // GET: /Modelo/Proyecto/DeleteElemento/5

        public ActionResult DeleteElemento(int id = 0)
        {
            Elemento elemento = db.Elementos.Include(e => e.TipoElemento).FirstOrDefault(e => e.Id == id);
            if (elemento == null)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            // Check user
            Proyecto proyecto = db.Proyectos.Find(elemento.IdProyecto); int currentId = getUserId();
            Colaborador current = db.Colaboradores.FirstOrDefault(c => c.IdUsuario == currentId && c.IdProyecto == proyecto.Id);
            if (current == null || current.SoloLectura)
            {
                return RedirectToAction("DeniedWhale", "Error", new { Area = "" });
            }

            var formulas = db.Formulas.Where(f => f.IdElemento == elemento.Id).ToList();
            foreach (Formula formula in formulas)
            {
                db.FormulasRequester.RemoveElementByID(formula.Id);
            }

            var parametros = db.Parametros.Where(p => p.Elemento.Id == elemento.Id).ToList();
            foreach (Parametro parametro in parametros)
            {
                var celdas = db.Celdas.Where(c => c.IdParametro == parametro.Id).ToList();
                foreach (Celda celda in celdas)
                {
                    db.CeldasRequester.RemoveElementByID(celda.Id);
                }
                db.ParametrosRequester.RemoveElementByID(parametro.Id);
            }

            db.ElementosRequester.RemoveElementByID(elemento.Id, true, true, elemento.IdProyecto, getUserId());
            return RedirectToAction("Journal", new { id = elemento.IdProyecto });
        }
    }
}

﻿using IridiumTest.Models;
using IridiumTest.Models.Discrete;
using TesisProj.Areas.Simulaciones.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using TesisProj.Models.Storage;

namespace TesisProj.Areas.Simulaciones.Controllers
{
    public class PoissonController : Controller
    {
        //
        // GET: /Poisson/

        [HttpGet]
        public ActionResult Index(int ProyectoId, int ParametroId)
        {
            ModeloSimulacion modelo = new ModeloSimulacion("Poisson");
            Session["_GraficoProbabilidad"] = null;
            Session["_GraficoMuestra"] = null;
            Session["ParametroId"] = ParametroId;
            Session["ProyectoId"] = ProyectoId;
            ViewBag.ParametroId = ParametroId;
            ViewBag.ProyectoId = ProyectoId;
            return View(modelo.poisson);
        }

        [HttpPost]
        public ActionResult Index(_Poisson p)
        {
            //g.ParamsIN[0].valorD  promedio
            //g.ParamsIN[1].valorI  Número de ocurrencias (n)
            ModeloSimulacion modelo = new ModeloSimulacion("Poisson", p.ParamsIN[0].valorD,0,0, 0);
            modelo.poisson.GetModelo();
            modelo.poisson.GetSimulacion(p.ParamsIN[1].valorI);
            modelo.poisson.GetResumen();

            Session["_GraficoProbabilidad"] = modelo.poisson.Graphics;
            Session["_GraficoMuestra"] = modelo.poisson.Results;

            ViewBag.ParametroId = (int)Session["ParametroId"];
            ViewBag.ProyectoId = (int)Session["ProyectoId"];

            for (int i = 0; i < modelo.poisson.ParamsIN.Count; i++)
            {
                try
                {
                    modelo.poisson.ParamsIN[i].valorD = p.ParamsIN[i].valorD;
                    modelo.poisson.ParamsIN[i].valorI = p.ParamsIN[i].valorI;
                }
                catch
                {
                    break;
                }
            }
            
            Asignar((int)Session["ProyectoId"], (int)Session["ParametroId"], "Poisson", p.ParamsIN[0].valorD,0,0, 0, modelo.poisson.ParamsIN);
            return View(modelo.poisson);
        }

        public void Asignar(int ProyectoId, int ParametroId, string Nombre, double a, double b, double c, double d, List<Param> parametros)
        {
            string cadena = "";
            using (TProjContext context = new TProjContext())
            {
                var parametro = context.Parametros.Include("Elemento").Include("Celdas").Where(e => e.Id == ParametroId).Where(oo => oo.Sensible == true).FirstOrDefault();
                parametro.XML_ModeloAsignado = Nombre + "|" + a + "|" + b + "|" + c + "|" + d + "|";
                foreach (Param p in parametros)
                {
                    cadena += "°" + p.indice + "?" + p.nombre + "?" + p.rango + "?" + p.valorD + "?" + p.valorI;
                }
                parametro.XML_ModeloAsignado += cadena;

                context.ParametrosRequester.ModifyElement(parametro, true, ProyectoId, context.UserProfiles.First(u => u.UserName == User.Identity.Name).UserId);
            }
        }

        [ChildActionOnly]
        public ActionResult _GraficoProbabilidad()
        {
            return PartialView((List<Graphic>)Session["_GraficoProbabilidad"]);
        }

        [ChildActionOnly]
        public ActionResult _GraficoMuestra()
        {
            return PartialView((List<Result>)Session["_GraficoMuestra"]);
        }

        [ChildActionOnly]
        public ActionResult _Resumen()
        {
            return PartialView((List<Result>)Session["_GraficoMuestra"]);
        }

        public FileContentResult getImg(int Id)
        {
            ModeloSimulacion m = new ModeloSimulacion("Poisson");
            byte[] byteArray;
            if (Id == -1)
                byteArray = m.poisson.Resumen;
            else
                byteArray = m.poisson.Formulates[Id].Imagen;
            if (byteArray != null)
            {
                return new FileContentResult(byteArray, "image/jpeg");
            }
            else
            {
                return null;
            }
        }
    }
}
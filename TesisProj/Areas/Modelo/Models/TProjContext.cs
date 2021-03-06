﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using TesisProj.Areas.Modelo.Models;
using TesisProj.Areas.Plantilla.Models;
using TesisProj.Areas.Seguridad.Models;

namespace TesisProj.Models.Storage
{
    public partial class TProjContext : DbContext
    {
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<Elemento> Elementos { get; set; }
        public DbSet<Parametro> Parametros { get; set; }
        public DbSet<Formula> Formulas { get; set; }
        public DbSet<SalidaProyecto> SalidaProyectos { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Celda> Celdas { get; set; }
        public DbSet<Operacion> Operaciones { get; set; }
        public DbSet<SalidaOperacion> SalidaOperaciones { get; set; }
        public DbSet<Audit> Audits { get; set; }
        public DbSet<DbVersion> DbVersions { get; set; }
        public DbSet<Colaborador> Colaboradores { get; set; }
        
        public DbRequester<Proyecto> ProyectosRequester { get; set; }
        public DbRequester<Elemento> ElementosRequester { get; set; }
        public DbRequester<Parametro> ParametrosRequester { get; set; }
        public DbRequester<Formula> FormulasRequester { get; set; }
        public DbRequester<SalidaProyecto> SalidaProyectosRequester { get; set; }
        public DbRequester<Celda> CeldasRequester { get; set; }
        public DbRequester<Operacion> OperacionesRequester { get; set; }
        public DbRequester<SalidaOperacion> SalidaOperacionesRequester { get; set; }
        public DbRequester<Audit> AuditsRequester { get; set; }
        public DbRequester<DbVersion> DbVersionsRequester { get; set; }
        public DbRequester<Colaborador> ColaboradoresRequester { get; set; }
        
        public void RegistrarTablasProyecto()
        {
            ProyectosRequester = new DbRequester<Proyecto>(this, Proyectos, Audits);
            ElementosRequester = new DbRequester<Elemento>(this, Elementos, Audits);
            ParametrosRequester = new DbRequester<Parametro>(this, Parametros, Audits);
            FormulasRequester = new DbRequester<Formula>(this, Formulas, Audits);
            SalidaProyectosRequester = new DbRequester<SalidaProyecto>(this, SalidaProyectos, Audits);
            CeldasRequester = new DbRequester<Celda>(this, Celdas, Audits);
            OperacionesRequester = new DbRequester<Operacion>(this, Operaciones, Audits);
            SalidaOperacionesRequester = new DbRequester<SalidaOperacion>(this, SalidaOperaciones, Audits);
            AuditsRequester = new DbRequester<Audit>(this, Audits);
            DbVersionsRequester = new DbRequester<DbVersion>(this, DbVersions, Audits);
            ColaboradoresRequester = new DbRequester<Colaborador>(this, Colaboradores, Audits);
        }

        public void SeedProyecto()
        {
            SeedProyectos();
            SeedElementos();
            SeedParametros();
            SeedFormulas();
            SeedSalidaElementos();
            SeedSalidaProyectos();
            SeedOperaciones();
            SeedSalidaOperaciones();
            SeedColaboradores();
        }

        public void SeedUserProfiles()
        {
        }

        public void SeedProyectos()
        {
        }

        public void SeedElementos()
        {
        }

        public void SeedParametros()
        {
        }

        public void SeedFormulas()
        {
        }

        public void SeedSalidaElementos()
        {
        }

        public void SeedSalidaProyectos()
        {
        }

        public void SeedOperaciones()
        {
        }

        public void SeedSalidaOperaciones()
        {
        }

        public void SeedColaboradores()
        {
        }
    }
}
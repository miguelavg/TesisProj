﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TesisProj.Areas.Modelo.Models;
using TesisProj.Models;
using TesisProj.Models.Storage;
using System.Data.Entity;

namespace TesisProj.Areas.Plantilla.Models
{
    [Table("PlantillaFormula")]
    public class PlantillaFormula : DbObject, IValidatableObject
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El campo {0} debe tener un mínimo de {2} y un máximo de {1} carácteres.")]
        [DisplayName("Fórmula")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Secuencia")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor que 0")]
        public int Secuencia { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "El campo {0} debe tener un mínimo de {2} y un máximo de {1} carácteres.")]
        [DisplayName("Referencia")]
        [RegularExpression("[A-Za-z]+[A-Za-z1-9]*", ErrorMessage = "El campo solo puede contener alfanuméricos y debe comenzar con una letra.")]
        public string Referencia { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Plantilla")]
        public int IdPlantillaElemento { get; set; }

        [ForeignKey("IdPlantillaElemento")]
        public virtual PlantillaElemento PlantillaElemento { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Tipo")]
        public int IdTipoFormula { get; set; }

        [ForeignKey("IdTipoFormula")]
        public virtual TipoFormula TipoFormula { get; set; }

        [DisplayName("Visible")]
        public bool Visible { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(1024, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener un máximo de {1} carácteres.")]
        [DisplayName("Período inicial")]
        public string PeriodoInicial { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(1024, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener un máximo de {1} carácteres.")]
        [DisplayName("Período final")]
        public string PeriodoFinal { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Tipo de dato")]
        public int IdTipoDato { get; set; }

        [ForeignKey("IdTipoDato")]
        public virtual TipoDato TipoDato { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(1024, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener un máximo de {1} carácteres.")]
        [DisplayName("Cadena")]
        public string Cadena { get; set; }

        public string ListName { get { return Nombre + " (" + Referencia + ")"; } }

        public PlantillaFormula(PlantillaFormula plantilla, int idPlantilla)
        {
            this.Referencia = plantilla.Referencia;
            this.Secuencia = plantilla.Secuencia;
            this.Nombre = plantilla.Nombre;
            this.IdTipoFormula = plantilla.IdTipoFormula;
            this.Cadena = plantilla.Cadena;
            this.PeriodoInicial = plantilla.PeriodoInicial;
            this.PeriodoFinal = plantilla.PeriodoFinal;
            this.Visible = plantilla.Visible;
            this.IdTipoDato = plantilla.IdTipoDato;
            this.IdPlantillaElemento = idPlantilla;
        }

        public PlantillaFormula(Formula plantilla, int idPlantilla)
        {
            this.Referencia = plantilla.Referencia;
            this.Secuencia = plantilla.Secuencia;
            this.Nombre = plantilla.Nombre;
            this.IdTipoFormula = plantilla.IdTipoFormula;
            this.Cadena = plantilla.Cadena;
            this.PeriodoInicial = plantilla.PeriodoInicial;
            this.PeriodoFinal = plantilla.PeriodoFinal;
            this.Visible = plantilla.Visible;
            this.IdTipoDato = plantilla.IdTipoDato;
            this.IdPlantillaElemento = idPlantilla;
        }

        public PlantillaFormula() { }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            using (TProjContext context = new TProjContext())
            {
                if (context.PlantillaFormulas.Any(f => f.Nombre == this.Nombre && f.IdPlantillaElemento == this.IdPlantillaElemento && (this.Id > 0 ? f.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe un registro con el mismo nombre en la misma plantilla.", new string[] { "Nombre" });
                }

                if (context.PlantillaFormulas.Any(f => f.Referencia == this.Referencia && f.IdPlantillaElemento == this.IdPlantillaElemento && (this.Id > 0 ? f.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe una fórmula con el mismo nombre de referencia en la misma plantilla.", new string[] { "Referencia" });
                }
                
                if (context.PlantillaParametros.Any(p => p.Referencia == this.Referencia && p.IdPlantillaElemento == this.IdPlantillaElemento && (this.Id > 0 ? p.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe un parámetro con el mismo nombre de referencia en la misma plantilla.", new string[] { "Referencia" });
                }

                if (context.PlantillaFormulas.Any(f => f.Secuencia == this.Secuencia && f.IdPlantillaElemento == this.IdPlantillaElemento && (this.Id > 0 ? f.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe un registro con el mismo número de secuencia en la misma plantilla.", new string[] { "Secuencia" });
                }

                if (Generics.Reservadas.Contains(this.Referencia))
                {
                    yield return new ValidationResult("Ya existe una palabra reservada con el mismo nombre.", new string[] { "Referencia" });
                }

                if (context.PlantillaFormulas.Include(f => f.TipoFormula).Any(f => f.IdTipoFormula == this.IdTipoFormula && f.TipoFormula.Unico && f.IdPlantillaElemento == this.IdPlantillaElemento && (this.Id > 0 ? f.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe una fórmula de este tipo en el elemento. Dicho tipo de fórmula solo permite una por elemento.", new string[] { "IdTipoFormula" });
                }

                MathParserNet.Parser parser = new MathParserNet.Parser();
                parser.AddVariable("Periodo", 5);
                parser.AddVariable("Horizonte", 10);
                parser.AddVariable("PeriodosCierre", 1);
                parser.AddVariable("PeriodosPreOperativos", 1);
                parser.RegisterCustomDoubleFunction("Amortizacion", Generics.Ppmt);
                parser.RegisterCustomDoubleFunction("Intereses", Generics.IPmt);
                parser.RegisterCustomDoubleFunction("Cuota", Generics.Pmt);
                parser.RegisterCustomDoubleFunction("DepreciacionLineal", Generics.Sln);
                parser.RegisterCustomDoubleFunction("DepreciacionAcelerada", Generics.Syn);
                parser.RegisterCustomDoubleFunction("ValorResidual", Generics.ResSln);

                var parametros = context.PlantillaParametros.Where(p => p.IdPlantillaElemento == this.IdPlantillaElemento);
                foreach (PlantillaParametro parametro in parametros)
                {
                    parser.AddVariable(parametro.Referencia, 2);
                }

                var formulas = context.PlantillaFormulas.Where(p => p.IdPlantillaElemento == this.IdPlantillaElemento && p.Secuencia < this.Secuencia);
                foreach (PlantillaFormula formula in formulas)
                {
                    parser.AddVariable(formula.Referencia, 2);
                }

                //  Valida la cadena
                if (!Generics.Validar(this.Cadena, parser))
                {
                    yield return new ValidationResult("Cadena inválida. La operación solo puede contener referencias a tipos de fórmula.", new string[] { "Cadena" });
                }

                parser.RemoveVariable("Periodo");

                //  Valida períodos
                if (!Generics.Validar(this.PeriodoInicial, parser))
                {
                    yield return new ValidationResult("Cadena inválida. Solo puede contener Horizonte o números.", new string[] { "PeriodoInicial" });
                }

                if (!Generics.Validar(this.PeriodoFinal, parser))
                {
                    yield return new ValidationResult("Cadena inválida. Solo puede contener Horizonte o números.", new string[] { "PeriodoFinal" });
                }
            }
        }

    }
}
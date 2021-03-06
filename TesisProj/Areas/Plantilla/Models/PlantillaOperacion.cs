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

namespace TesisProj.Areas.Plantilla.Models
{
    [Table("PlantillaOperacion")]
    public class PlantillaOperacion : DbObject, IValidatableObject
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El campo {0} debe tener un mínimo de {2} y un máximo de {1} carácteres.")]
        [DisplayName("Operación")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(1024, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener un máximo de {1} carácteres.")]
        [DisplayName("Período inicial")]
        public string PeriodoInicial { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(1024, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener un máximo de {1} carácteres.")]
        [DisplayName("Período final")]
        public string PeriodoFinal { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Secuencia")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor que 0")]
        public int Secuencia { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "El campo {0} debe tener un mínimo de {2} y un máximo de {1} carácteres.")]
        [DisplayName("Referencia")]
        [RegularExpression("[A-Za-z]+[A-Za-z1-9]*", ErrorMessage = "El campo solo puede contener alfanuméricos y debe comenzar con una letra.")]
        public string Referencia { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Plantilla")]
        public int IdPlantillaProyecto { get; set; }

        [ForeignKey("IdPlantillaProyecto")]
        public virtual PlantillaProyecto PlantillaProyecto { get; set; }

        [DisplayName("Indicador")]
        public bool Indicador { get; set; }

        [DisplayName("Subrayar")]
        public bool Subrayar { get; set; }

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

        public PlantillaOperacion(PlantillaOperacion plantilla, int idPlantilla)
        {
            this.Indicador = plantilla.Indicador;
            this.Nombre = plantilla.Nombre;
            this.PeriodoInicial = plantilla.PeriodoInicial;
            this.PeriodoFinal = plantilla.PeriodoFinal;
            this.Referencia = plantilla.Referencia;
            this.Secuencia = plantilla.Secuencia;
            this.Cadena = plantilla.Cadena;
            this.Subrayar = plantilla.Subrayar;
            this.IdTipoDato = plantilla.IdTipoDato;
            this.IdPlantillaProyecto = idPlantilla;
        }

        public PlantillaOperacion(Operacion plantilla, int idPlantilla)
        {
            this.Indicador = plantilla.Indicador;
            this.Nombre = plantilla.Nombre;
            this.PeriodoInicial = plantilla.PeriodoInicial;
            this.PeriodoFinal = plantilla.PeriodoFinal;
            this.Referencia = plantilla.Referencia;
            this.Secuencia = plantilla.Secuencia;
            this.Cadena = plantilla.Cadena;
            this.Subrayar = plantilla.Subrayar;
            this.IdTipoDato = plantilla.IdTipoDato;
            this.IdPlantillaProyecto = idPlantilla;
        }

        public PlantillaOperacion() { }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            using (TProjContext context = new TProjContext())
            {
                if (context.PlantillaOperaciones.Any(f => f.Referencia == this.Referencia && f.IdPlantillaProyecto == this.IdPlantillaProyecto && (this.Id > 0 ? f.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe un registro con el mismo nombre de referencia en la misma plantilla.", new string[] { "Referencia" });
                }

                if (context.TipoFormulas.Any(f => this.Referencia.Equals(f.Referencia)))
                {
                    yield return new ValidationResult("Ya existe un tipo de fórmula con el mismo nombre de referencia.", new string[] { "Referencia" });
                }

                if (context.PlantillaOperaciones.Any(f => f.Secuencia == this.Secuencia && f.IdPlantillaProyecto == this.IdPlantillaProyecto && (this.Id > 0 ? f.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe un registro con el mismo número de secuencia en la misma plantilla.", new string[] { "Secuencia" });
                }

                if (Generics.Reservadas.Contains(this.Referencia))
                {
                    yield return new ValidationResult("Ya existe una palabra reservada con el mismo nombre.", new string[] { "Referencia" });
                }
            
                MathParserNet.Parser parser = new MathParserNet.Parser();
                parser.AddVariable("Periodo", 5);
                parser.AddVariable("Horizonte", 10);
                parser.AddVariable("PeriodosCierre", 1);
                parser.AddVariable("PeriodosPreOperativos", 1);

                //  Valida cadena
                var tipoformulas = context.TipoFormulas;
                foreach (TipoFormula tipoformula in tipoformulas)
                {
                    parser.AddVariable(tipoformula.Referencia, 2);
                }

                var operaciones = context.PlantillaOperaciones.Where(o => o.IdPlantillaProyecto == this.IdPlantillaProyecto && o.Secuencia < this.Secuencia);
                foreach (PlantillaOperacion operacion in operaciones)
                {
                    parser.AddVariable(operacion.Referencia, 2);
                }

                //  Valida si es Tir o Van
                if (!Generics.Validar(this.Cadena, parser))
                {
                    if (!Generics.TestComplexParse(this.Cadena, operaciones.ToList()))
                    {
                        yield return new ValidationResult("Cadena inválida. La operación solo puede contener referencias a tipos de fórmula.", new string[] { "Cadena" });
                    }
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
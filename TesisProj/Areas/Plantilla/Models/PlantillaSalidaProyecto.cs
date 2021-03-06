﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using TesisProj.Areas.Modelo.Models;
using TesisProj.Areas.Plantilla.Models;
using TesisProj.Models;
using TesisProj.Models.Storage;

namespace TesisProj.Areas.Plantilla.Models
{
    [Table("PlantillaSalidaProyecto")]
    public class PlantillaSalidaProyecto : DbObject, IValidatableObject
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El campo {0} debe tener un mínimo de {2} y un máximo de {1} carácteres.")]
        [DisplayName("Salida")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Secuencia")]
        [Range(1, int.MaxValue, ErrorMessage = "El campo {0} debe ser mayor que 0")]
        public int Secuencia { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(1024, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener un máximo de {1} carácteres.")]
        [DisplayName("Período inicial")]
        public string PeriodoInicial { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [StringLength(1024, MinimumLength = 1, ErrorMessage = "El campo {0} debe tener un máximo de {1} carácteres.")]
        [DisplayName("Período final")]
        public string PeriodoFinal { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Plantilla")]
        public int IdPlantillaProyecto { get; set; }

        [ForeignKey("IdPlantillaProyecto")]
        public virtual PlantillaProyecto PlantillaProyecto { get; set; }

        [InverseProperty("Salida")]
        public virtual List<PlantillaSalidaOperacion> Operaciones { get; set; }

        public PlantillaSalidaProyecto(PlantillaSalidaProyecto plantilla, int idPlantilla)
        {
            this.Nombre = plantilla.Nombre;
            this.PeriodoInicial = plantilla.PeriodoInicial;
            this.PeriodoFinal = plantilla.PeriodoFinal;
            this.Secuencia = plantilla.Secuencia;
            this.IdPlantillaProyecto = idPlantilla;
        }

        public PlantillaSalidaProyecto(SalidaProyecto plantilla, int idPlantilla)
        {
            this.Nombre = plantilla.Nombre;
            this.PeriodoInicial = plantilla.PeriodoInicial;
            this.PeriodoFinal = plantilla.PeriodoFinal;
            this.Secuencia = plantilla.Secuencia;
            this.IdPlantillaProyecto = idPlantilla;
        }


        public PlantillaSalidaProyecto() { }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            using (TProjContext context = new TProjContext())
            {
                if (context.PlantillaSalidaProyectos.Any(s => s.Nombre == this.Nombre && s.IdPlantillaProyecto == this.IdPlantillaProyecto && (this.Id > 0 ? s.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe un registro con el mismo nombre en la misma plantilla.", new string[] { "Nombre" });
                }

                if (context.PlantillaSalidaProyectos.Any(s => s.Secuencia == this.Secuencia && s.IdPlantillaProyecto == this.IdPlantillaProyecto && (this.Id > 0 ? s.Id != this.Id : true)))
                {
                    yield return new ValidationResult("Ya existe un registro con el mismo número de secuencia en la misma plantilla.", new string[] { "Secuencia" });
                }

                MathParserNet.Parser parser = new MathParserNet.Parser();
                parser.AddVariable("Horizonte", 10);
                parser.AddVariable("PeriodosCierre", 1);
                parser.AddVariable("PeriodosPreOperativos", 1);

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
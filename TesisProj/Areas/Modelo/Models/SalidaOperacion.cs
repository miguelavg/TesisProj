﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Xml.Serialization;
using TesisProj.Areas.Modelo.Models;
using TesisProj.Areas.Plantilla.Models;
using TesisProj.Models;
using TesisProj.Models.Storage;

namespace TesisProj.Areas.Modelo.Models
{
    [Table("OperacionXSalida")]
    public class SalidaOperacion : DbObject
    {
        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Operacion")]
        public int IdOperacion { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Salida")]
        public int IdSalida { get; set; }

        [Required(ErrorMessage = "El campo {0} es obligatorio")]
        [DisplayName("Secuencia")]
        public int Secuencia { get; set; }

        [XmlIgnore]
        [ForeignKey("IdOperacion")]
        public virtual Operacion Operacion { get; set; }

        [XmlIgnore]
        [ForeignKey("IdSalida")]
        public virtual SalidaProyecto Salida { get; set; }

        [XmlIgnore]
        public String Nombre { get { return Operacion.Nombre; } }

        public override string LogValues()
        {
            return "Nombre = " + this.Nombre + Environment.NewLine +
                "Secuencia = " + this.Secuencia + Environment.NewLine +
                "Operación = " + this.Operacion.Nombre + Environment.NewLine +
                "Salida = " + this.Salida.Nombre;
        }
    }
}
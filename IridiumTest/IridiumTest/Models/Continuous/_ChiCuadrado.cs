﻿using IridiumTest.Const;
using IridiumTest.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IridiumTest.Models.Continuous
{
    public class _ChiCuadrado : Datos
    {
        private MathNet.Numerics.Distributions.ChiSquared modelo;
        private double minimo;
        private double maximo;

        public _ChiCuadrado()
        {
            modelo = new MathNet.Numerics.Distributions.ChiSquared(.01);
            CrearComplemento();
        }

        public _ChiCuadrado(double freedom)
        {
            modelo = new MathNet.Numerics.Distributions.ChiSquared(freedom);
            CrearComplemento();
        }

        public double Sample()
        {
            return Math.Round(modelo.Sample() * 1.0, 2);
        }

        private void CrearComplemento()
        {
            minimo = modelo.Mean-5;
            maximo = modelo.Mean+5;
            this.Titulo = "ChiCuadrado";
            this.Resumen = Img.Image2Bytes(_String.patchImgChiCuadrado + "Resumen.png");
            this.link = "http://www.wolframalpha.com/input/?i=chi+squared+distribution&lk=4&num=1";
            this.Definicion = "En estadística, la distribución χ² (de Pearson), llamada Chi cuadrado o Ji cuadrado, es una distribución de probabilidad " +
                              " continua con un parámetro k que representa los grados de libertad de la variable aleatoria " +
                              " X=(Z1)^2+(Z2)^2+ .... (Z3)^2 " +
                              " Donde Zi son variables aleatorias normales independientes de media cero y varianza uno ";

            Imagen = Img.Image2Bytes(_String.patchImgChiCuadrado + "Imagen.png");

            this.ParamsIN = new List<Param> {
                            new Param { indice=1, nombre = "Grados de Librertad", rango = "1 <= k", valorD = 0 },
                            new Param { indice=2, nombre = "Muestras", rango="M > 0",valorI=0 }
            };

            this.ParamsOUT = new List<Param> {
                            new Param { indice=1, nombre = "Máximo", rango = "max"},
                            new Param { indice=2, nombre = "Mínimo", rango = "min"},
                            new Param { indice=3, nombre = "Media ", rango = "E(X)"},
                            new Param { indice=4, nombre = "Mediana ", rango = "med"},
                            new Param { indice=5, nombre = "Moda ", rango = "moda"},
                            new Param { indice=6, nombre = "Varianza ", rango="D(X)"},
                            new Param { indice=7, nombre = "Coeficiente de Simetria ", rango="y1" }
            };

            this.Formulates = new List<Formulate> {
                            new Formulate { NombreFormula="E(X)=", Imagen=Img.Image2Bytes(_String.patchImgChiCuadrado +"EX.png")},
                            new Formulate { NombreFormula="D(X)=", Imagen=Img.Image2Bytes(_String.patchImgChiCuadrado +"DX.png")},
                            new Formulate { NombreFormula="y1=", Imagen=Img.Image2Bytes(_String.patchImgChiCuadrado +"y1.png")},
                            new Formulate { NombreFormula="P(X=r)", Imagen=Img.Image2Bytes(_String.patchImgChiCuadrado +"PX.png")},
            };

            this.Graphics = new List<Graphic>();
            this.Results = new List<Result>();
        }

        public void GetModelo()
        {
            IEnumerable<double> salida = modelo.Samples();
            for (double i = 0; i <= maximo; i = i + 0.1)
            {
                Graphic g = new Graphic();
                g.fx = modelo.Density(i);
                g.Ac = modelo.CumulativeDistribution(i);
                Graphics.Add(g);
            }
        }

        public void GetSimulacion(int valores)
        {
            for (int i = 1; i <= valores; i++)
            {
                Result r = new Result();
                Random rnd = modelo.RandomSource;
                r.Probabilidad = rnd.NextDouble();
                r.ValorObtenidoD = Math.Round(modelo.Sample(), 2);
                Results.Add(r);
            }
        }

        public void GetResumen()
        {
            ParamsOUT[0].valorD = modelo.Maximum;
            ParamsOUT[1].valorD = modelo.Minimum;
            ParamsOUT[2].valorD = Math.Round(modelo.Mean, 2);
            ParamsOUT[3].valorD = modelo.Median;
            ParamsOUT[4].valorD = modelo.Mode;
            ParamsOUT[5].valorD = Math.Round(modelo.Variance, 2);
            ParamsOUT[6].valorD = Math.Round(modelo.Skewness, 2);

            Results = (from s in Results
                       group s by new { s.ValorObtenidoD } into g
                       select new Result()
                       {
                           ValorObtenidoD = g.Key.ValorObtenidoD,
                           cantidad = g.Count(),
                           Probabilidad = g.Average(o => o.Probabilidad)
                       }).OrderBy(u => u.ValorObtenidoI).ToList();
        }

        public void Save()
        {
            String patch = Const._String.patchFileTxt + "ChiCuadrado.txt";

            foreach (Param p in ParamsOUT)
            {
                Util.Out.WriteLineFile(patch, true, "Indice=" + p.indice + " Nombre=" + p.nombre + " ValorI=" + p.valorI + " valorD=" + p.valorD);
            }
            foreach (Graphic p in Graphics)
            {
                Util.Out.WriteLineFile(patch, true, "N=" + p.N + " fx=" + p.fx + " AC=" + p.Ac);
            }
            foreach (Result p in Results)
            {
                Util.Out.WriteLineFile(patch, true, "P=" + p.ValorObtenidoI + " Valor=" + p.cantidad);
            }
        }
    }
}
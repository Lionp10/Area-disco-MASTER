using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace webAreaMaster.Models.ViewModels
{
    public class DepositoViewModel
    {
        [Display(Name = "Deposito")]
        public int depositoID { get; set; }
        public List<SelectListItem> depositoList { get; set; }

        public int stockID { get; set; }

        [Display(Name = "Familia")]
        [Required(ErrorMessage = "Debe seleccionar una familia.")]
        public int selFamilia { get; set; }
        public string familia { get; set; }
        public List<SelectListItem> familiaList { get; set; }

        [Display(Name = "Producto")]
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        public int selProducto { get; set; }
        public string producto { get; set; }
        public List<SelectListItem> productoList { get; set; }


        [Display(Name = "Precio venta por unidad")]
        [Required(ErrorMessage = "Debe ingresar un precio de venta.")]
        public decimal precioVentaUnidad { get; set; }

        [Display(Name = "Precio costo por unidad")]
        [Required(ErrorMessage = "Debe ingresar un precio de costo.")]
        public decimal precioCostoUnidad { get; set; }

        [Display(Name = "Precio costo total")]
        [Required(ErrorMessage = "Debe ingresar un precio de costo.")]
        public decimal precioCostoTotal { get; set; }


        [Display(Name = "Cantidad")]
        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "La cantidad ingresada debe ser mayor o igual a cero.")]
        [Required(ErrorMessage = "Debes ingresar una cantidad.")]
        public decimal cantidad { get; set; }

        [Display(Name = "Fecha de ingreso")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime fechaIngreso { get; set; }

        public List<SelectListItem> stockBarDataList { get; set; }
    }

    public class BarrasViewModel
    {
        public int stockID { get; set; }

        [Display(Name = "Barras")]
        [Required(ErrorMessage = "Debe seleccionar una barra.")]
        public int selBarra { get; set; }
        public string barras { get; set; }
        public List<SelectListItem> barrasList { get; set; }

        [Display(Name = "Familia")]
        [Required(ErrorMessage = "Debe seleccionar una familia.")]
        public int selFamilia { get; set; }
        public string familia { get; set; }
        public List<SelectListItem> familiaList { get; set; }

        [Display(Name = "Producto")]
        [Required(ErrorMessage = "Debe seleccionar un producto.")]
        public int selProducto { get; set; }
        public string producto { get; set; }
        public List<SelectListItem> productoList { get; set; }

        [Display(Name = "Cantidad")]
        [Range(typeof(decimal), "0", "999999999", ErrorMessage = "La cantidad ingresada debe ser mayor que cero.")]
        [Required(ErrorMessage = "Debes ingresar una cantidad.")]
        public decimal cantidad { get; set; }

        [Display(Name = "Disponible")]
        [Required(ErrorMessage = "La cantidad ingresada es mayor al stock disponible.")]
        public decimal cantidadDisponible { get; set; }
        public List<SelectListItem> cantDispList { get; set; }

        [Display(Name = "Precio venta unidad")]
        public decimal precioVentaUnidad { get; set; }

        [Display(Name = "Fecha de ingreso")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime fechaIngreso { get; set; }
    }
}
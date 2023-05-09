using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace webAreaMaster.Models.ViewModels
{
    public class CrearBarrasViewModel
    {
        public int barraID { get; set; }

        [Display(Name = "Nombre de barra")]
        [Required(ErrorMessage = "Debes ingresar un nombre para la barra.")]
        public string barraNombre { get; set; }

        [Display(Name = "Sector de barra")]
        [Required(ErrorMessage = "Debes seleccionar un sector para la barra.")]
        public int selSector { get; set; }
        public string sectorBarraNombre { get; set; }
        public List<SelectListItem> secBarraList { get; set; }
    }
}
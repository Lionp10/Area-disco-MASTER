using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CompareAttribute = System.ComponentModel.DataAnnotations.CompareAttribute;

namespace webAreaMaster.Models.ViewModels
{
    public class UsuarioViewModel
    {
        public int UserID { get; set; }
        [Display(Name = "Nombre")]
        public string UserNombre { get; set; }
        [Display(Name = "Apellido")]
        public string UserApellido { get; set; }
        [EmailAddress]
        [Display(Name = "Email")]
        public string UserEmail { get; set; }
        [Display(Name = "Contraseña")]
        public string UserContrasena { get; set; }
        public int UserRolID { get; set; }
    }

    public class CrearUsuarioViewModel
    {
        public int UserID { get; set; }
        [Display(Name = "Nombre")]
        [Required(ErrorMessage = "Debes ingresar el nombre del usuario.")]
        public string UserNombre { get; set; }

        [Display(Name = "Apellido")]
        [Required(ErrorMessage = "Debes ingresar el apellido del usuario.")]
        public string UserApellido { get; set; }

        public string userNombreCompleto { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Debes ingresar el email del usuario.")]
        public string UserEmail { get; set; }

        [Display(Name = "Contraseña")]
        [Required(ErrorMessage = "Debes crear una contraseña.")]
        public string UserContrasena { get; set; }

        [Compare("UserContrasena", ErrorMessage = "Las contraseñas no coinciden.")]
        [Display(Name = "Repetir contraseña")]
        [Required(ErrorMessage = "Debes repetir la contraseña.")]
        public string UserConfirmarContrasena { get; set; }

        [Display(Name = "Seleccionar rol")]
        [Required(ErrorMessage = "Debes seleccionar un rol.")]
        public int selRol { get; set; }
        public string rolNombre { get; set; }
        public List<SelectListItem> rolesList { get; set; }
    }
    public class NuevaContrasenaViewModel
    {
        public int UserID { get; set; }

        [Display(Name = "Nombre")]
        public string UserNombre { get; set; }

        [Display(Name = "Apellido")]
        public string UserApellido { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Debes ingresar el email con el cual registraste tu usuario.")]
        public string UserEmail { get; set; }

        [Display(Name = "Nueva contraseña")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string UserNuevaContraseña { get; set; }

        [Display(Name = "Confirmar contraseña")]
        [Compare("UserNuevaContraseña", ErrorMessage = "Las contraseñas no coinciden.")]        
        [DataType(DataType.Password)]
        public string UserConfirmarContraseña { get; set; }

        [Display(Name = "Seleccionar rol")]
        public int UserRol { get; set; }
    }

    public class CambiarContrasenaViewModel
    {
        public int UserID { get; set; }

        [NotMapped]
        public string Id { get; set; }

        [Display (Name = "Contraseña actual")]
        [DataType(DataType.Password)]
        public string UserContrasena { get; set; }

        [Display(Name = "Contraseña nueva")]
        [MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
        [DataType(DataType.Password)]
        public string UserContrasenaNueva { get; set; }

        [Display(Name = "Repetir contraseña nueva")]
        [NotMapped]
        [Compare("UserContrasenaNueva", ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string UserRepContrasena { get; set; }
    }
}
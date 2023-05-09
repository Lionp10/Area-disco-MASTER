using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using webAreaMaster.Models.ViewModels;
using webAreaMaster.Models;
using System.Threading.Tasks;
using webAreaMaster.Filters;
using System.Data.Entity;
using System.Net;

namespace webAreaMaster.Controllers
{
    public class UsuariosController : Controller
    {
        private sistemaAreaMasterEntities db = new sistemaAreaMasterEntities();

        [HttpGet]
        [CheckSession]
        public ActionResult Crear()
        {
            CrearUsuarioViewModel model = new CrearUsuarioViewModel();
            model.rolesList = ListarRoles();
            return View(model);
        }

        [HttpPost]
        [CheckSession]
        public async Task<ActionResult> Crear(CrearUsuarioViewModel model)
        {
            model.rolesList = ListarRoles();

            if (!ModelState.IsValid)
                return View(model);

            var existingUser = db.usuarioData.FirstOrDefault(u => u.userEmail == model.UserEmail);

            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "El correo electrónico ya está registrado.");
                return View(model);
            }
            else
            {
                usuarioData newUser = new usuarioData()
                {
                    userNombre = model.UserNombre,
                    userApellido = model.UserApellido,
                    userEmail = model.UserEmail,
                    userContrasena = Encrypt.GetSHA256(model.UserContrasena),
                    userRolID = model.selRol
                };

                db.usuarioData.Add(newUser);
                await db.SaveChangesAsync();

                return RedirectToAction("Listado", "Usuarios", new { id = newUser.userID });
            }
        }

        [HttpGet] // Vista
        [CheckSession]
        public ActionResult Listado()
        {
            return View();
        }

        [HttpPost] // Controller
        public JsonResult Eliminar(int id)
        {
            try
            {
                using (var db = new sistemaAreaMasterEntities())
                {
                    usuarioData user = db.usuarioData.Find(id);

                    string message = "Eliminaste el usuario n°: " + id;

                    if (user == null)
                        return Json(false, JsonRequestBehavior.AllowGet);

                    db.usuarioData.Remove(user);
                    db.SaveChanges();

                    return Json(new { result = true, message }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet] // Vista
        [CheckSession]
        public async Task<ActionResult> Modificar(int? id)
        {
            try
            {
                usuarioData userData = await db.usuarioData.FindAsync(id);

                if (userData == null)
                    throw new HttpException((int)HttpStatusCode.BadRequest, "La consulta no devuelve contenido.");

                var rol = await db.usuarioRolData.FindAsync(userData.userRolID);

                CrearUsuarioViewModel model = new CrearUsuarioViewModel()
                {
                    UserID = userData.userID,
                    UserNombre = userData.userNombre,
                    UserApellido = userData.userApellido,
                    UserEmail = userData.userEmail,
                    UserContrasena = userData.userContrasena,
                    rolNombre = rol.rolNombre,
                    rolesList = ListarRoles()
                };
                return View(model);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost] // Controller
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Modificar(CrearUsuarioViewModel model)
        {
            try
            {
                model.rolesList = ListarRoles();

                if (!ModelState.IsValid)
                    return View(model);


                usuarioData userData = db.usuarioData.FirstOrDefault(p => p.userID == model.UserID);

                userData.userNombre = model.UserNombre;
                userData.userApellido = model.UserApellido;
                userData.userEmail = model.UserEmail;
                userData.userRolID = model.selRol;

                db.Entry(userData).State = EntityState.Modified;
                await db.SaveChangesAsync();

                string message = "Modificaste el usuario n° <b>" + model.UserID + "</b> con éxito.";
                TempData["Info"] = message;

                return RedirectToAction("Listado", "Usuarios");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region --> Listados

        [HttpPost] // Controller
        [CheckSession]   
        public JsonResult ListadoUser()
        {
            try
            {
                var draw = Request.Form.GetValues("draw").FirstOrDefault();
                var start = Request.Form.GetValues("start").FirstOrDefault();
                var length = Request.Form.GetValues("length").FirstOrDefault();
                var searchValue = Request.Form.GetValues("search[value]").FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;

                var query = from usu in db.usuarioData
                            join rol in db.usuarioRolData on usu.userRolID equals rol.rolID
                            select new CrearUsuarioViewModel
                            {
                                UserID = usu.userID,
                                userNombreCompleto = usu.userApellido + ", " + usu.userNombre,
                                UserEmail = usu.userEmail,
                                rolNombre = rol.rolNombre
                            };

                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m => m.UserID.ToString().Contains(searchValue.ToLower()) || m.userNombreCompleto.ToLower().Contains(searchValue.ToLower()) || m.UserEmail.ToLower().Contains(searchValue.ToLower()) || m.rolNombre.ToLower().Contains(searchValue.ToLower()));
                }

                recordsTotal = query.Count();
                var data = query.OrderByDescending(s => s.UserID).Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static List<SelectListItem> ListarRoles()
        {
            List<SelectListItem> rolesList = new List<SelectListItem>();

            using (var db = new sistemaAreaMasterEntities())
            {
                var rol = db.usuarioRolData.ToList();
                foreach (var item in rol)
                {
                    rolesList.Add(new SelectListItem
                    {
                        Text = item.rolNombre.ToUpper(),
                        Value = item.rolID.ToString()
                    });
                }
            }
            return rolesList;
        }
        #endregion
    }
}
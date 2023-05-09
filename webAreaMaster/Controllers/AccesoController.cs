using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using webAreaMaster.Models.ViewModels;
using System.Web.Security;
using webAreaMaster.Models;
using System.Web.Routing;
using System.Data.Entity;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Configuration;
using System.Net.Mail;
using System.Net;
using Microsoft.Ajax.Utilities;
using webAreaMaster.Filters;
using Newtonsoft.Json.Linq;
using webAreaMaster.Helpers;

namespace webAreaMaster.Controllers
{
    public class AccesoController : Controller
    {
        private sistemaAreaMasterEntities db = new sistemaAreaMasterEntities();


        [HttpGet]
        public ActionResult Login()
        {
            if (Session["Session"] == null)
            {
                var userInfo = new UsuarioViewModel();
                try
                {
                    asegurarDesconexion();
                    return View(userInfo);
                }
                catch
                {
                    throw;
                }
            }
            else return RedirectToAction("Index", "Inicio");
        }

        [HttpPost]
        public async Task<ActionResult> Login(UsuarioViewModel model)
        {
            string ePass = Encrypt.GetSHA256(model.UserContrasena);
            if (ModelState.IsValid)
            {
                var user = await db.usuarioData.FirstOrDefaultAsync(u => u.userEmail == model.UserEmail && u.userContrasena == ePass);
                if (user != null)
                {
                    var identity = new FormsIdentity(new FormsAuthenticationTicket(user.userEmail, true, 60));
                    var principal = new ClaimsPrincipal(identity);
                    var roles = new[] { user.userRolID.ToString() };
                    HttpContext.User = new ClaimsPrincipal(identity);
                    recordarSesion(model.UserEmail, true);
                    HttpContext.Session["Session"] = user.userID;
                    HttpContext.Session["userEmail"] = user.userEmail;
                    FormsAuthentication.SetAuthCookie(user.userEmail, true);

                    return RedirectToAction("Index", "Inicio");
                }
            }
            ModelState.AddModelError("LoginError", "El correo electrónico o la contraseña son incorrectos.");
            return View(model);
        }
        public ActionResult Desconectarse()
        {
            try
            {
                FormsAuthentication.SignOut();
                HttpContext.User = new GenericPrincipal(new GenericIdentity(string.Empty), null);

                Session.Clear();
                System.Web.HttpContext.Current.Session.RemoveAll();
                return RedirectToAction("Login");
            }
            catch
            {
                throw;
            }
        }

        private void recordarSesion(string userName, bool userRecordar = false)
        {
            FormsAuthentication.SignOut();
            FormsAuthentication.SetAuthCookie(userName, userRecordar);
        }

        private void asegurarDesconexion()
        {
            if (Request.IsAuthenticated)
                Desconectarse();
        }

        [HttpGet]
        public ActionResult EmailRestPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EmailRestPassword(NuevaContrasenaViewModel model)
        {
            try
            {
                var user = await db.usuarioData.FirstOrDefaultAsync(u => u.userEmail == model.UserEmail);

                if (user == null)
                {
                    ModelState.AddModelError("Email", "El correo electrónico ingresado no existe.");
                    return View(model);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }
                else
                {
                    var token = GeneratePasswordResetToken(user);
                    var callbackUrl = Url.Action("ForgotPassword", "Acceso", new { token, email = user.userEmail }, protocol: Request.Url.Scheme);

                    await SendPasswordResetEmailAsync(user.userEmail, callbackUrl);
                }

                return RedirectToAction("_EmailRestPasswordCorrect");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public ActionResult _EmailRestPasswordCorrect()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ForgotPassword(string email)
        {
            usuarioData oUser = db.usuarioData.FirstOrDefault(p => p.userEmail == email);
            if (oUser == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "La consulta no devuelve contenido.");

            NuevaContrasenaViewModel model = new NuevaContrasenaViewModel()
            {
                UserID = oUser.userID,
                UserNombre = oUser.userNombre,
                UserApellido = oUser.userApellido,
                UserEmail = "",
                UserNuevaContraseña = "",
                UserConfirmarContraseña = "",
                UserRol = (int)oUser.userRolID,
            };

            if (model.UserNuevaContraseña == "")
            {
                ModelState.AddModelError("PassNull", "Debes ingresar una contraseña nueva .");
                return View(model);
            }

            return View(model);
        }

        [HttpPost] // Controller
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(NuevaContrasenaViewModel model)
        {
            try
            {
                usuarioData oUser = db.usuarioData.FirstOrDefault(p => p.userEmail == model.UserEmail);

                if (!ModelState.IsValid)
                    return View(model);

                if (oUser == null)
                {
                    ModelState.AddModelError("UserNull", "El usuario no existe.");
                    return View(model);
                }
                else
                {
                    oUser.userContrasena = Encrypt.GetSHA256(model.UserNuevaContraseña);
                    oUser.userID = model.UserID;
                    oUser.userNombre = model.UserNombre;
                    oUser.userApellido = model.UserApellido;
                    oUser.userEmail = model.UserEmail;
                    oUser.userRolID = model.UserRol;

                    db.Entry(oUser).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }

                return RedirectToAction("_ForgotPasswordCorrect");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        public ActionResult _ForgotPasswordCorrect()
        {
            return View();
        }

        private string GeneratePasswordResetToken(usuarioData user)
        {
            var passwordResetToken = db.PasswordResetTokens.FirstOrDefault(t => t.userID == user.userID);
            if (passwordResetToken == null)
            {
                // Si no existe un token para el usuario, creamos uno nuevo
                passwordResetToken = new PasswordResetTokens
                {
                    userID = user.userID,
                    Token = Guid.NewGuid().ToString(),
                    ExpirationDate = DateTime.UtcNow.AddHours(24)
                };
                db.PasswordResetTokens.Add(passwordResetToken);
            }
            else
            {
                // Si ya existe un token para el usuario, actualizamos su fecha de expiración
                passwordResetToken.ExpirationDate = DateTime.UtcNow.AddHours(24);
            }
            db.SaveChanges();

            return passwordResetToken.Token;
        }

        private async Task SendPasswordResetEmailAsync(string userEmail, string callbackUrl)
        {
            var fromAddress = new MailAddress("lionp.otros@gmail.com", "SistemaArea");
            var toAddress = new MailAddress(userEmail);
            const string fromPassword = "";
            const string subject = "Restablecer contraseña";
            var body = $"Estimado usuario,<br><br>Hemos recibido una solicitud para restablecer la contraseña de su cuenta. Por favor, haga clic en el siguiente enlace para restablecer su contraseña:<br><a href='{callbackUrl}'>{callbackUrl}</a><br><br>Si no ha solicitado restablecer su contraseña, ignore este correo electrónico.<br><br>Gracias,<br>El equipo de Area disco.";

            var smtp = new SmtpClient
            {
                Host = "smtp-relay.sendinblue.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                await smtp.SendMailAsync(message);
            }
        }

        [HttpGet] // Vista
        [CheckSession]
        public ActionResult CambiarContrasena()
        {
            var userID = HttpContext.Session["Session"];

            var model = new CambiarContrasenaViewModel();
            model.UserID = (int)userID;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [CheckSession]
        public async Task<ActionResult> CambiarContrasena(CambiarContrasenaViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return View(model);

                usuarioData userData = db.usuarioData.FirstOrDefault(p => p.userID == model.UserID);

                string contrasenaActual = userData.userContrasena; 

                userData.userContrasena = Encrypt.GetSHA256(model.UserContrasenaNueva); 

                if (contrasenaActual == Encrypt.GetSHA256(model.UserContrasena))
                {
                    db.Entry(userData).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                }
                else
                {
                    ModelState.AddModelError("ErrorPass", "Contraseña actual incorrecta.");
                    return View(model);
                }

                return RedirectToAction("_CambiarContrasenaCorrect", "Acceso");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet]
        [CheckSession]
        public ActionResult _CambiarContrasenaCorrect()
        {
            return View();
        }
    }
}
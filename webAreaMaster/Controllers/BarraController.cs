using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using webAreaMaster.Models;
using webAreaMaster.Models.ViewModels;

namespace webAreaMaster.Controllers
{
    public class BarraController : Controller
    {

        private sistemaAreaMasterEntities db = new sistemaAreaMasterEntities();

        [HttpGet] // Vista
        public ActionResult Crear()
        {
            CrearBarrasViewModel model = new CrearBarrasViewModel();
            model.secBarraList = ListarSectores();
            return View(model);            
        }

        [HttpPost] // Controller
        public async Task<ActionResult> Crear(CrearBarrasViewModel model)
        {
            model.secBarraList = ListarSectores();

            if (!ModelState.IsValid)
                return View(model);

            barraData newBarra = new barraData()
            {
                barraNombre = model.barraNombre,
                barraSector = model.selSector,
            };
            db.barraData.Add(newBarra);
            await db.SaveChangesAsync();

            string message = "Has creado una nueva barra exitosamente.";
            TempData["Info"] = message;

            return RedirectToAction("Crear", "Barra");
        }

        [HttpGet] // Vista
        public ActionResult Cargar()
        {
            BarrasViewModel model = new BarrasViewModel();
            model.barrasList = ListarBarras();
            model.familiaList = ListarFamilias();
            model.productoList = ListarProductos(null);

            var cantidades = ListarCantidades(null);
            if (cantidades.Count > 0)
            {
                var selectedValue = cantidades[0].Value;
                if (decimal.TryParse(selectedValue, out decimal cantidadDisponible))
                {
                    model.cantidadDisponible = cantidadDisponible;
                }
                else
                {
                    model.cantidadDisponible = 0;
                }
            }
            else
            {
                model.cantidadDisponible = 0;
            }

            return View(model);
        }

        [HttpPost] // Controller
        public async Task<ActionResult> Cargar(BarrasViewModel model, int? selProducto, int? cantidad)
        {
            try
            {
                model.barrasList = ListarBarras();
                model.familiaList = ListarFamilias();
                model.productoList = ListarProductos(null);

                if (cantidad == null)
                {
                    ModelState.AddModelError(model.cantidad.ToString(), "Debes ingresar una cantidad.");
                    return View(model);
                }
                else
                {
                    model.cantidad = (decimal)cantidad;
                }

                var producto = db.stockDepositoData.FirstOrDefault(p => p.stockID == selProducto); // Busca el producto correspondiente en la base de datos

                if (cantidad > producto.stockCantidad) // Si la cantidad a agregar es mayor a la cantidad disponible, muestra un mensaje de error
                {
                    ModelState.AddModelError("cantidad", "No hay suficiente cantidad disponible del producto.");
                    return View(model);
                }

                if (!ModelState.IsValid)
                    return View(model);

                

                stockBarrasData newStock = new stockBarrasData()
                {
                    sbBarraID = model.selBarra,
                    sbFamiliaID = model.selFamilia,
                    sbProductoID = model.selProducto,
                    sbCantidad = model.cantidad,
                    sbPrecioVentaUnidad = producto.stockPrecioVentaUnidad,
                    sbFechaIngreso = DateTime.Now
                };

                // buscar el registro correspondiente en la tabla
                var existingStock = db.stockBarrasData.FirstOrDefault(x => x.sbBarraID == model.selBarra && x.sbProductoID == selProducto);
                                 
                if (existingStock != null)
                {
                    // sumar la cantidad actual con la cantidad ingresada
                    existingStock.sbCantidad += newStock.sbCantidad;  // Cantidad actualizada
                    existingStock.sbFechaIngreso = DateTime.Now; // Fecha de ingreso actualizada

                    // actualizar el registro en la tabla
                    db.Entry(existingStock).State = EntityState.Modified;
                }
                else
                {
                    db.stockBarrasData.Add(newStock);
                }
             
                // Resta cantidad en tabla stockDeposito
                var cantARestar = db.stockDepositoData.FirstOrDefault(x => x.stockID == selProducto);
                cantARestar.stockCantidad -= model.cantidad;

                await db.SaveChangesAsync();

                var barra = await db.barraData.FindAsync(newStock.sbBarraID);
                TempData["Message"] = "Añadiste nuevo stock a la " + barra.barraNombre;

                return RedirectToAction("Cargar");


            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost] // Controller
        public JsonResult Eliminar(int id)
        {
            using (var db = new sistemaAreaMasterEntities())
            {
                stockBarrasData stock = db.stockBarrasData.Find(id);
                stockDepositoData stockDep = db.stockDepositoData.FirstOrDefault(x => x.stockID == stock.sbProductoID);
                decimal cantEliminada = (decimal)stock.sbCantidad;
                decimal cantActualizada = cantEliminada += (decimal)stockDep.stockCantidad;

                string message = "Eliminaste el stock n°: " + id;

                if (stock == null)
                    return Json(false, JsonRequestBehavior.AllowGet);

                stockDep.stockCantidad = cantActualizada;
                stock.sbCantidad = 0;
                db.Entry(stockDep).State = EntityState.Modified;                
                db.Entry(stock).State = EntityState.Modified;
                //db.stockBarrasData.Remove(stock);
                db.SaveChanges();

                return Json(new { result = true, message }, JsonRequestBehavior.AllowGet);
                
            }
        }

        [HttpGet] // Vista
        public ActionResult Listado()
        {
            return View();
        }

        [HttpGet] // Vista
        
        public async Task<ActionResult> Modificar(int? id)
        {
            stockBarrasData stockData = await db.stockBarrasData.FindAsync(id);

            if (stockData == null)
                throw new HttpException((int)HttpStatusCode.BadRequest, "La consulta no devuelve contenido.");

            var fam = await db.familiaData.FindAsync(stockData.sbFamiliaID);
            var prod = await db.productoData.FindAsync(stockData.sbProductoID);
            var bar = await db.barraData.FindAsync(stockData.sbBarraID);

            BarrasViewModel model = new BarrasViewModel()
            {
                stockID = stockData.sbID,
                selProducto = (int)stockData.sbProductoID,
                selFamilia = (int)stockData.sbFamiliaID,
                selBarra = (int)stockData.sbBarraID,
                barras = bar.barraNombre.ToUpper(),
                familia = fam.famNombre.ToUpper(),
                producto = prod.prodNombre.ToUpper(),
                cantidad = (decimal)stockData.sbCantidad,
                precioVentaUnidad = (decimal)stockData.sbPrecioVentaUnidad,
                fechaIngreso = (DateTime)stockData.sbFechaIngreso,

                barrasList = ListarBarras(),
                familiaList = ListarFamilias(),
                productoList = ListarProductos(prod.prodID)
            };
            return View(model);
        }

        [HttpPost] // Controller
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Modificar(BarrasViewModel model, int? selBarra, int? selFamilia, int? selProducto, decimal precioVentaUnidad)
        {
            try
            {
                model.barrasList = ListarBarras();
                model.familiaList = ListarFamilias();
                model.productoList = ListarProductos(null);

                if (!ModelState.IsValid)
                    return View(model);

                stockBarrasData stockData = db.stockBarrasData.FirstOrDefault(p => p.sbID == model.stockID);
                stockDepositoData stockDepData = db.stockDepositoData.FirstOrDefault(p => p.stockID == selProducto);

                decimal cantActual = (decimal)stockData.sbCantidad;
                decimal cantNueva = model.cantidad;
                decimal cantRestante = cantNueva - cantActual;

                if (cantRestante < 0)
                {
                    decimal cantRestanteAbs = Math.Abs(cantRestante);
                    stockDepData.stockCantidad += cantRestanteAbs;
                    db.Entry(stockDepData).State = EntityState.Modified;
                }
                else
                {
                    if (cantRestante > stockDepData.stockCantidad) // Si la cantidad a agregar es mayor a la cantidad disponible, muestra un mensaje de error
                    {
                        ModelState.AddModelError("cantidad", "No hay suficiente cantidad disponible del producto.");
                        return View(model);
                    }
                    else
                    {
                        stockDepData.stockCantidad -= cantRestante;
                        db.Entry(stockDepData).State = EntityState.Modified;
                    }                   
                }

                stockData.sbBarraID = model.selBarra;
                stockData.sbFamiliaID = model.selFamilia;
                stockData.sbProductoID = model.selProducto;
                stockData.sbCantidad = model.cantidad;
                stockData.sbPrecioVentaUnidad = model.precioVentaUnidad;

                db.Entry(stockData).State = EntityState.Modified;
                await db.SaveChangesAsync();

                string message = "Modificaste el stock n° <b>" + model.stockID + "</b> con éxito.";
                TempData["Info"] = message;

                return RedirectToAction("Listado", "Barra");
            }
            catch (Exception)
            {
                throw;
            }
        }

        #region -->Listados

        [HttpPost] // Controller
        public JsonResult ListadoBarras() // Listar Barras
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

                var query = from bar in db.barraData
                            join sec in db.sectorData on bar.barraSector equals sec.secID
                            select new CrearBarrasViewModel
                            {
                                barraID = bar.barraID,
                                barraNombre = bar.barraNombre,
                                sectorBarraNombre = sec.secNombre
                            };


                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m => m.barraID.ToString().Contains(searchValue.ToLower()) || m.barraNombre.ToLower().Contains(searchValue.ToLower()) || m.sectorBarraNombre.ToLower().Contains(searchValue.ToLower()));
                }

                recordsTotal = query.Count();
                var data = query.OrderByDescending(s => s.barraID).Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static List<SelectListItem> ListarBarras() // Llamar barras
        {
            BarrasViewModel model = new BarrasViewModel();
            List<SelectListItem> barrasList = new List<SelectListItem>();

            using (var db = new sistemaAreaMasterEntities())
            {
                var bar = db.barraData.ToList();
                foreach (var item in bar)
                {
                    barrasList.Add(new SelectListItem
                    {
                        Text = item.barraNombre.ToUpper(),
                        Value = item.barraID.ToString()
                    });
                }
            }
            return barrasList;
        }

        [HttpPost] // Controller
        public JsonResult ListarStockBarras(string fechaDesde, string fechaHasta) // Listar stock de las barras
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

                var query = from sb in db.stockBarrasData
                            join fam in db.familiaData on sb.sbFamiliaID equals fam.famID
                            join prod in db.productoData on sb.sbProductoID equals prod.prodID
                            join bar in db.barraData on sb.sbBarraID equals bar.barraID
                            where sb.sbCantidad > 0
                            select new BarrasViewModel
                            {
                                stockID = sb.sbID,
                                barras = bar.barraNombre,
                                familia = fam.famNombre,
                                producto = prod.prodNombre,
                                cantidad = (decimal)sb.sbCantidad,
                                precioVentaUnidad = (decimal)sb.sbPrecioVentaUnidad,
                                fechaIngreso = (DateTime)sb.sbFechaIngreso
                            };

                // Agrega el filtrado por fechas
                if (!string.IsNullOrEmpty(fechaDesde))
                {
                    DateTime dateDesde = Convert.ToDateTime(fechaDesde);
                    query = query.Where(m => m.fechaIngreso >= dateDesde);
                }
                if (!string.IsNullOrEmpty(fechaHasta))
                {
                    DateTime dateHasta = Convert.ToDateTime(fechaHasta);
                    query = query.Where(m => m.fechaIngreso <= dateHasta);
                }

                if (!string.IsNullOrEmpty(searchValue))
                {
                    query = query.Where(m => m.stockID.ToString().Contains(searchValue.ToLower()) || m.barras.ToLower().Contains(searchValue.ToLower()) || m.familia.ToLower().Contains(searchValue.ToLower()) || m.producto.ToLower().Contains(searchValue.ToLower()) || m.cantidad.ToString().Contains(searchValue.ToLower()));
                }

                recordsTotal = query.Count();
                var data = query.OrderByDescending(s => s.stockID).Skip(skip).Take(pageSize).ToList();

                return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static List<SelectListItem> ListarSectores()
        {
            CrearBarrasViewModel model = new CrearBarrasViewModel();
            List<SelectListItem> secBarraList = new List<SelectListItem>();

            using (var db = new sistemaAreaMasterEntities())
            {
                var bar = db.sectorData.ToList();
                foreach (var item in bar)
                {
                    secBarraList.Add(new SelectListItem
                    {
                        Text = item.secNombre.ToUpper(),
                        Value = item.secID.ToString()
                    });
                }
            }
            return secBarraList;
        }

        private static List<SelectListItem> ListarFamilias()
        {
            BarrasViewModel model = new BarrasViewModel();
            List<SelectListItem> familiaList = new List<SelectListItem>();

            using (var db = new sistemaAreaMasterEntities())
            {
                var fam = db.familiaData.ToList();
                foreach (var item in fam)
                {
                    familiaList.Add(new SelectListItem
                    {
                        Text = item.famNombre.ToUpper(),
                        Value = item.famID.ToString()
                    });
                }
            }
            return familiaList;
        }

        [HttpPost]
        public JsonResult ObtenerProductos(int id)
        {
            return Json(ListarProductos(id), JsonRequestBehavior.AllowGet);
        }

        private static List<SelectListItem> ListarProductos(int? id)
        {
            using (var db = new sistemaAreaMasterEntities())
            {
                if (id == null)
                {
                    BarrasViewModel model = new BarrasViewModel();
                    List<SelectListItem> productoList = new List<SelectListItem>();

                    var prod = db.productoData.ToList();
                    foreach (var item in prod)
                    {
                        productoList.Add(new SelectListItem
                        {
                            Text = item.prodNombre.ToUpper(),
                            Value = item.prodID.ToString()
                        });
                    }
                    return productoList;
                }
                else
                {
                    var prod = db.productoData.Where(x => x.prodFamiliaID == id)
                        .OrderBy(x => x.prodID)
                        .Select(x => new SelectListItem { Value = x.prodID.ToString(), Text = x.prodNombre.ToUpper() })
                        .ToList();

                    return prod;
                }
            }
        }

        public JsonResult ObtenerCantidades(int id)
        {
            return Json(ListarCantidades(id), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ObtenerCantidadDisponible(int id)
        {
            decimal cantidadDisponible = 0;
            using (var db = new sistemaAreaMasterEntities())
            {
                var stockDepositoData = db.stockDepositoData.FirstOrDefault(x => x.stockProductoID == id);
                if (stockDepositoData != null)
                {
                    cantidadDisponible = (decimal)stockDepositoData.stockCantidad;
                }
            }
            return Json(cantidadDisponible, JsonRequestBehavior.AllowGet);
        }

        private static List<SelectListItem> ListarCantidades(int? id)
        {
            BarrasViewModel model = new BarrasViewModel();
            List<SelectListItem> cantDispList = new List<SelectListItem>();

            using (var db = new sistemaAreaMasterEntities())
            {
                var cant = db.stockDepositoData.ToList();
                foreach (var item in cant)
                {
                    cantDispList.Add(new SelectListItem
                    {
                        Text = item.stockProductoID.ToString(),
                        Value = item.stockCantidad.ToString()
                    });
                }
            }
            return cantDispList;
        }

        #endregion
    }
}
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using webAreaMaster.Filters;
using webAreaMaster.Models;
using webAreaMaster.Models.ViewModels;
using WebGrease.Css.Extensions;

namespace webAreaMaster.Controllers
{
    public class DepositoController : Controller
    {
        private sistemaAreaMasterEntities db = new sistemaAreaMasterEntities();

        [HttpGet] // Vista
        [CheckSession]
        public ActionResult Cargar()
        {
            DepositoViewModel model = new DepositoViewModel();
            model.depositoList = ListarDepositos();
            model.familiaList = ListarFamilias();
            model.productoList = ListarProductos(null);
            return View(model);
        }

        [HttpPost] // Controller
        //[CustomAuthorize(Roles = "Administrador")]
        public async Task<ActionResult> Cargar(DepositoViewModel model)
        {
            try
            {
                model.depositoList = ListarDepositos();
                model.familiaList = ListarFamilias();
                model.productoList = ListarProductos(null);

                if (!ModelState.IsValid)
                    return View(model);

                var context = new sistemaAreaMasterEntities();
                stockDepositoData newStock = new stockDepositoData()
                {
                    stockID = model.stockID,
                    stockDepositoID = 1,
                    stockFamiliaID = model.selFamilia,
                    stockProductoID = model.selProducto,
                    stockCantidad = model.cantidad,
                    stockPrecioVentaUnidad = model.precioVentaUnidad,
                    stockPrecioCostoUnidad = model.precioCostoUnidad,
                    stockPrecioCostoTotal = model.precioCostoUnidad * model.cantidad,
                    stockFechaIngreso = DateTime.Now,
                    stockCodProducto = model.selProducto
                };

                // buscar el registro correspondiente en la tabla
                var existingStock = context.stockDepositoData.FirstOrDefault(x => x.stockProductoID == model.selProducto);

                if (existingStock != null)
                {
                    // Actualizar precio costo y precio costo toal
                    decimal pcostu = (decimal)existingStock.stockPrecioCostoUnidad; // Precio costo actual
                    decimal pcostutotal = (decimal)existingStock.stockPrecioCostoUnidad * (decimal)existingStock.stockCantidad;  // Precio costo actual total
                    decimal newpcostu = (decimal)model.precioCostoUnidad; // Precio costo nuevo
                    decimal newpcostutotal = (decimal)model.precioCostoUnidad * (decimal)model.cantidad; // Precio costo nuevo total
                    decimal newpcostototal = pcostutotal + newpcostutotal;

                    existingStock.stockPrecioCostoUnidad = newpcostu;  // Precio costo unidad actualizado
                    existingStock.stockPrecioCostoTotal = newpcostototal;  // Precio costo total actualizado

                    // Actualizar precio venta
                    existingStock.stockPrecioVentaUnidad = model.precioVentaUnidad;
                    existingStock.stockFechaIngreso = DateTime.Now;

                    // sumar la cantidad actual con la cantidad ingresada
                    existingStock.stockCantidad += newStock.stockCantidad;  // Cantidad actualizada

                    // actualizar el registro en la tabla
                    context.Entry(existingStock).State = EntityState.Modified;
                }
                else
                {
                    // agregar el registro a la tabla
                    context.stockDepositoData.Add(newStock);
                }

                List<stockBarrasData> stockBarDataList = db.stockBarrasData.Where(s => s.sbProductoID == model.selProducto).ToList();
                foreach (stockBarrasData stockBarData in stockBarDataList)
                {
                    stockBarData.sbPrecioVentaUnidad = model.precioVentaUnidad; 
                }

                await context.SaveChangesAsync();
                await db.SaveChangesAsync();

                TempData["Message"] = "Añadiste nuevo stock al deposito.";

                return RedirectToAction("Cargar");
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
                stockDepositoData stockData = await db.stockDepositoData.FindAsync(id);

                if (stockData == null)
                    throw new HttpException((int)HttpStatusCode.BadRequest, "La consulta no devuelve contenido.");

                var fam = await db.familiaData.FindAsync(stockData.stockFamiliaID);
                var prod = await db.productoData.FindAsync(stockData.stockProductoID);

                DepositoViewModel model = new DepositoViewModel()
                {
                    stockID = stockData.stockID,
                    depositoID = (int)stockData.stockDepositoID,
                    selFamilia = (int)stockData.stockFamiliaID,
                    selProducto = (int)stockData.stockProductoID,
                    familia = fam.famNombre.ToUpper(),
                    producto = prod.prodNombre.ToUpper(),
                    cantidad = (decimal)stockData.stockCantidad,
                    precioCostoUnidad = (decimal)stockData.stockPrecioCostoUnidad,
                    precioVentaUnidad = (decimal)stockData.stockPrecioVentaUnidad,
                    fechaIngreso = (DateTime)stockData.stockFechaIngreso,

                    familiaList = ListarFamilias(),
                    productoList = ListarProductos(prod.prodID)
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
        public async Task<ActionResult> Modificar(DepositoViewModel model)
        {
            try
            {
                model.depositoList = ListarDepositos();
                model.familiaList = ListarFamilias();
                model.productoList = ListarProductos(null);

                if (!ModelState.IsValid)
                    return View(model);


                stockDepositoData stockData = db.stockDepositoData.FirstOrDefault(p => p.stockID == model.stockID);
                List<stockBarrasData> stockBarDataList = db.stockBarrasData.Where(s => s.sbProductoID == model.selProducto).ToList();

                stockData.stockDepositoID = model.depositoID;
                stockData.stockFamiliaID = model.selFamilia;
                stockData.stockProductoID = model.selProducto;
                stockData.stockCantidad = model.cantidad;                
                stockData.stockPrecioCostoUnidad = model.precioCostoUnidad;
                stockData.stockPrecioVentaUnidad = model.precioVentaUnidad;
                foreach (stockBarrasData stockBarData in stockBarDataList)
                {
                    stockBarData.sbPrecioVentaUnidad = model.precioVentaUnidad; 
                }

                db.Entry(stockData).State = EntityState.Modified;
                await db.SaveChangesAsync();

                string message = "Modificaste el stock n° <b>" + model.stockID + "</b> con éxito.";
                TempData["Info"] = message;

                return RedirectToAction("Listado", "Deposito");
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost] // Controller
        public JsonResult Eliminar(int id)
        {
            try
            {
                using (var db = new sistemaAreaMasterEntities())
                {
                    stockDepositoData stock = db.stockDepositoData.Find(id);
                    stock.stockCantidad = 0;

                    string message = "Eliminaste el stock n°: " + id;

                    if (stock == null)
                        return Json(false, JsonRequestBehavior.AllowGet);

                    //db.stockDepositoData.Remove(stock);
                    db.Entry(stock).State = EntityState.Modified;
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
        public ActionResult Listado()
        {
            return View();
        }

        [HttpPost] // Controller
        public JsonResult ListarStockDeposito(string fechaDesde, string fechaHasta)
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

                var query = from sd in db.stockDepositoData
                                join fam in db.familiaData on sd.stockFamiliaID equals fam.famID
                                join prod in db.productoData on sd.stockProductoID equals prod.prodID
                                where sd.stockCantidad > 0
                                select new DepositoViewModel
                                {
                                    stockID = sd.stockID,
                                    familia = fam.famNombre,
                                    producto = prod.prodNombre,
                                    cantidad = (decimal)sd.stockCantidad,
                                    precioVentaUnidad = (decimal)sd.stockPrecioVentaUnidad,
                                    precioCostoUnidad = (decimal)sd.stockPrecioCostoUnidad,
                                    precioCostoTotal = (decimal)sd.stockPrecioCostoTotal,
                                    fechaIngreso = (DateTime)sd.stockFechaIngreso  
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
                    query = query.Where(m => m.stockID.ToString().Contains(searchValue.ToLower()) || m.familia.ToLower().Contains(searchValue.ToLower()) || m.producto.ToLower().Contains(searchValue.ToLower()) || m.cantidad.ToString().Contains(searchValue.ToLower()) || m.precioCostoTotal.ToString().Contains(searchValue.ToLower()));
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

        #region Listados

        private static List<SelectListItem> ListarDepositos()
        {
            DepositoViewModel model = new DepositoViewModel();
            List<SelectListItem> depositoList = new List<SelectListItem>();

            using (var db = new sistemaAreaMasterEntities())
            {
                var dep = db.depositoData.ToList();
                foreach (var item in dep)
                {
                    depositoList.Add(new SelectListItem
                    {
                        Text = item.depNombre.ToUpper(),
                        Value = item.depID.ToString()
                    });
                }
            }
            return depositoList;
        }

        private static List<SelectListItem> ListarFamilias()
        {
            DepositoViewModel model = new DepositoViewModel();
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
                    DepositoViewModel model = new DepositoViewModel();
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

        #endregion
    }
}
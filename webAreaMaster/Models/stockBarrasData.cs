//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace webAreaMaster.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class stockBarrasData
    {
        public int sbID { get; set; }
        public Nullable<int> sbFamiliaID { get; set; }
        public Nullable<decimal> sbCantidad { get; set; }
        public Nullable<int> sbBarraID { get; set; }
        public Nullable<System.DateTime> sbFechaIngreso { get; set; }
        public Nullable<int> sbProductoID { get; set; }
        public Nullable<decimal> sbPrecioVentaUnidad { get; set; }
        public Nullable<decimal> sbCantDispID { get; set; }
    
        public virtual barraData barraData { get; set; }
        public virtual familiaData familiaData { get; set; }
        public virtual productoData productoData { get; set; }
        public virtual stockDepositoData stockDepositoData { get; set; }
    }
}

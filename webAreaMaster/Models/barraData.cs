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
    
    public partial class barraData
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public barraData()
        {
            this.stockBarrasData = new HashSet<stockBarrasData>();
        }
    
        public int barraID { get; set; }
        public string barraNombre { get; set; }
        public Nullable<int> barraSector { get; set; }
    
        public virtual sectorData sectorData { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<stockBarrasData> stockBarrasData { get; set; }
    }
}

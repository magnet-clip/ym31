//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace YieldMap.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class RefCurrency
    {
        public RefCurrency()
        {
            this.InstrumentCustomBonds = new HashSet<InstrumentCustomBond>();
            this.InstrumentBonds = new HashSet<InstrumentBond>();
        }
    
        public long id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<InstrumentCustomBond> InstrumentCustomBonds { get; set; }
        public virtual ICollection<InstrumentBond> InstrumentBonds { get; set; }
    }
}

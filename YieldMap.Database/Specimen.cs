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
    
    public partial class Specimen
    {
        public Specimen()
        {
            this.InstrumentBonds = new HashSet<InstrumentBond>();
        }
    
        public long id { get; set; }
        public string name { get; set; }
    	public Specimen ToPocoSimple() {
    	    return new Specimen {
    			id = this.id,
    			name = this.name,
    		};
    	}
    		
        public virtual ICollection<InstrumentBond> InstrumentBonds { get; set; }
    }
}

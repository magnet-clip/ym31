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
    
    public partial class InstrumentType
    {
        public InstrumentType()
        {
            this.Instruments = new HashSet<Instrument>();
        }
    
        public long id { get; set; }
        public string Name { get; set; }
    	public InstrumentType ToPocoSimple() {
    	    return new InstrumentType {
    			id = this.id,
    			Name = this.Name,
    		};
    	}
    		
        public virtual ICollection<Instrument> Instruments { get; set; }
    }
}
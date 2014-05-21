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
    
    public partial class InstrumentFrn
    {
        public long id { get; set; }
        public Nullable<double> Cap { get; set; }
        public Nullable<double> Floor { get; set; }
        public string Frequency { get; set; }
        public Nullable<double> Margin { get; set; }
        public string Index { get; set; }
        public Nullable<long> id_Bond { get; set; }
    	public InstrumentFrn ToPocoSimple() {
    	    return new InstrumentFrn {
    			id = this.id,
    			Cap = this.Cap,
    			Floor = this.Floor,
    			Frequency = this.Frequency,
    			Margin = this.Margin,
    			Index = this.Index,
    			id_Bond = this.id_Bond,
    		};
    	}
    		
        public virtual InstrumentBond InstrumentBond { get; set; }
    }
}

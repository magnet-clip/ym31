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
    
    public partial class InstrumentIBView
    {
        public long id_Ric { get; set; }
        public long id_Instrument { get; set; }
        public Nullable<long> id_Borrower { get; set; }
        public Nullable<long> id_Issuer { get; set; }
        public string Name { get; set; }
    	public InstrumentIBView ToPocoSimple() {
    	    return new InstrumentIBView {
    			id_Ric = this.id_Ric,
    			id_Instrument = this.id_Instrument,
    			id_Borrower = this.id_Borrower,
    			id_Issuer = this.id_Issuer,
    			Name = this.Name,
    		};
    	}
    		}
}

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
    
    public partial class RefRating
    {
        public RefRating()
        {
            this.RefRatingToBonds = new HashSet<RefRatingToBond>();
        }
    
        public long id { get; set; }
        public long Value { get; set; }
        public string Name { get; set; }
        public Nullable<long> id_RatingAgency { get; set; }
    
        public virtual RefRatingAgency RefRatingAgency { get; set; }
        public virtual ICollection<RefRatingToBond> RefRatingToBonds { get; set; }
    }
}

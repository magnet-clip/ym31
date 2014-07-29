﻿using System;

namespace YieldMap.Transitive.Native.Entities {
    public class NFeed : IIdentifyable, IEquatable<NFeed> {
        [DbField(0)]
        public long id { get; set; }

        [DbField(1)]
        public string Name { get; set; }

        [DbField(2)] 
        public string Description { get; set; }

        public bool Equals(NFeed other) {
            if (other == null)
                return false;
            if (id != default(long) && other.id != default(long) && id == other.id)
                return true;
            return Name == other.Name && Description == other.Description;
        }
    }
}
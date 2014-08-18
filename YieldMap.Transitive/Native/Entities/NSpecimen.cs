﻿using System;

namespace YieldMap.Transitive.Native.Entities {
    public class NSpecimen : IIdName, IEquatable<NSpecimen> {
        [DbField(0)]
        public long id { get; set; }

        [DbField(1)]
        public string Name { get; set; }

        public override int GetHashCode() {
            return (int)id;
        }

        public bool Equals(NSpecimen other) {
            if (other == null)
                return false;
            if (id != default(long) && other.id != default(long) && id == other.id)
                return true;
            return Name == other.Name;
        }
    }
}
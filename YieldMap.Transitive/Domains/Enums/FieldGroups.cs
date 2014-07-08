﻿using System.Linq;
using YieldMap.Database;
using YieldMap.Transitive.Domains.Contexts;

namespace YieldMap.Transitive.Domains.Enums {
    internal class FieldGroups : IFieldGroups {
        public FieldGroup Default { get; private set; }
        public FieldGroup Micex { get; private set; }
        public FieldGroup Eurobonds { get; private set; }
        public FieldGroup RussiaCpi { get; private set; }
        public FieldGroup Mosprime { get; private set; }
        public FieldGroup Swaps { get; private set; }

        public FieldGroups() {
            var ctx = new EnumerationsContext();

            var items = ctx.FieldGroups.ToList();
            Default = items.First(x => x.Default).ToPocoSimple();
            Micex = items.First(x => x.Name == "Micex").ToPocoSimple();
            Eurobonds = items.First(x => x.Name == "Eurobonds").ToPocoSimple();
            RussiaCpi = items.First(x => x.Name == "Russian CPI Index").ToPocoSimple();
            Mosprime = items.First(x => x.Name == "Mosprime").ToPocoSimple();
            Swaps = items.First(x => x.Name == "Swaps").ToPocoSimple();
        }
    }
}

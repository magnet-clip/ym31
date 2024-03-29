﻿using System;
using System.Collections.Generic;
using YieldMap.Transitive.Tools;

namespace YieldMap.Transitive.Events {
    public class DbEventArgs : IDbEventArgs {
        private readonly IEnumerable<long> _added;
        private readonly IEnumerable<long> _changed;
        private readonly IEnumerable<long> _removed;
        public Type Source { get; private set; }

        public DbEventArgs(IEnumerable<long> added, IEnumerable<long> changed, IEnumerable<long> removed, Type source) {
            _added = added;
            _changed = changed;
            _removed = removed;
            Source = source;
        }

        public DbEventArgs(IReadOnlyDictionary<EntityAction, IEnumerable<long>> data, Type source) {
            _added = data[EntityAction.Added];
            _changed = data[EntityAction.Updated];
            _removed = data[EntityAction.Removed];
            Source = source;
        }

        public IEnumerable<long> Added {
            get { return new List<long>(_added); }
        }

        public IEnumerable<long> Changed {
            get { return new List<long>(_changed); }
        }

        public IEnumerable<long> Removed {
            get { return new List<long>(_removed); }
        }

        public override string ToString() {
            return 
                string.Format("Got updates on {3} with ids: added ({0}), changed({1}), removed({2})", 
                    string.Join(",", Added), string.Join(",",Changed), string.Join(",",Removed), Source);
        }
    }
}
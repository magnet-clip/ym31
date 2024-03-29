﻿using System;
using System.Collections.Generic;
using YieldMap.Language;

namespace YieldMap.Transitive.Procedures {
    public class Evaluatable {
        private readonly string _expression;
        private readonly IEnumerable<Tuple<int, Syntan.Syntel>> _grammar;
        private readonly long _idInstrumentType;

        public override string ToString() {
            return string.Format("Expr {0}", Expression);
        }

        public Evaluatable(string expression, long idInstrumentType) {
            _expression = expression;
            _idInstrumentType = idInstrumentType;
            _grammar = Syntan.grammarizeExtended(expression);
        }

        public string Expression {
            get { return _expression; }
        }

        public IEnumerable<Tuple<int, Syntan.Syntel>> Grammar {
            get { return _grammar; }
        }

        public long IdInstrumentType {
            get { return _idInstrumentType; }
        }
    }
}
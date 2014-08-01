﻿using System;
using System.Data.SQLite;
using Autofac;
using YieldMap.Tools.Logging;
using YieldMap.Transitive.Native.Entities;

namespace YieldMap.Transitive.Native.Crud {
    public class SourceTypeCrud : CrudBase<NSourceType>, ISourceTypeCrud {
        private static readonly Logging.Logger TheLogger = Logging.LogFactory.create("YieldMap.Transitive.Native.SourceTypeCrud");

        public SourceTypeCrud(SQLiteConnection connection) : base(connection) {
        }

        public SourceTypeCrud(Func<IContainer> containerF)
            : base(containerF) {
        }

        protected override Logging.Logger Logger {
            get { return TheLogger; }
        }    
    }
}
﻿using System;
using System.Data.SQLite;
using Autofac;
using YieldMap.Tools.Logging;
using YieldMap.Transitive.Native.Entities;

namespace YieldMap.Transitive.Native.Crud {
    public class PropertyCrud : CrudBase<NProperty>, IPropertyCrud {
        private static readonly Logging.Logger TheLogger = Logging.LogFactory.create("YieldMap.Transitive.Native.PropertyCrud");

        public PropertyCrud(SQLiteConnection connection)
            : base(connection) {
        }

        public PropertyCrud(Func<IContainer> containerF)
            : base(containerF) {
        }

        protected override Logging.Logger Logger {
            get { return TheLogger; }
        }
    }
}
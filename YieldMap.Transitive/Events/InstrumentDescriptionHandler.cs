﻿using System;
using System.Linq;
using Autofac;
using YieldMap.Tools.Logging;
using YieldMap.Transitive.Registry;

namespace YieldMap.Transitive.Events {
    public class InstrumentDescriptionHandler : TriggerManagerBase {
        private static readonly Logging.Logger Logger = Logging.LogFactory.create("YieldMap.Transitive.InstrumentDescriptionHandler");
        public InstrumentDescriptionHandler(ITriggerManager next)
            : base(next) {
        }

        public override void Handle(object source, IDbEventArgs args) {
            Logger.Trace("Handle()");
            if (args != null && args.Source == EventSource.Instrument) {
                Logger.Debug("Recalculating properties for instruments");
                Logger.Debug(args.ToString());
                try {
                    var updater = DatabaseBuilder.Container.Resolve<IPropertiesUpdater>();
                    updater.RecalculateBonds(view => args.Added.Contains(view.id_Instrument));
                    updater.RecalculateBonds(view => args.Changed.Contains(view.id_Instrument));
                } catch (Exception e) {
                    Logger.ErrorEx("Failed to recalculate", e);
                }
            } else {
                if (Next != null) Next.Handle(source, args);
            }
        }
    }
}
﻿namespace YieldMap.Database.Procedures.Enums {
    public interface IFieldGroups {
        FieldGroup Default { get; }
        FieldGroup Micex { get; }
        FieldGroup Eurobonds { get; }
    }
}
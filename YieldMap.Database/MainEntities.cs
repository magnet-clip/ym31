﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;

namespace YieldMap.Database {
    public partial class MainEntities {
        private static readonly Dictionary<string, string> Variables = new Dictionary<string, string>();

        public static void SetVariable(string name, string value) {
            Variables[name] = value;
        }

        private static string GetCnnString(string name) {
            try {
                return ConfigurationManager.ConnectionStrings[name].ConnectionString;
            } catch (Exception) {
                return name;
            }
        }

        public static string GetConnectionString(string name) {
            return Variables.Aggregate(
                GetCnnString(name),
                (current, variable) => current.Replace(@"${" + variable.Key + "}", variable.Value));
        }

        public MainEntities(string nameOrConnectionString) : base(nameOrConnectionString) {
            Debug.Print(nameOrConnectionString);
        }
    }
}
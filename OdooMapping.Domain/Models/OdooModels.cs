using System;
using System.Collections.Generic;

namespace OdooMapping.Domain.Models
{
    /// <summary>
    /// Represents an installed Odoo module
    /// </summary>
    public class OdooModule
    {
        public string Name { get; set; }
        public string State { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Represents an Odoo model (table)
    /// </summary>
    public class OdooModel
    {
        public string Model { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public bool IsTransient { get; set; }
        public string ModuleInfo { get; set; }
        public List<OdooField> Fields { get; set; } = new List<OdooField>();
    }

    /// <summary>
    /// Represents a field in an Odoo model
    /// </summary>
    public class OdooField
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string FieldType { get; set; }
        public string Relation { get; set; }
        public bool Required { get; set; }
        public bool ReadOnly { get; set; }
        public bool Stored { get; set; }

        /// <summary>
        /// Gets whether this field represents a relation to another table
        /// </summary>
        public bool IsRelationalField => FieldType == "many2one" || FieldType == "one2many" || FieldType == "many2many";

        /// <summary>
        /// Gets the appropriate database column name for this field
        /// </summary>
        public string GetColumnName()
        {
            if (FieldType == "many2one" && !string.IsNullOrEmpty(Relation))
            {
                return $"{Name}_id";
            }
            return Name;
        }

        /// <summary>
        /// Gets a user-friendly display name
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(Description))
            {
                return Description;
            }
            return Name;
        }

        /// <summary>
        /// Gets a user-friendly type description
        /// </summary>
        public string GetTypeDescription()
        {
            switch (FieldType)
            {
                case "char":
                    return "Text";
                case "text":
                    return "Long Text";
                case "integer":
                    return "Integer";
                case "float":
                    return "Float";
                case "monetary":
                    return "Currency";
                case "boolean":
                    return "Boolean";
                case "date":
                    return "Date";
                case "datetime":
                    return "Date & Time";
                case "binary":
                    return "Binary";
                case "many2one":
                    return $"Relation (Many → One): {Relation}";
                case "one2many":
                    return $"Relation (One → Many): {Relation}";
                case "many2many":
                    return $"Relation (Many ↔ Many): {Relation}";
                default:
                    return FieldType;
            }
        }
    }
} 
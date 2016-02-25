using System;

namespace Common.Data
{
    public enum SQLIndexTypes : byte
    {
        None = 0,
        PrimaryKey = 1,
        NormalIndex = 2,
        UniqueIndex = 3,
        FullText = 4
    }
    public abstract class SQLIndex : IDisposable
    {
        public string Name { get; private set; }
        public SQLIndexTypes IndexType { get; private set; }
        public string[] FieldNames { get; private set; }
        public abstract string CreateLine { get; }
        public void Dispose()
        {
            this.Name = null;
            this.FieldNames = null;
        }

        public SQLIndex(SQLIndexTypes indexType, string name,
                        params string[] fieldNames)
        {
            this.IndexType = indexType;
            this.Name = name;
            this.FieldNames = fieldNames;
        }
    }
}

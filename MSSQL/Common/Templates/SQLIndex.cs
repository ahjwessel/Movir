using System;

namespace Common.Templates
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
        #region Properties
        public string Name { get; private set; }
        public SQLIndexTypes IndexType { get; private set; }
        public string[] FieldNames { get; private set; }
        public abstract string CreateLine { get; }
        #endregion
        public void Dispose()
        {
            this.Name = null;
            this.FieldNames = null;
        }

        public SQLIndex(SQLIndexTypes parIndexType, string parName,
                        params string[] parFieldNames)
        {
            this.IndexType = parIndexType;
            this.Name = parName;
            this.FieldNames = parFieldNames;
        }
    }
}

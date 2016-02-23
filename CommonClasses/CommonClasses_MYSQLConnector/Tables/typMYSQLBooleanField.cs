using System;
using System.Collections.Generic;
using System.Text;

namespace CommonClasses.Tables
{
  public class typMYSQLBooleanField:typMYSQLField
  {
    public typMYSQLBooleanField(string parName, bool parDefaultValue)
      : base(parName, DataTypeEnumEx.Boolean, parDefaultValue)
    { }
  }
}

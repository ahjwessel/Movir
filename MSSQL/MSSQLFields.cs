using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Templates;
using System.Data;
using System.Collections;

namespace MSSQL
{
    public class MSSQLFields:SQLFields
    {
        new public MSSQLField this[string parName]
        {
            get
            {
                return (MSSQLField)base[parName];
            }
        }
        new public MSSQLField this[int parIndex]
        {
            get
            {
                return (MSSQLField)base[parIndex];
            }
        }
        public MSSQLFields(params MSSQLField[] parFields)
            :base(parFields)
        { }

        public MSSQLFields(DataTable parSchemaTable, DataSet parDataSet)
        {
            _fields = new MSSQLField[parSchemaTable.Rows.Count];
            for (int varCounter = 0; varCounter < parSchemaTable.Rows.Count; varCounter++)
            {
                _fields[varCounter] = new MSSQLField(varCounter + 1, parSchemaTable);
                if (parDataSet != null && parDataSet.Tables[0].Rows.Count > 0)
                    this[varCounter].Refresh(parDataSet.Tables[0].Rows[0]);
            }
        }
    }
}

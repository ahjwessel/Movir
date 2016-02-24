using System;
using System.Collections;
using System.Text;

namespace Common.Tables
{
  public abstract class typFields:IEnumerable,IDisposable
  {
    public event dlgFieldEvent ValueChanged;
    typField[] mtxFields = null;

    #region Properties
    public typField this[int parIndex]
    {
      get
      {
        if (mtxFields==null)
          return null;
        else
          return mtxFields[parIndex];
      }
    }
    public typField this[string parName]
    {
      get
      {
        if (mtxFields == null)
          return null;
        else
        {
          foreach (typField fld in mtxFields)
          {
            if (fld.Name == parName)
              return fld;
          }
          foreach (typField fld in mtxFields)
          {
            if (fld.Name.ToLower() == parName.ToLower())
            {
              return fld;
            }
          }
          return null;
        }
      }
    }
    public int Count
    {
      get
      {
        return mtxFields.Length;
      }
    }
    public bool IsDirty
    {
      get
      {
        if (mtxFields == null)
          return false;
        else
        {
          foreach (typField fld in mtxFields)
          {
            if (fld.IsDirty)
              return true;
          }
          return false;
        }
      }
    }
    #endregion

    #region Publics
    public void RollBackToInitValue()
    {
      if (mtxFields != null)
      {
        foreach (typField fld in mtxFields)
        {
          fld.RollBackToInitValue();
        }
      }
    }
    public void RollBackToOldValue()
    {
      if (mtxFields != null)
      {
        foreach (typField fld in mtxFields)
        {
          fld.RollBackToOldValue();
        }
      }
    }
    public void SaveValueToOldValue()
    {
      if (mtxFields != null)
      {
        foreach (typField fld in mtxFields)
        {
          fld.SaveValueToOldValue();
        }
      }
    }
    #endregion

    #region Raise events
    protected virtual void onValueChanged(typField parField)
    {
      if (this.ValueChanged != null)
        this.ValueChanged(parField);
    }
    #endregion

    #region Events
    private void typFields_ValueChanged(typField parSender)
    {
      this.onValueChanged(parSender);
    }
    #endregion

    #region IEnumerable Members
    public IEnumerator GetEnumerator()
    {
      return mtxFields.GetEnumerator();
    }
    #endregion

    #region IDisposable Members
    public void Dispose()
    {
      if (mtxFields != null)
      {
        foreach (typField fld in mtxFields)
        {
          fld.Dispose();
        }
        mtxFields = null;
      }
    }
    #endregion

    protected typFields(params typField[] parFields)
    {
      mtxFields = parFields;

      if (mtxFields!=null)
      {
        foreach (object fld in parFields)
        {
          ((typField)fld).ValueChanged += new dlgFieldEvent(typFields_ValueChanged);
        }
      }
    }
  }
}

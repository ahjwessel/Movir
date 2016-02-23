using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Ini
{
  public class typIniBlankLine:typIniLine
  {
    public override string LineText
    {
      get 
      {
        return "";
      }
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Ini
{
  public class typApplicationIniFile : typIniFile
  {
    public typApplicationIniFile()
    :base(Common.Tools.FileTools.getApplicationIniPath())
    {
    }
  }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Common.Controls.Grid.Columns
{
  public class CustomPictureBox:PictureBox
  {
    public event EventHandler ImageChanged;
    public Image Image
    {
      get
      {
        return base.Image;
      }
      set
      {
        base.Image = value;
        if (this.ImageChanged != null)
          this.ImageChanged(this, new EventArgs());
      }
    }
  }
}

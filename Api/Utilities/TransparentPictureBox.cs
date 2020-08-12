using System;
using System.Drawing;
using System.Windows.Forms;

namespace MCC_Mod_Manager.Api.Utilities {
    public class TransparentPictureBox : PictureBox {
        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.DrawImage(base.Image, new Rectangle(base.Location, new Size(base.Image.Width, base.Image.Height)));
        }
    }
}

using System;
using System.Drawing;
using System.Windows.Forms;

namespace MCC_Mod_Manager.Api.Utilities {
    public class TransparentPictureBox : Control {

        private Image _image = null;
        private Point _location = new Point(0,0);

        public TransparentPictureBox() {

        }

        protected override CreateParams CreateParams {
            get {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x20;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e) {
            e.Graphics.DrawImage(_image, _location);
        }

        protected override void OnPaintBackground(PaintEventArgs e) {
            // Do nothing
        }

        public Image Image {
            get {
                return _image;
            }
            set {
                _image = value;
                base.RecreateHandle();
            }
        }

        public new Point Location {
            get {
                return _location;
            }
            set {
                _location = value;
            }
        }
    }
}

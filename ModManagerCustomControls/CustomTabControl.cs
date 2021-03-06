﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModManagerCustomControls {
    public class CustomTabControl : TabControl {

        private const int TCM_ADJUSTRECT = 0x1328;

        protected override void WndProc(ref Message m) {
            //Hide the tab headers at run-time
            if (m.Msg == TCM_ADJUSTRECT) {

                RECT rect = (RECT)(m.GetLParam(typeof(RECT)));
                rect.Left = this.Left - this.Margin.Left - 1;
                rect.Right = this.Right + this.Margin.Right + 1;

                rect.Top = this.Top - this.Margin.Top;
                rect.Bottom = this.Bottom + this.Margin.Bottom + 1;
                Marshal.StructureToPtr(rect, m.LParam, true);
                //m.Result = (IntPtr)1;
                //return;
            }
            //else
            // call the base class implementation
            base.WndProc(ref m);
        }

        private struct RECT {
            public int Left, Top, Right, Bottom;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImpromptuNinjas.UltralightSharp.Enums;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Word = Microsoft.Office.Interop.Word;
using Newtonsoft.Json;

namespace OutlookHTMLTest
{
    public partial class RenderScreen : Form
    {
        public delegate void TryUpdateRectD(IntPtr ptr);
        public delegate void ULResize(int width, int heigth);
        public static IntPtr WordWindowPtr;
        public static Word.Window WordWindow;
        public Microsoft.Office.Interop.Outlook.Inspector inspector;
        public IntPtr ScreenDisplayContext;
        public IntPtr InMemoryDisplayContext;
        public IntPtr Framebuffer;
        public IntPtr Framebuffer_Bitmap;
        public List<HighlightPanel> panels = new List<HighlightPanel>();
        public RenderScreen(Microsoft.Office.Interop.Outlook.MailItem itm)
        {
            while (true)
            {
                try
                {
                    inspector = itm.GetInspector;
                    Thread.Sleep(3);
                    break;
                }
                catch { }
            }
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            var WindowPtr = WinAPI.GetForegroundWindow();
            WordWindowPtr = WinAPI.FindAllChildWindowByClassName(WinAPI.GetForegroundWindow(), "_WwG", true).FirstOrDefault();
            WordWindow = ((Word.Document)inspector.WordEditor).Windows.Cast<Word.Window>().FirstOrDefault();
            this.Show();
            this.TopMost = true;

            InitGDI();
            //WordWatcher.Start(((Word.Document)inspector.WordEditor).Windows.Cast<Word.Window>().FirstOrDefault());

            TryUpdateRect(WordWindowPtr);
            inspector.BeforeMove += Insp_BeforeMoveOrSize;
            inspector.BeforeSize += Insp_BeforeMoveOrSize;
            while (true)
            {
                ProcessPanels(WordWindow);
                this.Invalidate();
                Application.DoEvents();
                Thread.Sleep(25);
            }
        }
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                if (!DesignMode)
                {
                      cp.ExStyle |= 0x80;
                      cp.ExStyle |= 0x00080000; // This form has to have the WS_EX_LAYERED extended style
                }
                return cp;
            }
        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            MouseButton button;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    button = MouseButton.Left;
                    break;
                case MouseButtons.Right:
                    button = MouseButton.Right;
                    break;
                default:
                    button = MouseButton.None;
                    break;
            }
            Ultralight.Core._commandBuffer.Enqueue(new Ultralight.Command { Type = Ultralight.Command.TypeEnum.SYSTEM_MOUSEEVENT, arg0 = MouseEventType.MouseDown, arg1 = e.X, arg2 = e.Y, arg3 = button });
            Console.WriteLine("MOUSEDOWN");
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            MouseButton button;
            switch (e.Button)
            {
                case MouseButtons.Left:
                    button = MouseButton.Left;
                    break;
                case MouseButtons.Right:
                    button = MouseButton.Right;
                    break;
                default:
                    button = MouseButton.None;
                    break;
            }
            Ultralight.Core._commandBuffer.Enqueue(new Ultralight.Command { Type = Ultralight.Command.TypeEnum.SYSTEM_MOUSEEVENT, arg0 = MouseEventType.MouseUp, arg1 = e.X, arg2 = e.Y, arg3 = button });
            Console.WriteLine("MOUSEUP");
        }

        protected override void OnScroll(ScrollEventArgs se)
        {
            base.OnScroll(se);
            Ultralight.Core._commandBuffer.Enqueue(new Ultralight.Command
            {
                Type = Ultralight.Command.TypeEnum.SYSTEM_SCROLL,
                arg0 = se.ScrollOrientation == ScrollOrientation.HorizontalScroll
                                                                                   ? se.OldValue - se.NewValue : 0,
                arg1 = se.ScrollOrientation == ScrollOrientation.VerticalScroll
                                                                                   ? se.OldValue - se.NewValue : 0
            });
            Console.WriteLine("MOUSESCROLL");
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            Ultralight.Core.MouseMoveEvent(e.X, e.Y);
            Console.WriteLine("MOUSEMOVE");
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            var bitmap = Ultralight.Core.Render();
            SetBitmap(bitmap, 175);
        }
        /// <summary>
        /// Move and Size handler for Outlook E-Mail Window
        /// </summary>
        /// <param name="Cancel">Cancel this event</param>
        private void Insp_BeforeMoveOrSize(ref bool Cancel)
        {
            Task.Factory.StartNew(() =>
            {

                while (Control.MouseButtons == MouseButtons.Left)
                {
                    Thread.Sleep(2);
                    Application.DoEvents();
                }

                TryUpdateRect(WordWindowPtr);
            });
        }

        public void ProcessPanels(Word.Window wordWnd)
        {
            var textLength = wordWnd.Document.Range().Text.Length;
            var textRange = wordWnd.Document.Range(0, textLength);
            wordWnd.GetPoint(out var left, out var top, out var width, out var height, textRange);
            var coords = this.PointToClient(new Point(left, top));
            panels.Clear();
            panels.Add(new HighlightPanel { X = coords.X, Y = coords.Y, Width = width, Height = height });
            Ultralight.Core._commandBuffer.Enqueue(new Ultralight.Command
            {
                Type = Ultralight.Command.TypeEnum.DATA_HIGHLIGHT_UPDATE,
                arg0 = JsonConvert.SerializeObject(panels)
            });
        }

        internal void TryUpdateRect(IntPtr ptr = default)
        {
            if (InvokeRequired)
            {
                Invoke(new TryUpdateRectD(TryUpdateRect), ptr);
                return;
            }
            var ptrParent = ptr;

            var rect = new WinAPI.RECT();
            if (!WinAPI.GetWindowRect(ptrParent, ref rect))
            {
                return;
            }

            var dpi = 96f;
            try
            {
                dpi = WinAPI.GetDpiForWindow(ptrParent) / 96f;
            }
            catch { dpi = 1; }

            var rc = new Rectangle(
                x: (int)((rect.Left) / dpi),
                y: (int)((rect.Top) / dpi),
                width: (int)((rect.Right - rect.Left) / dpi),
                height: (int)((rect.Bottom - rect.Top) / dpi)
                );

            Size = rc.Size;
            Location = rc.Location;
            Ultralight.Core._commandBuffer.Enqueue(new Ultralight.Command { Type = Ultralight.Command.TypeEnum.SYSTEM_RESIZEEVENT, arg0 = Size.Width, arg1 = Size.Height });
            Application.DoEvents();
        }

        public void SetBitmap(Bitmap bitmap, byte opacity)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
                throw new ApplicationException("The bitmap must be 32ppp with alpha-channel.");

            // The ideia of this is very simple,
            // 1. Create a compatible DC with screen;
            // 2. Select the bitmap with 32bpp with alpha-channel in the compatible DC;
            // 3. Call the UpdateLayeredWindow.
            var hBitmap = IntPtr.Zero;
            var oldBitmap = IntPtr.Zero;

            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));  // grab a GDI handle from this GDI+ bitmap
                oldBitmap = WinAPI.SelectObject(InMemoryDisplayContext, hBitmap);

                var size = new Size(bitmap.Width, bitmap.Height);
                var pointSource = new Point(0, 0);
                var topPos = new Point(Left, Top);
                var blend = new WinAPI.BLENDFUNCTION
                {
                    BlendOp = WinAPI.AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = opacity,
                    AlphaFormat = WinAPI.AC_SRC_ALPHA
                };

                WinAPI.UpdateLayeredWindow(Handle, ScreenDisplayContext, ref topPos, ref size, InMemoryDisplayContext, ref pointSource, 0, ref blend, WinAPI.ULW_ALPHA);
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    WinAPI.SelectObject(InMemoryDisplayContext, oldBitmap);
                    //Windows.DeleteObject(hBitmap); // The documentation says that we have to use the Windows.DeleteObject... but since there is no such method I use the normal DeleteObject from Win32 GDI and it's working fine without any resource leak.
                    WinAPI.DeleteObject(hBitmap);
                }
            }
        }
        public void SetBitmapOptimized(IntPtr bitmap, byte opacity)
        {
            var oldBitmap = IntPtr.Zero;
            try
            {
                oldBitmap = WinAPI.SelectObject(InMemoryDisplayContext, bitmap);

                var pointSource = new Point(0, 0);
                var topPos = new Point(Left, Top);
                var size = this.Size;
                var blend = new WinAPI.BLENDFUNCTION
                {
                    BlendOp = WinAPI.AC_SRC_OVER,
                    BlendFlags = 0,
                    SourceConstantAlpha = opacity,
                    AlphaFormat = WinAPI.AC_SRC_ALPHA
                };
                WinAPI.UpdateLayeredWindow(Handle, ScreenDisplayContext, ref topPos, ref size, InMemoryDisplayContext, ref pointSource, 0, ref blend, WinAPI.ULW_ALPHA);
            }
            finally
            {
                //WinAPI.ReleaseDC(IntPtr.Zero, screenDc);
                if (bitmap != IntPtr.Zero)
                {
                    WinAPI.SelectObject(InMemoryDisplayContext, oldBitmap);
                    WinAPI.DeleteObject(bitmap); // The documentation says that we have to use the Windows.DeleteObject... but since there is no such method I use the normal DeleteObject from Win32 GDI and it's working fine without any resource leak.
                }
                //WinAPI.DeleteDC(memDc);
            }
        }

        private void InitGDI()
        {
            ScreenDisplayContext = WinAPI.GetDC(IntPtr.Zero);
            InMemoryDisplayContext = WinAPI.CreateCompatibleDC(ScreenDisplayContext);
        }
    }
}

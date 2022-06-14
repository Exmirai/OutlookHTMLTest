using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Drawing;
using System.Collections.Concurrent;
using ImpromptuNinjas.UltralightSharp.Enums;
using ImpromptuNinjas.UltralightSharp.Safe;
using Unsf = ImpromptuNinjas.UltralightSharp;

namespace OutlookHTMLTest.Ultralight
{
    public class Command
    {
        public enum TypeEnum
        {
            SYSTEM_RESIZEEVENT = 0,
            SYSTEM_MOUSEEVENT = 1,
            SYSTEM_KEYBOARDEVENT = 2,
            DATA_CLEAR = 3,
            DATA_POLICIES_UPDATE = 4,
            DATA_SEARCHRESULT_UPDATE = 5,
            DATA_HIGHLIGHT_UPDATE = 6,
            UI_AUTHENTICATION_SETSTATUS = 7,
            SYSTEM_SCROLL = 8
        }
        public TypeEnum Type { get; set; }
        public object arg0 { get; set; }
        public object arg1 { get; set; }
        public object arg2 { get; set; }
        public object arg3 { get; set; }
    }

    public static class Core
    {
        private static View _view;
        private static Renderer _renderer;
        private static Surface _surface;
        private static int bitmapWidth = 640;
        private static int bitmapHeight = 480;
        private static object _lock = new object();

        public static volatile bool _loaded = false;
        public static ConcurrentQueue<Command> _commandBuffer = new ConcurrentQueue<Command>();
        public static volatile int mouseMoveX = 0;
        public static volatile int mouseMoveY = 0;

        public static void Init(int width, int height)
        {
            lock (_lock)
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "outlooktest");
                var cachePath = Path.Combine(path, "Cache");
                var resourcePath = Path.Combine(path, "resources");
                var assetsPath = Path.Combine(path, "assets");
                var cfg = new Config();
                cfg.SetCachePath(cachePath);
                cfg.SetEnableImages(true);
                cfg.SetEnableJavaScript(true);
                cfg.SetResourcePath(resourcePath);
                cfg.SetUseGpuRenderer(false);
                AppCore.EnablePlatformFileSystem(assetsPath);
                AppCore.EnableDefaultLogger(Path.Combine(path, "log.log"));
                AppCore.EnablePlatformFontLoader();

                _renderer = new Renderer(cfg);
                var session = new Session(_renderer, false, "Judico UI");
                _view = new View(_renderer, (uint)width, (uint)height, true, session);
                _surface = _view.GetSurface();

                _view.SetFinishLoadingCallback((IntPtr userData, View caller, ulong frameId, bool isMainFrame, string? url) =>
                {
                    _loaded = true;
                    BindCommands(caller.LockJsContext());
                    caller.UnlockJsContext();
                }, IntPtr.Zero);
                _view.SetFailLoadingCallback((IntPtr userData, View caller, ulong frameId, bool isMainFrame, string? url, string? description, string? errorDomain, int errorCode) =>
                {
                    _loaded = false;
                }, IntPtr.Zero);
                _view.SetBeginLoadingCallback((IntPtr userData, View caller, ulong frameId, bool isMainFrame, string? url) =>
                {
                    _loaded = false;
                }, IntPtr.Zero);
            }
        }

        public static void LoadPage()
        {
            lock (_lock)
            {
                _view.LoadUrl("file:///index.html");
            }
        }

        public static void Update()
        {
            lock (_lock)
            {
                ProcessCommands();
                _renderer.Update();
                _renderer.Render();
            }
        }

        public static void Resize(int newWidth, int newHeight)
        {
            if (!_view.IsLoading())
            {
                bitmapWidth = newWidth;
                bitmapHeight = newHeight;
                _view.Resize((uint)newWidth, (uint)newHeight);
                _renderer.Update();
            }
        }

        public static void MouseMoveEvent(int deltaX, int deltaY)
        {
            lock (_lock)
            {
                mouseMoveX = deltaX;
                mouseMoveY = deltaY;
            }
        }

        public static void KeyboardEvent()
        {
            lock (_lock)
            {

            }
        }

        public static System.Drawing.Bitmap Render()
        {
            lock (_lock)
            {
                if (!_loaded)
                {
                    return new System.Drawing.Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                }
                _renderer.Render();
                var bitmap = _surface.GetBitmap();
                bitmap.SwapRedBlueChannels();
                var pixels = bitmap.LockPixels();
                var output = new System.Drawing.Bitmap((int)bitmap.GetWidth(), (int)bitmap.GetHeight(), (int)bitmap.GetRowBytes(), System.Drawing.Imaging.PixelFormat.Format32bppArgb, pixels);
                bitmap.UnlockPixels();
                bitmap.SwapRedBlueChannels();
                return output;
            }
        }
        private static void ProcessCommands()
        {
            if (mouseMoveX != 0 || mouseMoveY != 0)
            {
                _view.FireMouseEvent(new MouseEvent(MouseEventType.MouseMoved, mouseMoveX, mouseMoveY, MouseButton.None));
            }

            if (_commandBuffer.TryDequeue(out var command))
            {
                switch (command.Type)
                {
                    case Command.TypeEnum.SYSTEM_RESIZEEVENT:
                        Resize((int)command.arg0, (int)command.arg1);
                        break;
                    case Command.TypeEnum.SYSTEM_MOUSEEVENT:
                        _view.FireMouseEvent(new MouseEvent((MouseEventType)command.arg0, (int)command.arg1, (int)command.arg2, (MouseButton)command.arg3));
                        break;
                    case Command.TypeEnum.SYSTEM_KEYBOARDEVENT:
                        break;
                    case Command.TypeEnum.DATA_CLEAR:
                        break;
                    case Command.TypeEnum.DATA_POLICIES_UPDATE:
                        break;
                    case Command.TypeEnum.DATA_SEARCHRESULT_UPDATE:
                        break;
                    case Command.TypeEnum.DATA_HIGHLIGHT_UPDATE:
                        _view.EvaluateScript($"highlightPanels = '{(string)command.arg0}'");
                        _view.EvaluateScript("updatePanels()");
                        break;
                    case Command.TypeEnum.UI_AUTHENTICATION_SETSTATUS:
                        break;
                    case Command.TypeEnum.SYSTEM_SCROLL:
                        _view.FireScrollEvent(new ScrollEvent(ScrollEventType.ScrollByPixel, (int)command.arg0, (int)command.arg1));
                        break;
                }
            }
        }

        private static unsafe void BindCommands(JsContext ctx)
        {
            var globalObj = ctx.GetGlobalObject();

            var twitterFncStrName = new JsString("openTwitterWnd".AsSpan());
            var twitterFnc = Unsf.JavaScriptCore.JsObjectMakeFunctionWithCallback(ctx.Unsafe, twitterFncStrName.Unsafe, new Unsf.FnPtr<Unsf.ObjectCallAsFunctionCallback>((Unsf.JsContext* ctx, Unsf.JsValue* function, Unsf.JsValue* thisObject, UIntPtr argumentCount, Unsf.JsValue** arguments, Unsf.JsValue** exception) => {
                Process.Start("https://www.google.ru/search?q=twitter");
                return Unsf.JavaScriptCore.JsValueMakeNull(ctx);
            }) );
            Unsf.JavaScriptCore.JsObjectSetProperty(ctx.Unsafe, globalObj.Unsafe, twitterFncStrName.Unsafe, twitterFnc, 0, null);
            twitterFncStrName.Dispose();

            var facebookFncStrName = new JsString("openFacebookWnd".AsSpan());
            var facebookFnc = Unsf.JavaScriptCore.JsObjectMakeFunctionWithCallback(ctx.Unsafe, facebookFncStrName.Unsafe, new Unsf.FnPtr<Unsf.ObjectCallAsFunctionCallback>((Unsf.JsContext* ctx, Unsf.JsValue* function, Unsf.JsValue* thisObject, UIntPtr argumentCount, Unsf.JsValue** arguments, Unsf.JsValue** exception) => {
                Process.Start("https://www.google.ru/search?q=facebook");
                return Unsf.JavaScriptCore.JsValueMakeNull(ctx);
            }));
            Unsf.JavaScriptCore.JsObjectSetProperty(ctx.Unsafe, globalObj.Unsafe, facebookFncStrName.Unsafe, facebookFnc, 0, null);
            facebookFncStrName.Dispose();

            var whatsappFncStrName = new JsString("openWhatsappWnd".AsSpan());
            var whatsappFnc = Unsf.JavaScriptCore.JsObjectMakeFunctionWithCallback(ctx.Unsafe, whatsappFncStrName.Unsafe, new Unsf.FnPtr<Unsf.ObjectCallAsFunctionCallback>((Unsf.JsContext* ctx, Unsf.JsValue* function, Unsf.JsValue* thisObject, UIntPtr argumentCount, Unsf.JsValue** arguments, Unsf.JsValue** exception) => {
                Process.Start("https://www.google.ru/search?q=whatsapp");
                return Unsf.JavaScriptCore.JsValueMakeNull(ctx);
            }));
            Unsf.JavaScriptCore.JsObjectSetProperty(ctx.Unsafe, globalObj.Unsafe, whatsappFncStrName.Unsafe, whatsappFnc, 0, null);
            whatsappFncStrName.Dispose();

        }
    }
}

using System;
using Office = Microsoft.Office.Core;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.Outlook;
using System.Globalization;

namespace OutlookHTMLTest
{
    public partial class ThisAddIn
    {
        private void ThisAddIn_Startup(object sender, System.EventArgs e)
        {
            GlobalContext.Handle =  WinAPI.GetForegroundWindow();
        }

        private void ThisAddIn_Shutdown(object sender, System.EventArgs e)
        {
            // Примечание. Outlook больше не выдает это событие. Если имеется код, который 
            //    должно выполняться при завершении работы Outlook, см. статью на странице https://go.microsoft.com/fwlink/?LinkId=506785
        }

        #region Код, автоматически созданный VSTO

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InternalStartup()
        {
            this.Startup += new System.EventHandler(ThisAddIn_Startup);
            this.Shutdown += new System.EventHandler(ThisAddIn_Shutdown);
        }

        protected override Office.IRibbonExtensibility CreateRibbonExtensibilityObject()
        {
            GlobalContext.Init(GetHostItem<Application>(typeof(Application), "Application"));
            GlobalContext.Language = new CultureInfo(GlobalContext.App.LanguageSettings.LanguageID[MsoAppLanguageID.msoLanguageIDUI]);
            return new Ribbon();
        }
        #endregion
    }

    public class GlobalContext
    {
        public static bool Initialized => _app != null;

        private static Application _app;
        public static Application App => _app ?? throw new System.Exception("GlobalContext was not initialized. Please, call GlobalContext.Init method from your add-in code to initialize global context.");

        public static CultureInfo Language { get; internal set; }

        internal static void Init(Application application)
        {
            _app = application;
        }

        public static IntPtr Handle { get; set; }
    }
}

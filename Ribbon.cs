﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Office = Microsoft.Office.Core;
using System.Threading;

// TODO:  Выполните эти шаги, чтобы активировать элемент XML ленты:

// 1: Скопируйте следующий блок кода в класс ThisAddin, ThisWorkbook или ThisDocument.

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new Ribbon1();
//  }

// 2. Создайте методы обратного вызова в области "Обратные вызовы ленты" этого класса, чтобы обрабатывать действия
//    пользователя, например нажатие кнопки. Примечание: если эта лента экспортирована из конструктора ленты,
//    переместите свой код из обработчиков событий в методы обратного вызова и модифицируйте этот код, чтобы работать с
//    моделью программирования расширения ленты (RibbonX).

// 3. Назначьте атрибуты тегам элементов управления в XML-файле ленты, чтобы идентифицировать соответствующие методы обратного вызова в своем коде.  

// Дополнительные сведения можно найти в XML-документации для ленты в справке набора средств Visual Studio для Office.


namespace OutlookHTMLTest
{
    [ComVisible(true)]
    public class Ribbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;
        private RenderScreen renderScreen;
        public Ribbon()
        {


            new Thread(() =>
            {
                Ultralight.Core.Init(640, 480);
                Ultralight.Core.LoadPage();
                while (true)
                {
                    Ultralight.Core.Update();
                    Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();
        }
        private void App_ItemLoad(dynamic Item)
        {
            if (Item is Microsoft.Office.Interop.Outlook.MailItem itm)
            {
                new Thread(() =>
                {
                    renderScreen = new RenderScreen(itm);
                }){ IsBackground = true }.Start();
            }
        }

        private void App_ItemSend(object Item, ref bool Cancel)
        {

        }
        #region Элементы IRibbonExtensibility

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("OutlookHTMLTest.Ribbon.xml");
        }

        #endregion

        #region Обратные вызовы ленты
        //Информацию о методах создания обратного вызова см. здесь. Дополнительные сведения о методах добавления обратного вызова см. по ссылке https://go.microsoft.com/fwlink/?LinkID=271226

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
            GlobalContext.App.ItemLoad += App_ItemLoad;
            GlobalContext.App.ItemSend += App_ItemSend;
        }

        #endregion

        #region Вспомогательные методы

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}

﻿#if !STANDARDCORE
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MVVMSidekick.ViewModels;
using System.Reactive.Linq;
using System.Windows;
using System.IO;
using MVVMSidekick.Services;



#if NETFX_CORE
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;


#elif WPF
using System.Windows.Controls;
using System.Windows.Media;

using System.Collections.Concurrent;
using System.Windows.Navigation;

using MVVMSidekick.Views;
using System.Windows.Controls.Primitives;
using MVVMSidekick.Utilities;
#elif SILVERLIGHT_5 || SILVERLIGHT_4
						   using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Controls.Primitives;
#elif WINDOWS_PHONE_8 || WINDOWS_PHONE_7
using System.Windows.Media;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.Windows.Data;
using System.Windows.Navigation;
using System.Windows.Controls.Primitives;
#endif


namespace MVVMSidekick.Views
{

    /// <summary>
    /// Class ViewHelper.
    /// </summary>
    public static class ViewHelper
    {
        /// <summary>
        /// The default vm name
        /// </summary>
        public static readonly string DEFAULT_VM_NAME = "DesignVM";
        /// <summary>
        /// Gets the default designing view model.
        /// </summary>
        /// <param name="view">The view.</param>
        /// <returns>System.Object.</returns>
        public static object GetDefaultDesigningViewModel(this IView view)
        {
            var f = view as FrameworkElement;
            object rval = null;
#if NETFX_CORE
				if (!f.Resources.ContainsKey(DEFAULT_VM_NAME))
#else
            if (!f.Resources.Contains(DEFAULT_VM_NAME))
#endif
            {
                return null;
            }
            else
            {
                rval = f.Resources[DEFAULT_VM_NAME];
            }
            return rval;
        }

        /// <summary>
        /// The view unload call back
        /// </summary>
        internal static RoutedEventHandler ViewUnloadCallBack
            = async (o, e) =>
            {
                IView v = o as IView;
                if (v != null)
                {
                    var m = v.ViewModel as IViewModelLifetime;
                    if (m != null)
                    {
                        await m.OnBindedViewUnload(v);
                    }
                }
            };

        /// <summary>
        /// The view load call back
        /// </summary>
        internal static RoutedEventHandler ViewLoadCallBack
            = async (o, e) =>
            {
                IView v = o as IView;
                if (v != null)
                {
                    var m = v.ViewModel as IViewModelLifetime;
                    if (m != null)
                    {
                        await m.OnBindedViewLoad(v);
                    }
                }
            };
        /// <summary>
        /// The designing view model changed call back
        /// </summary>
        internal static PropertyChangedCallback DesigningViewModelChangedCallBack
            = (o, e) =>
            {
                var oiview = o as IView;
                if (Utilities.Runtime.IsInDesignMode)
                {
                    oiview.ViewModel = e.NewValue as IViewModel;
                }
            };



        /// <summary>
        /// The view model changed callback
        /// </summary>
        internal static PropertyChangedCallback ViewModelChangedCallback
            = (o, e) =>
            {
                dynamic item = o;
                var oiview = o as IView;
                var fele = (oiview.ContentObject as FrameworkElement);
                if (fele == null)
                {
                    return;
                }
                if (object.ReferenceEquals(fele.DataContext, e.NewValue))
                {
                    return;
                }
                (oiview.ContentObject as FrameworkElement).DataContext = e.NewValue;
                var nv = e.NewValue as IViewModel;
                var ov = e.OldValue as IViewModel;
                if (ov != null)
                {
                    ov.OnUnbindedFromView(oiview, nv);
                }
                if (nv != null)
                {
                    nv.OnBindedToView(oiview, ov);
                }

            };

        /// <summary>
        /// Gets the content and create if null.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <returns>FrameworkElement.</returns>
        internal static FrameworkElement GetContentAndCreateIfNull(this IView control)
        {
            var c = (control.ContentObject as FrameworkElement);
            if (c == null)
            {
                control.ContentObject = c = new Grid();
            }
            return c;
        }

        /// <summary>
        /// Selfs the close.
        /// </summary>
        /// <param name="view">The view.</param>
        public static void SelfClose(this IView view)
        {

            if (view is UserControl || view is Page)
            {
                var viewElement = view as FrameworkElement;
                var parent = viewElement.Parent;
                if (parent is Panel)
                {
                    (parent as Panel).Children.Remove(viewElement);
                }
                else if (parent is Frame)
                {
                    var f = (parent as Frame);
                    if (f.CanGoBack)
                    {
                        f.GoBack();
                    }
                    else
                    {
                        f.Content = null;
                    }
                }
                else if (parent is ContentControl)
                {
                    (parent as ContentControl).Content = null;
                }
                else if (parent is Page)
                {
                    (parent as Page).Content = null;
                }
                else if (parent is UserControl)
                {
                    (parent as UserControl).Content = null;
                }

            }
#if WPF
				else if (view is Window)
				{
					(view as Window).Close();
				}
#endif


        }

    }

}
#endif

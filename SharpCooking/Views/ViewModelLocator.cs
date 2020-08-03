using SharpCooking.ViewModels;
using System;
using System.Globalization;
using System.Reflection;
using Xamarin.Forms;
using TinyIoC;
using SharpCooking.Models;
using SharpCooking.Data;
using System.IO;

namespace SharpCooking.Views
{
    public static class ViewModelLocator
    {
        public static readonly BindableProperty AutoWireViewModelProperty =
            BindableProperty.CreateAttached("AutoWireViewModel", typeof(bool), typeof(ViewModelLocator), default(bool), propertyChanged: OnAutoWireViewModelChanged);

        public static bool GetAutoWireViewModel(BindableObject bindable)
        {
            return (bool)bindable.GetValue(ViewModelLocator.AutoWireViewModelProperty);
        }

        public static void SetAutoWireViewModel(BindableObject bindable, bool value)
        {
            bindable.SetValue(ViewModelLocator.AutoWireViewModelProperty, value);
        }
        
        private static void OnAutoWireViewModelChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var view = bindable as Element;
            if (view == null)
            {
                return;
            }

            var viewType = view.GetType();
            var viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            var viewModelName = string.Format(CultureInfo.InvariantCulture, "{0}Model, {1}", viewName, viewAssemblyName);

            var viewModelType = Type.GetType(viewModelName);
            if (viewModelType == null)
            {
                return;
            }
            var viewModel = TinyIoCContainer.Current.Resolve(viewModelType);
            
            view.BindingContext = viewModel;

            if(view is ContentPage contentPage && viewModel is BaseViewModel baseViewModel)
            {
                contentPage.Appearing += async (obj, args) => await baseViewModel.InitializeAsync();
                contentPage.Disappearing += async (obj, args) => await baseViewModel.TerminateAsync();

                baseViewModel.ViewName = view.GetType().Name;
            }
        }
    }
}

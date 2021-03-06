﻿using Caliburn.Micro;
using Lagou.API;
using Lagou.UWP.Attributes;
using Lagou.UWP.Common;
using Lagou.UWP.ViewModels;
using Lagou.UWP.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.Phone.UI.Input;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Notifications;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Lagou.UWP {

    public sealed partial class App {
        private WinRTContainer _container;
        private IEventAggregator _eventAggregator;

        private RootFrameViewModel _rootFrameVM = null;

        public App() {
            InitializeComponent();
        }

        protected override void Configure() {
            this._container = new WinRTContainer();
            this._container.RegisterWinRTServices();
            this.RegistInstances(this._container);
            this._eventAggregator = _container.GetInstance<IEventAggregator>();
        }

        private void RegistInstances(SimpleContainer _container) {
            var types = this.GetType().GetTypeInfo().Assembly.DefinedTypes
                .Select(t => new { T = t, Mode = t.GetCustomAttribute<RegistAttribute>()?.Mode })
                .Where(o => o.Mode != null && o.Mode != InstanceMode.None);

            foreach (var t in types) {
                var type = t.T.AsType();
                if (t.Mode == InstanceMode.Singleton) {
                    _container.RegisterSingleton(type, null, type);
                } else if (t.Mode == InstanceMode.PreRequest) {
                    _container.RegisterPerRequest(type, null, type);
                }
            }
        }

        protected override void PrepareViewFirst(Frame frame) {
            this._container.RegisterNavigationService(frame);
        }

        protected override Frame CreateApplicationFrame() {
            var view = new RootFrameView();
            Window.Current.Content = view;

            var model = RootFrameViewModel.Instance.Value;// ViewModelLocator.LocateForView(view);
            ViewModelBinder.Bind(model, view, null);
            this._rootFrameVM = (RootFrameViewModel)model;

            //Must set frame's datacontext as null
            //if not set null, will show nothing.
            view.Frm.DataContext = null;
            return view.Frm;
        }

        /// <summary>
        /// fire before configure
        /// </summary>
        /// <param name="args"></param>
        protected override void OnLaunched(LaunchActivatedEventArgs args) {
            this.DisplayRootView<ShellView>();

            ApiMessageHandler.Init();
            HardwareButtonsHelper.Init();

            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) {
                var statusBar = StatusBar.GetForCurrentView();
                statusBar.BackgroundColor = Color.FromArgb(0xff, 0x00, 0x97, 0xa7);//#0097A7
                statusBar.BackgroundOpacity = 1;
            }

            if (args.PreviousExecutionState == ApplicationExecutionState.Terminated) {
                _eventAggregator.PublishOnUIThread(new ResumeStateMessage());
            }

            //this.ConnectXamlSpy();
        }

        protected override void OnSuspending(object sender, SuspendingEventArgs e) {
            _eventAggregator.PublishOnUIThread(new SuspendStateMessage(e.SuspendingOperation));
        }

        protected override object GetInstance(Type service, string key) {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service) {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance) {
            _container.BuildUp(instance);
        }



        protected override void OnUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            e.Handled = true;
        }




        //private void ConnectXamlSpy() {
        //    var service = FirstFloor.XamlSpy.Services.XamlSpyService.Current;
        //    service.Connect("192.168.0.124", 4530, "41169");
        //}

    }
}

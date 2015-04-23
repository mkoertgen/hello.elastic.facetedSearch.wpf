using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Autofac;
using Caliburn.Micro;

namespace HelloFacets
{
    // DI configuration for AutoFac.
    // Adapted from http://www.nuget.org/packages/Analects.Caliburn.Micro.Autofac/1.0.0
    public abstract class AutofacBootstrapper<TViewModel> : BootstrapperBase where TViewModel : IScreen
    {
        private IContainer _container;

        protected AutofacBootstrapper(bool useApplication = true) : base(useApplication)
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<TViewModel>();
        }

        protected override void Configure()
        {
            var builder = new ContainerBuilder();
            ConfigureCaliburn(builder);
            ConfigureIoC(builder);
            _container = builder.Build();
        }

        protected abstract void ConfigureIoC(ContainerBuilder builder);

        private static void ConfigureCaliburn(ContainerBuilder builder)
        {
            // common caliburn instances
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<EventAggregator>().As<IEventAggregator>().SingleInstance();

            //  register view models
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(type => type.Name.EndsWith("ViewModel"))
                .AsSelf()
                .AsImplementedInterfaces()
                .InstancePerDependency();

            // register views
            builder.RegisterAssemblyTypes(AssemblySource.Instance.ToArray())
                .Where(type => type.Name.EndsWith("View"))
                .AsSelf()
                .InstancePerDependency();

        }

        protected override object GetInstance(Type service, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                object result;
                if (_container.TryResolve(service, out result))
                    return result;
            }
            else
            {
                object result;
                if (_container.TryResolveNamed(key, service, out result))
                    return result;
            }
            throw new Exception(string.Format("Could not locate any instances of contract {0}.", key ?? service.Name));
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.Resolve(typeof(IEnumerable<>).MakeGenericType(new[] { service })) as IEnumerable<object>;
        }

        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }
    }
}
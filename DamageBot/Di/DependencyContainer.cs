using System;
using Autofac;

namespace DamageBot.Di {
    public class DependencyContainer : IDisposable {
        
        private ContainerBuilder containerBuilder;

        private IContainer initialisedContainer;

        private ILifetimeScope resolver;
        
        public DependencyContainer() {
            this.containerBuilder = new ContainerBuilder();
        }

        public T Get<T>() {
            return this.resolver.Resolve<T>();
        }

        /// <summary>
        /// Add a new Binding to the dependency containerBuilder.
        /// Binds a service type to a target type.
        /// </summary>
        /// <param name="service"></param>
        /// <param name="target"></param>
        public void AddBinding(Type service, Type target) {
            this.containerBuilder.RegisterType(target).As(service);
        }

        public ContainerBuilder GetImplementation() {
            return this.containerBuilder;
        }

        public void BuildAndCreateResolver() {
            Dispose();
            this.initialisedContainer = containerBuilder.Build();
            this.resolver = this.initialisedContainer.BeginLifetimeScope();
        }

        public void Dispose() {
            if (this.resolver != null) {
                this.resolver.Dispose();
                this.resolver = null;
            }
            if (this.initialisedContainer != null) {
                this.initialisedContainer.Dispose();
                this.initialisedContainer = null;
            }
        }
    }
}
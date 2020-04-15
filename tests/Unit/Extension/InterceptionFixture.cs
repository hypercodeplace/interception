﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;
using Unity.Interception;
using Unity.Interception.ContainerIntegration;
using Unity.Interception.Interceptors.InstanceInterceptors.InterfaceInterception;
using Unity.Interception.Interceptors.TypeInterceptors.VirtualMethodInterception;
using Unity.Interception.PolicyInjection;
using Unity.Interception.PolicyInjection.MatchingRules;
using Unity.Interception.PolicyInjection.Pipeline;
using Unity.Interception.Tests;
using Unity.Lifetime;

namespace Extension.Tests
{
    [TestClass]
    public partial class InterceptionFixture : TestFixtureBase
    {
        [TestMethod]
        public void AttributeDrivenPolicyIsAddedByDefault()
        {
            GlobalCountCallHandler.Calls.Clear();

            Container.RegisterType<Interface, WrappableThroughInterfaceWithAttributes>(
                new Interceptor<InterfaceInterceptor>(),
                new InterceptionBehavior<PolicyInjectionBehavior>());

            Interface wrappedOverInterface = Container.Resolve<Interface>();
            wrappedOverInterface.Method();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["WrappableThroughInterfaceWithAttributes-Method"]);
        }

        [TestMethod]
        public void CanInterceptWrappedObject()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanCreateWrappedObject");
            container.RegisterType<Wrappable>(
                new Interceptor<VirtualMethodInterceptor>(),
                new InterceptionBehavior<PolicyInjectionBehavior>());

            Wrappable wrappable = container.Resolve<Wrappable>();
            wrappable.Method2();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanCreateWrappedObject"]);
        }

        [TestMethod]
        public void CanInterceptWrappedObjectWithRef()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanInterceptWrappedObjectWithRef");
            container.RegisterType<Wrappable>(
                new Interceptor<VirtualMethodInterceptor>(),
                new InterceptionBehavior<PolicyInjectionBehavior>());

            Wrappable wrappable = container.Resolve<Wrappable>();
            object someObj = null;
            wrappable.MethodRef(ref someObj);

            Assert.AreEqual("parameter", someObj);
            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanInterceptWrappedObjectWithRef"]);
        }

        [TestMethod]
        public void CanInterceptWrappedObjectWithValueTypeRef()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanInterceptWrappedObjectWithValueTypeRef");
            container.RegisterType<Wrappable>(
                new Interceptor<VirtualMethodInterceptor>(),
                new InterceptionBehavior<PolicyInjectionBehavior>());

            Wrappable wrappable = container.Resolve<Wrappable>();
            int someObj = 0;
            wrappable.MethodRefValue(ref someObj);

            Assert.AreEqual(42, someObj);
            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanInterceptWrappedObjectWithValueTypeRef"]);
        }

        [TestMethod]
        public void CanInterceptLifetimeManagedWrappedObject()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanCreateWrappedObject");
            container
                .RegisterType<Wrappable>(new ContainerControlledLifetimeManager(),
                    new Interceptor<VirtualMethodInterceptor>(),
                    new InterceptionBehavior<PolicyInjectionBehavior>());

            Wrappable wrappable = container.Resolve<Wrappable>();
            wrappable.Method2();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanCreateWrappedObject"]);
        }

        [TestMethod]
        public void CanInterceptNamedWrappedObject()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanCreateWrappedObject")
                .RegisterType<Wrappable>("wrappable",
                    new Interceptor<VirtualMethodInterceptor>(),
                    new InterceptionBehavior<PolicyInjectionBehavior>());

            Wrappable wrappable1 = container.Resolve<Wrappable>("wrappable");
            Wrappable wrappable2 = container.Resolve<Wrappable>();
            wrappable1.Method2();
            wrappable2.Method2();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanCreateWrappedObject"]);
        }

        [TestMethod]
        public void InterfaceImplementationsOnDerivedClassesAreWrappedMultipleTimes()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer(
                "InterfaceImplementationsOnDerivedClassesAreWrappedMultipleTimes")
                .RegisterType<DerivedWrappable>(
                    new Interceptor<VirtualMethodInterceptor>(),
                    new InterceptionBehavior<PolicyInjectionBehavior>());

            DerivedWrappable wrappable = container.Resolve<DerivedWrappable>();
            wrappable.Method();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["InterfaceImplementationsOnDerivedClassesAreWrappedMultipleTimes"]);
        }

        [TestMethod]
        public void CanCreateWrappedObjectOverInterface()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanCreateWrappedObjectOverInterface");
            container
                .RegisterType<Interface, WrappableThroughInterface>(new Interceptor<InterfaceInterceptor>(),
                                                                    new InterceptionBehavior<PolicyInjectionBehavior>());

            Interface wrappedOverInterface = container.Resolve<Interface>();
            wrappedOverInterface.Method();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanCreateWrappedObjectOverInterface"]);
        }

        [TestMethod]
        public void CanCreatLifetimeManagedeWrappedObjectOverInterface()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanCreateWrappedObjectOverInterface");
            container
                .RegisterType<Interface, WrappableThroughInterface>(new ContainerControlledLifetimeManager(),
                                                                    new Interceptor<InterfaceInterceptor>(),
                                                                    new InterceptionBehavior<PolicyInjectionBehavior>());

            Interface wrappedOverInterface = container.Resolve<Interface>();
            wrappedOverInterface.Method();
            Interface wrapped = container.Resolve<Interface>();
            wrapped.Method();

            Assert.AreEqual(2, GlobalCountCallHandler.Calls["CanCreateWrappedObjectOverInterface"]);
        }

        [TestMethod]
        public void CanInterceptExistingWrappedObjectOverInterface()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanCreateWrappedObjectOverInterface")
                .RegisterType<Interface>(
                    new Interceptor<InterfaceInterceptor>(),
                    new InterceptionBehavior<PolicyInjectionBehavior>());

            Interface wrappedOverInterface = container.BuildUp<Interface>(new WrappableThroughInterface());
            wrappedOverInterface.Method();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanCreateWrappedObjectOverInterface"]);
        }

        private IUnityContainer CreateContainer(string globalCallHandlerName)
        {
            Container.RegisterInstance<IMatchingRule>("alwaystrue", new AlwaysMatchingRule());
            Container.RegisterInstance<ICallHandler>("globalCountHandler", new GlobalCountCallHandler(globalCallHandlerName));

            Container.Configure<Interception>()
                .AddPolicy("policy")
                    .AddMatchingRule("alwaystrue")
                    .AddCallHandler("globalCountHandler");

            return Container;
        }

        [TestMethod]
        public void CanInterceptCallFromBaseOfWrappedInterface()
        {
            GlobalCountCallHandler.Calls.Clear();

            IUnityContainer container = CreateContainer("CanInterceptCallFromBaseOfWrappedInterface");
            container.RegisterType<Interface, WrappableThroughInterface>(new Interceptor<InterfaceInterceptor>(),
                                                                         new InterceptionBehavior<PolicyInjectionBehavior>());

            Interface wrappedOverInterface = container.Resolve<Interface>();
            wrappedOverInterface.Method3();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanInterceptCallFromBaseOfWrappedInterface"]);
        }

        [TestMethod]
        public void CanInterceptMethodOnDerivedType()
        {
            GlobalCountCallHandler.Calls.Clear();

            var container = CreateContainer("CanInterceptMethodOnDerivedType");
            container.RegisterType<BaseInterceptable, DerivedInterceptable>(
                new Interceptor<VirtualMethodInterceptor>(),
                new InterceptionBehavior<PolicyInjectionBehavior>());

            var instance = container.Resolve<BaseInterceptable>();

            instance.Method();

            Assert.AreEqual(1, GlobalCountCallHandler.Calls["CanInterceptMethodOnDerivedType"]);
        }

        public partial class BaseInterceptable
        {
            public virtual void Method()
            {
            }
        }

        public class DerivedInterceptable : BaseInterceptable
        {
        }
    }
}
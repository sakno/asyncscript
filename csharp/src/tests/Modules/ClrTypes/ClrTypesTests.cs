﻿using System;
using NUnit.Framework;
using DynamicScript.Testing;
using DynamicScript.Runtime;
using DynamicScript.Runtime.Environment;

namespace DynamicScript.Modules.ClrTypes
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SemanticTestBase = DynamicScript.Runtime.SemanticTestBase;

    [ComVisible(false)]
    [SemanticTest]
    [TestClass(typeof(Module))]
    sealed class ClrTypesTests : SemanticTestBase
    {
        [Test(Description="Load CLR-TYPES module.")]
        public void ModuleLoadingTest()
        {
            var r = Run("return use('clrtypes.dll');");
            Assert.IsFalse(ScriptObject.IsVoid(r));
        }

        [Test(Description="Make generic test.")]
        public void MakeGenericTest()
        {
            IScriptGeneric r = Run(@"
const clr = use('clrtypes.dll');
var Uri = clr.system.class('System.Uri');
var IConvertible = clr.mscorlib.class('System.IConvertible');
return clr.generic(Uri, [IConvertible], true);
");
            Assert.IsTrue(r.DefaultConstructor);
            Assert.AreEqual(typeof(Uri), r.BaseType.NativeType);
            Assert.AreEqual(1L, r.Interfaces.LongLength);
            Assert.AreEqual(typeof(IConvertible), r.Interfaces[0].NativeType);
        }

        [Test(Description = "Generating generic from test.")]
        public void AutoGenericTest()
        {
            IScriptGeneric r = Run("var clr = use('clrtypes.dll'); return $(clr.system.class('System.Uri'));");
            Assert.AreEqual(typeof(Uri), r.BaseType.NativeType);
        }

        [Test(Description = "Simple method invocation.")]
        public void SimpleMethodInvocation()
        {
            string s = Run(@"
const clr = use('clrtypes.dll');
var uri = clr.system.class('System.Uri')('http://www.homepage.com');
return uri.ToString();
");
            Assert.AreEqual("http://www.homepage.com/", s);
        }

        [Test(Description="Event subscribing.")]
        public void EventSubscribing()
        {
            bool r = Run(@"
const clr = use('clrtypes.dll');
var raised = false;
var component = clr.system.class('System.ComponentModel.Component')();
component.disposed.subscribe(@a, b -> void: raised = true);
component.dispose();
return raised;
");
            Assert.IsTrue(r);
        }

        [Test(Description = "Delegate creation.")]
        public void DelegateCreating()
        {
            INativeObject r = Run(@"
const clr = use('clrtypes.dll');
return clr.mscorlib.class('System.EventHandler')(@sender, e -> void: puts('Hello, world!'));
");
            Assert.IsInstanceOf<EventHandler>(r.Instance);
        }

        [Test(Description="Working with System.Collections.ArrayList class.")]
        public void ArrayListTest()
        {
            INativeObject r = Run(@"
var clr = use('clrtypes.dll');
var list = clr.mscorlib.class('System.Collections.ArrayList')();
return list;
");
            Assert.IsInstanceOf<System.Collections.ArrayList>(r.Instance);
            Assert.AreEqual(0, ((System.Collections.ArrayList)r.Instance).Count);
        }

        [Test(Description = "Working with System.Collections.ArrayList indexer.")]
        public void ArrayListIndexerTest()
        {
            long v = Run(@"
var clr = use('clrtypes.dll');
var list = clr.mscorlib.class('System.Collections.ArrayList')();
list.add(2);
return list[0];
");
            Assert.AreEqual(2, v);
        }

        [Test(Description = "Generic specification with ctor() constraint.")]
        public void GenericWithDefaultCtorTest()
        {
            INativeObject r = Run(@"
var clr = use('clrtypes.dll');
const component = clr.system.class('System.ComponentModel.Component');
const ctor = @t: clr.generic(object, void, true) -> object: t();
return ctor(component);
");
            Assert.IsInstanceOf<System.ComponentModel.Component>(r.Instance);
        }

        [Test(Description = ".NET array creation test.")]
        public void ArrayCreationTest()
        {
            INativeObject r = Run(@"
var clr = use('clrtypes.dll');
var array = clr.mscorlib.array('System.Int32', 1) (10);
return array;
");
            Assert.IsInstanceOf<Array>(r.Instance);
            Assert.AreEqual(10, ((Array)r.Instance).GetLength(0));
        }

        [Test(Description=".NET array indexer test.")]
        public void ArrayIndexerTest()
        {
            var r = Run(@"
var clr = use('clrtypes.dll');
var array = clr.mscorlib.array('System.Int32', 1) (10);
array[0] = 10;
return array[0];
");
            Assert.AreEqual(new ScriptInteger(10), r);
        }

        [Test(Description = "Iteration through array elements.")]
        public void IterationTest()
        {
            IScriptArray r = Run(@"
var clr = use('clrtypes.dll');
var intArray = clr.mscorlib.array('System.Int32', 1)(2);
intArray[0] = 1;
intArray[1] = 2;
return for var i in intArray do i + 1;
");
            Assert.AreEqual(2L, r.GetLength(0));
            Assert.AreEqual(new ScriptInteger(2L), r[new[] { 0L }, InterpreterState.Current]);
            Assert.AreEqual(new ScriptInteger(3L), r[new[] { 1L }, InterpreterState.Current]);
        }
    }
}

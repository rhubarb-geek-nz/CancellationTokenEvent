// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Management.Automation.Runspaces;
using System.Management.Automation;
using RhubarbGeekNz.CancellationTokenEvent;
using System.Threading;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace LegacyTests
{
    [TestClass]
    public class LegacyTests
    {
        readonly InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
        PowerShell powerShell;

        public LegacyTests()
        {
            initialSessionState.AddCancellationTokenEventCmdlets();

            initialSessionState.Variables.Add(new SessionStateVariableEntry("ErrorActionPreference", ActionPreference.Stop, "Stop"));
        }

        [TestInitialize]
        public void Initialize()
        {
            powerShell = PowerShell.Create(initialSessionState);
        }

        [TestCleanup]
        public void Cleanup()
        {
            powerShell.Dispose();
            powerShell = null;
        }

        [TestMethod]
        public void TestCancellationTokenByName()
        {
            powerShell.AddScript(Resources.CancellationTokenTests).AddArgument(true);

            var res = powerShell.Invoke();

            object last = null;

            foreach (var c in res)
            {
                last = c.BaseObject;
            }

            Assert.AreEqual("CancellationToken", last);
        }

        [TestMethod]
        public void TestInvokeCommandWithCancellation()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                bool exceptionCorrect = false;

                try
                {
                    Invoke(cancellationTokenSource);
                }
                catch (RuntimeException ex)
                {
                    exceptionCorrect = (ex.InnerException as OperationCanceledException).CancellationToken == cancellationTokenSource.Token;
                }

                Assert.IsTrue(exceptionCorrect, "exception should be OperationCanceledException");
            }
        }

        [TestMethod]
        public void TestInvokeCommandWithCancellationInPowerShell()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource);

                Assert.AreEqual(1, list.Count);
                Assert.IsTrue((bool)list[0].BaseObject);
            }
        }

        private Collection<PSObject> Invoke(CancellationTokenSource cancellationTokenSource, [CallerMemberName] string name = null)
        {
            using (var cancellationTokenSourceCallback = new CancellationTokenSource())
            {
                using (var cancellationTokenSourceRegistration = cancellationTokenSourceCallback.Token.Register(() =>
                {
                    cancellationTokenSource.Cancel();
                }))
                {
                    powerShell.AddScript(Resources.CommandWithCancellationTokenTests)
                        .AddArgument(name)
                        .AddArgument(cancellationTokenSource.Token)
                        .AddArgument(cancellationTokenSourceCallback);

                    return powerShell.Invoke();
                }
            }
        }
    }
}

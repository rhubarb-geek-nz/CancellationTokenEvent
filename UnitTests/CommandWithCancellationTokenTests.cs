// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;

namespace UnitTests
{
    [TestClass]
    public class CommandWithCancellationTokenTests
    {
        readonly InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
        PowerShell powerShell;

        public CommandWithCancellationTokenTests()
        {
            initialSessionState.AddCancellationTokenEventCmdlets();

            initialSessionState.Variables.Add(new SessionStateVariableEntry("ErrorActionPreference", ActionPreference.Stop, "Stop"));
        }

        [TestInitialize]
        public void Initialize()
        {
            powerShell=PowerShell.Create(initialSessionState);
        }

        [TestCleanup]
        public void Cleanup()
        {
            powerShell.Dispose();
            powerShell = null;
        }

        [TestMethod]
        public void TestInvokeCommandWithCancellation()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(5000);
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
        public void TestInvokeCommandWithoutCancellation()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource);

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("sleep ok", list[0].BaseObject);
            }
        }

        [TestMethod]
        public void TestInvokeCommandWithArgumentList()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource);

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("TestInvokeCommandWithArgumentList", list[0].BaseObject);
            }
        }

        [TestMethod]
        public void TestInvokeCommandWithInputObject()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource);

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("TestInvokeCommandWithInputObject", list[0].BaseObject);
            }
        }

        [TestMethod]
        public void TestOutputPipeline()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource);

                Assert.AreEqual(3, list.Count);
                Assert.AreEqual("one", list[0].BaseObject);
                Assert.AreEqual("two", list[1].BaseObject);
                Assert.AreEqual("three", list[2].BaseObject);
            }
        }

        [TestMethod]
        public void TestCancelledOutputPipeline()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(5000);
                var list = Invoke(cancellationTokenSource);

                Assert.AreEqual(3, list.Count);
                Assert.AreEqual("eins", list[0].BaseObject);
                Assert.AreEqual("zwei", list[1].BaseObject);
                Assert.AreEqual("drei", list[2].BaseObject);
            }
        }

        [TestMethod]
        public void TestAlreadyCancelled()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.Cancel();
                var list = Invoke(cancellationTokenSource);

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual($"{typeof(OperationCanceledException).Name},{typeof(InvokeCommandWithCancellationToken).FullName}", list[0].BaseObject);
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

        [TestMethod]
        public void TestStop()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                PowerShell localPowerShell = powerShell;

                using (var registration = cancellationTokenSource.Token.Register(() =>
                {
                    localPowerShell.Stop();
                }))
                {
                    var list = Invoke(cancellationTokenSource);

                    Assert.AreEqual("alpha", list[0].BaseObject);
                }
            }
        }

        [TestMethod]
        public void TestStopWaitEvent()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                PowerShell localPowerShell = powerShell;

                using (var registration = cancellationTokenSource.Token.Register(() =>
                {
                    localPowerShell.Stop();
                }))
                {
                    var list = Invoke(cancellationTokenSource);

                    Assert.AreEqual(1, list.Count);
                    Assert.AreEqual("alpha", list[0].BaseObject);
                }
            }
        }

        [TestMethod]
        public void TestStopInvokeCommand()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                PowerShell localPowerShell = powerShell;

                using (var registration = cancellationTokenSource.Token.Register(() =>
                {
                    localPowerShell.Stop();
                }))
                {
                    var list = Invoke(cancellationTokenSource);

                    Assert.AreEqual(1, list.Count);
                    Assert.AreEqual("alpha", list[0].BaseObject);
                }
            }
        }

        private Collection<PSObject> Invoke(CancellationTokenSource cancellationTokenSource, [CallerMemberName] string name = null)
        {
            using var cancellationTokenSourceCallback = new CancellationTokenSource();
            using var cancellationTokenSourceRegistration = cancellationTokenSourceCallback.Token.Register(() => {
                cancellationTokenSource.Cancel(); 
            });

            powerShell.AddScript(Resources.CommandWithCancellationTokenTests)
                .AddArgument(name)
                .AddArgument(cancellationTokenSource.Token)
                .AddArgument(cancellationTokenSourceCallback);

            return powerShell.Invoke();
        }
    }
}

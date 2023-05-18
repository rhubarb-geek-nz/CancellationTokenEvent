// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UnitTests
{
    [TestClass]
    public class CommandWithCancellationTokenTests
    {
        InitialSessionState initialSessionState;
        PowerShell powerShell;

        [TestInitialize]
        public void Initialize()
        {
            initialSessionState = InitialSessionState.CreateDefault();

            foreach (Type t in new Type[] {
                typeof(InvokeCommandWithCancellationToken)
            })
            {
                CmdletAttribute ca = t.GetCustomAttribute<CmdletAttribute>();

                initialSessionState.Commands.Add(new SessionStateCmdletEntry($"{ca.VerbName}-{ca.NounName}", t, ca.HelpUri));
            }

            initialSessionState.Variables.Add(new SessionStateVariableEntry("ErrorActionPreference",ActionPreference.Stop,"Stop"));

            powerShell=PowerShell.Create(initialSessionState);
        }

        [TestCleanup]
        public void Cleanup()
        {
            powerShell.Dispose();
            powerShell = null;
            initialSessionState = null;
        }

        [TestMethod]
        public void TestInvokeCommandWithCancellation()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                cancellationTokenSource.CancelAfter(5000);
                CancellationToken cancellationToken = cancellationTokenSource.Token;
                bool exceptionCorrect = false;

                try
                {
                    Invoke(cancellationToken);
                }
                catch (RuntimeException ex)
                {
                    exceptionCorrect = (ex.InnerException as OperationCanceledException).CancellationToken == cancellationToken;
                }

                Assert.IsTrue(exceptionCorrect, "exception should be OperationCanceledException");
            }
        }

        [TestMethod]
        public void TestInvokeCommandWithoutCancellation()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                CancellationToken cancellationToken = cancellationTokenSource.Token;

                var list = Invoke(cancellationToken); ;

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("sleep ok", list[0].BaseObject);
            }
        }

        [TestMethod]
        public void TestInvokeCommandWithArgumentList()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource.Token);

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("TestInvokeCommandWithArgumentList", list[0].BaseObject);
            }
        }

        [TestMethod]
        public void TestInvokeCommandWithInputObject()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource.Token);

                Assert.AreEqual(1, list.Count);
                Assert.AreEqual("TestInvokeCommandWithInputObject", list[0].BaseObject);
            }
        }

        [TestMethod]
        public void TestOutputPipeline()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource.Token);

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
                var list = Invoke(cancellationTokenSource.Token);

                Assert.AreEqual(3, list.Count);
                Assert.AreEqual("eins", list[0].BaseObject);
                Assert.AreEqual("zwei", list[1].BaseObject);
                Assert.AreEqual("drei", list[2].BaseObject);
            }
        }

        [TestMethod]
        public void TestInvokeCommandWithCancellationInPowerShell()
        {
            using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
            {
                var list = Invoke(cancellationTokenSource.Token);

                Assert.AreEqual(1, list.Count);
                Assert.IsTrue((bool)list[0].BaseObject);
            }
        }

        [TestMethod]
        public void TestStop()
        {
            using (CancellationTokenSource cancellationTokenSourceOuter = new CancellationTokenSource())
            {
                PowerShell localPowerShell = powerShell;

                using (var registration = cancellationTokenSourceOuter.Token.Register(() =>
                {
                    localPowerShell.Stop();
                }))
                {
                    cancellationTokenSourceOuter.CancelAfter(5000);

                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                    {
                        var list = Invoke(cancellationTokenSource.Token);

                        Assert.AreEqual("alpha", list[0].BaseObject);
                    }
                }
            }
        }

        [TestMethod]
        public void TestStopWaitEvent()
        {
            using (CancellationTokenSource cancellationTokenSourceOuter = new CancellationTokenSource())
            {
                PowerShell localPowerShell = powerShell;

                using (var registration = cancellationTokenSourceOuter.Token.Register(() =>
                {
                    localPowerShell.Stop();
                }))
                {
                    cancellationTokenSourceOuter.CancelAfter(5000);

                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                    {
                        var list = Invoke(cancellationTokenSource.Token);

                        Assert.AreEqual(1, list.Count);
                        Assert.AreEqual("alpha", list[0].BaseObject);
                    }
                }
            }
        }

        [TestMethod]
        public void TestStopInvokeCommand()
        {
            using (CancellationTokenSource cancellationTokenSourceOuter = new CancellationTokenSource())
            {
                PowerShell localPowerShell = powerShell;

                using (var registration = cancellationTokenSourceOuter.Token.Register(() =>
                {
                    localPowerShell.Stop();
                }))
                {
                    cancellationTokenSourceOuter.CancelAfter(5000);

                    using (CancellationTokenSource cancellationTokenSource = new CancellationTokenSource())
                    {
                        var list = Invoke(cancellationTokenSource.Token);

                        Assert.AreEqual(1, list.Count);
                        Assert.AreEqual("alpha", list[0].BaseObject);
                    }
                }
            }
        }

        private Collection<PSObject> Invoke(CancellationToken cancellationToken, [CallerMemberName] string name = null)
        {
            powerShell.AddScript(Resources.CommandWithCancellationTokenTests).AddArgument(name).AddArgument(cancellationToken);

            return powerShell.Invoke();
        }
    }
}

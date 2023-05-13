// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace UnitTests
{
    [TestClass]
    public class CancellationTokenTests
    {
        InitialSessionState initialSessionState;
        PowerShell powerShell;

        [TestInitialize]
        public void Initialize()
        {
            initialSessionState = InitialSessionState.CreateDefault();

            foreach (Type t in new Type[] {
                typeof(RegisterCancellationTokenEvent)
            })
            {
                CmdletAttribute ca = t.GetCustomAttribute<CmdletAttribute>();

                initialSessionState.Commands.Add(new SessionStateCmdletEntry($"{ca.VerbName}-{ca.NounName}", t, ca.HelpUri));
            }

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
        public void TestCancellationTokenByName()
        {
            powerShell.AddScript(Resources.CancellationTokenTests).AddArgument(true);

            var res = powerShell.Invoke();

            object last = null;

            foreach (var c in res)
            {
                last = c.BaseObject;
            }

            Assert.AreEqual("CancellationToken",last);
        }

        [TestMethod]
        public void TestCancellationTokenByOrder()
        {
            powerShell.AddScript(Resources.CancellationTokenTests).AddArgument(false);

            var res = powerShell.Invoke();

            object last = null;

            foreach (var c in res)
            {
                last = c.BaseObject;
            }

            Assert.AreEqual("CancellationToken", last);
        }

        [TestMethod]
        public void TestCancellationTokenSourceIdentifier()
        {
            string SourceIdentifier = "foo";

            powerShell.AddScript(Resources.CancellationTokenTests).AddArgument(false).AddArgument(SourceIdentifier);

            var res = powerShell.Invoke();

            object last = null;

            foreach (var c in res)
            {
                last = c.BaseObject;
            }

            Assert.AreEqual(SourceIdentifier, last);
        }

        [TestMethod]
        public void TestNullEvent()
        {
            CancellationTokenSource src = new CancellationTokenSource();
            CancellationTokenEventRegistration registration = new CancellationTokenEventRegistration(src.Token);
            src.Cancel();
        }
    }
}

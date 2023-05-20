// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace UnitTests
{
    [TestClass]
    public class CancellationTokenTests
    {
        readonly InitialSessionState initialSessionState = InitialSessionState.CreateDefault();
        PowerShell powerShell;

        public CancellationTokenTests()
        {
            initialSessionState.AddCancellationTokenEventCmdlets();
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
            using (CancellationTokenSource src = new CancellationTokenSource())
            {
                using (CancellationTokenEventRegistration registration = new CancellationTokenEventRegistration(src.Token))
                {
                    src.Cancel();
                }
            }
        }
    }
}

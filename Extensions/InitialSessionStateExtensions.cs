// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using System;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace RhubarbGeekNz.CancellationTokenEvent
{
    public static class InitialSessionStateExtensions
    {
        public static void AddCancellationTokenEventCmdlets(this InitialSessionState initialSessionState)
        {
            foreach (Type t in new Type[] {
                typeof(RegisterCancellationTokenEvent),
                typeof(InvokeCommandWithCancellationToken) 
            })
            {
                CmdletAttribute ca = t.GetCustomAttribute<CmdletAttribute>();

                initialSessionState.Commands.Add(new SessionStateCmdletEntry($"{ca.VerbName}-{ca.NounName}", t, ca.HelpUri));
            }
        }
    }
}

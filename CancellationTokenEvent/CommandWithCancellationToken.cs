// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using System;
using System.Management.Automation;
using System.Reflection;
using System.Threading;

namespace RhubarbGeekNz.CancellationTokenEvent
{
    [Cmdlet("Invoke", "CommandWithCancellationToken")]
    public class InvokeCommandWithCancellationToken : PSCmdlet
    {
        [Parameter(Mandatory = true)]
        public CancellationToken CancellationToken { get; set; }

        [Parameter(Mandatory = true)]
        public ScriptBlock ScriptBlock { get; set; }

        [Parameter]
        public object [] ArgumentList { get; set; }

        [Parameter]
        public SwitchParameter NoNewScope { get; set; }

        [Parameter]
        public PSObject InputObject { get; set; }

        private PowerShell m_powerShell;

        private object m_lock = new object();

        protected override void StopProcessing()
        {
            PowerShell powerShell;

            lock (m_lock)
            {
                powerShell = m_powerShell;
            }

            if (powerShell != null )
            {
                powerShell.Stop();
            }
        }

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            using (PowerShell powerShell = PowerShell.Create(RunspaceMode.CurrentRunspace))
            {
                PowerShell cmd = powerShell.AddCommand("Invoke-Command", true);

                cmd = cmd.AddParameter("ScriptBlock", ScriptBlock);

                if (NoNewScope)
                {
                    cmd = cmd.AddParameter("NoNewScope");
                }

                if (ArgumentList != null)
                {
                    cmd = cmd.AddParameter("ArgumentList", ArgumentList);
                }

                if (InputObject != null)
                {
                    cmd = cmd.AddParameter("InputObject", InputObject);
                }

                using (CancellationTokenRegistration registration = CancellationToken.Register(() =>
                {
                    powerShell.Stop();
                }))
                {
                    PSDataCollection<object> input = new PSDataCollection<object>();
                    input.Complete();
                    PSDataCollection<object> output = new PSDataCollection<object>();

                    output.DataAdded += (s, e) =>
                    {
                        object data = output[e.Index];
                        WriteObject(data);
                    };

                    PSInvocationSettings invocationSettings = new PSInvocationSettings
                    {
                        Host = Host,
                        ExposeFlowControlExceptions = true,
                        ErrorActionPreference = GetErrorActionPreference()
                    };

                    try
                    {
                        lock (m_lock)
                        {
                            m_powerShell = powerShell;
                        }

                        if (!Stopping && !CancellationToken.IsCancellationRequested)
                        {
                            powerShell.Invoke<object, object>(input, output, invocationSettings);
                        }
                    }
                    finally
                    {
                        lock (m_lock)
                        {
                            m_powerShell = null;
                        }
                    }
                }
            }

            if (CancellationToken.IsCancellationRequested)
            {
                Exception exception = new OperationCanceledException(CancellationToken);
                string errorId = exception.GetType().Name;

                WriteError(new ErrorRecord(exception, errorId, ErrorCategory.OperationStopped, null));
            }
        }

        protected override void EndProcessing()
        {
        }

        private ActionPreference ? GetErrorActionPreference()
        {
            ActionPreference? result;
            PropertyInfo pi = CommandRuntime.GetType().GetProperty("ErrorAction", BindingFlags.Instance | BindingFlags.NonPublic);

            if ((pi != null) && typeof(ActionPreference).IsAssignableFrom(pi.PropertyType))
            {
                result = (ActionPreference?)pi.GetValue(CommandRuntime, null);
            }
            else
            {
                object ErrorActionPreference = GetVariableValue("ErrorActionPreference");

                if (ErrorActionPreference != null)
                {
                    if (ErrorActionPreference is ActionPreference)
                    {
                        result = (ActionPreference)ErrorActionPreference;
                    }
                    else
                    {
                        if (ErrorActionPreference is string)
                        {
                            result = (ActionPreference)Enum.Parse(typeof(ActionPreference), (string)ErrorActionPreference);
                        }
                        else
                        {
                            throw new ArgumentException($"ErrorActionPreference {ErrorActionPreference}");
                        }
                    }
                }
                else
                {
                    result = null;
                }
            }

            return result;
        }
    }
}

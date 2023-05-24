// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using System.Management.Automation;

namespace RhubarbGeekNz.CancellationTokenEvent
{
    [Cmdlet(VerbsLifecycle.Register, "CancellationTokenEvent")]
    [OutputType(typeof(CancellationTokenEventRegistration))]
    public class RegisterCancellationTokenEvent : PSCmdlet
    {
        [Parameter(
            Mandatory = true,
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        public System.Threading.CancellationToken CancellationToken { get; set; }

        protected override void BeginProcessing()
        {
        }

        protected override void ProcessRecord()
        {
            WriteObject(new CancellationTokenEventRegistration(CancellationToken));
        }

        protected override void EndProcessing()
        {
        }
    }
}

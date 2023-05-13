// Copyright (c) 2023 Roger Brown.
// Licensed under the MIT License.

using System;
namespace RhubarbGeekNz.CancellationTokenEvent
{
    public class CancellationTokenEventRegistration : IDisposable
    {
        public event Action Cancelled;
        public void Dispose() => CancellationTokenRegistration.Dispose();
        private readonly IDisposable CancellationTokenRegistration;
        public CancellationTokenEventRegistration(System.Threading.CancellationToken ct)
        {
            CancellationTokenRegistration = ct.Register(() =>
            {
                if (Cancelled != null)
                {
                    Cancelled();
                }
            });
        }
    }
}

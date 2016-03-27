using System;
using System.Threading.Tasks;

namespace Granite.Core.Interfaces
{
    internal interface IMessagePipeline : IDisposable
    {
        void Start();

        Task StopAsync(bool finishMessages);
    }
}

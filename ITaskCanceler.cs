using System.Threading;
using System;

namespace OptionPricing
{
    interface ITaskCanceler
    {
        CancellationTokenSource RegisterTask(int taskId);
        void CancelTask();
        void NotifyTaskFinished();
        void DisposeTask(int taskId);
    }

}
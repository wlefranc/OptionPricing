using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace OptionPricing
{
    public class DictTaskCanceler : ITaskCanceler
    {
        private int m_lastTaskId;
        private ConcurrentDictionary<int, CancellationTokenSource> m_dict;

        public DictTaskCanceler()
        {
            m_dict = new ConcurrentDictionary<int, CancellationTokenSource>();
        }
        public CancellationTokenSource RegisterTask(int taskId)
        {
            Volatile.Write(ref m_lastTaskId, taskId);
            m_dict.TryAdd(taskId, new CancellationTokenSource());
            return m_dict[taskId];
        }

        public void CancelTask()
        {
            var prevId = Volatile.Read(ref m_lastTaskId);
            if (prevId != 0)
            {
                m_dict[prevId].Cancel();
            }
        }

        public void NotifyTaskFinished()
        {
            Volatile.Write(ref m_lastTaskId, 0);
        }

        public void DisposeTask(int taskId)
        {
            m_dict[taskId].Dispose();
        }
    }

}
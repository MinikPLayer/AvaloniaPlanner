using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Utils
{
    public class NetworkActionQueue
    {
        public static NetworkActionQueue Api = new NetworkActionQueue();

        private Task currentTask = Task.FromResult(true);
        private object mutex = new object();


        public Task AddToQueue(NetworkAction action)
        {
            Task newTask;
            lock (mutex)
            {
                newTask = currentTask.ContinueWith(t => action.ExecuteOnline()).Unwrap();
                currentTask = newTask;
            }

            return newTask;
        }

        public (NetworkAction action, Task task) AddToQueue<T>() where T : NetworkAction, new()
        {
            var action = NetworkAction.CreateAction<T>();
            var task = AddToQueue(action);
            return (action, task);
        }
    }
}

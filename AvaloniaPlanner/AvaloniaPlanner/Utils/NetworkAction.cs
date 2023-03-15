using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaPlanner.Utils
{
    public abstract class NetworkAction
    {
        public bool Invalidated { get; set; } = false;
        public bool networkResult { get; protected set; } = false;
        public bool networkDone { get; private set; } = false;
        protected abstract Task<bool> OnlineAction();
        protected abstract bool OfflineAction();

        public async Task ExecuteOnline()
        {
            if (Invalidated)
                return;

            if (networkDone)
                return;

            networkResult = await OnlineAction();
            networkDone = true;
        }

        public bool ExecuteOffline()
        {
            if (Invalidated)
                return false;

            return OfflineAction();
        }

        public static T CreateAction<T>() where T : NetworkAction, new()
        {
            var action = new T();
            action.ExecuteOffline();
            return action;
        }
    }
}

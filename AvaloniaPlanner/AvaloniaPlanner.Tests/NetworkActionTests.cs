using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvaloniaPlanner.Utils;

namespace AvaloniaPlanner.Tests
{
    public class TestAction : NetworkAction
    {
        List<int> actionOrderList;
        int delay;
        int actionNumber;

        protected override bool OfflineAction()
        {
            Debug.WriteLine("Offline");
            return true;
        }

        protected override async Task<bool> OnlineAction()
        {
            await Task.Delay(delay);
            actionOrderList.Add(actionNumber);
            Debug.WriteLine("ONLINE");
            await Task.Delay(delay);
            actionOrderList.Add(actionNumber);

            return true;
        }

        public TestAction(int delay, int actionNumber, List<int> actionOrderList)
        {
            this.delay = delay;
            this.actionNumber = actionNumber;
            this.actionOrderList = actionOrderList;
        }
    }

    public class NetworkActionTests
    {
        [Fact]
        public void Test()
        {
            var actionOrderList = new List<int>();
            var expectedList = new List<int> { 1, 1, 2, 2, 3, 3, 4, 4, 5, 5 };

            NetworkActionQueue.Api.AddToQueue(new TestAction(200, 1, actionOrderList));
            NetworkActionQueue.Api.AddToQueue(new TestAction(100, 2, actionOrderList));
            NetworkActionQueue.Api.AddToQueue(new TestAction(500, 3, actionOrderList));
            NetworkActionQueue.Api.AddToQueue(new TestAction(300, 4, actionOrderList));
            var testAction = NetworkActionQueue.Api.AddToQueue(new TestAction(100, 5, actionOrderList));
            testAction.Wait();

            Action.Equals(actionOrderList, expectedList);
        }
    }
}

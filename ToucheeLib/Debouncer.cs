using System;
using System.Threading;

namespace Touchee {

    public class Debouncer {

        public Action Action { get; protected set; }
        public TimeSpan Delay { get; protected set; }
        Timer _timer;

        public Debouncer(Action action, TimeSpan delay) {
            this.Action = action;
            this.Delay = delay;
            _timer = new Timer(_ => action());
        }

        public void Call() {
            _timer.Change(this.Delay, TimeSpan.FromMilliseconds(-1));
        }


    }
}

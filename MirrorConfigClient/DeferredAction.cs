using System;
using System.Collections.Generic;
using System.Windows.Threading;

namespace MirrorConfigClient
{
    public class DeferredAction
    {
        private static Dictionary<Action<object>, DeferredAction> m_Actions = new Dictionary<Action<object>, DeferredAction>();

        public static void Execute(Action<object> ActionToDefer, object Parameter, int Milliseconds)
        {
            if (!m_Actions.ContainsKey(ActionToDefer))
                m_Actions.Add(ActionToDefer, new DeferredAction(ActionToDefer, Milliseconds));
            DeferredAction DeferredAction = m_Actions[ActionToDefer];
            DeferredAction.Execute(Parameter);
        }

        public static void Cancel(Action<object> ActionToCancel)
        {
            if (m_Actions.ContainsKey(ActionToCancel))
            {
                m_Actions[ActionToCancel].m_Timer.Stop();
                m_Actions[ActionToCancel].m_Timer.Tick -= m_Actions[ActionToCancel].Timer_Tick;
                m_Actions[ActionToCancel].m_Timer = null;
                m_Actions[ActionToCancel].m_DeferredAction = null;
                m_Actions.Remove(ActionToCancel);
            }
        }

        private Action<object> m_DeferredAction;
        private object m_Parameter;
        private TimeSpan m_DeferredInterval;
        private DispatcherTimer m_Timer;

        private DeferredAction(Action<object> DeferredAction, int Milliseconds)
        {
            m_DeferredAction = DeferredAction;
            m_DeferredInterval = new TimeSpan(0, 0, 0, 0, Milliseconds);
            m_Timer = new DispatcherTimer();
            m_Timer.Interval = m_DeferredInterval;
            m_Timer.Tick += Timer_Tick;
        }

        private void Execute(object Parameter)
        {
            m_Timer.Stop();
            m_Parameter = Parameter;
            m_Timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            m_Timer.Stop();
            m_DeferredAction(m_Parameter);
            m_Actions.Remove(m_DeferredAction);
            m_Timer.Tick -= Timer_Tick;
            m_Timer = null;
            m_DeferredAction = null;
        }
    }
}

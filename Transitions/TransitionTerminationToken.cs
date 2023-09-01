using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VitchinnikMonoCore.Transitions
{
    public class TransitionTerminationToken
    {
        internal event Action OnTerminate;
        internal event Action OnForcedExpiration;
        public void Terminate()
        {
            OnTerminate.Invoke();
        }
        public void ForceTransitionExpire()
        {
            OnForcedExpiration.Invoke();
        }
    }
}

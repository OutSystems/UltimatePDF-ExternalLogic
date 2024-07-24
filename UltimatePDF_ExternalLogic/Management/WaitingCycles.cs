using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OutSystems.UltimatePDF_ExternalLogic.Management.Troubleshooting;

namespace OutSystems.UltimatePDF_ExternalLogic.Management {

    /// <summary>
    /// This class controls the number of seconds ultimate pdf code will wait
    /// for specific javascript to be ready.
    /// We can wait for JS half the timeout value, or 30 seconds if no timeout
    /// value is given.
    /// </summary>
    internal class WaitingCycles {
        private int waitingCyclesInSeconds;
        private readonly Logger logger;
        
        public int WaitingCyclesInSecondsLeft {
            get {
                return waitingCyclesInSeconds;
            }
        }
        
        public WaitingCycles(int waitingCycles, Logger logger) {
            this.waitingCyclesInSeconds = waitingCycles;
            this.logger = logger;
        }


        public T RunOnceAndWaitForCondition<T>(String operationName, Func<T> operation, Func<T, bool> condition) {
            var result = operation();
            
            while(waitingCyclesInSeconds > 0 && !condition(result)) {
                logger.Log($"{operationName} did not meet the condition to continue. Sleeping 1000ms");
                System.Threading.Thread.Sleep(1000);
                waitingCyclesInSeconds--;
                operation();
            }

            return result;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Formix.Synchronization
{
    public class SemaphoreTask : Task
    {
        public Guid TaskId { get; set; }
        public int Usage { get; private set; }

        public SemaphoreTask(Action action, int usage) 
            : base(action)
        {
            if (usage <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(usage), "The parameter myst be grater than 0.");
            }

            Usage = usage;
            TaskId = Guid.NewGuid();
        }
    }
}

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace FPSGame.src.Common{
    class QueueTaskSchedWrapper{
        private static readonly Lazy<QueueTaskSchedWrapper> _lazyVar = new Lazy<QueueTaskSchedWrapper>(() => new QueueTaskSchedWrapper());
        public QueuedTaskScheduler queuedTaskScheduler {get;}
        private ConcurrentDictionary<int,TaskScheduler> prioritySchedulerReferences;
        
        private QueueTaskSchedWrapper(){
            int maxConcurrency = System.Environment.ProcessorCount-1;
            
            queuedTaskScheduler = new QueuedTaskScheduler(maxConcurrency);
            prioritySchedulerReferences = new ConcurrentDictionary<int, TaskScheduler>();
            Console.WriteLine("Test activation: {0}",queuedTaskScheduler.MaximumConcurrencyLevel);
        }
        public TaskScheduler CreatePriorityQueue(int priority){
            TaskScheduler taskScheduler;
            if(!prioritySchedulerReferences.ContainsKey(priority)){
                taskScheduler = queuedTaskScheduler.ActivateNewQueue(priority);
                prioritySchedulerReferences.TryAdd(priority,taskScheduler);
            }
            else{
                taskScheduler = this.prioritySchedulerReferences[priority];
            }
            return taskScheduler;
            
            
        }
        public TaskScheduler GetPriorityQueueScheduler(int priority){
            return prioritySchedulerReferences[priority];
        }

        public static QueueTaskSchedWrapper Instance{
            get{
                return _lazyVar.Value;
            }
        }

        
    }
}
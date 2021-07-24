using System;
using System.Collections.Generic;
using System.Linq;

namespace fermiac {
    public class TimedQueue<T>
    {
        private List<Tuple<DateTime, T>> _queue;

        public TimedQueue()
        {
            _queue = new List<Tuple<DateTime, T>>();
        }

        public void Enqueue(T item)
        {
            _queue.Add(new Tuple<DateTime, T>(DateTime.Now, item));
        }

        public void Enqueue(T item, double offset)
        {
            _queue.Add(new Tuple<DateTime, T>(DateTime.Now.AddMilliseconds(offset), item));
        }

        public int Count() 
        {
            return(_queue.Count);
        }

        public bool CanDequeue
        {
            get {
                return _queue.Any(ix => ix.Item1 <= DateTime.Now);
            }
        }

        public T Dequeue() {
            if(CanDequeue) {
                var ox = _queue.OrderBy(qx => qx.Item1);
                var item = ox.First();
                _queue.Remove(item);
                return(item.Item2);
            } else {
                throw new InvalidOperationException("Queue is empty or all items reside in the future");
            }
        }
    }
}
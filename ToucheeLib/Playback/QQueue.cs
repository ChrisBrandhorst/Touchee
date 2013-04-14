using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Touchee.Playback {
    
    /// <summary>
    /// Represents a playback queue
    /// </summary>
    public class QQueue {


        #region Privates

        /// <summary>
        /// Contains the non-priority items in the original order
        /// </summary>
        List<IItem> _itemsOriginal;

        /// <summary>
        /// Contains the non-priority items in the active order
        /// </summary>
        List<IItem> _items;

        /// <summary>
        /// Contains the priority items
        /// </summary>
        List<IItem> _itemsPriority;

        /// <summary>
        /// Shuffle value
        /// </summary>
        bool _shuffle = false;

        /// <summary>
        /// Position of the queue in the non-priority list
        /// </summary>
        int _index = -1;

        #endregion




        #region Properties


        /// <summary>
        /// All items in the queue in the order in which they will be played
        /// </summary>
        public IEnumerable<IItem> Items {
            get {
                var items = new List<IItem>(_items);
                items.InsertRange(_index + 1, _itemsPriority);
                return items;
            }
        }


        /// <summary>
        /// The upcoming items in the queue in the order in which they will be played
        /// </summary>
        public IEnumerable<IItem> Upcoming {
            get {
                var items = Items.ToList();
                return _index + 1 > items.Count ? new List<IItem>() : items.GetRange(_index + 1, items.Count - _index - 1);
            }
        }


        /// <summary>
        /// Returns the current item in the queue
        /// </summary>
        public IItem Current {
            get {
                var items = Items.ToList();
                return Index >= 0 && Index < items.Count ? items[Index] : null;
            }
        }


        /// <summary>
        /// Returns the previous item in the queue
        /// </summary>
        public IItem Prev {
            get {
                var items = Items.ToList();
                var i = _index - 1;
                return i >= 0 && i < items.Count ? items[i] : null;
            }
        }


        /// <summary>
        /// Returns the next item in the queue
        /// </summary>
        public IItem Next {
            get {
                var items = Items.ToList();
                var i = _index + 1;
                return i >= 0 && i < items.Count ? items[i] : null;
            }
        }


        /// <summary>
        /// Returns the item at the given index
        /// </summary>
        /// <param name="index">The index of the item</param>
        /// <exception cref="ArgumentOutOfRangeException">If the given value is out of range</exception>
        public IItem ItemAt(int index) {
            return Items.ToList()[index];
        }


        /// <summary>
        /// The index of the current item in the queue
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">If the given value is less than the current index (except -1) or larger than the length of the queue</exception>
        public int Index {
            get { return _index; }
            set {
                var itemCount = Items.Count();

                // Going back is not supported
                if (value != -1 && value < _index)
                    throw new ArgumentOutOfRangeException("Changing the index to a value before the current index (except -1) is not supported");
                
                // Cannot move to position after the end of queue
                else if (value > itemCount)
                    throw new ArgumentOutOfRangeException("Given value is larger than the length of the queue");
                
                // Other cases
                else {

                    // Going past the end of the queue
                    if (value == itemCount)
                        _index = -1;

                    // Skipping ahead: remove items from the priority queue
                    else if (value > _index) {
                        var removed = Math.Min(PriorityCount, value - _index - 1);
                        _itemsPriority.RemoveRange(0, removed);
                        _index = value - removed;
                    }

                    // Callbacks
                    if (IndexChanged != null)
                        IndexChanged.Invoke(this);
                    if (_index == -1 && Finished != null)
                        Finished.Invoke(this);
                    
                }

            }
        }


        /// <summary>
        /// The number of items in the priority part of this queue
        /// </summary>
        public int PriorityCount {
            get {
                return _itemsPriority.Count; 
            }
        }


        /// <summary>
        /// Whether the queue has started
        /// </summary>
        public bool IsBeforeFirstItem { get { return _index == -1; } }


        /// <summary>
        /// Whether the current item is the first item of the queue
        /// </summary>
        public bool IsAtFirstItem { get { return _index == 0; } }


        /// <summary>
        /// Whether the current item is the last item of the queue
        /// </summary>
        public bool IsAtLastItem { get { return _index == Items.Count() - 1; } }


        /// <summary>
        /// Gets or sets the shuffling of the queue
        /// </summary>
        public bool Shuffle {
            get { return _shuffle; }
            set {
                if (_shuffle == value) return;
                // We are going to shuffle
                if (_shuffle = value) {
                    
                }
                // Back to original
                else {
                    
                }
            }
        }


        /// <summary>
        /// The repeat mode of the queue
        /// </summary>
        public RepeatMode Repeat { get; set; }








        #endregion



        
        #region Constructors

        /// <summary>
        /// Constructs a new, empty queue.
        /// </summary>
        public QQueue() {
            Repeat = RepeatMode.Off;
            _items = new List<IItem>();
            _itemsOriginal = new List<IItem>();
            _itemsPriority = new List<IItem>();
        }


        /// <summary>
        /// Constructs a new queue with one item in it.
        /// </summary>
        /// <param name="item">The item to add to the queue</param>
        public QQueue(IItem item) : this() {
            _itemsOriginal.Add(item);
            _items.Add(item);
        }


        /// <summary>
        /// Constructs a new queue with a number of items in it.
        /// </summary>
        /// <param name="items">The initial set of items in the queue</param>
        public QQueue(IEnumerable<IItem> items) : this() {
            _itemsOriginal.AddRange(items);
            _items.AddRange(items);
        }


        #endregion




        #region Events

        public event QueueItemsUpdatedEventHandler ItemsUpdated;
        public event QueueIndexChangedEventHandler IndexChanged;
        public event QueueFinishedEventHandler Finished;

        #endregion




        #region Helpers


        #endregion



        public delegate void QueueItemsUpdatedEventHandler(QQueue queue);
        public delegate void QueueIndexChangedEventHandler(QQueue queue);
        public delegate void QueueFinishedEventHandler(QQueue queue);

    }

}

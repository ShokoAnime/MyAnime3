using System.Collections.Generic;

namespace Shoko.MyAnime3.DataHelpers
{
    /// <summary>
    /// Helper class that implements a priority queue
    /// </summary>
    /// <typeparam name="T">The type of the values placed in the queue</typeparam>
    public class PriorityQueue<T> : ListMapping<int, T>
    {
        #region Constructor

        #endregion

        #region Public Functions

        /// <summary>
        /// Peek at the next thing in the queue
        /// </summary>
        /// <returns></returns>
        public virtual T Peek()
        {
            if (Items.ContainsKey(HighestKey))
                return Items[HighestKey][0];
            return default(T);
        }

        public override void Add(int Priority, T Value)
        {
            if (Priority > HighestKey)
                HighestKey = Priority;
            base.Add(Priority, Value);
        }

        /// <summary>
        /// Removes an item from the queue and returns it
        /// </summary>
        /// <returns>The next item in the queue</returns>
        public T Remove()
        {
            T ReturnValue = default(T);
            if (Items.ContainsKey(HighestKey) && Items[HighestKey].Count >= 1)
            {
                ReturnValue = Items[HighestKey][0];
                Items[HighestKey].Remove(ReturnValue);
                if (Items[HighestKey].Count == 0)
                {
                    Items.Remove(HighestKey);
                    HighestKey = int.MinValue;
                    foreach (int Key in Items.Keys)
                        if (Key > HighestKey)
                            HighestKey = Key;
                }
            }
            return ReturnValue;
        }

        #endregion

        #region Protected Variables

        protected int HighestKey = int.MinValue;

        #endregion
    }

    /// <summary>
    /// Maps a key to a list of data
    /// </summary>
    /// <typeparam name="T1">Key value</typeparam>
    /// <typeparam name="T2">Type that the list should contain</typeparam>
    public class ListMapping<T1, T2>
    {
        #region Constructors

        #endregion

        #region Private Variables

        protected Dictionary<T1, List<T2>> Items = new Dictionary<T1, List<T2>>();

        #endregion

        #region Public Functions

        /// <summary>
        /// Adds an item to the mapping
        /// </summary>
        /// <param name="Key">Key value</param>
        /// <param name="Value">The value to add</param>
        public virtual void Add(T1 Key, T2 Value)
        {
                /*
                if (Value.GetType() == typeof(IAniDBUDPCommand))
                {
                    IAniDBUDPCommand icmd = Value as IAniDBUDPCommand;
                    AniDBUDPCommand cmd = (AniDBUDPCommand)icmd;
                    BaseConfig.MyAnimeLog.Write("Add Command to PQueue: {0} - {1}", cmd.commandType, cmd.commandID);
                }
                else
                {
                    IAniDBHTTPCommand icmd = Value as IAniDBHTTPCommand;
                    AniDBHTTPCommand cmd = (AniDBHTTPCommand)icmd;
                    BaseConfig.MyAnimeLog.Write("Add Command to PQueue: {0} - {1}", cmd.commandType, cmd.commandID);
                }*/

                if (Items.ContainsKey(Key))
                {
                    Items[Key].Add(Value);
                }
                else
                {
                    Items.Add(Key, new List<T2>());
                    Items[Key].Add(Value);
                }
        }

        /// <summary>
        /// Determines if a key exists
        /// </summary>
        /// <param name="key">Key to check on</param>
        /// <returns>True if it exists, false otherwise</returns>
        public virtual bool ContainsKey(T1 key)
        {
            return Items.ContainsKey(key);
        }



        /// <summary>
        /// The list of keys within the mapping
        /// </summary>
        public virtual ICollection<T1> Keys
        {
            get
            {
                return Items.Keys;
            }
        }

        /// <summary>
        /// Remove a list of items associated with a key
        /// </summary>
        /// <param name="key">Key to use</param>
        /// <returns>True if the key is found, false otherwise</returns>
        public virtual bool Remove(T1 key)
        {
            return Items.Remove(key);
        }

        /// <summary>
        /// Gets a list of values associated with a key
        /// </summary>
        /// <param name="key">Key to look for</param>
        /// <returns>The list of values</returns>
        public virtual List<T2> this[T1 key]
        {
            get { return Items[key]; }
            set { Items[key] = value; }
        }

        /// <summary>
        /// Clears all items from the listing
        /// </summary>
        public virtual void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        /// The number of items in the listing
        /// </summary>
        public virtual int Count
        {
            get
            {
                int total = 0;
                foreach (KeyValuePair<T1, List<T2>> kvp in Items)
                    total += kvp.Value.Count;
                return total;
            }
        }

        #endregion
    }
}
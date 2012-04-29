using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataStructures
{
  /// <summary>
  /// Node from a skip list. Has a set height.
  /// <remarks>I am not particularly fond of git. Or at least the damned passphrase thing.</remarks>
  /// </summary>
  public class SkipNode<T>
  {
    /// <summary>
    /// No parameterless constructor.
    /// </summary>
    private SkipNode(){}

    /// <summary>
    /// Create a node of a set height.
    /// </summary>
    public SkipNode(int height)
    {
      Neighbors = new SkipNode<T>[height];
    }

    /// <summary>
    /// Create a node with a set height and value.
    /// </summary>
    public SkipNode(int height, T value):this(height)
    {
      Value = value;
    }

    /// <summary>
    /// Neighbors of the node at various heights.
    /// </summary>
    private SkipNode<T>[] Neighbors { get; set; }

    /// <summary>
    /// Get the Neighbor node at a given height. Might be null.
    /// </summary>
    public SkipNode<T> this[int index]
    {
      get { return Neighbors[index]; }
      set { Neighbors[index] = value; }
    }
    
    /// <summary>
    /// How many neighbors do we keep track of anyway?
    /// </summary>
    public int Height
    {
      get { return Neighbors.Count(); }
    }

    /// <summary>
    /// The value of the node
    /// </summary>
    public T Value { get; set; }

    public override string ToString()
    {
      return "H:" + Height + "V:" + Value;
    }

    /// <summary>
    /// Get the next node
    /// </summary>
    public SkipNode<T> Next()
    {
      return this[0];
    }

    /// <summary>
    /// Previous node in sequence.
    /// </summary>
    public SkipNode<T> Previous { get; set; }
  }

  /// <summary>
  /// Skiplist, a structure with log(n) add/remove/search. Probabalistically.
  /// </summary>
  public class SkipList<T> : ICollection<T>
  {
    #region constructors
    public SkipList(int seed, IComparer<T> c)
    {
      _Rand = seed > 0 ? new Random(seed) : new Random();
      Comparer = c;
      Count = 0;
      _Root = new SkipNode<T>(1);
    }

    public SkipList():this(-1, Comparer<T>.Default)
    {
    }

    public SkipList(int seed): this(seed, Comparer<T>.Default)
    {
    }

    public SkipList(IComparer<T> c): this(-1, c)
    {
    }
    #endregion

    private const double DENOMINATOR = 2.0;
    private const double FRACTION = 1/DENOMINATOR;
    private readonly Random _Rand;
    protected SkipNode<T> _Root;

    /// <summary>
    /// Based on the current length + 1, get the random height for a new node
    /// </summary>
    protected int GetHeight()
    {
      double r = _Rand.NextDouble();
      int max = (int)Math.Ceiling(Math.Log(Count + 1, DENOMINATOR));
      int result = 0;
      for (int i = 0; i < max; i++)
      {
        if(r > FRACTION) break;
        result = i;
        r = r*DENOMINATOR;
      }

      return result + 1;
    }

    /// <summary>
    /// Make sure that it is monotonicly increasing at all nodeheights.
    /// </summary>
    protected virtual bool CheckIntegrity()
    {
      var previousQueue = new Queue<SkipNode<T>>();
      for (int i = _Root.Height-1; i >= 0; i--)
      {
        //currentQueue will get all of the nodes (in order) at level i
        //previousQueue will have all of the nodes (in order) at level i+1. It might be empty.
        var currentQueue = new Queue<SkipNode<T>>();
        var currentNode = _Root[i];
        while (currentNode != null)
        {
          currentQueue.Enqueue(currentNode);
          
          //the head of previousQueue may or may not be this item. If so, dequeue it.
          if (previousQueue.Count > 0 && 
            previousQueue.Peek() == currentNode)
            previousQueue.Dequeue();

          //Also, we should be always increasing (or at least not backtracking.
          if (currentNode[i] != null &&
            this.Comparer.Compare(currentNode.Value, currentNode[i].Value) > 0)
            throw new Exception(string.Format("{0} is not less than or equal to {1}. At level {2}", 
              currentNode.Value, currentNode[i].Value, i));

          currentNode = currentNode[i];
        }

        //but by the time we exhaust nodes at level i, we've emptied previousQueue
        if (previousQueue.Count > 0) 
          throw new Exception(previousQueue.Count + " items remain at level " + i);

        previousQueue = currentQueue;
      }

      //check that previous-ness holds
      CheckBackwards();

      return true;
    }


    private void CheckBackwards()
    {
      var nodeStack = new Stack<SkipNode<T>>(Count);
      var currentNode = _Root;
      do
      {
        nodeStack.Push(currentNode);
        currentNode = currentNode.Next();
      } while (currentNode != null);

      currentNode = nodeStack.Pop();//we'll always have 1, due to the root.
      while (nodeStack.Count > 0)
      {
        var tmp = nodeStack.Pop();
        if (tmp != currentNode.Previous)
          throw new Exception(string.Format("Walking the node backwards is broken. {0} != {1}",
              tmp.ToString(), currentNode.Previous.ToString()));

        currentNode = tmp;
      }
    }

    /// <summary>
    /// Used to compare elements
    /// </summary>
    public IComparer<T> Comparer { get; protected set; }

    public override string ToString()
    {
      var result = new StringBuilder();
      var current = _Root;
      while (current.Next() != null)
      {
        current = current.Next();
        result.Append(current.ToString());
        result.Append(", ");
      }
      return result.ToString();
    }

    internal void Testy()
    {
    }

    

    #region ICollection<T> Members


    /// <summary>
    /// Add an item to the list. Log(n) operation.
    /// </summary>
    public void Add(T item)
    { 
      SkipNode<T> dummy;
      Add(item, out dummy);
    }

    /// <summary>
    /// Add an item to the list. Log(n) operation.
    /// </summary>
    virtual public SkipNode<T>[] Add(T item, out SkipNode<T> itemNode)
    {
      //How high is this node?
      var height = ReadjustHeight();

      //find the one that it goes just after
      var predecessors = GetPredecessors(item);

      //rethread the references
      itemNode = new SkipNode<T>(height, item);
      Rethread(itemNode, predecessors);

      //And now we're bigger.
      Count++;

      return predecessors;
    }

    /// <summary>
    /// rethread the references with this itemNode and array of predecessors
    /// </summary>
    protected static void Rethread(SkipNode<T> itemNode, SkipNode<T>[] predecessors)
    {
      int height = itemNode.Height;
      for (int i = 0; i < height; i++)
      {
        itemNode[i] = predecessors[i][i];
        predecessors[i][i] = itemNode;
      }

      //fix Previouses
      if (itemNode.Next() != null)
        itemNode.Next().Previous = itemNode;
      itemNode.Previous = predecessors[0];
    }

    /// <summary>
    /// Get a height for a new node, readjusting the root height if needed.
    /// </summary>
    /// <returns></returns>
    protected int ReadjustHeight()
    {
      var height = GetHeight();

      //see if we have to readjust the root
      if (height > _Root.Height)
      {
        var newRoot = new SkipNode<T>(height);
        for (int i = 0; i < _Root.Height; i++)
          newRoot[i] = _Root[i];
        _Root = newRoot;
        if (_Root.Next() != null)
          _Root.Next().Previous = _Root;
      }
      return height;
    }
    public bool Remove(T item)
    {
      SkipNode<T> dummy;
      return Remove(item, out dummy);
    }

    virtual public bool Remove(T item, out SkipNode<T> removed)
    {
      //find the one that it goes just after
      removed = null;
      var predecessors = GetPredecessors(item);
      if (predecessors[0] == null) return false;

      removed = predecessors[0].Next();
      if (removed == null) return false;

      //did we not find it?
      if (Comparer.Compare(item, removed.Value) != 0) return false;

      //rethread the references. the trick is that we don't care about predecessors at a height > the deleted node's
      for (int i = 0; i < removed.Height; i++)
        predecessors[i][i] = removed[i];

      //fix Previouses
      if (removed.Next() != null)
        removed.Next().Previous = removed.Previous;
      
      //And now we're smaller.
      Count--;
      return true;
    }

    protected SkipNode<T>[] BuildUpdateTable(T value)
    {
      SkipNode<T>[] updates = new SkipNode<T>[_Root.Height];
      SkipNode<T> current = _Root;

      // determine the nodes that need to be updated at each level
      for (int i = _Root.Height - 1; i >= 0; i--)
      {
        while (current[i] != null && Comparer.Compare(value, current[i].Value) >= 0)
          current = current[i];

        updates[i] = current;
      }

      return updates;
    }

    protected SkipNode<T>[] GetPredecessors(T item)
    {
      var predecessors = new SkipNode<T>[_Root.Height];
      var current = _Root;

      for (int i = current.Height - 1; i >= 0; i--)
      {

        //go either til we find it, or the last one at this level that is less than item
        while (current[i] != null &&
               //(Comparer.Compare(item, current[i].Value) >= 0))
               (Comparer.Compare(item, current[i].Value) > 0)) // > is what I had before....
        {
          current = current[i];
        }
        predecessors[i] = current;

      }

      return predecessors;
    }

    /// <summary>
    /// Scotch the list.
    /// </summary>
    public void Clear()
    {
      _Root = new SkipNode<T>(1);
    }

    /// <summary>
    /// Does the list contain the item?
    /// </summary>
    public bool Contains(T item)
    {
      var current = _Root;
      for (int i = current.Height - 1; i >= 0; i--)
      {
        //go either til we find it, or the last one at this level that is less than item
        while (current[i] != null)
        {
          int comp = Comparer.Compare(item, current[i].Value);
          if (comp < 0) break;
          if (comp == 0) return true;
          if (comp > 0)
            current = current[i];
        }
      }
      
      return false;
    }

    /// <summary>
    /// Copy all of the skiplist's contents into array, starting at array[arrayIndex]
    /// </summary>
    public void CopyTo(T[] array, int arrayIndex)
    {
      var current = _Root.Next();
      int i = arrayIndex;
      while (current != null)
      {
        array[i++] = current.Value;
        current = current.Next();
      }
    }

    /// <summary>
    /// How many elements do we have?
    /// </summary>
    public int Count { get; protected set; }

    public bool IsReadOnly
    {
      get { return false; }
    }

    #endregion

    #region IEnumerable<T> Members

    protected class SkipEnumerator : IEnumerator<T>
    {
      public SkipEnumerator (SkipList<T> list)
      {
        _Current = list._Root;
        _TheList = list;
      }

      private SkipNode<T> _Current;
      private SkipList<T> _TheList;
      

      #region Implementation of IDisposable

      /// <summary>
      /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
      /// </summary>
      /// <filterpriority>2</filterpriority>
      public void Dispose()
      {}

      #endregion

      #region Implementation of IEnumerator

      /// <summary>
      /// Advances the enumerator to the next element of the collection.
      /// </summary>
      /// <returns>
      /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
      /// </returns>
      /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
      public bool MoveNext()
      {
        bool result = (_Current.Next() != null);
        if (result)
          _Current = _Current.Next();
        return result;
      }

      /// <summary>
      /// Sets the enumerator to its initial position, which is before the first element in the collection.
      /// </summary>
      /// <exception cref="T:System.InvalidOperationException">The collection was modified after the enumerator was created. </exception><filterpriority>2</filterpriority>
      public void Reset()
      {
        _Current = _TheList._Root;
      }

      /// <summary>
      /// Gets the element in the collection at the current position of the enumerator.
      /// </summary>
      /// <returns>
      /// The element in the collection at the current position of the enumerator.
      /// </returns>
      public T Current
      {
        get { return _TheList._Root == _Current ? default(T) : _Current.Value; }
      }

      /// <summary>
      /// Gets the current element in the collection.
      /// </summary>
      /// <returns>
      /// The current element in the collection.
      /// </returns>
      /// <exception cref="T:System.InvalidOperationException">The enumerator is positioned before the first element of the collection or after the last element.</exception><filterpriority>2</filterpriority>
      object IEnumerator.Current
      {
        get { return Current; }
      }

      #endregion
    }

    public IEnumerator<T> GetEnumerator()
    {
      return new SkipEnumerator(this);
    }

    #endregion

    #region IEnumerable Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return new SkipEnumerator(this);
    }

    #endregion
  }
}

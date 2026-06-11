using ConsoleEngine.Data;
using ConsoleEngine.Services;

namespace ConsoleEngine.Properties
{
  public class Attributes
  {
    private Dictionary<string, object> attr;
    private Dictionary<string, Action<object?, object>> listeners;
    private Tile? _parentTile;
    private Entity? _parentEntity;
    public object? Parent
    {
      get
      {
        if (_parentTile == null && _parentEntity == null) { return null; }
        if (_parentTile == null) { return _parentEntity; }
        if (_parentEntity == null) { return _parentTile; }
        return null;
      }
    }
    public Attributes Value
    {
      get
      {
        var ToReturn = new Attributes(Parent);

        foreach (string key in attr.Keys)
        {
          ToReturn.Set(key, this.Get(key));
          if (this.listeners.ContainsKey(key))
          {
            ToReturn.listeners.Add(key, new Action<object?, object>(this.listeners[key]));
          }
        }
        return ToReturn;
      }
    }

    public Attributes()
    {
      attr = new Dictionary<string, object>();
      listeners = new Dictionary<string, Action<object?, object>>();
    }
    public Attributes(Tile parent) : this() => _parentTile = parent;
    public Attributes(Entity parent) : this() => _parentEntity = parent;
    public Attributes(object? nullParent) : this() { }

    // Methods

    public void Set(string key, object val)
    {
      if (!Exists(key)) { attr.Add(key, val); return; }
      attr[key] = val;
      if (listeners.ContainsKey(key))
      {
        listeners[key].Invoke(this.Parent, val);
      }
    }
    public bool Exists(string key)
    {
      return attr.ContainsKey(key);
    }
    public object? Get(string key)
    {
      return attr.GetValueOrDefault(key);
    }
    public void Remove(string key)
    {
      attr.Remove(key);
      if (listeners.ContainsKey(key))
      {
        listeners.Remove(key);
      }
    }
    public void ListenTo(string key, Action<object?, object> handler)
    {
      if (!Exists(key)) { throw new Exception($"Cannot listen to attribute '{key}' because it doesn't exist!"); }
      listeners.Add(key, handler);
    }
    public void DisconnectParent()
    {
      if (_parentEntity != null)
      {
        var oldParent = _parentEntity;
        _parentEntity = null;
        if (oldParent.Attributes == this) { oldParent.ResetAttributes(); }
      }
      else if (_parentTile != null)
      {
        var oldParent = _parentTile;
        _parentTile = null;
        if (oldParent.Attributes == this) { oldParent.ResetAttributes(); }
      }
    }

    public void ConnectParent(Tile parent)
    {
      if (this.Parent != null) { DisconnectParent(); }
      this._parentTile = parent;
      if (parent.Attributes != this) { parent.ConnectAttributes(this); }
    }
    public void ConnectParent(Entity parent)
    {
      if (this.Parent != null) { DisconnectParent(); }
      this._parentEntity = parent;
      if (parent.Attributes != this) { parent.ConnectAttributes(this); }
    }

  }

}
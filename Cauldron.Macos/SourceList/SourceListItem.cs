using System;
using System.Collections;
using System.Collections.Generic;
using AppKit;
using Foundation;

namespace Cauldron.Macos.SourceList
{
	public class SourceListItem : NSObject, IEnumerator, IEnumerable
	{
		#region Private Properties

		private string _title;
		private NSImage _icon;
		private string _tag;
		private bool _isHeader = false;
		private List<SourceListItem> _items = new();

		#endregion

		#region Computed Properties

		public string Title
		{
			get { return _title; }
			set { _title = value; }
		}

		public NSImage Icon
		{
			get { return _icon; }
			set { _icon = value; }
		}

		public string Tag
		{
			get { return _tag; }
			set { _tag = value; }
		}

		public bool IsHeader
		{
			get => this._isHeader;
			set => this._isHeader = value;
		}

		#endregion

		#region Indexer

		public SourceListItem this[int index]
		{
			get
			{
				return _items[index];
			}

			set
			{
				_items[index] = value;
			}
		}

		public int Count
		{
			get { return _items.Count; }
		}

		public bool HasChildren
		{
			get { return (Count > 0); }
		}

		#endregion

		#region Enumerable Routines

		private int _position = -1;

		public IEnumerator GetEnumerator()
		{
			_position = -1;
			return (IEnumerator)this;
		}

		public bool MoveNext()
		{
			_position++;
			return (_position < _items.Count);
		}

		public void Reset()
		{ _position = -1; }

		public object Current
		{
			get
			{
				try
				{
					return _items[_position];
				}

				catch (IndexOutOfRangeException)
				{
					throw new InvalidOperationException();
				}
			}
		}

		#endregion

		#region Constructors

		public SourceListItem() { }

		public SourceListItem(string title)
		{
			this._title = title;
		}

		public SourceListItem(string title, string icon)
		{
			this._title = title;
			this._icon = NSImage.ImageNamed(icon);
		}

		public SourceListItem(string title, string icon, ClickedDelegate clicked)
		{
			this._title = title;
			this._icon = NSImage.ImageNamed(icon);
			this.Clicked = clicked;
		}

		public SourceListItem(string title, NSImage icon)
		{
			this._title = title;
			this._icon = icon;
		}

		public SourceListItem(string title, NSImage icon, ClickedDelegate clicked)
		{
			this._title = title;
			this._icon = icon;
			this.Clicked = clicked;
		}

		public SourceListItem(string title, NSImage icon, string tag)
		{
			this._title = title;
			this._icon = icon;
			this._tag = tag;
		}

		public SourceListItem(string title, NSImage icon, string tag, ClickedDelegate clicked)
		{
			this._title = title;
			this._icon = icon;
			this._tag = tag;
			this.Clicked = clicked;
		}

		#endregion

		#region Public Methods

		public void AddItem(SourceListItem item)
		{
			_items.Add(item);
		}

		public void AddItem(string title)
		{
			_items.Add(new SourceListItem(title));
		}

		public void AddItem(string title, string icon)
		{
			_items.Add(new SourceListItem(title, icon));
		}

		public void AddItem(string title, string icon, ClickedDelegate clicked)
		{
			_items.Add(new SourceListItem(title, icon, clicked));
		}

		public void AddItem(string title, NSImage icon)
		{
			_items.Add(new SourceListItem(title, icon));
		}

		public void AddItem(string title, NSImage icon, ClickedDelegate clicked)
		{
			_items.Add(new SourceListItem(title, icon, clicked));
		}

		public void AddItem(string title, NSImage icon, string tag)
		{
			_items.Add(new SourceListItem(title, icon, tag));
		}

		public void AddItem(string title, NSImage icon, string tag, ClickedDelegate clicked)
		{
			_items.Add(new SourceListItem(title, icon, tag, clicked));
		}

		public void Insert(int n, SourceListItem item)
		{
			_items.Insert(n, item);
		}

		public void RemoveItem(SourceListItem item)
		{
			_items.Remove(item);
		}

		public void RemoveItem(int n)
		{
			_items.RemoveAt(n);
		}

		public void Clear()
		{
			_items.Clear();
		}

		#endregion

		#region Events

		public delegate void ClickedDelegate();
		public event ClickedDelegate Clicked;

		internal void RaiseClickedEvent()
		{
			this.Clicked?.Invoke();
		}

		#endregion
	}
}
using System;
using AppKit;
using Foundation;

namespace Cauldron.Macos.SourceList
{
	[Register("SourceListView")]
	public class SourceListView : NSOutlineView
	{
		#region Computed Properties

		public SourceListDataSource Data
		{
			get { return (SourceListDataSource)this.DataSource; }
		}

		#endregion

		#region Constructors

		public SourceListView() { }

		public SourceListView(IntPtr handle) : base(handle) { }

		public SourceListView(NSCoder coder) : base(coder) { }

		public SourceListView(NSObjectFlag t) : base(t) { }

		public SourceListView(ObjCRuntime.NativeHandle handle) : base(handle) { }

		#endregion

		#region Override Methods

		public override void AwakeFromNib()
		{
			base.AwakeFromNib();
		}

		#endregion

		#region Public Methods

		public void Initialize()
		{
			this.DataSource = new SourceListDataSource(this);
			this.Delegate = new SourceListDelegate(this);
		}

		public void AddItem(SourceListItem item)
		{
			Data?.Items.Add(item);
		}

		#endregion

		#region Events

		public delegate void ItemSelectedDelegate(SourceListItem item);
		public event ItemSelectedDelegate ItemSelected;

		internal void RaiseItemSelected(SourceListItem item)
		{
			this.ItemSelected?.Invoke(item);
		}

		#endregion
	}
}

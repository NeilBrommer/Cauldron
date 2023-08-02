using AppKit;
using CoreGraphics;
using Foundation;

namespace Cauldron.Macos.SourceList
{
	public class SourceListDelegate : NSOutlineViewDelegate
	{
		#region Private variables

		private SourceListView _controller;

		#endregion

		#region Constructors

		public SourceListDelegate(SourceListView controller)
		{
			this._controller = controller;
		}

		#endregion

		#region Override Methods

		public override bool ShouldEditTableColumn(NSOutlineView outlineView,
			NSTableColumn tableColumn, NSObject item)
		{
			return false;
		}

		public override NSCell GetCell(NSOutlineView outlineView, NSTableColumn tableColumn,
			NSObject item)
		{
			nint row = outlineView.RowForItem(item);
			return tableColumn.DataCellForRow(row);
		}

		public override bool IsGroupItem(NSOutlineView outlineView, NSObject item)
		{
			return ((SourceListItem)item).HasChildren;
		}

		public override NSView GetView(NSOutlineView outlineView, NSTableColumn tableColumn,
			NSObject item)
		{
			NSTableCellView view;

			// Is this a group item?
			if (((SourceListItem)item).IsHeader)
			{
				view = (NSTableCellView)outlineView.MakeView("HeaderCell", this);
			}
			else
			{
				view = (NSTableCellView)outlineView.MakeView("DataCell", this);
				view.ImageView.Image = ((SourceListItem)item).Icon;
				view.TextField.LineBreakMode = NSLineBreakMode.CharWrapping;
				view.TextField.UsesSingleLineMode = false;
				view.TextField.MaximumNumberOfLines = 0;
			}

			view.TextField.StringValue = ((SourceListItem)item).Title;
			view.TextField.SetBoundsSize(CalculateTextFieldHeight(view));

			return view;
		}

		public override bool ShouldSelectItem(NSOutlineView outlineView, NSObject item)
		{
			return (outlineView.GetParent(item) != null);
		}

		public override void SelectionDidChange(NSNotification notification)
		{
			NSIndexSet selectedIndexes = _controller.SelectedRows;

			// More than one item selected?
			if (selectedIndexes.Count > 1)
			{
				// Not handling this case
			}
			else
			{
				// Grab the item
				var item = _controller.Data.ItemForRow((int)selectedIndexes.FirstIndex);

				// Was an item found?
				if (item != null)
				{
					// Fire the clicked event for the item
					item.RaiseClickedEvent();

					// Inform caller of selection
					_controller.RaiseItemSelected(item);
				}
			}
		}

		private static CGSize CalculateTextFieldHeight(NSTableCellView cell)
		{
			CGRect rect = new(0, 0, cell.TextField.Bounds.Width, double.MaxValue);
			NSString str = new(cell.TextField.StringValue);
			CGRect bounds = str.BoundingRectWithSize(rect.Size, 0,
				new NSDictionary(NSStringAttributeKey.Font, cell.TextField.Font));

			return new CGSize(cell.TextField.Bounds.Width, bounds.Size.Height);
		}

		#endregion
	}
}

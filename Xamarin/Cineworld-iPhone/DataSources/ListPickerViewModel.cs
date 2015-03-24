using System;
using UIKit;
using System.Collections.Generic;

namespace CineworldiPhone
{
	public abstract class ListPickerViewModel<TItem> : UIPickerViewModel
	{
		public TItem SelectedItem { get; private set; }

		IList<TItem> _items;
		public IList<TItem> Items
		{
			get { return _items; }
			set { _items = value; }
		}

		public ListPickerViewModel()
		{
		}

		public ListPickerViewModel(IList<TItem> items)
		{
			Items = items;
		}

		public override nint GetRowsInComponent (UIPickerView pickerView, nint component)
		{
			if (NoItem())
				return 1;
			return Items.Count;
		}

		public override string GetTitle(UIPickerView picker, nint row, nint component)
		{
			int r = (int)row;
			if (NoItem(r))
				return "";
			var item = Items[r];
			return GetTitleForItem(item);
		}

		public override void Selected (UIPickerView pickerView, nint row, nint component)
		{
			int r = (int)row;

			if (NoItem(r))
				SelectedItem = default(TItem);
			else
				SelectedItem = Items[r];
		}

		public override nint GetComponentCount (UIPickerView pickerView)
		{
			
			return 1;
		}

		public virtual string GetTitleForItem(TItem item)
		{
			return item.ToString();
		}

		bool NoItem(int row = 0)
		{
			return Items == null || row >= Items.Count;
		}
	}
}


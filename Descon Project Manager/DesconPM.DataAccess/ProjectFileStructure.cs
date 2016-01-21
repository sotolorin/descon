using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Descon.Data;

namespace DesconPM.DataAccess
{
	public delegate void NotifyParentDelegate(EventArgs e);

	/// <summary>
	/// Complete Project File structure including all of the individual items
	/// </summary>
	[Serializable]
	public class ProjectFileStructure : INotifyPropertyChangedDescon
	{
		private ObservableCollection<DrawingItem> _drawingItems;
		private string _fileName;
		private DateTime _dateModified;

		public ProjectFileStructure()
		{
			DrawingItems = new ObservableCollection<DrawingItem>();
			DrawingItems.CollectionChanged += ItemCollectionChanged;
			OnPropertyChanged("SelectAllCheckBox");
		}

		#region Properties

		public string Name { get; set; }

		public string FileName
		{
			get { return _fileName; }
			set
			{
				_fileName = value;
				OnPropertyChanged("FileName");
				OnPropertyChanged("FileNameDisplay");
			}
		}

		public string FileNameDisplay
		{
			get
			{
				if (string.IsNullOrEmpty(_fileName))
					return string.Empty;
				else
					return _fileName.Split("\\".ToCharArray()).Last();
			}
		}

		public string ProjectNumber { get; set; }
		public DateTime DateCreated { get; set; }

		public DateTime DateModified
		{
			get { return _dateModified; }
			set
			{
				_dateModified = value;
				OnPropertyChanged("DateModified");
			}
		}

		public string Description { get; set; }

		/// <summary>
		/// Each drawing plus some additional data
		/// </summary>
		public ObservableCollection<DrawingItem> DrawingItems
		{
			get { return _drawingItems; }
			set
			{
				_drawingItems = value;
				OnPropertyChanged("DrawingItems");
			}
		}

		// Used to bind the Select All CheckBox in the grid header. Check box is triple state.
		public bool? SelectAllCheckBox
		{
			get
			{
				if (DrawingItems.Count == 0 || DrawingItems.All(i => !i.Checked))
					return false;
				else if (DrawingItems.All(i => i.Checked))
					return true;
				else
					return null;
			}
			set
			{
				foreach (var item in DrawingItems)
					item.Checked = value == true;
			}
		}

		#endregion

		/// <summary>
		/// Clears and resets all of the events when the DrawingItems collection is updated
		/// </summary>
		private void ItemCollectionChanged(object sender, EventArgs e)
		{
			// Clear all of the events
			foreach (var item in DrawingItems)
				item.NotifyParentEvent += null;
			// Add all of the events back again
			foreach (var item in DrawingItems)
				item.NotifyParentEvent += OnSelectChanged;

			OnPropertyChanged("SelectAllCheckBox");
		}

		/// <summary>
		/// Sets the master Select All checkbox depending on how many DrawingItems have been selected
		/// </summary>
		private void OnSelectChanged(EventArgs e)
		{
			OnPropertyChanged("SelectAllCheckBox");
		}
	}

	/// <summary>
	/// Individual drawings and related data
	/// </summary>
	[Serializable]
	public class DrawingItem : INotifyPropertyChangedDescon
	{
		private bool _checked;
		private int _index;

		/// <summary>
		/// Tells the parents something updated so the additional properties can reflect the updates. Used for the check boxes.
		/// </summary>
		[field: NonSerialized]
		public event NotifyParentDelegate NotifyParentEvent;
		private void NotifyParent()
		{
			if (NotifyParentEvent != null)
				NotifyParentEvent(new EventArgs());
		}

		/// <summary>
		/// Project Name, not file name
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Project Description
		/// </summary>
		public string Description { get; set; }

		public int Index
		{
			get { return _index; }
			set
			{
				_index = value;
				OnPropertyChanged("Index");
			}
		}

		public List<DetailData> DetailDataList { get; set; }

		public bool Checked
		{
			get { return _checked; }
			set
			{
				_checked = value;
				OnPropertyChanged("Checked");
				NotifyParent();
			}
		}
	}
}
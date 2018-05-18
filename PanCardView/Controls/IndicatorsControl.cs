﻿using PanCardView.Behaviors;
using PanCardView.Extensions;
using System.Collections;
using System.Linq;
using Xamarin.Forms;
using static PanCardView.Controls.Styles.DefaultIndicatorItemStyles;
using static System.Math;

namespace PanCardView.Controls
{
	public class IndicatorsControl : StackLayout
	{
		public readonly BindableProperty CurrentIndexProperty = BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(IndicatorsControl), 0, BindingMode.TwoWay, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsIndicatorsControl().ResetIndicatorsStyles();
		});

		public readonly BindableProperty ItemsCountProperty = BindableProperty.Create(nameof(ItemsCount), typeof(int), typeof(IndicatorsControl), 0, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsIndicatorsControl().ResetIndicatorsCount((int)oldValue, (int)newValue);
		});

		public readonly BindableProperty SelectedIndicatorStyleProperty = BindableProperty.Create(nameof(SelectedIndicatorStyle), typeof(Style), typeof(IndicatorsControl), DefaultSelectedIndicatorItemStyle, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsIndicatorsControl().ResetIndicatorsStyles();
		});

		public readonly BindableProperty UnselectedIndicatorStyleProperty = BindableProperty.Create(nameof(UnselectedIndicatorStyle), typeof(Style), typeof(IndicatorsControl), DefaultUnselectedIndicatorItemStyle, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsIndicatorsControl().ResetIndicatorsStyles();
		});

		public readonly BindableProperty UseCardItemsAsIndicatorsBindingContextsProperty = BindableProperty.Create(nameof(UseCardItemsAsIndicatorsBindingContexts), typeof(bool), typeof(IndicatorsControl), true, propertyChanged: (bindable, oldValue, newValue) =>
		{
			bindable.AsIndicatorsControl().ResetIndicatorsContexts();
		});

		public static readonly BindableProperty IndicatorsContextsProperty = BindableProperty.Create(nameof(IndicatorsContexts), typeof(IList), typeof(IndicatorsControl), null);

		public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(IndicatorsControl), new DataTemplate(typeof(IndicatorItemView)));

		public readonly BindableProperty UseParentAsBindingContextProperty = BindableProperty.Create(nameof(UseParentAsBindingContext), typeof(bool), typeof(IndicatorsControl), true);

		static IndicatorsControl()
		{
		}

		private readonly TapGestureRecognizer _itemTapGesture;

		public IndicatorsControl()
		{
			Spacing = 5;
			Orientation = StackOrientation.Horizontal;

			this.SetBinding(CurrentIndexProperty, nameof(CardsView.CurrentIndex));
			this.SetBinding(ItemsCountProperty, nameof(CardsView.ItemsCount));
			this.SetBinding(IndicatorsContextsProperty, nameof(CardsView.Items));

			Margin = new Thickness(10, 20);
			AbsoluteLayout.SetLayoutBounds(this, new Rectangle(.5, 1, -1, -1));
			AbsoluteLayout.SetLayoutFlags(this, AbsoluteLayoutFlags.PositionProportional);

			Behaviors.Add(new ProtectedControlBehavior());

			_itemTapGesture = new TapGestureRecognizer();
			_itemTapGesture.Tapped += (tapSender, tapArgs) =>
			{
				CurrentIndex = IndexOf(tapSender as View);
			};
		}

		public int CurrentIndex
		{
			get => (int)GetValue(CurrentIndexProperty);
			set => SetValue(CurrentIndexProperty, value);
		}

		public int ItemsCount
		{
			get => (int)GetValue(ItemsCountProperty);
			set => SetValue(ItemsCountProperty, value);
		}

		public Style SelectedIndicatorStyle
		{
			get => GetValue(SelectedIndicatorStyleProperty) as Style;
			set => SetValue(SelectedIndicatorStyleProperty, value);
		}

		public Style UnselectedIndicatorStyle
		{
			get => GetValue(UnselectedIndicatorStyleProperty) as Style;
			set => SetValue(UnselectedIndicatorStyleProperty, value);
		}

		public IList IndicatorsContexts
		{
			get => GetValue(IndicatorsContextsProperty) as IList;
			set => SetValue(IndicatorsContextsProperty, value);
		}

		public DataTemplate ItemTemplate
		{
			get => GetValue(ItemTemplateProperty) as DataTemplate;
			set => SetValue(ItemTemplateProperty, value);
		}

		public bool UseParentAsBindingContext
		{
			get => (bool)GetValue(UseParentAsBindingContextProperty);
			set => SetValue(UseParentAsBindingContextProperty, value);
		}

		public bool UseCardItemsAsIndicatorsBindingContexts
		{
			get => (bool)GetValue(UseCardItemsAsIndicatorsBindingContextsProperty);
			set => SetValue(UseCardItemsAsIndicatorsBindingContextsProperty, value);
		}

		protected override void OnParentSet()
		{
			base.OnParentSet();
			if (UseParentAsBindingContext)
			{
				BindingContext = Parent;
			}
		}

		protected virtual void ApplySelectedStyle(View view, int index)
		=> view.Style = SelectedIndicatorStyle;

		protected virtual void ApplyUnselectedStyle(View view, int index)
		=> view.Style = UnselectedIndicatorStyle;

		protected virtual int IndexOf(View view) => Children.IndexOf(view);

		protected virtual void OnResetIndicatorsStyles(int currentIndex)
		{
			foreach (var child in Children)
			{
				ApplyStyle(child, currentIndex);
			}
		}

		protected virtual void OnResetIndicatorsContexts()
		{
			for (var i = 0; i < Min(Children.Count, ItemsCount); ++i)
			{
				Children[i].BindingContext = IndicatorsContexts[i];
			}
		}

		protected virtual void AddExtraIndicatorsItems()
		{
			var oldCount = Children.Count;
			for (var i = 0; i < ItemsCount - oldCount; ++i)
			{
				var item = ItemTemplate.CreateView();
				AddItemTapGesture(item);
				Children.Add(item);
			}
		}

		protected virtual void RemoveRedundantIndicatorsItems()
		{
			foreach (var item in Children.Where((v, i) => i >= ItemsCount).ToArray())
			{
				Children.Remove(item);
				item.BindingContext = null;
			}
		}

		private void ApplyStyle(View view, int cyclingIndex)
		{
			try
			{
				view.BatchBegin();
				if (IndexOf(view) == cyclingIndex)
				{
					ApplySelectedStyle(view, cyclingIndex);
					return;
				}
				ApplyUnselectedStyle(view, cyclingIndex);
			}
			finally
			{
				view.BatchCommit();
			}
		}

		private void ResetIndicatorsStylesNonBatch()
		{
			var cyclingIndex = CurrentIndex.ToCyclingIndex(ItemsCount);
			OnResetIndicatorsStyles(cyclingIndex);
		}

		private void ResetIndicatorsStyles()
		{
			try
			{
				BatchBegin();
				ResetIndicatorsStylesNonBatch();
			}
			finally
			{
				BatchCommit();
			}
		}

		private void ResetIndicatorsCount(int oldValue, int newValue)
		{
			try
			{
				BatchBegin();
				if (oldValue < 0)
				{
					oldValue = 0;
				}

				if (oldValue > newValue)
				{
					RemoveRedundantIndicatorsItems();
					return;
				}

				AddExtraIndicatorsItems();
				ResetIndicatorsContexts();
			}
			finally
			{
				ResetIndicatorsStylesNonBatch();
				BatchCommit();
			}
		}

		private void ResetIndicatorsContexts()
		{
			if (UseCardItemsAsIndicatorsBindingContexts && IndicatorsContexts != null)
			{
				OnResetIndicatorsContexts();
			}
		}

		private void AddItemTapGesture(View view)
		{
			var gestures = view.GestureRecognizers;
			if (!gestures.Contains(_itemTapGesture))
			{
				view.GestureRecognizers.Add(_itemTapGesture);
			}
		}
	}
}
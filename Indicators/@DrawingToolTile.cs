﻿// 
// Copyright (C) 2022, NinjaTrader LLC <ninjatrader@ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using NinjaTrader.Gui;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Windows.Data;
using NinjaTrader.Cbi;
using NinjaTrader.Gui.Chart;
#endregion

namespace NinjaTrader.NinjaScript.Indicators
{
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.DrawingToolIndicatorTypeConverter")]
	[CategoryOrder(typeof(Custom.Resource), "NinjaScriptParameters", 1)]
	[CategoryOrder(typeof(Resource), "PropertyCategoryDataSeries", 2)]
	[CategoryOrder(typeof(Resource), "NinjaScriptSetup", 3)]
	[CategoryOrder(typeof(Custom.Resource), "NinjaScriptDrawingTools", 4)]
	[CategoryOrder(typeof(Custom.Resource), "NinjaScriptIndicatorVisualGroup", 5)]
	[CategoryExpanded(typeof(Custom.Resource), "NinjaScriptDrawingTools", false)]
	public class DrawingToolTile : Indicator
	{
		private		Border		b;
		private		Grid		grid;
		private		Thickness	margin;
		private		bool		subscribedToSize;
		private		Point		startPoint;

		protected override void OnBarUpdate()
		{
			if (!subscribedToSize && ChartPanel != null)
			{
				subscribedToSize = true;

				ChartPanel.SizeChanged += (o, e) =>
				{
					if (grid == null || ChartPanel == null)
						return;
					if (grid.Margin.Left + grid.ActualWidth > ChartPanel.ActualWidth || grid.Margin.Top + grid.ActualHeight > ChartPanel.ActualHeight)
					{
						double left	= Math.Max(0, Math.Min(grid.Margin.Left, ChartPanel.ActualWidth - grid.ActualWidth));
						double top	= Math.Max(0, Math.Min(grid.Margin.Top, ChartPanel.ActualHeight - grid.ActualHeight));
						grid.Margin	= new Thickness(left, top, 0, 0);
						Left		= left;
						Top			= top;
					}
				};
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name							= Custom.Resource.DrawingToolIndicatorName;
				Description						= Custom.Resource.DrawingToolIndicatorDescription;
				IsOverlay						= true;
				IsChartOnly						= true;
				DisplayInDataBox				= false;
				PaintPriceMarkers				= false;
				IsSuspendedWhileInactive		= true;
				SelectedTypes					= new XElement("SelectedTypes");

				foreach (Type type in new[]
				{
					typeof(DrawingTools.Ellipse), typeof(DrawingTools.ExtendedLine),
					typeof(DrawingTools.FibonacciExtensions), typeof(DrawingTools.FibonacciRetracements),
					typeof(DrawingTools.HorizontalLine), typeof(DrawingTools.Line),
					typeof(DrawingTools.Ray), typeof(DrawingTools.Rectangle), typeof(DrawingTools.Text), typeof(DrawingTools.VerticalLine)
				})
				{
					XElement	el				= new XElement(type.FullName);
					el.Add(new XAttribute("Assembly", "NinjaTrader.Custom"));
					SelectedTypes.Add(el);
				}
				Left			= 5;
				Top				= -1;
				NumberOfRows	= 5;
			}
			else if (State == State.Historical)
			{
				if (IsVisible)
				{
					if (ChartControl != null)
					{
						if (Top < 0)
							Top = 25;

						ChartControl.Dispatcher.InvokeAsync(() => { UserControlCollection.Add(CreateControl()); });
					}
				}
			}
		}

		private FrameworkElement CreateControl()
		{
			if (grid != null)
				return grid;

			grid = new Grid { VerticalAlignment = VerticalAlignment.Top, HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(Left, Top, 0, 0) };

			grid.ColumnDefinitions	.Add(new ColumnDefinition	{ Width		= new GridLength() });
			grid.ColumnDefinitions	.Add(new ColumnDefinition	{ Width		= new GridLength() });
			grid.RowDefinitions		.Add(new RowDefinition		{ Height	= new GridLength() });

			Brush	background	= Application.Current.FindResource("BackgroundMainWindow")	as Brush ?? Brushes.White;
			Brush	borderBrush	= Application.Current.FindResource("BorderThinBrush")		as Brush ?? Brushes.Black;

			Grid	g			= new Grid();
			g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });
			g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
			g.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star) });

			for (int r = 0; r < g.RowDefinitions.Count; r++)
			{
				System.Windows.Shapes.Ellipse e = new System.Windows.Shapes.Ellipse
				{
					Width				= 3,
					Height				= 3,
					HorizontalAlignment	= HorizontalAlignment.Center,
					VerticalAlignment	= VerticalAlignment.Center,
					Fill				= borderBrush
				};
				Grid.SetRow(e, r);
				g.Children.Add(e);
			}
			
			b = new Border
			{
				VerticalAlignment	= VerticalAlignment.Top,
				BorderThickness		= new Thickness(0, 1, 1, 1),
				BorderBrush			= borderBrush,
				Background			= background,
				Width				= 12,
				Height				= 24,
				Cursor				= System.Windows.Input.Cursors.Hand,
				Child				= g
			};

			b.MouseDown += (o, e) =>
			{
				startPoint	= e.GetPosition(ChartPanel);
				margin		= grid.Margin;
				if (e.ClickCount > 1)
				{
					b.ReleaseMouseCapture();
					ChartControl.OnIndicatorsHotKey(this, null);
				}
				else
					b.CaptureMouse();
			};

			b.MouseUp += (o, e) => { b.ReleaseMouseCapture(); };

			b.MouseMove += (o, e) =>
			{
				if (!b.IsMouseCaptured || grid == null || ChartPanel == null)
					return;

				Point newPoint	= e.GetPosition(ChartPanel);
				grid.Margin		= new Thickness	{
													Left	= Math.Max(0, Math.Min(margin.Left	+ (newPoint.X - startPoint.X), ChartPanel.ActualWidth	- grid.ActualWidth)),
													Top		= Math.Max(0, Math.Min(margin.Top	+ (newPoint.Y - startPoint.Y), ChartPanel.ActualHeight	- grid.ActualHeight))
												};

				Left			= grid.Margin.Left;
				Top				= grid.Margin.Top;
			};

			Grid.SetColumn(b, 1);

			grid.Children.Add(b);

			Grid			contentGrid		= new Grid();
			List<XElement>	elements		= SortElements(XElement.Parse(SelectedTypes.ToString()));
			int				column			= 0;
			int				count			= 0;
			FontFamily		fontFamily		= Application.Current.Resources["IconsFamily"] as FontFamily;
			Style			style			= Application.Current.Resources["LinkButtonStyle"] as Style;

			while (count < elements.Count)
			{
				if (contentGrid.ColumnDefinitions.Count <= column)
					contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star)});
				for (int j = 0; j < NumberOfRows && count < elements.Count; j++)
				{
					if (contentGrid.RowDefinitions.Count <= j)
						contentGrid.RowDefinitions.Add(new RowDefinition {Height = new GridLength(1, GridUnitType.Auto)});
					XElement element = elements[count];
					try
					{
						DrawingTools.DrawingTool dt = Core.Globals.AssemblyRegistry[element.Attribute("Assembly").Value].CreateInstance(element.Name.ToString()) as DrawingTools.DrawingTool;
						if (dt != null && dt.DisplayOnChartsMenus)
						{
							Button bb = new Button
							{
								Content		= dt.Icon ?? Gui.Tools.Icons.DrawPencil,
								ToolTip		= dt.DisplayName,
								Style		= style,
								FontFamily	= fontFamily,
								FontSize	= 16,
								FontStyle	= FontStyles.Normal,
								Margin		= new Thickness(3),
								Padding		= new Thickness(3)
							};

							Grid.SetRow(bb, j);
							Grid.SetColumn(bb, column);

							bb.Click += (sender, args) =>
							{
								if (ChartControl != null)
									ChartControl.TryStartDrawing(dt.GetType().FullName);
							};

							contentGrid.Children.Add(bb);
							//tb.Items.Add(bb);
							count++;
						}
						else
						{
							elements.RemoveAt(j);
							j--;
						}
					}
					catch (Exception e)
					{
						elements.RemoveAt(j);
						j--;
						Cbi.Log.Process(typeof(Custom.Resource), "NinjaScriptTileError", new object[] { element.Name.ToString(), e }, LogLevel.Error, LogCategories.NinjaScript);
					}
				}
				column++;
			}

			Border tileHolder	= new Border
			{
				Cursor				= System.Windows.Input.Cursors.Arrow,
				Background			= Application.Current.FindResource("BackgroundMainWindow")as Brush,
				BorderThickness		= new Thickness ((double)(Application.Current.FindResource("BorderThinThickness") ?? 1)),
				BorderBrush			= Application.Current.FindResource("BorderThinBrush")as Brush,
				Child				= contentGrid
			};

			grid.Children.Add(tileHolder);

			if (IsVisibleOnlyFocused)
			{
				Binding binding = new Binding("IsActive") { Source = ChartControl.OwnerChart, Converter = Application.Current.FindResource("BoolToVisConverter") as IValueConverter};
				grid.SetBinding(UIElement.VisibilityProperty, binding);
			}

			return grid;
		}

		public override void CopyTo(NinjaScript ninjaScript)
		{
			DrawingToolTile dti = ninjaScript as DrawingToolTile;
			if (dti != null)
			{
				dti.Left	= Left;
				dti.Top		= Top;
			}
			base.CopyTo(ninjaScript);
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale) { }

		private List<XElement> SortElements(XElement elements)
		{
			string[] ordered =	{
									typeof(DrawingTools.Ruler)					.FullName,
									typeof(DrawingTools.RiskReward)				.FullName,
									typeof(DrawingTools.RegionHighlightX)		.FullName,
									typeof(DrawingTools.RegionHighlightY)		.FullName,
									typeof(DrawingTools.Line)					.FullName,
									typeof(DrawingTools.Ray)					.FullName,
									typeof(DrawingTools.ExtendedLine)			.FullName,
									typeof(DrawingTools.ArrowLine)				.FullName,
									typeof(DrawingTools.HorizontalLine)			.FullName,
									typeof(DrawingTools.VerticalLine)			.FullName,
									typeof(DrawingTools.PathTool)				.FullName,
									typeof(DrawingTools.FibonacciRetracements)	.FullName,
									typeof(DrawingTools.FibonacciExtensions)	.FullName,
									typeof(DrawingTools.FibonacciTimeExtensions).FullName,
									typeof(DrawingTools.FibonacciCircle)		.FullName,
									typeof(DrawingTools.AndrewsPitchfork)		.FullName,
									typeof(DrawingTools.GannFan)				.FullName,
									typeof(DrawingTools.RegressionChannel)		.FullName,
									typeof(DrawingTools.TrendChannel)			.FullName,
									typeof(DrawingTools.TimeCycles)				.FullName,
									typeof(DrawingTools.Ellipse)				.FullName,
									typeof(DrawingTools.Rectangle)				.FullName,
									typeof(DrawingTools.Triangle)				.FullName,
									typeof(DrawingTools.Polygon)				.FullName,
									"NinjaTrader.NinjaScript.DrawingTools.OrderFlowVolumeProfile",
									typeof(DrawingTools.Arc)					.FullName,
									typeof(DrawingTools.Text)					.FullName,
									typeof(DrawingTools.ArrowUp)				.FullName,
									typeof(DrawingTools.ArrowDown)				.FullName,
									typeof(DrawingTools.Diamond)				.FullName,
									typeof(DrawingTools.Dot)					.FullName,
									typeof(DrawingTools.Square)					.FullName,
									typeof(DrawingTools.TriangleUp)				.FullName,
									typeof(DrawingTools.TriangleDown)			.FullName
								};

			List<XElement> ret = new List<XElement>();
			foreach (string s in ordered)
			{
				XElement c = elements.Element(s);
				if (c != null)
				{
					ret.Add(XElement.Parse(c.ToString()));
					c.Remove();
				}
			}

			foreach (XElement element in elements.Elements())
				ret.Add(element);

			return ret;
		}

		#region Properties

		[Browsable(false)]
		public double Top { get; set; }
		[Browsable(false)]
		public double Left { get; set; }

		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptIsVisibleOnlyFocused", GroupName = "NinjaScriptIndicatorVisualGroup", Order = 499)]
		public bool IsVisibleOnlyFocused { get; set; }

		public XElement SelectedTypes { get; set; }
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptNumberOfRows", GroupName = "NinjaScriptParameters", Order = 0)]
		public int NumberOfRows { get; set; }
		#endregion
	}

	public class DrawingToolPropertyDescriptor : PropertyDescriptor
	{
		private readonly string		displayName;
		private readonly string		name;
		private readonly int		order;
		private readonly Type		type;

		public override AttributeCollection Attributes
		{
			get
			{
				Attribute[] attr	= new Attribute[1];
				attr[0]				= new DisplayAttribute { Name = DisplayName, GroupName = Custom.Resource.NinjaScriptDrawingTools, Order = order };

				return new AttributeCollection(attr);
			}
		}

		public DrawingToolPropertyDescriptor(Type type, string displayName, int order) : base(type.FullName, null)
		{
			name					= type.FullName;
			this.displayName		= displayName;
			this.order				= order;
			this.type				= type;
		}

		public	override	Type	ComponentType							{ get { return typeof (DrawingToolTile); } }
		public	override	string	DisplayName								{ get { return displayName; } }
		public	override	bool	IsReadOnly								{ get { return false; } }
		public	override	string	Name									{ get { return name; } }
		public	override	Type	PropertyType							{ get { return typeof (bool); } }

		public	override	bool	CanResetValue(object component)			{ return true; }
		public	override	bool	ShouldSerializeValue(object component)	{ return true; }

		public	override	object	GetValue(object component)
		{
			DrawingToolTile c = component as DrawingToolTile;

			return c != null && c.SelectedTypes.Element(Name) != null;
		}

		public override void ResetValue(object component) { }

		public override void SetValue(object component, object value)
		{
			DrawingToolTile c = component as DrawingToolTile;
			if (c == null)
				return;
			bool val = (bool) value;

			if (val && c.SelectedTypes.Element(Name) == null)
			{
				XElement toAdd = new XElement(Name);
				toAdd.Add(new XAttribute("Assembly", Core.Globals.AssemblyRegistry.IsNinjaTraderCustomAssembly(type) ? "NinjaTrader.Custom" : type.Assembly.GetName().Name));
				c.SelectedTypes.Add(toAdd);
			}
			else if(!val && c.SelectedTypes.Element(Name) != null)
				c.SelectedTypes.Element(Name).Remove();
		}
	}

	public class DrawingToolIndicatorTypeConverter : TypeConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			TypeConverter					tc								= component is IndicatorBase ? TypeDescriptor.GetConverter(typeof(IndicatorBase)) : TypeDescriptor.GetConverter(typeof(DrawingTools.DrawingTool));
			PropertyDescriptorCollection	propertyDescriptorCollection	= tc.GetProperties(context, component, attrs);

			if (propertyDescriptorCollection == null) 
				return null;

			PropertyDescriptorCollection properties	= new PropertyDescriptorCollection(null);

			foreach (PropertyDescriptor pd in propertyDescriptorCollection)
			{
				if (!pd.IsBrowsable || pd.IsReadOnly) continue;

				if (pd.Name == "IsAutoScale"	|| pd.Name == "DisplayInDataBox"	|| pd.Name == "MaximumBarsLookBack"	|| pd.Name == "Calculate"
												|| pd.Name == "PaintPriceMarkers"	|| pd.Name == "Displacement"		|| pd.Name == "ScaleJustification")
					continue;

				if (pd.Name == "SelectedTypes")
				{
					int i = 1;
					foreach (Type type in Core.Globals.AssemblyRegistry.GetDerivedTypes(typeof(DrawingTools.DrawingTool)))
					{
						DrawingTools.DrawingTool tool = type.Assembly.CreateInstance(type.FullName) as DrawingTools.DrawingTool;
						if (tool == null || !tool.DisplayOnChartsMenus) continue;
						DrawingToolPropertyDescriptor descriptor = new DrawingToolPropertyDescriptor(type, tool.Name, i);
						properties.Add(descriptor);
						i++;
					}
					continue;
				}

				properties.Add(pd);
			}
			return properties;
		}
	}
}
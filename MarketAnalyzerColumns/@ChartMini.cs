using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	[TypeConverter("NinjaTrader.NinjaScript.MarketAnalyzerColumns.ChartMiniConverter")]
	public class ChartMini : Gui.NinjaScript.MarketAnalyzerColumnRenderBase
	{
		private Brush	color;
		private Brush	fillBrush;
		private Pen		linePen;
		private int		opacity;
		private Brush	outlineBrush;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionChartMini;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameChartMini;
				Color					= Brushes.DimGray;
				IsDataSeriesRequired	= true;
				Opacity					= 50;
				OutlineBrush			= Brushes.DimGray;
				Span					= ChartSpan.Day;
			}
			else if (State == State.Configure)
			{
				BarsPeriod				= Span == ChartSpan.Day || Span == ChartSpan.Week ? new Data.BarsPeriod { BarsPeriodType = Data.BarsPeriodType.Minute, Value = 1 } : Span == ChartSpan.Month || Span == ChartSpan.Year ? new Data.BarsPeriod { BarsPeriodType = Data.BarsPeriodType.Day, Value = 1 } : new Data.BarsPeriod { BarsPeriodType = Data.BarsPeriodType.Second, Value = 1 };
				DaysBack				= Span == ChartSpan.Week ? 7 : Span == ChartSpan.Month ? 30 : Span == ChartSpan.Year ? 365 : 1;
				RangeType				= Cbi.RangeType.Days;
				TradingHoursInstance	= Data.TradingHours.All.FirstOrDefault(s => s.Name == Data.TradingHours.SystemDefault);
				To						= Now.Date;
			}
		}

		public override string Format(double value)
		{
			return (value == double.MinValue || Instrument == null ? string.Empty : Instrument.MasterInstrument.FormatPrice(value));
		}

		private DateTime Now
		{
			get { return Cbi.Connection.PlaybackConnection != null ? Cbi.Connection.PlaybackConnection.Now : Core.Globals.Now; }
		}

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
				CurrentValue = double.MinValue;
			else if (marketDataUpdate.MarketDataType == Data.MarketDataType.Last)
				CurrentValue = marketDataUpdate.Price;
		}

		public override void OnRender(DrawingContext dc, System.Windows.Size renderSize)
		{
			DateTime minTime;
			switch (Span)
			{
				case ChartSpan.Min1:	minTime = Now.AddMinutes(-1);		break;
				case ChartSpan.Min5:	minTime = Now.AddMinutes(-5);		break;
				case ChartSpan.Min15:	minTime = Now.AddMinutes(-15);		break;
				case ChartSpan.Min30:	minTime = Now.AddMinutes(-30);		break;
				case ChartSpan.Min60:	minTime = Now.AddMinutes(-60);		break;
				case ChartSpan.Min240:	minTime = Now.AddMinutes(-240);	break;
				case ChartSpan.Week:	minTime = Now.AddDays(-7);			break;
				case ChartSpan.Month:	minTime = Now.Date.AddMonths(-1);	break;
				case ChartSpan.Year:	minTime = Now.Date.AddYears(-1);	break;
				default:
				case ChartSpan.Day:		minTime = Now.AddDays(-1);			break;
			}

			if (fillBrush == null)
				fillBrush = new SolidColorBrush() { Color = (Color as SolidColorBrush).Color, Opacity = (double) Opacity / 100 };
			if (linePen == null)
				linePen = new Pen(OutlineBrush, 1);

			int						firstIdx		= Math.Max(0, BarsArray[0].GetBar(minTime) - 1);
			List<LineSegment>		lineSegments	= new List<LineSegment>();
			int						margin			= 1;
			double					maxCloseOnX		= double.MinValue;
			double					maxPrice		= double.MinValue;
			DateTime				maxTime			= Now;
			int						maxX			= -1;
			double					minPrice		= double.MaxValue;
			int						minX			= margin;
			int						netHeight		= (int) Math.Floor(renderSize.Height) - margin;
			int						netWidth		= (int) Math.Floor(renderSize.Width) - 2 * margin;
			int						prevX			= -1;
			System.Windows.Point	startPoint		= new System.Windows.Point();
			DateTime				time0			= minTime.Date.AddDays(-1);
			int						x0				= (int) - (maxTime.Subtract(time0).TotalMinutes / Math.Max(Math.Round(maxTime.Subtract(minTime).TotalMinutes), 1) * netWidth) + netWidth + minX;

			BarsArray[0].BarsSeries.SyncRoot.EnterReadLock();
			try
			{
				for (int idx = firstIdx + 1; idx < BarsArray[0].Count; idx++)
				{
					double close = BarsArray[0].GetClose(idx);
					minPrice = Math.Min(close, minPrice);
					maxPrice = Math.Max(close, maxPrice);
				}

				if (minPrice == double.MaxValue || maxPrice == double.MinValue)
					return;

				for (int idx = firstIdx; idx < BarsArray[0].Count; idx++)
				{
					DateTime	time	= new DateTime(Math.Min(Math.Max(BarsArray[0].GetTime(idx).Ticks, minTime.Ticks), maxTime.Ticks));
					double		close	= idx == firstIdx ? Math.Max(minPrice, Math.Min(BarsArray[0].GetClose(idx), maxPrice)) : BarsArray[0].GetClose(idx);
					int			x		= idx == firstIdx ? margin : x0 + Convert.ToInt32(time.Subtract(time0).TotalSeconds * (netWidth / Math.Max(maxTime.Subtract(minTime).TotalSeconds, 1)));

					if (x == prevX)
					{
						maxCloseOnX	= Math.Max(close, maxCloseOnX);
						close		= maxCloseOnX;
					}
					else
						maxCloseOnX = close;

					int y = margin + Convert.ToInt32(((maxPrice - close) / Math.Max(BarsArray[0].Instrument.MasterInstrument.TickSize, maxPrice - minPrice)) * (netHeight - margin));
					if (idx == firstIdx)
						startPoint = new System.Windows.Point(margin, y);
					if (idx == BarsArray[0].Count - 1)
						maxX = x;
					if (x == prevX)
						lineSegments[lineSegments.Count - 1] = new LineSegment(new System.Windows.Point(x, y), true);
					else
						lineSegments.Add(new LineSegment(new System.Windows.Point(x, y), true));
					prevX = x;
				}
			}
			finally
			{
				BarsArray[0].BarsSeries.SyncRoot.ExitReadLock();
			}

			if (lineSegments.Count > 0)
			{
				List<PathFigure> pathFiguresOut = new List<PathFigure>();
				pathFiguresOut.Add(new PathFigure(startPoint, lineSegments.ToArray(), false));
				PathGeometry pgOut = new PathGeometry(pathFiguresOut);
				dc.DrawGeometry(null, linePen, pgOut);

				lineSegments.Add(new LineSegment(new System.Windows.Point(maxX, margin + netHeight), true));
				lineSegments.Add(new LineSegment(new System.Windows.Point(minX, margin + netHeight), true));

				List<PathFigure> pathFiguresFill = new List<PathFigure>();
				pathFiguresFill.Add(new PathFigure(startPoint, lineSegments.ToArray(), true));
				PathGeometry pgFill = new PathGeometry(pathFiguresFill);
				dc.DrawGeometry(fillBrush, null, pgFill);
			}
		}

		#region Properties
		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartMiniColor", GroupName = "GuiPropertyCategoryVisual", Order = 10)]
		public Brush Color
		{
			get { return  color; }
			set { color = value; fillBrush = null; }
		}

		[Range(0, 100)]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartMiniOpacity", GroupName = "GuiPropertyCategoryVisual", Order = 20)]
		public int Opacity
		{
			get { return opacity;}
			set { opacity = value; fillBrush = null; }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartMiniOutline", GroupName = "GuiPropertyCategoryVisual", Order = 30)]
		public Brush OutlineBrush
		{
			get { return outlineBrush; }
			set { outlineBrush = value; linePen = null; }
		}

		[Browsable(false)]
		public string OutlineBrushSeralizer { get { return Serialize.BrushToString(OutlineBrush); } set { OutlineBrush = Serialize.StringToBrush(value); }}

		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartMiniSpan", GroupName = "NinjaScriptSetup", Order = 300)]
		public ChartSpan Span { get; set; }

		[Browsable(false)]
		public string UpBrushSeralizer { get { return Serialize.BrushToString(Color); } set { Color = Serialize.StringToBrush(value); }}
		#endregion
	}

	public class ChartMiniConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			ChartMini chartMini = component as ChartMini;
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, component, attrs) : TypeDescriptor.GetProperties(component, attrs);
			if (chartMini == null || propertyDescriptorCollection == null)
				return propertyDescriptorCollection;

			PropertyDescriptorCollection ret = new PropertyDescriptorCollection(null);
			foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
			{
				if (propertyDescriptor.Name != "Color" && propertyDescriptor.Name != "Opacity" && propertyDescriptor.Name != "OutlineBrush" 
					&& propertyDescriptor.Name != "Span" && propertyDescriptor.Name != "Name" && propertyDescriptor.Name != "IsVisible")
					continue;

				ret.Add(propertyDescriptor);
			}

			return ret;
		}
	}

	[TypeConverter("NinjaTrader.Custom.ResourceEnumConverter")]
	public enum ChartSpan
	{
		Min1	= 0,
		Min5	= 1,
		Min15	= 2,
		Min30	= 3,
		Min60	= 4,
		Min240	= 5,
		Day		= 6,
		Week	= 7,
		Month	= 8,
		Year	= 9,
	};
}

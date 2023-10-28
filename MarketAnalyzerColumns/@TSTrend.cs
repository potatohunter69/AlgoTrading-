// 
// Copyright (C) 2022, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
#endregion

//This namespace holds Market Analyzer columns in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	[TypeConverter("NinjaTrader.NinjaScript.MarketAnalyzerColumns.TSTrendConverter")]
	public class TSTrend : Gui.NinjaScript.MarketAnalyzerColumnRenderBase
	{
		private Brush					aboveAsk;
		private Brush					atAsk;
		private Brush					atBid;
		private Brush					belowBid;
		private Brush					between;
		private double					lastAsk			= double.MinValue;
		private double					lastBid			= double.MinValue;
		private int						margin			= 1;
		private int						maxSlots		= 10;
		private List<TrendValue>		slots			= new List<TrendValue>();
		private List<Tuple<Brush, Pen>> trendColors		= null;


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionTSTrend;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameTSTrend;
				AboveAsk				= Brushes.Gold;
				AtAsk					= Brushes.ForestGreen;
				AtBid					= Brushes.Chocolate;
				BelowBid				= Brushes.DeepPink;
				Between					= Brushes.Sienna;
				BarWidth				= 4;
				IsDataSeriesRequired	= false;
			}
		}

		private List<Tuple<Brush, Pen>> TrendColors
		{
			get
			{
				if (trendColors == null)
				{
					trendColors = new List<Tuple<Brush, Pen>>();
					trendColors.Add(new Tuple<Brush, Pen>(AboveAsk,	new Pen(AboveAsk, 1)));
					trendColors.Add(new Tuple<Brush, Pen>(AtAsk,	new Pen(AtAsk, 1)));
					trendColors.Add(new Tuple<Brush, Pen>(Between,	new Pen(Between, 1)));
					trendColors.Add(new Tuple<Brush, Pen>(AtBid,	new Pen(AtBid, 1)));
					trendColors.Add(new Tuple<Brush, Pen>(BelowBid, new Pen(BelowBid, 1)));
				}
				return trendColors;
			}
		}

		public override string Format(double value) { return string.Empty; }

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.MarketDataType == MarketDataType.Bid)
			{
				lastBid = marketDataUpdate.Price;
				return;
			}
			else if (marketDataUpdate.MarketDataType == MarketDataType.Ask)
			{
				lastAsk = marketDataUpdate.Price;
				return;
			}
			else if (marketDataUpdate.MarketDataType != MarketDataType.Last)
				return;

			if (lastAsk == double.MinValue || lastBid == double.MinValue)	slots.Insert(0, TrendValue.Between);
			else if (marketDataUpdate.Price.ApproxCompare(lastAsk) == 0)	slots.Insert(0, TrendValue.AtAsk);
			else if (marketDataUpdate.Price.ApproxCompare(lastAsk) == 1)	slots.Insert(0, TrendValue.AboveAsk);
			else if (marketDataUpdate.Price.ApproxCompare(lastBid) == 0)	slots.Insert(0, TrendValue.AtBid);
			else if (marketDataUpdate.Price.ApproxCompare(lastBid) == -1)	slots.Insert(0, TrendValue.BelowBid);
			else 															slots.Insert(0, TrendValue.Between);

			if (slots.Count > maxSlots)
				slots.RemoveRange(maxSlots - 1, slots.Count - maxSlots);
		}

		public override void OnRender(DrawingContext dc, System.Windows.Size renderSize)
		{
			int netHeight	= (int) Math.Floor(renderSize.Height) - margin;
			int netWidth	= (int) Math.Floor(renderSize.Width) - 2 * margin;

			maxSlots = (int) Math.Ceiling((double) netWidth / (double) BarWidth);
			if (slots.Count > maxSlots)
				slots.RemoveRange(maxSlots - 1, slots.Count - maxSlots);

			for (int index = 0; index < maxSlots; index++)
			{
				if (index < maxSlots - slots.Count)
					continue;

				int		x0		= index == 0 ? Math.Max(margin, netWidth + ((-maxSlots + index) * BarWidth)) : netWidth + ((-maxSlots + index) * BarWidth);
				int		x1		= netWidth + ((-maxSlots + index + 1) * BarWidth);
				int		y0		= margin;
				int		y1		= netHeight;

				List<LineSegment>	lineSegments	= new List<LineSegment>();
				Point				startPoint		= new Point(x0, y0);
				lineSegments.Add(new LineSegment(new Point(x0, y1), true));
				lineSegments.Add(new LineSegment(new Point(x1, y1), true));
				lineSegments.Add(new LineSegment(new Point(x1, y0), true));
				lineSegments.Add(new LineSegment(new Point(x1, y0), true));

				List<PathFigure> pathFiguresFill = new List<PathFigure>();
				pathFiguresFill.Add(new PathFigure(startPoint, lineSegments.ToArray(), true));
				PathGeometry pgFill = new PathGeometry(pathFiguresFill);
				int trendValue = (int) slots[maxSlots - 1 - index];
				dc.DrawGeometry(TrendColors[trendValue].Item1, TrendColors[trendValue].Item2, pgFill);
			}
		}

		#region Properties
		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnTSTrendAboveAsk", GroupName = "GuiPropertyCategoryVisual", Order = 32)]
		public Brush AboveAsk
		{
			get { return aboveAsk;  }
			set { aboveAsk = value; trendColors = null; }
		}

		[Browsable(false)]
		public string AboveAskSeralizer { get { return Serialize.BrushToString(AboveAsk); } set { AboveAsk = Serialize.StringToBrush(value); }}

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnTSTrendAtAsk", GroupName = "GuiPropertyCategoryVisual", Order = 34)]
		public Brush AtAsk
		{
			get { return atAsk;  }
			set { atAsk = value; trendColors = null; }
		}

		[Browsable(false)]
		public string AtAskSeralizer { get { return Serialize.BrushToString(AtAsk); } set { AtAsk = Serialize.StringToBrush(value); }}

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnTSTrendAtBid", GroupName = "GuiPropertyCategoryVisual", Order = 36)]
		public Brush AtBid
		{
			get { return atBid;  }
			set { atBid = value; trendColors = null; }
		}

		[Browsable(false)]
		public string AtBidSeralizer { get { return Serialize.BrushToString(AtBid); } set { AtBid = Serialize.StringToBrush(value); }}

		[Range(0, int.MaxValue)]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnTSTrendBarWidth", GroupName = "GuiPropertyCategoryMiscellaneous", Order = 10)]
		public int BarWidth { get; set; }

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnTSTrendBelowBid", GroupName = "GuiPropertyCategoryVisual", Order = 38)]
		public Brush BelowBid
		{
			get { return belowBid;  }
			set { belowBid = value; trendColors = null; }
		}

		[Browsable(false)]
		public string BelowBidSeralizer { get { return Serialize.BrushToString(BelowBid); } set { BelowBid = Serialize.StringToBrush(value); }}

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnTSTrendBetween", GroupName = "GuiPropertyCategoryVisual", Order = 40)]
		public Brush Between
		{
			get { return between;  }
			set { between = value; trendColors = null; }
		}

		[Browsable(false)]
		public string BetweenSeralizer { get { return Serialize.BrushToString(Between); } set { Between = Serialize.StringToBrush(value); }}
		#endregion
	}

	public class TSTrendConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			TSTrend tsTrend = component as TSTrend;
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, component, attrs) : TypeDescriptor.GetProperties(component, attrs);
			if (tsTrend == null || propertyDescriptorCollection == null)
				return propertyDescriptorCollection;

			PropertyDescriptorCollection ret = new PropertyDescriptorCollection(null);
			foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
			{
				if (propertyDescriptor.Name != "AboveAsk" && propertyDescriptor.Name != "AtAsk" && propertyDescriptor.Name != "AtBid" 
					&& propertyDescriptor.Name != "BarWidth" && propertyDescriptor.Name != "BelowBid" && propertyDescriptor.Name != "Between"
					&& propertyDescriptor.Name != "Name")
					continue;

				ret.Add(propertyDescriptor);
			}

			return ret;
		}
	}

	public enum TrendValue
	{
		AboveAsk,
		AtAsk,
		Between,
		AtBid,
		BelowBid
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Linq;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Gui;

using NinjaTrader.Core.FloatingPoint;

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	[TypeConverter("NinjaTrader.NinjaScript.MarketAnalyzerColumns.ChartNetChangeConverter")]
	public class ChartNetChange : Gui.NinjaScript.MarketAnalyzerColumnRenderBase
	{
		private Brush			downArea;
		private Brush			downAreaBrush;
		private Brush			downOutline;
		private Pen				downOutlinePen;
		private double			lastClose			= double.MinValue;
		private DateTime		nextTradingDayBegin = Core.Globals.MaxDate;
		private int				opacity;
		Data.SessionIterator	sessionIterator;
		private Brush			upArea;
		private Brush			upAreaBrush;
		private Brush			upOutline;
		private Pen				upOutlinePen;
		private DateTime		tradingDayBegin		= Core.Globals.MinDate;
		private DateTime		tradingDayEnd		= Core.Globals.MinDate;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnDescriptionChartNetChange;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptMarketAnalyzerColumnNameChartNetChange;
				DownArea				= Brushes.Red;
				DownOutline				= Brushes.Red;
				IsDataSeriesRequired	= true;
				Opacity					= 50;
				UpArea					= Brushes.LimeGreen;
				UpOutline				= Brushes.LimeGreen;
			}
			else if (State == State.Configure)
			{
				BarsPeriod				= new Data.BarsPeriod { BarsPeriodType = Data.BarsPeriodType.Minute, Value = 1 };
				DaysBack				= 0;
				RangeType				= Cbi.RangeType.Days;
				To						= Now.Date;

				if (Instrument != null)
				{
					TradingHoursInstance	= Instrument.MasterInstrument.TradingHours;
					sessionIterator			= new Data.SessionIterator(TradingHoursInstance);

					if (sessionIterator.IsInSession(Now, false, true))
					{
						tradingDayBegin	= sessionIterator.GetTradingDayBeginLocal(sessionIterator.ActualTradingDayExchange);
						tradingDayEnd	= sessionIterator.ActualTradingDayEndLocal;
					}
					else
					{
						DateTime	tradingDay				= sessionIterator.ActualTradingDayExchange;
						bool		isPreviousTradingDay	= true;

						while (true)
						{
							tradingDay = tradingDay.AddDays(-1);
							if (sessionIterator.IsTradingDayDefined(tradingDay))
							{
								if (isPreviousTradingDay)
								{
									tradingDayBegin	= sessionIterator.GetTradingDayBeginLocal(tradingDay);
									tradingDayEnd	= sessionIterator.GetTradingDayEndLocal(tradingDay);
									isPreviousTradingDay = false;
								}
								else
								{
									DateTime tmpTradingDayEnd = sessionIterator.GetTradingDayEndLocal(tradingDay);
									DaysBack = (int) To.Subtract(tmpTradingDayEnd.Date).TotalDays + (tmpTradingDayEnd.Hour == 0 && tmpTradingDayEnd.Minute == 0 ? 1 : 0);
									break;
								}
							}
						}

						sessionIterator.GetNextSession(tradingDayBegin.AddSeconds(1), false);
					}
				}
			}
		}

		private DateTime Now
		{
			get { return Cbi.Connection.PlaybackConnection != null ? Cbi.Connection.PlaybackConnection.Now : Core.Globals.Now; }
		}

		protected override void OnMarketData(Data.MarketDataEventArgs marketDataUpdate)
		{
			if (marketDataUpdate.IsReset)
			{
				CurrentValue = double.MinValue;
				return;
			}
			else if (marketDataUpdate.MarketDataType != Data.MarketDataType.Last || marketDataUpdate.Instrument.MarketData.LastClose == null)
				return;

			lastClose		= marketDataUpdate.Instrument.MarketData.LastClose.Price;
			CurrentValue	= (marketDataUpdate.Price - marketDataUpdate.Instrument.MarketData.LastClose.Price) / marketDataUpdate.Instrument.MarketData.LastClose.Price;
		}

		public override void OnRender(DrawingContext dc, System.Windows.Size renderSize)
		{
			DateTime now = Now;
			if (now > nextTradingDayBegin)
			{
				tradingDayBegin			= sessionIterator.GetTradingDayBeginLocal(sessionIterator.ActualTradingDayExchange);
				tradingDayEnd			= sessionIterator.ActualTradingDayEndLocal;
				nextTradingDayBegin		= Core.Globals.MaxDate;
			}
			else if (sessionIterator.IsNewSession(now, false))
			{
				sessionIterator.GetNextSession(now, false);
				if (sessionIterator.IsInSession(now, false, true))
				{
					tradingDayBegin		= sessionIterator.GetTradingDayBeginLocal(sessionIterator.ActualTradingDayExchange);
					tradingDayEnd		= sessionIterator.ActualTradingDayEndLocal;
				}
				else
					nextTradingDayBegin	= sessionIterator.GetTradingDayBeginLocal(sessionIterator.ActualTradingDayExchange);
			}

			if (downAreaBrush == null)
				downAreaBrush = new SolidColorBrush() { Color = (downArea as SolidColorBrush).Color, Opacity = (double) Opacity / 100 };
			if (upAreaBrush == null)
				upAreaBrush = new SolidColorBrush() { Color = (upArea as SolidColorBrush).Color, Opacity = (double) Opacity / 100 };
			if (downOutlinePen == null)
				downOutlinePen = new Pen(downOutline, 1);
			if (upOutlinePen == null)
				upOutlinePen = new Pen(upOutline, 1);

			int						firstIdx			= Math.Max(0, BarsArray[0].GetBar(tradingDayBegin));
			bool					isInSession			= now >= tradingDayBegin && now < tradingDayEnd;
			List<LineSegment>		lineSegmentsDown	= new List<LineSegment>();
			List<LineSegment>		lineSegmentsUp		= new List<LineSegment>();
			int						margin				= 1;
			double					maxCloseOnX			= double.MinValue;
			double					minCloseOnX			= double.MinValue;
			double					maxPrice			= double.MinValue;
			int						maxX				= -1;
			double					minPrice			= double.MaxValue;
			int						minX				= margin;
			int						netHeight			= (int) Math.Floor(renderSize.Height) - margin;
			int						netWidth			= (int) Math.Floor(renderSize.Width) - 2 * margin;
			int						prevX				= -1;
			int						prevY				= -1;
			System.Windows.Point	startPoint			=	new System.Windows.Point();
			int						yPreviousClose		= -1;
			double					previousClose		= !isInSession && firstIdx > 0 ? BarsArray[0].GetClose(firstIdx - 1) : lastClose;

			BarsArray[0].BarsSeries.SyncRoot.EnterReadLock();
			try
			{
				minPrice = Math.Min(previousClose, minPrice);
				maxPrice = Math.Max(previousClose, maxPrice);

				for (int idx = firstIdx; idx < BarsArray[0].Count; idx++)
				{
					double close = BarsArray[0].GetClose(idx);
					minPrice = Math.Min(close, minPrice);
					maxPrice = Math.Max(close, maxPrice);
				}

				if (minPrice == double.MaxValue || maxPrice == double.MinValue || previousClose == double.MinValue)
					return;

				for (int idx = firstIdx; idx < BarsArray[0].Count; idx++)
				{
					DateTime time = BarsArray[0].GetTime(idx);
					if (time > tradingDayEnd)
						break;

					double		close		= BarsArray[0].GetClose(idx);
					int			x			= margin + Convert.ToInt32(netWidth * time.Subtract(tradingDayBegin).TotalSeconds / Math.Max(1, tradingDayEnd.Subtract(tradingDayBegin).TotalSeconds));
					maxCloseOnX				= x == prevX ? Math.Max(close, maxCloseOnX) : close;
					minCloseOnX				= x == prevX ? Math.Min(close, minCloseOnX) : close;
					double		closeOnX	= minCloseOnX.ApproxCompare(minPrice) == 0		? minCloseOnX 
												: maxCloseOnX.ApproxCompare(maxPrice) == 0	? maxCloseOnX 
												: close.ApproxCompare(previousClose) > 0	? maxCloseOnX : minCloseOnX;
					int			y			= margin + Convert.ToInt32(((maxPrice - closeOnX) / Math.Max(BarsArray[0].Instrument.MasterInstrument.TickSize, maxPrice - minPrice)) * (netHeight - margin));

					if (idx == firstIdx)
					{
						yPreviousClose	= margin + Convert.ToInt32(((maxPrice - previousClose) / Math.Max(BarsArray[0].Instrument.MasterInstrument.TickSize, maxPrice - minPrice)) * (netHeight - margin));;
						startPoint		= new System.Windows.Point(x, yPreviousClose);
					}
					if (idx == BarsArray[0].Count - 1)
						maxX = x;

					bool isCrossover = prevY < yPreviousClose && y > yPreviousClose || prevY > yPreviousClose && y < yPreviousClose;
					if (x == prevX)
					{
						lineSegmentsUp[lineSegmentsUp.Count - 1] = new LineSegment(new System.Windows.Point(x, Math.Min(y, yPreviousClose)), isCrossover ? true : y <= yPreviousClose);
						lineSegmentsDown[lineSegmentsUp.Count - 1] = new LineSegment(new System.Windows.Point(x, Math.Max(y, yPreviousClose)), isCrossover ? true : y > yPreviousClose);
					}
					else
					{
						lineSegmentsUp.Add(new LineSegment(new System.Windows.Point(x, Math.Min(y, yPreviousClose)), isCrossover ? true : y <= yPreviousClose));
						lineSegmentsDown.Add(new LineSegment(new System.Windows.Point(x, Math.Max(y, yPreviousClose)), isCrossover ? true : y > yPreviousClose));
					}

					prevX = x;
					prevY = y;
				}
			}
			finally
			{
				BarsArray[0].BarsSeries.SyncRoot.ExitReadLock();
			}

			if (lineSegmentsUp.Count > 0)
			{
				List<PathFigure> pathFiguresOutlineUp = new List<PathFigure>();
				pathFiguresOutlineUp.Add(new PathFigure(startPoint, lineSegmentsUp.ToArray(), false));
				dc.DrawGeometry(null, upOutlinePen, new PathGeometry(pathFiguresOutlineUp));

				lineSegmentsUp.Add(new LineSegment(new System.Windows.Point(maxX, yPreviousClose), true));
				lineSegmentsUp.Add(new LineSegment(new System.Windows.Point(minX, yPreviousClose), true));

				List<PathFigure> pathFiguresFillUp = new List<PathFigure>();
				pathFiguresFillUp.Add(new PathFigure(startPoint, lineSegmentsUp.ToArray(), true));
				PathGeometry pgFillUp = new PathGeometry(pathFiguresFillUp);
				dc.DrawGeometry(upAreaBrush, null, pgFillUp);

				List<PathFigure> pathFiguresOutlineDown = new List<PathFigure>();
				pathFiguresOutlineDown.Add(new PathFigure(startPoint, lineSegmentsDown.ToArray(), false));
				dc.DrawGeometry(null, downOutlinePen, new PathGeometry(pathFiguresOutlineDown));

				lineSegmentsDown.Add(new LineSegment(new System.Windows.Point(maxX, yPreviousClose), true));
				lineSegmentsDown.Add(new LineSegment(new System.Windows.Point(minX, yPreviousClose), true));

				List<PathFigure> pathFiguresFillDown = new List<PathFigure>();
				pathFiguresFillDown.Add(new PathFigure(startPoint, lineSegmentsDown.ToArray(), true));
				PathGeometry pgFillDown = new PathGeometry(pathFiguresFillDown);
				dc.DrawGeometry(downAreaBrush, null, pgFillDown);
			}
		}

		#region Properties
		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartNetChangeDownArea", GroupName = "GuiPropertyCategoryVisual", Order = 30)]
		public Brush DownArea
		{
			get { return downArea; }
			set { downArea = value; downAreaBrush = null; }
		}

		[Browsable(false)]
		public string DownAreaSeralizer { get { return Serialize.BrushToString(downArea); } set { downArea = Serialize.StringToBrush(value); }}

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartNetChangeDownOutline", GroupName = "GuiPropertyCategoryVisual", Order = 60)]
		public Brush DownOutline
		{
			get { return downOutline; }
			set { downOutline = value; downOutlinePen = null; }
		}
		[Browsable(false)]

		public string DownOutlineSeralizer { get { return Serialize.BrushToString(downOutline); } set { downOutline = Serialize.StringToBrush(value); }}

		public override string Format(double value)
		{
			if (value == double.MinValue)
				return string.Empty;

			return value.ToString("P", Core.Globals.GeneralOptions.CurrentCulture);
		}

		[Range(0, 100)]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartMiniOpacity", GroupName = "GuiPropertyCategoryVisual", Order = 40)]
		public int Opacity
		{
			get { return opacity;}
			set { opacity = value; upAreaBrush = null; downAreaBrush = null;  }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartNetChangeUpArea", GroupName = "GuiPropertyCategoryVisual", Order = 20)]
		public Brush UpArea
		{
			get { return upArea; }
			set { upArea = value; upAreaBrush = null; }
		}

		[Browsable(false)]
		public string UpAreaSeralizer { get { return Serialize.BrushToString(upArea); } set { upArea = Serialize.StringToBrush(value); }}

		[XmlIgnore]
		[Display(ResourceType = typeof(Resource), Name = "NinjaScriptMarketAnalyzerColumnChartNetChangeUpOutline", GroupName = "GuiPropertyCategoryVisual", Order = 50)]
		public Brush UpOutline
		{
			get { return upOutline; }
			set { upOutline = value; upOutlinePen = null; }
		}

		[Browsable(false)]
		public string UpOutlineSeralizer { get { return Serialize.BrushToString(upOutline); } set { upOutline = Serialize.StringToBrush(value); }}
		#endregion
	}

	public class ChartNetChangeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attrs)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, component, attrs) : TypeDescriptor.GetProperties(component, attrs);
			if (!(component is ChartNetChange) || propertyDescriptorCollection == null)
				return propertyDescriptorCollection;

			PropertyDescriptorCollection ret = new PropertyDescriptorCollection(null);
			foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
			{
				if (propertyDescriptor.Name != "DownArea" && propertyDescriptor.Name != "DownOutline" 
					&& propertyDescriptor.Name != "IsVisible" && propertyDescriptor.Name != "Name" && propertyDescriptor.Name != "Opacity" 
					&& propertyDescriptor.Name != "UpArea" && propertyDescriptor.Name != "UpOutline")
					continue;

				ret.Add(propertyDescriptor);
			}

			return ret;
		}
	}
}

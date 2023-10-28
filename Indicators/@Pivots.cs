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
using NinjaTrader.Core;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.DirectWrite;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	[TypeConverter("NinjaTrader.NinjaScript.Indicators.PivotsTypeConverter")]
	public class Pivots : Indicator
	{
		private DateTime				cacheMonthlyEndDate		= Globals.MinDate;
		private DateTime				cacheSessionDate		= Globals.MinDate;
		private DateTime				cacheSessionEnd			= Globals.MinDate;
		private DateTime				cacheTime;
		private DateTime				cacheWeeklyEndDate		= Globals.MinDate;
		private DateTime				currentDate				= Globals.MinDate;
		private DateTime				currentMonth			= Globals.MinDate;
		private DateTime				currentWeek				= Globals.MinDate;
		private DateTime				sessionDateTmp			= Globals.MinDate;
		private HLCCalculationMode		priorDayHlc;
		private PivotRange				pivotRangeType;
		private SessionIterator			storedSession;
		private double					currentClose;
		private double					currentHigh				= double.MinValue;
		private double					currentLow				= double.MaxValue;
		private double					dailyBarClose			= double.MinValue;
		private double					dailyBarHigh			= double.MinValue;
		private double					dailyBarLow				= double.MinValue;
		private double					pp;
		private double					r1;
		private double					r2;
		private double					r3;
		private double					s1;
		private double					s2;
		private double					s3;
		private double					userDefinedClose;
		private double					userDefinedHigh;
		private double					userDefinedLow;
		private int						cacheBar;
		private int						width					= 20;
		private readonly List<int>		newSessionBarIdxArr		= new List<int>();

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description				= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionPivots;
				Name					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNamePivots;
				Calculate				= Calculate.OnBarClose;
				DisplayInDataBox		= true;
				DrawOnPricePanel		= false;
				IsAutoScale				= false;
				IsOverlay				= true;
				PaintPriceMarkers		= true;
				ScaleJustification		= ScaleJustification.Right;

				AddPlot(Brushes.Goldenrod,	NinjaTrader.Custom.Resource.PivotsPP);
				AddPlot(Brushes.DodgerBlue,	NinjaTrader.Custom.Resource.PivotsR1);
				AddPlot(Brushes.DodgerBlue,	NinjaTrader.Custom.Resource.PivotsR2);
				AddPlot(Brushes.DodgerBlue,	NinjaTrader.Custom.Resource.PivotsR3);
				AddPlot(Brushes.Crimson,	NinjaTrader.Custom.Resource.PivotsS1);
				AddPlot(Brushes.Crimson,	NinjaTrader.Custom.Resource.PivotsS2);
				AddPlot(Brushes.Crimson,	NinjaTrader.Custom.Resource.PivotsS3);
			}
			else if (State == State.Configure)
			{
				if (priorDayHlc == HLCCalculationMode.DailyBars)
					AddDataSeries(BarsPeriodType.Day, 1);
			}
			else if (State == State.DataLoaded)
			{
				storedSession = new SessionIterator(Bars);
			}
			else if (State == State.Historical)
			{

				if (BarsArray == null || BarsArray.Length < 2)
				return;

				if (priorDayHlc == HLCCalculationMode.DailyBars && BarsArray[1].DayCount <= 0)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsDailyDataError, TextPosition.BottomRight);
					Log(NinjaTrader.Custom.Resource.PiviotsDailyDataError, LogLevel.Error);
					return;
				}

				if (!Bars.BarsType.IsIntraday && BarsPeriod.BarsPeriodType != BarsPeriodType.Day && (BarsPeriod.BarsPeriodType != BarsPeriodType.HeikenAshi && BarsPeriod.BarsPeriodType != BarsPeriodType.Volumetric || BarsPeriod.BaseBarsPeriodType != BarsPeriodType.Day))
				{
					Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsDailyBarsError, TextPosition.BottomRight);
					Log(NinjaTrader.Custom.Resource.PiviotsDailyBarsError, LogLevel.Error);
				}
				if ((BarsPeriod.BarsPeriodType == BarsPeriodType.Day || ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Day)) && pivotRangeType == PivotRange.Daily)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsWeeklyBarsError, TextPosition.BottomRight);
					Log(NinjaTrader.Custom.Resource.PiviotsWeeklyBarsError, LogLevel.Error);
				}
				if ((BarsPeriod.BarsPeriodType == BarsPeriodType.Day || ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Day)) && BarsPeriod.Value > 1)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsPeriodTypeError, TextPosition.BottomRight);
					Log(NinjaTrader.Custom.Resource.PiviotsPeriodTypeError, LogLevel.Error);
				}
				if ((priorDayHlc == HLCCalculationMode.DailyBars &&
					(pivotRangeType == PivotRange.Monthly && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddMonths(-1)
					|| pivotRangeType == PivotRange.Weekly && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddDays(-7)
					|| pivotRangeType == PivotRange.Daily && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddDays(-1)))
					|| pivotRangeType == PivotRange.Monthly && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddMonths(-1)
					|| pivotRangeType == PivotRange.Weekly && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddDays(-7)
					|| pivotRangeType == PivotRange.Daily && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddDays(-1)
					)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", NinjaTrader.Custom.Resource.PiviotsInsufficentDataError, TextPosition.BottomRight);
					Log(NinjaTrader.Custom.Resource.PiviotsInsufficentDataError, LogLevel.Error);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress != 0)
				return;

			if ((priorDayHlc == HLCCalculationMode.DailyBars && BarsArray[1].DayCount <= 0)
				|| (!Bars.BarsType.IsIntraday && BarsPeriod.BarsPeriodType != BarsPeriodType.Day && (BarsPeriod.BarsPeriodType != BarsPeriodType.HeikenAshi && BarsPeriod.BarsPeriodType != BarsPeriodType.Volumetric || BarsPeriod.BaseBarsPeriodType != BarsPeriodType.Day))
				|| ((BarsPeriod.BarsPeriodType == BarsPeriodType.Day || ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Day)) && pivotRangeType == PivotRange.Daily)
				|| ((BarsPeriod.BarsPeriodType == BarsPeriodType.Day || ((BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Day)) && BarsPeriod.Value > 1)
				|| ((priorDayHlc == HLCCalculationMode.DailyBars && (pivotRangeType == PivotRange.Monthly && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddMonths(-1)
				|| pivotRangeType == PivotRange.Weekly && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddDays(-7)
				|| pivotRangeType == PivotRange.Daily && BarsArray[1].GetTime(0).Date >= BarsArray[1].GetTime(BarsArray[1].Count - 1).Date.AddDays(-1)))
				|| pivotRangeType == PivotRange.Monthly && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddMonths(-1)
				|| pivotRangeType == PivotRange.Weekly && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddDays(-7)
				|| pivotRangeType == PivotRange.Daily && BarsArray[0].GetTime(0).Date >= BarsArray[0].GetTime(BarsArray[0].Count - 1).Date.AddDays(-1)))
				return;

			RemoveDrawObject("NinjaScriptInfo");

			if (PriorDayHlc == HLCCalculationMode.DailyBars && CurrentBars[1] >= 0)
			{
				// Get daily bars like this to avoid situation where primary series moves to next session before previous day OHLC are added
				if (cacheTime != Times[0][0])
				{
					cacheTime	= Times[0][0];
					cacheBar	= BarsArray[1].GetBar(Times[0][0]);
				}
				dailyBarHigh	= BarsArray[1].GetHigh(cacheBar);
				dailyBarLow		= BarsArray[1].GetLow(cacheBar);
				dailyBarClose	= BarsArray[1].GetClose(cacheBar);
			}
			else
			{
				dailyBarHigh	= double.MinValue;
				dailyBarLow		= double.MinValue;
				dailyBarClose	= double.MinValue;
			}

			double high		= (dailyBarHigh == double.MinValue)		? Highs[0][0]	: dailyBarHigh;
			double low		= (dailyBarLow == double.MinValue)		? Lows[0][0]	: dailyBarLow;
			double close	= (dailyBarClose == double.MinValue)	? Closes[0][0]	: dailyBarClose;

			DateTime lastBarTimeStamp = GetLastBarSessionDate(Times[0][0], pivotRangeType);

			if ((currentDate != Globals.MinDate && pivotRangeType == PivotRange.Daily && lastBarTimeStamp != currentDate)
				|| (currentWeek != Globals.MinDate && pivotRangeType == PivotRange.Weekly && lastBarTimeStamp != currentWeek)
				|| (currentMonth != Globals.MinDate && pivotRangeType == PivotRange.Monthly && lastBarTimeStamp != currentMonth))
			{
				pp				= (currentHigh + currentLow + currentClose) / 3;
				s1				= 2 * pp - currentHigh;
				r1				= 2 * pp - currentLow;
				s2				= pp - (currentHigh - currentLow);
				r2				= pp + (currentHigh - currentLow);
				s3				= pp - 2 * (currentHigh - currentLow);
				r3				= pp + 2 * (currentHigh - currentLow);
				currentClose	= (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedClose	: close;
				currentHigh		= (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedHigh	: high;
				currentLow		= (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedLow	: low;
			}
			else
			{
				currentClose	= (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedClose	: close;
				currentHigh		= (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedHigh	: Math.Max(currentHigh, high);
				currentLow		= (priorDayHlc == HLCCalculationMode.UserDefinedValues) ? UserDefinedLow	: Math.Min(currentLow, low);
			}


			if (pivotRangeType == PivotRange.Daily)
				currentDate = lastBarTimeStamp;
			if (pivotRangeType == PivotRange.Weekly)
				currentWeek = lastBarTimeStamp;
			if (pivotRangeType == PivotRange.Monthly)
				currentMonth = lastBarTimeStamp;

			if ((pivotRangeType == PivotRange.Daily && currentDate != Globals.MinDate)
				|| (pivotRangeType == PivotRange.Weekly && currentWeek != Globals.MinDate)
				|| (pivotRangeType == PivotRange.Monthly && currentMonth != Globals.MinDate))
			{
				Pp[0] = pp;
				R1[0] = r1;
				R2[0] = r2;
				R3[0] = r3;
				S1[0] = s1;
				S2[0] = s2;
				S3[0] = s3;
			}
		}

		#region Misc
		private DateTime GetLastBarSessionDate(DateTime time, PivotRange pivotRange)
		{
			// Check the time[0] against the previous session end
			if (time > cacheSessionEnd)
			{
				if (Bars.BarsType.IsIntraday)
				{
					// Make use of the stored session iterator to find the next session...
					storedSession.GetNextSession(time, true);
					// Store the actual session's end datetime as the session
					cacheSessionEnd = storedSession.ActualSessionEnd;
					// We need to convert that time from the session to the users time zone settings
					sessionDateTmp = TimeZoneInfo.ConvertTime(cacheSessionEnd.AddSeconds(-1), Globals.GeneralOptions.TimeZoneInfo, Bars.TradingHours.TimeZoneInfo).Date;
				}
				else
					sessionDateTmp = time.Date;
			}

			if (pivotRange == PivotRange.Daily)
			{
				if (sessionDateTmp != cacheSessionDate)
				{
					if (newSessionBarIdxArr.Count == 0 || newSessionBarIdxArr.Count > 0 && CurrentBar > newSessionBarIdxArr[newSessionBarIdxArr.Count - 1])
						newSessionBarIdxArr.Add(CurrentBar);
					cacheSessionDate = sessionDateTmp;
				}
				return sessionDateTmp;
			}

			DateTime tmpWeeklyEndDate = RoundUpTimeToPeriodTime(sessionDateTmp, PivotRange.Weekly);
			if (pivotRange == PivotRange.Weekly)
			{
				if (tmpWeeklyEndDate != cacheWeeklyEndDate)
				{
					if (newSessionBarIdxArr.Count == 0 || newSessionBarIdxArr.Count > 0 && CurrentBar > newSessionBarIdxArr[newSessionBarIdxArr.Count - 1])
						newSessionBarIdxArr.Add(CurrentBar);
					cacheWeeklyEndDate = tmpWeeklyEndDate;
				}
				return tmpWeeklyEndDate;
			}

			DateTime tmpMonthlyEndDate = RoundUpTimeToPeriodTime(sessionDateTmp, PivotRange.Monthly);
			if (tmpMonthlyEndDate != cacheMonthlyEndDate)
			{
				if (newSessionBarIdxArr.Count == 0 || newSessionBarIdxArr.Count > 0 && CurrentBar > newSessionBarIdxArr[newSessionBarIdxArr.Count - 1])
					newSessionBarIdxArr.Add(CurrentBar);
				cacheMonthlyEndDate = tmpMonthlyEndDate;
			}
			return tmpMonthlyEndDate;
		}

		private DateTime RoundUpTimeToPeriodTime(DateTime time, PivotRange pivotRange)
		{
			if (pivotRange == PivotRange.Weekly)
				return Gui.Tools.Extensions.GetEndOfWeekTime(time);
			if (pivotRange == PivotRange.Monthly)
				return Gui.Tools.Extensions.GetEndOfMonthTime(time);
			return time;
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			// Set text to chart label color and font
			TextFormat	textFormat			= chartControl.Properties.LabelFont.ToDirectWriteTextFormat();

			// Loop through each Plot Values on the chart
			for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
			{
				double	y					= -1;
				double	startX				= -1;
				double	endX				= -1;
				int		firstBarIdxToPaint	= -1;
				int		firstBarPainted		= ChartBars.FromIndex;
				int		lastBarPainted		= ChartBars.ToIndex;
				Plot	plot				= Plots[seriesCount];

				for (int i = newSessionBarIdxArr.Count - 1; i >= 0; i--)
				{
					int prevSessionBreakIdx = newSessionBarIdxArr[i];
					if (prevSessionBreakIdx <= lastBarPainted)
					{
						firstBarIdxToPaint = prevSessionBreakIdx;
						break;
					}
				}

				// Loop through visble bars to render plot values
				for (int idx = lastBarPainted; idx >= Math.Max(firstBarPainted, lastBarPainted - width); idx--)
				{
					if (idx < firstBarIdxToPaint)
						break;

					startX		= chartControl.GetXByBarIndex(ChartBars, idx);
					endX		= chartControl.GetXByBarIndex(ChartBars, lastBarPainted);
					double val	= Values[seriesCount].GetValueAt(idx);
					y			= chartScale.GetYByValue(val);
				}

				// Draw pivot lines
				Point startPoint	= new Point(startX, y);
				Point endPoint		= new Point(endX, y);
				RenderTarget.DrawLine(startPoint.ToVector2(), endPoint.ToVector2(), plot.BrushDX, plot.Width, plot.StrokeStyle);

				// Draw pivot text
				TextLayout textLayout = new TextLayout(Globals.DirectWriteFactory, plot.Name, textFormat, ChartPanel.W, textFormat.FontSize);
				RenderTarget.DrawTextLayout(startPoint.ToVector2(), textLayout, plot.BrushDX);
				textLayout.Dispose();
			}
			textFormat.Dispose();
		}
		#endregion

		#region Properties

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "PivotRange", GroupName = "NinjaScriptParameters", Order = 0)]
		public PivotRange PivotRangeType
		{
			get { return pivotRangeType; }
			set { pivotRangeType = value; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Pp
		{
			get { return Values[0]; }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "HLCCalculationMode", GroupName = "NinjaScriptParameters", Order = 1)]
		[RefreshProperties(RefreshProperties.All)] // Update UI when value is changed
		public HLCCalculationMode PriorDayHlc
		{
			get { return priorDayHlc; }
			set { priorDayHlc = value; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> R1
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> R2
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> R3
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> S1
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> S2
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> S3
		{
			get { return Values[6]; }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "UserDefinedClose", GroupName = "NinjaScriptParameters", Order = 2)]
		public double UserDefinedClose
		{
			get { return userDefinedClose; }
			set { userDefinedClose = value; }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "UserDefinedHigh", GroupName = "NinjaScriptParameters", Order = 3)]
		public double UserDefinedHigh
		{
			get { return userDefinedHigh; }
			set { userDefinedHigh = value; }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "UserDefinedLow", GroupName = "NinjaScriptParameters", Order = 4)]
		public double UserDefinedLow
		{
			get { return userDefinedLow; }
			set { userDefinedLow = value; }
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Width", GroupName = "NinjaScriptParameters", Order = 5)]
		public int Width
		{
			get { return width; }
			set { width = value; }
		}

		#endregion
	}

	// Hide UserDefinedValues properties when not in use by the HLCCalculationMode.UserDefinedValues
	// When creating a custom type converter for indicators it must inherit from NinjaTrader.NinjaScript.IndicatorBaseConverter to work correctly with indicators
	public class PivotsTypeConverter : NinjaTrader.NinjaScript.IndicatorBaseConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context) { return true; }

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			PropertyDescriptorCollection propertyDescriptorCollection = base.GetPropertiesSupported(context) ? base.GetProperties(context, value, attributes) : TypeDescriptor.GetProperties(value, attributes);

			Pivots						thisPivotsInstance			= (Pivots) value;
			HLCCalculationMode	selectedHLCCalculationMode	= thisPivotsInstance.PriorDayHlc;
			if (selectedHLCCalculationMode == HLCCalculationMode.UserDefinedValues)
				return propertyDescriptorCollection;

			PropertyDescriptorCollection adjusted = new PropertyDescriptorCollection(null);
			foreach (PropertyDescriptor thisDescriptor in propertyDescriptorCollection)
			{
				if (thisDescriptor.Name == "UserDefinedClose" || thisDescriptor.Name == "UserDefinedHigh" || thisDescriptor.Name == "UserDefinedLow")
					adjusted.Add(new PropertyDescriptorExtended(thisDescriptor, o => value, null, new Attribute[] {new BrowsableAttribute(false), }));
				else
					adjusted.Add(thisDescriptor);
			}
			return adjusted;
		}
	}
}

[TypeConverter("NinjaTrader.Custom.ResourceEnumConverter")]
public enum HLCCalculationMode
{
	CalcFromIntradayData,
	DailyBars,
	UserDefinedValues
}

[TypeConverter("NinjaTrader.Custom.ResourceEnumConverter")]
public enum PivotRange
{
	Daily,
	Weekly,
	Monthly,
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private Pivots[] cachePivots;
		public Pivots Pivots(PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return Pivots(Input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}

		public Pivots Pivots(ISeries<double> input, PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			if (cachePivots != null)
				for (int idx = 0; idx < cachePivots.Length; idx++)
					if (cachePivots[idx] != null && cachePivots[idx].PivotRangeType == pivotRangeType && cachePivots[idx].PriorDayHlc == priorDayHlc && cachePivots[idx].UserDefinedClose == userDefinedClose && cachePivots[idx].UserDefinedHigh == userDefinedHigh && cachePivots[idx].UserDefinedLow == userDefinedLow && cachePivots[idx].Width == width && cachePivots[idx].EqualsInput(input))
						return cachePivots[idx];
			return CacheIndicator<Pivots>(new Pivots(){ PivotRangeType = pivotRangeType, PriorDayHlc = priorDayHlc, UserDefinedClose = userDefinedClose, UserDefinedHigh = userDefinedHigh, UserDefinedLow = userDefinedLow, Width = width }, input, ref cachePivots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Pivots Pivots(PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return indicator.Pivots(Input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}

		public Indicators.Pivots Pivots(ISeries<double> input , PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return indicator.Pivots(input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Pivots Pivots(PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return indicator.Pivots(Input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}

		public Indicators.Pivots Pivots(ISeries<double> input , PivotRange pivotRangeType, HLCCalculationMode priorDayHlc, double userDefinedClose, double userDefinedHigh, double userDefinedLow, int width)
		{
			return indicator.Pivots(input, pivotRangeType, priorDayHlc, userDefinedClose, userDefinedHigh, userDefinedLow, width);
		}
	}
}

#endregion

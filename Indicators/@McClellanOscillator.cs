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
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// McClellan Oscillator is the difference between two exponential moving averages of the NYSE advance decline spread. This indicator require ADV and DECL index data.
	/// </summary>
	public class McClellanOscillator : Indicator
	{
		private Series<double> subtractADVDECL;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionMcClellanOscillator;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameMcClellanOscillator;
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= false;
				DrawOnPricePanel			= false;
				IsSuspendedWhileInactive	= true;
				FastPeriod					= 19;
				SlowPeriod					= 39;
				NegativeColor				= Brushes.Red;

				AddPlot(new Stroke(Brushes.LimeGreen, DashStyleHelper.Solid, 1), PlotStyle.Line, NinjaTrader.Custom.Resource.NinjaScriptIndicatorMcClellanOscillatorLine);

				AddLine(Brushes.DarkCyan, 70,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorOverBoughtLine);
				AddLine(Brushes.DarkCyan, -70,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorOverSoldLine);
				AddLine(Brushes.DarkGray, 0,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
			}
			else if (State == State.Configure)
			{
				AddDataSeries("^ADV");
				AddDataSeries("^DECL");
			}
			else if (State == State.DataLoaded)
				subtractADVDECL = new Series<double>(this);
		}

		protected override void OnBarUpdate()
		{
			if (BarsInProgress == 0 && CurrentBars[0] >= 0 && CurrentBars[1] >= 0 && CurrentBars[2] >= 0)
			{
				subtractADVDECL[0]	= Closes[1][0] - Closes[2][0];
				Value[0]			= EMA(subtractADVDECL, FastPeriod)[0] - EMA(subtractADVDECL, SlowPeriod)[0];

				if (Value[0] < 0)
					PlotBrushes[0][0] = NegativeColor;
			}
		}
		
		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "FastPeriod", GroupName = "NinjaScriptParameters", Order = 0)]
		public int FastPeriod { get; set; }
		
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "SlowPeriod", GroupName = "NinjaScriptParameters", Order = 1)]
		public int SlowPeriod { get; set; }
		
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NegativeColor", GroupName = "NinjaScriptIndicatorVisualGroup", Order = 1800)]
		public Brush NegativeColor { get; set; }
		
		[Browsable(false)]
		public string NegativeColorSerialize
		{
			get { return Serialize.BrushToString(NegativeColor); }
			set { NegativeColor = Serialize.StringToBrush(value); }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private McClellanOscillator[] cacheMcClellanOscillator;
		public McClellanOscillator McClellanOscillator(int fastPeriod, int slowPeriod)
		{
			return McClellanOscillator(Input, fastPeriod, slowPeriod);
		}

		public McClellanOscillator McClellanOscillator(ISeries<double> input, int fastPeriod, int slowPeriod)
		{
			if (cacheMcClellanOscillator != null)
				for (int idx = 0; idx < cacheMcClellanOscillator.Length; idx++)
					if (cacheMcClellanOscillator[idx] != null && cacheMcClellanOscillator[idx].FastPeriod == fastPeriod && cacheMcClellanOscillator[idx].SlowPeriod == slowPeriod && cacheMcClellanOscillator[idx].EqualsInput(input))
						return cacheMcClellanOscillator[idx];
			return CacheIndicator<McClellanOscillator>(new McClellanOscillator(){ FastPeriod = fastPeriod, SlowPeriod = slowPeriod }, input, ref cacheMcClellanOscillator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.McClellanOscillator McClellanOscillator(int fastPeriod, int slowPeriod)
		{
			return indicator.McClellanOscillator(Input, fastPeriod, slowPeriod);
		}

		public Indicators.McClellanOscillator McClellanOscillator(ISeries<double> input , int fastPeriod, int slowPeriod)
		{
			return indicator.McClellanOscillator(input, fastPeriod, slowPeriod);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.McClellanOscillator McClellanOscillator(int fastPeriod, int slowPeriod)
		{
			return indicator.McClellanOscillator(Input, fastPeriod, slowPeriod);
		}

		public Indicators.McClellanOscillator McClellanOscillator(ISeries<double> input , int fastPeriod, int slowPeriod)
		{
			return indicator.McClellanOscillator(input, fastPeriod, slowPeriod);
		}
	}
}

#endregion

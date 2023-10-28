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
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Directional Movement Index. 
	/// An indicator developed by J. Welles Wilder for identifying when a definable trend is present in an instrument. 
	/// That is, the DMI tells whether an instrument is trending or not.
	/// </summary>
	public class DMI : Indicator
	{
		private Series<double>		dmMinus;
		private Series<double>		dmPlus;
		private Series<double>		tr;
		private SMA					smaTr;
		private	SMA					smaDmPlus;
		private	SMA					smaDmMinus;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionDMI;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameDMI;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.DarkCyan, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameDMI);
			}
			else if (State == State.DataLoaded)
			{
				dmMinus		= new Series<double>(this);
				dmPlus		= new Series<double>(this);
				tr			= new Series<double>(this);
				smaTr		= SMA(tr, Period);
				smaDmMinus	= SMA(dmMinus, Period);
				smaDmPlus	= SMA(dmPlus, Period);
			}
		}

		protected override void OnBarUpdate()
		{
			double high0	= High[0];
			double low0		= Low[0];

			if (CurrentBar == 0)
			{
				dmMinus[0]		= 0;
				dmPlus[0]		= 0;
				tr[0]			= high0 - low0;
				Value[0]		= 0;
			}
			else
			{
				double low1				= Low[1];
				double High1			= High[1];
				double close1			= Close[1];

				dmMinus[0]				= low1 - low0 > high0 - High1 ? Math.Max(low1 - low0, 0) : 0;
				dmPlus[0]				= high0 - High1 > low1 - low0 ? Math.Max(high0 - High1, 0) : 0;
				tr[0]					= Math.Max(high0 - low0, Math.Max(Math.Abs(high0 - close1), Math.Abs(low0 - close1)));

				double smaTr0			= smaTr[0];
				double smaDmMinus0		= smaDmMinus[0];
				double smaDmPlus0		= smaDmPlus[0];
				double diMinus			= (smaTr0 == 0) ? 0 : smaDmMinus0 / smaTr0;
				double diPlus			= (smaTr0 == 0) ? 0 : smaDmPlus0 / smaTr0;

				Value[0]				= (diPlus + diMinus == 0) ? 0 : (diPlus - diMinus) / (diPlus + diMinus);
			}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DMI[] cacheDMI;
		public DMI DMI(int period)
		{
			return DMI(Input, period);
		}

		public DMI DMI(ISeries<double> input, int period)
		{
			if (cacheDMI != null)
				for (int idx = 0; idx < cacheDMI.Length; idx++)
					if (cacheDMI[idx] != null && cacheDMI[idx].Period == period && cacheDMI[idx].EqualsInput(input))
						return cacheDMI[idx];
			return CacheIndicator<DMI>(new DMI(){ Period = period }, input, ref cacheDMI);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DMI DMI(int period)
		{
			return indicator.DMI(Input, period);
		}

		public Indicators.DMI DMI(ISeries<double> input , int period)
		{
			return indicator.DMI(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DMI DMI(int period)
		{
			return indicator.DMI(Input, period);
		}

		public Indicators.DMI DMI(ISeries<double> input , int period)
		{
			return indicator.DMI(input, period);
		}
	}
}

#endregion

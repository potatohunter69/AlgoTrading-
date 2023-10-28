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
	/// Donchian Channel. The Donchian Channel indicator was created by Richard Donchian.
	///  It uses the highest high and the lowest low of a period of time to plot the channel.
	/// </summary>
	public class DonchianChannel : Indicator
	{
		private MAX max;
		private MIN min;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionDonchianChannel;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameDonchianChannel;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod,	NinjaTrader.Custom.Resource.DonchianChannelMean);
				AddPlot(Brushes.DodgerBlue,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorUpper);
				AddPlot(Brushes.DodgerBlue,	NinjaTrader.Custom.Resource.NinjaScriptIndicatorLower);
			}
			else if (State == State.DataLoaded)
			{
				max = MAX(High, Period);
				min	= MIN(Low, Period);
			}
		}

		protected override void OnBarUpdate()
		{
			double max0 = max[0];
			double min0	= min[0];

			Value[0]	= (max0 + min0) / 2;
			Upper[0]	= max0;
			Lower[0]	= min0;
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Mean
		{
			get { return Values[0]; }
		}

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper
		{
			get { return Values[1]; }
		}
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DonchianChannel[] cacheDonchianChannel;
		public DonchianChannel DonchianChannel(int period)
		{
			return DonchianChannel(Input, period);
		}

		public DonchianChannel DonchianChannel(ISeries<double> input, int period)
		{
			if (cacheDonchianChannel != null)
				for (int idx = 0; idx < cacheDonchianChannel.Length; idx++)
					if (cacheDonchianChannel[idx] != null && cacheDonchianChannel[idx].Period == period && cacheDonchianChannel[idx].EqualsInput(input))
						return cacheDonchianChannel[idx];
			return CacheIndicator<DonchianChannel>(new DonchianChannel(){ Period = period }, input, ref cacheDonchianChannel);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DonchianChannel DonchianChannel(int period)
		{
			return indicator.DonchianChannel(Input, period);
		}

		public Indicators.DonchianChannel DonchianChannel(ISeries<double> input , int period)
		{
			return indicator.DonchianChannel(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DonchianChannel DonchianChannel(int period)
		{
			return indicator.DonchianChannel(Input, period);
		}

		public Indicators.DonchianChannel DonchianChannel(ISeries<double> input , int period)
		{
			return indicator.DonchianChannel(input, period);
		}
	}
}

#endregion

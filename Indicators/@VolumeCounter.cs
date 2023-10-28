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
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using SharpDX.DirectWrite;
using SharpDX.Direct2D1;
using Point = System.Windows.Point;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	public class VolumeCounter : Indicator
	{
		private double volume;
		private bool isVolume, isVolumeBase;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionVolumeCounter;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameVolumeCounter;
				Calculate					= Calculate.OnEachTick;
				CountDown					= true;
				DisplayInDataBox			= false;
				DrawOnPricePanel			= false;
				IsChartOnly					= true;
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				ShowPercent					= true;
			}
			else if(State == State.DataLoaded)
			{
				isVolume 		= BarsPeriod.BarsPeriodType == BarsPeriodType.Volume;
				isVolumeBase 	= (BarsPeriod.BarsPeriodType == BarsPeriodType.HeikenAshi || BarsPeriod.BarsPeriodType == BarsPeriodType.Volumetric) && BarsPeriod.BaseBarsPeriodType == BarsPeriodType.Volume;
			}
		}

		protected override void OnBarUpdate()
		{
			volume = Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency ? Core.Globals.ToCryptocurrencyVolume((long)Volume[0]) : Volume[0];

			double volumeCount = ShowPercent
				? CountDown
					? (1 - Bars.PercentComplete) * 100
					: Bars.PercentComplete * 100
				: CountDown
					? (isVolumeBase
						? BarsPeriod.BaseBarsPeriodValue
						: BarsPeriod.Value) - volume
					: volume;

			string volume1 = (isVolume || isVolumeBase)
				? ((CountDown
					? NinjaTrader.Custom.Resource.VolumeCounterVolumeRemaining + volumeCount
					: NinjaTrader.Custom.Resource.VolumeCounterVolumeCount + volumeCount) + (ShowPercent ? "%" : ""))
				: NinjaTrader.Custom.Resource.VolumeCounterBarError;

			Draw.TextFixed(this, "NinjaScriptInfo", volume1, TextPosition.BottomRight);
		}

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "CountDown", GroupName = "NinjaScriptParameters", Order = 0)]
		public bool CountDown
		{ get; set; }

		[NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "ShowPercent", GroupName = "NinjaScriptParameters", Order = 0)]
		public bool ShowPercent
		{ get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VolumeCounter[] cacheVolumeCounter;
		public VolumeCounter VolumeCounter(bool countDown, bool showPercent)
		{
			return VolumeCounter(Input, countDown, showPercent);
		}

		public VolumeCounter VolumeCounter(ISeries<double> input, bool countDown, bool showPercent)
		{
			if (cacheVolumeCounter != null)
				for (int idx = 0; idx < cacheVolumeCounter.Length; idx++)
					if (cacheVolumeCounter[idx] != null && cacheVolumeCounter[idx].CountDown == countDown && cacheVolumeCounter[idx].ShowPercent == showPercent && cacheVolumeCounter[idx].EqualsInput(input))
						return cacheVolumeCounter[idx];
			return CacheIndicator<VolumeCounter>(new VolumeCounter(){ CountDown = countDown, ShowPercent = showPercent }, input, ref cacheVolumeCounter);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VolumeCounter VolumeCounter(bool countDown, bool showPercent)
		{
			return indicator.VolumeCounter(Input, countDown, showPercent);
		}

		public Indicators.VolumeCounter VolumeCounter(ISeries<double> input , bool countDown, bool showPercent)
		{
			return indicator.VolumeCounter(input, countDown, showPercent);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VolumeCounter VolumeCounter(bool countDown, bool showPercent)
		{
			return indicator.VolumeCounter(Input, countDown, showPercent);
		}

		public Indicators.VolumeCounter VolumeCounter(ISeries<double> input , bool countDown, bool showPercent)
		{
			return indicator.VolumeCounter(input, countDown, showPercent);
		}
	}
}

#endregion

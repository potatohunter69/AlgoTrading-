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
	/// OBV (On Balance Volume) is a running total of volume. It shows if volume is flowing into
	/// or out of a security. When the security closes higher than the previous close, all
	/// of the day's volume is considered up-volume. When the security closes lower than the
	/// previous close, all of the day's volume is considered down-volume.
	/// </summary>
	public class OBV : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionOBV;
				Name						= NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameOBV;
				IsSuspendedWhileInactive	= true;
				DrawOnPricePanel			= false;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameOBV);
			}
			else if (State == State.Historical)
			{
				if (Calculate == Calculate.OnPriceChange)
				{
					Draw.TextFixed(this, "NinjaScriptInfo", string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnPriceChangeError, Name), TextPosition.BottomRight);
					Log(string.Format(NinjaTrader.Custom.Resource.NinjaScriptOnPriceChangeError, Name), LogLevel.Error);
				}
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
				Value[0] = 0;
			else
			{
				double close0	= Close[0];
				double close1	= Close[1];
				double volume0	= Instrument.MasterInstrument.InstrumentType == InstrumentType.CryptoCurrency ? Core.Globals.ToCryptocurrencyVolume((long)Volume[0]) : Volume[0];

				if (close0 > close1)
					Value[0] = Value[1] + volume0;
				else if (close0  < close1)
					Value[0] = Value[1] - volume0;
				else
					Value[0] = Value[1];
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OBV[] cacheOBV;
		public OBV OBV()
		{
			return OBV(Input);
		}

		public OBV OBV(ISeries<double> input)
		{
			if (cacheOBV != null)
				for (int idx = 0; idx < cacheOBV.Length; idx++)
					if (cacheOBV[idx] != null &&  cacheOBV[idx].EqualsInput(input))
						return cacheOBV[idx];
			return CacheIndicator<OBV>(new OBV(), input, ref cacheOBV);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OBV OBV()
		{
			return indicator.OBV(Input);
		}

		public Indicators.OBV OBV(ISeries<double> input )
		{
			return indicator.OBV(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OBV OBV()
		{
			return indicator.OBV(Input);
		}

		public Indicators.OBV OBV(ISeries<double> input )
		{
			return indicator.OBV(input);
		}
	}
}

#endregion

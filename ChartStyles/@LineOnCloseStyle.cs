#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Windows.Media;
#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class LineOnCloseStyle : ChartStyle
	{
		private object icon;

		public override int GetBarPaintWidth(int barWidth)
		{
			return 1 + 2 * (barWidth - 1) + 2 * barWidth;
		}

		public override object Icon
		{
			get { return icon ?? (icon = Gui.Tools.Icons.ChartLineOnClose); }
		}

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			Bars bars = chartBars.Bars;

			if (chartBars.FromIndex > 0)
				chartBars.FromIndex--;

			SharpDX.Direct2D1.PathGeometry		lineGeometry	= new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
			GeometrySink						sink			= lineGeometry.Open();
			sink.BeginFigure(new Vector2(chartControl.GetXByBarIndex(chartBars, chartBars.FromIndex > -1 ? chartBars.FromIndex : 0), chartScale.GetYByValue(bars.GetClose(chartBars.FromIndex > -1 ? chartBars.FromIndex : 0))), FigureBegin.Filled);

			for (int idx = chartBars.FromIndex + 1; idx <= chartBars.ToIndex; idx++)
			{
				double	closeValue	= bars.GetClose(idx);
				float	close		= chartScale.GetYByValue(closeValue);
				float	x			= chartControl.GetXByBarIndex(chartBars, idx);
				sink.AddLine(new Vector2(x, close));
			}

			sink.EndFigure(FigureEnd.Open);
			sink.Close();
			AntialiasMode oldAliasMode	= RenderTarget.AntialiasMode;
			RenderTarget.AntialiasMode	= AntialiasMode.PerPrimitive;
			RenderTarget.DrawGeometry(lineGeometry, UpBrushDX, (float) Math.Max(1, chartBars.Properties.ChartStyle.BarWidth));
			RenderTarget.AntialiasMode	= oldAliasMode;
			lineGeometry.Dispose();
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleLineOnClose;
				ChartStyleType	= ChartStyleType.LineOnClose;
				
				UpBrush			= Brushes.DimGray;
				BarWidth		= 1;
			}
			else if (State == State.Configure)
			{
				Properties.Remove(Properties.Find("DownBrush",	true));
				Properties.Remove(Properties.Find("Stroke",		true));
				Properties.Remove(Properties.Find("Stroke2",	true));

				SetPropertyName("BarWidth",	Custom.Resource.NinjaScriptChartStyleBarWidth);
				SetPropertyName("UpBrush",	Custom.Resource.NinjaScriptChartStyleLineOnCloseColor);
			}
		}
	}
}

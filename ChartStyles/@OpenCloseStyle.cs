#region Using declarations
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;
#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class OpenCloseStyle : ChartStyle
	{
		private object icon;

		public override int GetBarPaintWidth(int barWidth) { return 1 + 2 * (barWidth - 1) + 2 * (int)Math.Round(Stroke.Width); }

		public override object Icon { get { return icon ?? (icon = Gui.Tools.Icons.ChartOpenClose); } }

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			Bars			bars			= chartBars.Bars;
			float			barWidth		= GetBarPaintWidth(BarWidthUI);
			RectangleF		rect			= new RectangleF();


			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				Brush		overriddenBrush				= chartControl.GetBarOverrideBrush(chartBars, idx);
				Brush		overriddenOutlineBrush		= chartControl.GetCandleOutlineOverrideBrush(chartBars, idx);
				double		closeValue					= bars.GetClose(idx);
				float		close						= chartScale.GetYByValue(closeValue);
				double		openValue					= bars.GetOpen(idx);
				float		open						= chartScale.GetYByValue(openValue);
				float		x							= chartControl.GetXByBarIndex(chartBars, idx);

				Gui.Stroke	outlineStroke				= closeValue >= openValue ? Stroke		: Stroke2;

				rect.X									= x - barWidth * 0.5f + 0.5f;
				rect.Y									= Math.Min(open, close);
				rect.Width								= barWidth - 1;
				rect.Height								= Math.Max(open, close) - Math.Min(open, close);

				Brush b									= overriddenBrush ?? (closeValue >= openValue ? UpBrushDX : DownBrushDX);
				if (!(b is SolidColorBrush))
					TransformBrush(b, rect);
				RenderTarget.FillRectangle(rect, b);

				b = overriddenBrush ?? outlineStroke.BrushDX;
				if (!(b is SolidColorBrush))
					TransformBrush(b, rect);
				RenderTarget.DrawRectangle(rect, overriddenOutlineBrush ?? outlineStroke.BrushDX, outlineStroke.Width, outlineStroke.StrokeStyle);
			}
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleOpenClose;
				ChartStyleType	= ChartStyleType.OpenClose;
				BarWidth		= 3;
			}
			else if (State == State.Configure)
			{
				SetPropertyName("BarWidth",		Custom.Resource.NinjaScriptChartStyleBarWidth);
				SetPropertyName("DownBrush",	Custom.Resource.NinjaScriptChartStyleOpenCloseDownBarsColor);
				SetPropertyName("UpBrush",		Custom.Resource.NinjaScriptChartStyleOpenCloseUpBarsColor);
				SetPropertyName("Stroke",		Custom.Resource.NinjaScriptChartStyleOpenCloseUpBarsOutline);
				SetPropertyName("Stroke2",		Custom.Resource.NinjaScriptChartStyleOpenCloseDownBarsOutline);
			}
		}
	}
}

#region Using declarations

using System.ComponentModel.DataAnnotations;
using NinjaTrader.Data;
using NinjaTrader.Gui.Chart;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Windows.Media;
#endregion

namespace NinjaTrader.NinjaScript.ChartStyles
{
	public class MountainStyle : ChartStyle
	{
		private object								icon;

		public override int GetBarPaintWidth(int barWidth)
		{
			return 1 + 2 * (barWidth - 1) + 2 * barWidth;
		}

		public override object Icon
		{
			get { return icon ?? (icon = NinjaTrader.Gui.Tools.Icons.ChartMountainChart); }
		}

		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolAreaOpacity", GroupName = "NinjaScriptGeneral")]
		public int Opacity { get; set; }

		public override void OnRender(ChartControl chartControl, ChartScale chartScale, ChartBars chartBars)
		{
			Bars bars = chartBars.Bars;

			if (chartBars.FromIndex > 0)
				chartBars.FromIndex--;

			SharpDX.Direct2D1.PathGeometry		lineGeometry	= new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
			AntialiasMode						oldAliasMode	= RenderTarget.AntialiasMode;
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
			RenderTarget.AntialiasMode	= AntialiasMode.PerPrimitive;
			RenderTarget.DrawGeometry(lineGeometry, UpBrushDX, (float)Math.Max(1, chartBars.Properties.ChartStyle.BarWidth));
			lineGeometry.Dispose();

			SharpDX.Direct2D1.SolidColorBrush	fillOutline		= new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Transparent);
			SharpDX.Direct2D1.PathGeometry		fillGeometry	= new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
			GeometrySink						fillSink			= fillGeometry.Open();
			fillSink.BeginFigure(new Vector2(chartControl.GetXByBarIndex(chartBars, chartBars.FromIndex > -1 ? chartBars.FromIndex : 0), chartScale.GetYByValue(chartScale.MinValue)), FigureBegin.Filled);
			float fillx = float.NaN;
			for (int idx = chartBars.FromIndex; idx <= chartBars.ToIndex; idx++)
			{
				double	closeValue	= bars.GetClose(idx);
				float	close		= chartScale.GetYByValue(closeValue);
				fillx				= chartControl.GetXByBarIndex(chartBars, idx);
				fillSink.AddLine(new Vector2(fillx, close));
			}
			if (!double.IsNaN(fillx))
				fillSink.AddLine(new Vector2(fillx, chartScale.GetYByValue(chartScale.MinValue)));

			fillSink.EndFigure(FigureEnd.Open);
			fillSink.Close();
			DownBrushDX.Opacity	= Opacity / 100f;
			if (!(DownBrushDX is SharpDX.Direct2D1.SolidColorBrush))
				TransformBrush(DownBrushDX, new RectangleF(0, 0, (float) chartScale.Width, (float) chartScale.Height));
			RenderTarget.FillGeometry(fillGeometry, DownBrushDX);
			RenderTarget.DrawGeometry(fillGeometry, fillOutline, (float)chartBars.Properties.ChartStyle.BarWidth);
			fillOutline.Dispose();
			RenderTarget.AntialiasMode = oldAliasMode;
			fillGeometry.Dispose();
		}

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Name			= Custom.Resource.NinjaScriptChartStyleMountain;
				ChartStyleType	= ChartStyleType.Mountain;

				UpBrush			= Brushes.DimGray;
				DownBrush		= Brushes.DimGray;
				BarWidth		= 1;
				Opacity			= 50;

			}
			else if (State == State.Configure)
			{
				Properties.Remove(Properties.Find("Stroke", true));
				Properties.Remove(Properties.Find("Stroke2", true));

				SetPropertyName("UpBrush",		Custom.Resource.NinjaScriptChartStyleMountainOutline);
				SetPropertyName("DownBrush",	Custom.Resource.NinjaScriptChartStyleMountainColor);
			}
		}
	}
}

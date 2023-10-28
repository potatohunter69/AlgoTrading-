/* Supply System Automated Trading System Youtube Jacob Amaral
Original Author Ninjacators - www.ninjacators.com Edited By - Jacob Amaral
Please test yourself to make it meets your personalized trading goals.
*/
#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Data;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
#endregion


namespace NinjaTrader.NinjaScript.Strategies
{

    public class SupplySystemStrat : Strategy
    {
        #region Properties

        [NinjaScriptProperty]
        [Display(GroupName = "Variables", Order = 1, Name = "Start Time"), Range(0, 2359)]
        public int StartTime { get; set; }

        [NinjaScriptProperty]
        [Display(GroupName = "Variables", Order = 12, Name = "End Time"), Range(0, 2359)]
        public int EndTime { get; set; }

        [NinjaScriptProperty]
        [Display(GroupName = "Variables", Order = 31, Name = "Trade Long")]
        public bool TradeLong { get; set; }

        [NinjaScriptProperty]
        [Display(GroupName = "Variables", Order = 32, Name = "Trade Short")]
        public bool TradeShort { get; set; }

        [NinjaScriptProperty]
        [Display(GroupName = "Variables", Order = 51, Name = " Stop Loss In Ticks"), Range(0, int.MaxValue)]
        public int StopLossTicks { get; set; }

        [NinjaScriptProperty]
        [Display(GroupName = "Variables", Order = 62, Name = " Profit Target In Ticks"), Range(0, int.MaxValue)]
        public int ProfitTargetTicks { get; set; }
        #endregion

        #region Area
        [XmlIgnore]
        [Display(GroupName = "Area", Order = 21, Name = "Supply Color")]
        public Brush LongDemandColor { get; set; }
        [Browsable(false)]
        public string LongDemandColorSerialize { get { return Serialize.BrushToString(LongDemandColor); } set { LongDemandColor = Serialize.StringToBrush(value); } }

        [XmlIgnore]
        [Display(GroupName = "Area", Order = 22, Name = "Demand Color")]
        public Brush ShortSupplyColor { get; set; }
        [Browsable(false)]
        public string ShortSupplyColorSerialize { get { return Serialize.BrushToString(ShortSupplyColor); } set { ShortSupplyColor = Serialize.StringToBrush(value); } }

        [Display(GroupName = "Area", Order = 23, Name = "Active Zone Opacity"), Range(0, 100)]
        public int ActiveAreaOpacity { get; set; }

        [Display(GroupName = "Area", Order = 24, Name = "Historical Zone Opacity"), Range(0, 100)]
        public int BrokenAreaOpacity { get; set; }

        [Display(GroupName = "Area", Order = 25, Name = "Zone Outline Width"), Range(1, int.MaxValue)]
        public int LineWidth { get; set; }

        #endregion

        #region Classes

        public class Zone
        {
            public double high = 0.0;		// high
            public double low = 0.0;   		// low
            public int b = 0;     			// bar
            public int e = 0;     			// end
            public string type = "";    	// type
            public string c = "";    		// context
            public bool flipped = false; 	// flipped
            public bool active = true;      // active

            public int touches;

            public Zone(double l, double h, int b, string t, string c, bool a)
            {
                this.low = l;
                this.high = h;
                this.b = b;
                this.type = t;
                this.c = c;
                this.active = a;

                this.touches = 0;
            }
        }

        #endregion

        #region Variables

        private int barIndex = 0;
        private int atrPeriod = 10;

        private int currHiBar, currLoBar, prevHiBar, prevLoBar = 0;
        private double currHiVal, currLoVal, prevHiVal, prevLoVal = 0;
        private double currLoCon, currHiCon;

        private List<Zone> Area;

        private double atr;

        private float activeLineOpacity = 0.50f;
        private float brokenLineOpacity = 0.10f;

        private Order BuyOrder;
        private Order SellOrder;

        private Order StopLoss;
        private Order ProfitTarget1;
        private Order ProfitTarget2;
        private Order Reversal;

        private int buy_zone_idx;
        private int sell_zone_idx;

        private DateTime entry_time;
        private int entry_bar;

        private double last_tick_price;

        #endregion

        #region OnStateChange

        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Description = @"In this system, traders look for areas on the price chart where there is a significant amount of supply (sellers) or demand (buyers). These levels are identified by looking for areas where the price has previously reacted or reversed, indicating a significant imbalance between buyers and sellers.";
                Name = "SupplySystem";
                Calculate = Calculate.OnBarClose;
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = false;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlat;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 20;

                IsInstantiatedOnEachOptimizationIteration = true;

                #region Defaults

                StartTime = 1500;
                EndTime = 2300;
                TradeLong = true;
                TradeShort = true;
                StopLossTicks = 20;
                ProfitTargetTicks = 20;     

                LongDemandColor = Brushes.SpringGreen;
                ShortSupplyColor = Brushes.MediumPurple;
                ActiveAreaOpacity = 18;
                BrokenAreaOpacity = 7;
                LineWidth = 1;

                #endregion
            }
            else if (State == State.Configure)
            {
                SetProfitTarget(CalculationMode.Ticks, ProfitTargetTicks);
                SetStopLoss(CalculationMode.Ticks, StopLossTicks);
            }
            else if (State == State.DataLoaded)
            {
                Area = new List<Zone>();
            }
        }

        #endregion

        #region OnBarUpdate

        protected override void OnBarUpdate()
        {
            #region Areas

            if (BarsInProgress == barIndex)
            {
                if (CurrentBars[barIndex] < 20)
                    return;


                atr = Instrument.MasterInstrument.RoundToTickSize(ATR(BarsArray[barIndex], atrPeriod)[0] * 1.25);

                checkSupply();
                checkDemand();
                updateArea();

                prevHiBar = currHiBar;
                prevHiVal = currHiVal;

                prevLoBar = currLoBar;
                prevLoVal = currLoVal;
            }

            #endregion

            if (Position.MarketPosition == MarketPosition.Flat && CheckTime(Time[0]))
            {
                double buy_price = double.MinValue;
                double sell_price = double.MaxValue;

                int _buy_zone_idx = -1;
                int _sell_zone_idx = -1;

                for (int i = Area.Count - 1; i >= 0; i--)
                {
                    if (!Area[i].active)
                        continue;
                    if (Area[i].type == "d" && Close[0] - Area[i].high > TickSize)
                    {
                        if (buy_price.ApproxCompare(Area[i].high) < 0)
                        {
                                _buy_zone_idx = i;
                                buy_price = Area[i].high;
                            
                        }
                    }

                    if (Area[i].type == "s" && Area[i].low - Close[0] > TickSize)
                    {
                        if (sell_price.ApproxCompare(Area[i].low) > 0)
                        {
                                _sell_zone_idx = i;
                                sell_price = Area[i].low;
                            
                        }
                    }
                }

                if (buy_price != double.MinValue && TradeLong)
                {
                    {
                        EnterLongLimit(buy_price);
                    }
                }

                if (sell_price != double.MaxValue && TradeShort)
                {
                    {
                        EnterShortLimit(sell_price);
                    }
                }


            }

        }

        #endregion

        #region Check Time
        private bool CheckTime(DateTime time)
        {
            if (StartTime < EndTime)
                return (ToTime(time) > StartTime * 100 && ToTime(time) <= EndTime * 100);
            else
                return (ToTime(time) > StartTime * 100 || ToTime(time) <= EndTime * 100);
        }
        #endregion

        #region Area


        private void checkSupply()
        {
            // Regular
            if (currHiVal != prevHiVal)
            {
                if (MAX(Highs[barIndex], currHiBar)[0] <= currHiVal)
                {
                    if (!activeSupplyZoneExists(currHiVal) && isValidSupplyZone(currHiVal, currLoVal))
                    {
                        double br = Highs[barIndex][currHiBar] - Lows[barIndex][currHiBar];
                        double zr = Highs[barIndex][currHiBar] - Math.Min(Opens[barIndex][currHiBar], Closes[barIndex][currHiBar]);
                        double zl = (zr > atr) ? Math.Max(Opens[barIndex][currHiBar], Closes[barIndex][currHiBar]) : Math.Min(Opens[barIndex][currHiBar], Closes[barIndex][currHiBar]);
                        double zh = currHiVal;
                        int zb = CurrentBars[barIndex] - currHiBar;
                        string zt = "s";
                        string zc = "r";
                        bool za = true;

                        zl = (zh - zl < TickSize) ? (zh - TickSize) : zl;

                        Area.Add(new Zone(zl, zh, zb, zt, zc, za));
                    }
                }
            }

            // Continuation
            int con = isDnContinuation();

            if (con != -1)
            {
                currHiCon = MAX(Highs[barIndex], con)[0];
                currLoCon = MIN(Lows[barIndex], con)[1];

                if (currHiCon - currLoCon <= atr)
                {
                    if (!activeSupplyZoneExists(currHiCon) && isValidSupplyZone(currHiCon, currLoCon))
                    {
                        double zl = currLoCon;
                        double zh = currHiCon;
                        int zb = CurrentBars[barIndex] - (con);
                        string zt = "s";
                        string zc = "c";
                        bool za = true;

                        zl = (zh - zl < TickSize) ? (zh - TickSize) : zl;

                        Area.Add(new Zone(zl, zh, zb, zt, zc, za));
                    }
                }
            }
        }

        #region Supply

        private int getNextSupplyZone(double price)
        {
            double min = double.MaxValue;
            int ind = -1;

            for (int i = 0; i < Area.Count; i++)
            {
                if (Area[i].active == true && Area[i].type == "s")
                {
                    if (Area[i].low > price && Area[i].low < min)
                    {
                        ind = i;
                    }
                }
            }

            return ind;
        }

        private bool activeSupplyZoneExists(double hi)
        {
            bool exists = false;

            for (int i = 0; i < Area.Count; i++)
            {
                if (Area[i].active == true && Area[i].type == "s")
                {
                    if (Area[i].high == hi)
                    {
                        exists = true;
                        break;
                    }
                }
            }

            return exists;
        }

        private bool isValidSupplyZone(double hi, double lo)
        {
            bool valid = true;

            for (int i = 0; i < Area.Count; i++)
            {
                if (Area[i].active == true && Area[i].type == "s")
                {
                    if ((hi <= Area[i].high && hi >= Area[i].low) ||
                        (lo <= Area[i].high && lo >= Area[i].low))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            return valid;
        }

        private int isDnContinuation()
        {
            bool val = true;
            int bar = -1;

            for (int i = 10; i >= 2; i--)
            {
                if (isDnMove(i))
                {
                    val = true;

                    for (int j = i; j >= 1; j--)
                    {
                        if (!isInsideDnBar(j, i))
                        {
                            val = false;
                            break;
                        }
                    }

                    if (val)
                    {
                        val = false;

                        for (int j = i; j >= 1; j--)
                        {
                            if (Closes[barIndex][j] >= Opens[barIndex][j])
                            {
                                val = true;
                                break;
                            }
                        }
                    }

                    if (val)
                    {
                        if (isInsideDnBreakoutBar(0, i))
                        {
                            bar = i;
                            break;
                        }
                    }
                }
            }

            return bar;
        }

        private bool isDnMove(int index)
        {
            if (Closes[barIndex][index] < KeltnerChannel(BarsArray[barIndex], 1.0, 10).Lower[index] ||
                Closes[barIndex][index + 1] < KeltnerChannel(BarsArray[barIndex], 1.0, 10).Lower[index + 1] ||
                Closes[barIndex][index + 2] < KeltnerChannel(BarsArray[barIndex], 1.0, 10).Lower[index + 2])
            {
                if (isDnBar(index) && isDnBar(index + 1) && isDnBar(index + 2))
                    return true;

                if (isStrongDnBar(index))
                    return true;
            }

            return false;
        }

        private bool isDnBar(int index)
        {
            if (Closes[barIndex][index] < Opens[barIndex][index] &&
                Closes[barIndex][index] < Closes[barIndex][index + 1] &&
                Highs[barIndex][index] < Highs[barIndex][index + 1] &&
                Lows[barIndex][index] < Lows[barIndex][index + 1])
            {
                return true;
            }

            return false;
        }

        private bool isStrongDnBar(int index)
        {
            if (Closes[barIndex][index] < Opens[barIndex][index] &&
                Closes[barIndex][index] < Closes[barIndex][index + 1] &&
                Highs[barIndex][index] < Highs[barIndex][index + 1] &&
                Lows[barIndex][index] < Lows[barIndex][index + 1] &&
                Lows[barIndex][index] < MIN(Lows[barIndex], 3)[index + 1] &&
                Highs[barIndex][index] - Lows[barIndex][index] > ATR(BarsArray[barIndex], atrPeriod)[1])
            {
                return true;
            }

            if (Closes[barIndex][index] < Opens[barIndex][index] &&
                Closes[barIndex][index] < Closes[barIndex][index + 1] &&
                Closes[barIndex][index] < MIN(Lows[barIndex], 3)[index + 1] &&
                Highs[barIndex][index] - Lows[barIndex][index] > ATR(BarsArray[barIndex], atrPeriod)[1] * 2)
            {
                return true;
            }

            return false;
        }

        private bool isInsideDnBar(int indexOne, int indexTwo)
        {
            if (Highs[barIndex][indexOne] <= Highs[barIndex][indexTwo] &&
                Math.Min(Opens[barIndex][indexOne], Closes[barIndex][indexOne]) >= Lows[barIndex][indexTwo])
            {
                return true;
            }

            return false;
        }

        private bool isInsideDnBreakoutBar(int indexOne, int indexTwo)
        {
            if (Highs[barIndex][indexOne] <= Highs[barIndex][indexTwo] &&
                Closes[barIndex][indexOne] <= MIN(Lows[barIndex], indexTwo - indexOne)[1] &&
                Lows[barIndex][indexOne] < MIN(Lows[barIndex], indexTwo - indexOne)[1])
            {
                return true;
            }

            return false;
        }

        #endregion

        private void checkDemand()
        {
            // Regular
            if (currLoVal != prevLoVal)
            {
                if (MIN(Lows[barIndex], currLoBar)[0] >= currLoVal)
                {
                    if (!activeDemandZoneExists(currLoVal) && isValidDemandZone(currHiVal, currLoVal))
                    {
                        double br = Highs[barIndex][currHiBar] - Lows[barIndex][currHiBar];
                        double zr = Math.Max(Opens[barIndex][currLoBar], Closes[barIndex][currLoBar]) - Lows[barIndex][currHiBar];
                        double zl = currLoVal;
                        double zh = (zr > atr) ? Math.Min(Opens[barIndex][currLoBar], Closes[barIndex][currLoBar]) : Math.Max(Opens[barIndex][currLoBar], Closes[barIndex][currLoBar]);
                        int zb = CurrentBars[barIndex] - currLoBar;
                        string zt = "d";
                        string zc = "r";
                        bool za = true;

                        zh = (zh - zl < TickSize) ? (zl + TickSize) : zh;

                        Area.Add(new Zone(zl, zh, zb, zt, zc, za));
                    }
                }
            }

            // Continuation
            int con = isUpContinuation();

            if (con != -1)
            {
                currHiCon = MAX(Highs[barIndex], con)[1];
                currLoCon = MIN(Lows[barIndex], con)[0];

                if (currHiCon - currLoCon <= atr)
                {
                    if (!activeDemandZoneExists(currLoCon) && isValidDemandZone(currHiCon, currLoCon))
                    {
                        double zl = currLoCon;
                        double zh = currHiCon;
                        int zb = CurrentBars[barIndex] - con;
                        string zt = "d";
                        string zc = "c";
                        bool za = true;

                        zh = (zh - zl < TickSize) ? (zl + TickSize) : zh;

                        Area.Add(new Zone(zl, zh, zb, zt, zc, za));
                    }
                }
            }
        }

        #region Demand

        private int getNextDemandZone(double price)
        {
            double max = double.MinValue;
            int ind = -1;

            for (int i = 0; i < Area.Count; i++)
            {
                if (Area[i].active == true && Area[i].type == "d")
                {
                    if (Area[i].low < price && Area[i].high > max)
                        ind = i;
                }
            }

            return ind;
        }

        private bool activeDemandZoneExists(double lo)
        {
            bool exists = false;

            for (int i = 0; i < Area.Count; i++)
            {
                if (Area[i].active == true && Area[i].type == "d")
                {
                    if (Area[i].low == lo)
                    {
                        exists = true;
                        break;
                    }
                }
            }

            return exists;
        }

        private bool isValidDemandZone(double hi, double lo)
        {
            bool valid = true;

            for (int i = 0; i < Area.Count; i++)
            {
                if (Area[i].active == true && Area[i].type == "d")
                {
                    if ((lo >= Area[i].low && lo <= Area[i].high) ||
                        (hi >= Area[i].low && hi <= Area[i].high))
                    {
                        valid = false;
                        break;
                    }
                }
            }

            return valid;
        }

        private int isUpContinuation()
        {
            bool val = true;
            int bar = -1;

            for (int i = 10; i >= 2; i--)
            {
                if (isUpMove(i))
                {
                    val = true;
                    for (int j = i - 1; j >= 1; j--)
                    {
                        if (!isInsideUpBar(j, i))
                        {
                            val = false;
                            break;
                        }
                    }

                    if (val)
                    {
                        val = false;

                        for (int j = i; j >= 1; j--)
                        {
                            if (Closes[barIndex][j] <= Opens[barIndex][j])
                            {
                                val = true;
                                break;
                            }
                        }
                    }

                    if (val)
                    {
                        if (isInsideUpBreakoutBar(0, i))
                        {
                            bar = i;
                            break;
                        }
                    }
                }
            }

            return bar;
        }

        private bool isUpMove(int index)
        {
            if (Closes[barIndex][index] > KeltnerChannel(BarsArray[barIndex], 1.0, 10).Upper[index] ||
                Closes[barIndex][index + 1] > KeltnerChannel(BarsArray[barIndex], 1.0, 10).Upper[index + 1] ||
                Closes[barIndex][index + 2] > KeltnerChannel(BarsArray[barIndex], 1.0, 10).Upper[index + 2])
            {
                if (isUpBar(index) && isUpBar(index + 1) && isUpBar(index + 2))
                    return true;

                if (isStrongUpBar(index))
                    return true;
            }

            return false;
        }

        private bool isUpBar(int index)
        {
            return (Closes[barIndex][index] > Opens[barIndex][index] &&
                    Closes[barIndex][index] > Closes[barIndex][index + 1] &&
                    Highs[barIndex][index] > Highs[barIndex][index + 1] &&
                    Lows[barIndex][index] > Lows[barIndex][index + 1]);
        }

        private bool isStrongUpBar(int index)
        {
            if (Closes[barIndex][index] > Opens[barIndex][index] &&
                Closes[barIndex][index] > Closes[barIndex][index + 1] &&
                Highs[barIndex][index] > Highs[barIndex][index + 1] &&
                Lows[barIndex][index] > Lows[barIndex][index + 1] &&
                Highs[barIndex][index] > MAX(Highs[barIndex], 3)[index + 1] &&
                Highs[barIndex][index] - Lows[barIndex][index] > ATR(BarsArray[barIndex], atrPeriod)[1])
            {
                return true;
            }

            if (Closes[barIndex][index] > Opens[barIndex][index] &&
                Closes[barIndex][index] > Closes[barIndex][index + 1] &&
                Closes[barIndex][index] > MAX(Highs[barIndex], 3)[index + 1] &&
                Highs[barIndex][index] - Lows[barIndex][index] > ATR(BarsArray[barIndex], atrPeriod)[1] * 2)
            {
                return true;
            }

            return false;
        }

        private bool isInsideUpBar(int indexOne, int indexTwo)
        {
            return (Lows[barIndex][indexOne] >= Lows[barIndex][indexTwo] &&
                    Highs[barIndex][indexOne] <= Highs[barIndex][indexTwo] &&
                    Math.Max(Opens[barIndex][indexOne], Closes[barIndex][indexOne]) <= Highs[barIndex][indexTwo]);
        }

        private bool isInsideUpBreakoutBar(int indexOne, int indexTwo)
        {
            return (Lows[barIndex][indexOne] >= Lows[barIndex][indexTwo] &&
                    Closes[barIndex][indexOne] >= MAX(Highs[barIndex], indexTwo - indexOne)[1] &&
                    Highs[barIndex][indexOne] > MAX(Highs[barIndex], indexTwo - indexOne)[1]);
        }

        #endregion

        private void updateArea()
        {
            for (int i = 0; i < Area.Count; i++)
            {
                if (Area[i].active == true)
                {
                    if (Area[i].type == "s")
                    {
                        if (Highs[barIndex][0] > Area[i].high)
                        {
                            Area[i].e = CurrentBars[barIndex];
                            Area[i].active = false;
                        }
                    }

                    if (Area[i].type == "d")
                    {
                        if (Lows[barIndex][0] < Area[i].low)
                        {
                            Area[i].e = CurrentBars[barIndex];
                            Area[i].active = false;
                        }
                    }
                }
            }
        }

        #endregion

        #region OnRender

        protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
        {
            if (Bars == null || Bars.Instrument == null || IsInHitTest)
                return;

            base.OnRender(chartControl, chartScale);

            DrawArea(chartControl, chartScale);
        }

        private int findBar(Zone z)
        {
            int curr = BarsArray[0].GetBar(BarsArray[barIndex].GetTime(z.b));
            int prev = BarsArray[0].GetBar(BarsArray[barIndex].GetTime(z.b - 1));
            int rVal = curr;

            for (int i = prev; i <= curr; i++)
            {
                if (z.type == "s")
                {
                    if (BarsArray[0].GetHigh(i) == z.high)
                    {
                        rVal = i;
                        break;
                    }
                }

                if (z.type == "d")
                {
                    if (BarsArray[0].GetLow(i) == z.low)
                    {
                        rVal = i;
                        break;
                    }
                }
            }

            return rVal;
        }

        private void DrawArea(ChartControl chartControl, ChartScale chartScale)
        {
            if (Area.Count == 0)
                return;

            SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;
            RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;

            SharpDX.Direct2D1.Brush longdemandBrush = LongDemandColor.ToDxBrush(RenderTarget);
            SharpDX.Direct2D1.Brush shortsupplyBrush = ShortSupplyColor.ToDxBrush(RenderTarget);

            float x1 = 0;
            float x2 = 0;
            float y1 = 0;
            float y2 = 0;

            for (int i = 0; i < Area.Count; i++)
            {
                if (barIndex == 0)
                {
                    x1 = ChartControl.GetXByBarIndex(ChartBars, Area[i].b);
                    x2 = Area[i].active ? chartControl.CanvasRight : ChartControl.GetXByBarIndex(ChartBars, Area[i].e);
                }
                else
                {
                    x1 = ChartControl.GetXByBarIndex(ChartBars, findBar(Area[i]));
                    x2 = Area[i].active ? chartControl.CanvasRight : ChartControl.GetXByBarIndex(ChartBars, ChartBars.GetBarIdxByTime(chartControl, BarsArray[barIndex].GetTime(Area[i].e)));
                }

                if (x2 < x1)
                    continue;

                if (x2 < 0 || x1 > chartControl.CanvasRight)
                    continue;

                y1 = chartScale.GetYByValue(Area[i].high);
                y2 = chartScale.GetYByValue(Area[i].low);

                if (y2 < 0 || y1 > ChartPanel.Height)
                    continue;

                // area
                SharpDX.RectangleF rect = new SharpDX.RectangleF(x1, y1, Math.Abs(x2 - x1), Math.Abs(y1 - y2) - 1);

                SharpDX.Vector2 p1 = new SharpDX.Vector2(x1, y1);
                SharpDX.Vector2 p2 = new SharpDX.Vector2(x2, y1);

                SharpDX.Vector2 p3 = new SharpDX.Vector2(x1, y2);
                SharpDX.Vector2 p4 = new SharpDX.Vector2(x2, y2);

                if (Area[i].active)
                {
                    if (Area[i].type == "d")
                    {
                        longdemandBrush.Opacity = (float)ActiveAreaOpacity / 100;
                        RenderTarget.FillRectangle(rect, longdemandBrush);

                        longdemandBrush.Opacity = activeLineOpacity;
                        RenderTarget.DrawLine(p1, p2, longdemandBrush, LineWidth);
                        RenderTarget.DrawLine(p3, p4, longdemandBrush, LineWidth);
                    }

                    if (Area[i].type == "s")
                    {
                        shortsupplyBrush.Opacity = (float)ActiveAreaOpacity / 100;
                        RenderTarget.FillRectangle(rect, shortsupplyBrush);

                        shortsupplyBrush.Opacity = activeLineOpacity;
                        RenderTarget.DrawLine(p1, p2, shortsupplyBrush, LineWidth);
                        RenderTarget.DrawLine(p3, p4, shortsupplyBrush, LineWidth);
                    }
                }
                else
                {
                    if (Area[i].type == "d")
                    {
                        longdemandBrush.Opacity = (float)BrokenAreaOpacity / 100;
                        RenderTarget.FillRectangle(rect, longdemandBrush);

                        longdemandBrush.Opacity = brokenLineOpacity;
                        RenderTarget.DrawLine(p1, p2, longdemandBrush, LineWidth);
                        RenderTarget.DrawLine(p3, p4, longdemandBrush, LineWidth);
                    }

                    if (Area[i].type == "s")
                    {
                        shortsupplyBrush.Opacity = (float)BrokenAreaOpacity / 100;
                        RenderTarget.FillRectangle(rect, shortsupplyBrush);

                        shortsupplyBrush.Opacity = brokenLineOpacity;
                        RenderTarget.DrawLine(p1, p2, shortsupplyBrush, LineWidth);
                        RenderTarget.DrawLine(p3, p4, shortsupplyBrush, LineWidth);
                    }
                }
            }

            RenderTarget.AntialiasMode = oldAntialiasMode;

            longdemandBrush.Dispose();
            shortsupplyBrush.Dispose();
        }

        #endregion

    }

}



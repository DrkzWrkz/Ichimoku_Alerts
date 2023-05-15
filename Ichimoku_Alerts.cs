#region Assembly ATAS.Indicators.Technical, Version=6.0.2.311, Culture=neutral, PublicKeyToken=330427d8594115c7
// C:\Program Files (x86)\ATAS Platform\ATAS.Indicators.Technical.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Media;
using ATAS.Indicators.Technical.Properties;
using OFT.Attributes;
using OFT.Rendering;
using OFT.Rendering.Tools;
using Utils.Common.Localization;
using ATAS.Indicators;
using ATAS.Indicators.Drawing;
using System.Drawing;
using Color = System.Windows.Media.Color;
using OFT.Rendering.Context;
using OFT.Rendering.Settings;

namespace ATAS.Indicators.Technical
{


    [DisplayName("Ichimoku w/ Alerts")]

    public class Ichimoku_Alerts : Indicator
    {


        private bool tk_TriggeredBullish = false;
        private bool tk_TriggeredBearish = false;
        private bool lagging_Bullish_Triggered = false;
        private bool lagging_Bearish_Triggered = false;
        private bool bullishCloud_Triggered = false;
        private bool bearishCloud_Triggered = false;
        private bool above_Kumo_Triggered = false;
        private bool below_Kumo_Triggered = false;


        private readonly Highest _baseHigh = new Highest
        {
            Period = 26
        };

        private readonly ValueDataSeries _baseLine = new ValueDataSeries("Base")
        {
            Color = Colors.Red
        };

        private readonly Lowest _baseLow = new Lowest
        {
            Period = 26
        };

        private readonly Highest _conversionHigh = new Highest
        {
            Period = 9
        };

        private readonly ValueDataSeries _conversionLine = new ValueDataSeries("Conversion")
        {
            Color = Colors.AliceBlue
        };

        private readonly Lowest _conversionLow = new Lowest
        {
            Period = 9
        };

        private readonly RangeDataSeries _downSeries = new RangeDataSeries("Down")
        {
            RangeColor = Colors.Red
        };

        private readonly ValueDataSeries _laggingSpan = new ValueDataSeries("Lagging Span")
        {
            Color = Colors.Green
        };

        private readonly ValueDataSeries _leadLine1 = new ValueDataSeries("Lead1")
        {
            Color = Colors.Green
        };

        private readonly ValueDataSeries _leadLine2 = new ValueDataSeries("Lead2")
        {
            Color = Colors.Red
        };

        private readonly Highest _spanHigh = new Highest
        {
            Period = 52
        };

        private readonly Lowest _spanLow = new Lowest
        {
            Period = 52
        };

        private readonly RangeDataSeries _upSeries = new RangeDataSeries("Up")
        {
            RangeColor = Colors.Green
        };
        private int _days;
        private int _displacement = 26;
        private int _targetBar;
        private bool _enable_Alerts = true;
        [Display(ResourceType = typeof(Resources), GroupName = "Calculation", Name = "DaysLookBack", Order = int.MaxValue, Description = "DaysLookBackDescription")]
        [Range(0, 10000)]
        public int Days
        {
            get
            {
                return _days;
            }
            set
            {
                _days = value;
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Tenkan-sen")]
        [Range(1, 10000)]
        public int Tenkan
        {
            get
            {
                return _conversionHigh.Period;
            }
            set
            {
                int num3 = (_conversionHigh.Period = (_conversionLow.Period = value));
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Kijun-sen")]
        [Range(1, 10000)]
        public int Kijun
        {
            get
            {
                return _baseHigh.Period;
            }
            set
            {
                int num3 = (_baseHigh.Period = (_baseLow.Period = value));
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Senkou Span B")]
        [Range(1, 10000)]
        public int Senkou
        {
            get
            {
                return _spanHigh.Period;
            }
            set
            {
                int num3 = (_spanHigh.Period = (_spanLow.Period = value));
                RecalculateValues();
            }
        }

        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Displacement")]
        [Range(1, 10000)]
        public int Displacement
        {
            get
            {
                return _displacement;
            }
            set
            {
                _displacement = value;
                RecalculateValues();
            }
        }



        [LocalizedCategory(typeof(Resources), "Settings")]
        [DisplayName("Enable Alerts")]
        public bool enable_Alerts
        {
            get
            {
                return _enable_Alerts;
            }
            set
            {
                _enable_Alerts = value;
                RecalculateValues();

            }
        }

        public Ichimoku_Alerts()
            : base(useCandles: true)
        {
            //IL_0028: Unknown result type (might be due to invalid IL or missing references)
            //IL_0074: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ab: Unknown result type (might be due to invalid IL or missing references)
            //IL_00cf: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ea: Unknown result type (might be due to invalid IL or missing references)
            //IL_0105: Unknown result type (might be due to invalid IL or missing references)
            //IL_014f: Unknown result type (might be due to invalid IL or missing references)
            Font.PropertyChanged += (a, b) => RedrawChart();
            AdditionalFont.PropertyChanged += (a, b) => RedrawChart();

            
            DenyToChangePanel = true;
            EnableCustomDrawing = true;
            SubscribeToDrawingEvents(DrawingLayouts.Historical);
            DrawAbovePrice = false;

            base.DataSeries[0] = _conversionLine;
            base.DataSeries.Add(_laggingSpan);
            base.DataSeries.Add(_baseLine);
            base.DataSeries.Add(_leadLine1);
            base.DataSeries.Add(_leadLine2);
            base.DataSeries.Add(_upSeries);
            base.DataSeries.Add(_downSeries);
        }
        private int lastBarAlert = 0;
        int closeAboveKumoCount = 0;
        int tkBullishCount = 0;
        int laggingBullishCount = 0;
        int bullishCloudCount = 0;
        int totalBullishConf = 0;
        int closeBelowKumoCount = 0;
        int tkBearishCount = 0;
        int laggingBearishCount = 0;
        int bearishCloudCount = 0;
        int totalBearishConf = 0;

        protected override void OnCalculate(int bar, decimal value)
        {
            IndicatorCandle candle = GetCandle(bar);
            if (bar == 0)
            {
                base.DataSeries.ForEach(delegate (IDataSeries x)
                {
                    x.Clear();
                });
                _targetBar = 0;
                if (_days > 0)
                {
                    int num = 0;
                    for (int num2 = base.CurrentBar - 1; num2 >= 0; num2--)
                    {
                        _targetBar = num2;
                        if (IsNewSession(num2))
                        {
                            num++;
                            if (num == _days)
                            {
                                break;
                            }
                        }
                    }

                    if (_targetBar > 0)
                    {
                        _conversionLine.SetPointOfEndLine(_targetBar - 1);
                        _laggingSpan.SetPointOfEndLine(_targetBar - _displacement);
                        _baseLine.SetPointOfEndLine(_targetBar - 1);
                        _leadLine1.SetPointOfEndLine(_targetBar + _displacement - 2);
                        _leadLine2.SetPointOfEndLine(_targetBar + _displacement - 2);
                    }
                }
            }

            _conversionHigh.Calculate(bar, candle.High);
            _conversionLow.Calculate(bar, candle.Low);
            _baseHigh.Calculate(bar, candle.High);
            _baseLow.Calculate(bar, candle.Low);
            _spanHigh.Calculate(bar, candle.High);
            _spanLow.Calculate(bar, candle.Low);
            if (bar < _targetBar)
            {
                return;
            }

            _baseLine[bar] = (_baseHigh[bar] + _baseLow[bar]) / 2m;
            _conversionLine[bar] = (_conversionHigh[bar] + _conversionLow[bar]) / 2m;
            int index = bar + Displacement;
            _leadLine1[index] = (_conversionLine[bar] + _baseLine[bar]) / 2m;
            _leadLine2[index] = (_spanHigh[bar] + _spanLow[bar]) / 2m;
            if (bar - _displacement + 1 >= 0)
            {
                int num3 = bar - _displacement;
                _laggingSpan[num3] = candle.Close;
                if (bar == base.CurrentBar - 1)
                {
                    for (int i = num3 + 1; i < base.CurrentBar; i++)
                    {
                        _laggingSpan[i] = candle.Close;
                    }
                }
            }

            if (_leadLine1[bar] == 0m || _leadLine2[bar] == 0m)
            {
                return;
            }

            if (_leadLine1[bar] > _leadLine2[bar])
            {
                _upSeries[bar].Upper = _leadLine1[bar];
                _upSeries[bar].Lower = _leadLine2[bar];
                if (_leadLine1[bar - 1] < _leadLine2[bar - 1])
                {
                    _downSeries[bar] = _upSeries[bar];
                }
            }
            else
            {
                _downSeries[bar].Upper = _leadLine2[bar];
                _downSeries[bar].Lower = _leadLine1[bar];
                if (_leadLine1[bar - 1] > _leadLine2[bar - 1])
                {
                    _upSeries[bar] = _downSeries[bar];
                }
            }

            //Alert Conditions

            bool tk_Bullish = _conversionLine[bar] > _baseLine[bar] && !tk_TriggeredBullish;
            bool tk_Bearish = _conversionLine[bar] < _baseLine[bar] && !tk_TriggeredBearish;
            bool lagging_Bullish = _laggingSpan[bar] > _leadLine1[bar - Displacement] && _laggingSpan[bar] > _leadLine2[bar - Displacement] && !lagging_Bullish_Triggered;
            bool lagging_Bearish = _laggingSpan[bar] < _leadLine1[bar - Displacement] && _laggingSpan[bar] < _leadLine2[bar - Displacement] && !lagging_Bearish_Triggered;
            bool bullishCloud = _leadLine1[bar] > _leadLine2[bar] && !bullishCloud_Triggered;
            bool bearishCloud = _leadLine1[bar] < _leadLine2[bar] && !bearishCloud_Triggered;
            bool close_Above_Kumo = _laggingSpan[bar] > _leadLine1[bar] && _laggingSpan[bar] > _leadLine2[bar] && !above_Kumo_Triggered;
            bool close_Below_Kumo = _laggingSpan[bar] < _leadLine1[bar] && _laggingSpan[bar] < _leadLine2[bar] && !below_Kumo_Triggered;


            ///////////////////////////////////////////////
            ///3rd solution for Confirmation Counter
            //////////////////////////////////////////////

            //// bullish conf counter

            void TkBullishCount()
            {
                if (tk_TriggeredBullish)
                {
                    tkBullishCount = 1;
                }
                else { tkBullishCount = 0; }

            }
            void LaggingBullishCount()
            {
                if (lagging_Bullish_Triggered)
                {
                    laggingBullishCount = 1;
                }
                else { laggingBullishCount = 0; }
            }
            void BullishCloudCount()
            {
                if (bullishCloud_Triggered)
                {
                    bullishCloudCount = 1;
                }
                else { bullishCloudCount = 0; }
            }
            void AboveCloudCount()
            {
                if (above_Kumo_Triggered)
                {
                    closeAboveKumoCount = 1;
                }
                else
                {
                    closeAboveKumoCount = 0;
                }
            }
            void CalcBullishConf()
            {
                TkBullishCount();
                LaggingBullishCount();
                BullishCloudCount();
                AboveCloudCount();
                totalBullishConf = bullishCloudCount + tkBullishCount + closeAboveKumoCount + laggingBullishCount;
                AddAlert("Alert1", TimeFrame, totalBullishConf + " bullish condition(s) currently present", Colors.Black, Colors.Green);
            }

            ///bearish conf counter


            void TkBearishCount()
            {
                if (tk_TriggeredBearish)
                    tkBearishCount = 1;
                else
                    tkBearishCount = 0;
            }

            void LaggingBearishCount()
            {
                if (lagging_Bearish_Triggered)
                    laggingBearishCount = 1;
                else
                    laggingBearishCount = 0;
            }

            void BearishCloudCount()
            {
                if (bearishCloud_Triggered)
                    bearishCloudCount = 1;
                else
                    bearishCloudCount = 0;
            }

            void BelowCloudCount()
            {
                if (below_Kumo_Triggered)
                    closeBelowKumoCount = 1;
                else
                    closeBelowKumoCount = 0;
            }

            void CalcBearishConf()
            {
                TkBearishCount();
                LaggingBearishCount();
                BearishCloudCount();
                BelowCloudCount();
                totalBearishConf = closeBelowKumoCount + tkBearishCount + laggingBearishCount + bearishCloudCount;
                AddAlert("Alert1", TimeFrame, totalBearishConf + " bearish condition(s) currently present", Colors.Black, Colors.Green);
            }

            //// Alert Conditions
            if (enable_Alerts && CurrentBar - 1 == bar && lastBarAlert != bar)
            {

                if (close_Above_Kumo)
                {
                    lastBarAlert = bar;
                    above_Kumo_Triggered = true;
                    below_Kumo_Triggered = false;
                    AddAlert("Alert1", TimeFrame , "Close Above CLoud" , Colors.Black, Colors.Green);
                    CalcBullishConf();
                    CalcBearishConf();
                }
                else if (close_Below_Kumo)
                {
                    lastBarAlert = bar;
                    above_Kumo_Triggered = false;
                    below_Kumo_Triggered = true;
                    AddAlert("Alert1", TimeFrame , "Close Below CLoud", Colors.Black, Colors.Green);
                    CalcBullishConf();
                    CalcBearishConf();
                }

                if (tk_Bullish)
                {
                    lastBarAlert = bar;
                    tk_TriggeredBullish = true;
                    tk_TriggeredBearish = false;
                    AddAlert("Alert1", TimeFrame, "Bullish Crossover", Colors.Black, Colors.Green);
                    CalcBullishConf();
                    CalcBearishConf();

                }
                else if (tk_Bearish)
                {
                    lastBarAlert = bar;
                    tk_TriggeredBullish = false;
                    tk_TriggeredBearish = true;
                    AddAlert("Alert1", TimeFrame, "Bearish Crossover", Colors.Black, Colors.Green);
                    CalcBullishConf();
                    CalcBearishConf();

                }


                if (lagging_Bullish)
                {
                    lastBarAlert = bar;
                    lagging_Bullish_Triggered = true;
                    lagging_Bearish_Triggered = false;
                    AddAlert("Alert1", TimeFrame, "Lagging Span is Bullish", Colors.Black, Colors.Green);
                    CalcBullishConf();
                    CalcBearishConf();

                }
                else if (lagging_Bearish)
                {
                    lastBarAlert = bar;
                    lagging_Bullish_Triggered = false;
                    lagging_Bearish_Triggered = true;
                    AddAlert("Alert1", TimeFrame, "Lagging Span is Bearish", Colors.Black, Colors.Green);
                    CalcBullishConf();
                    CalcBearishConf();

                }

                if (bullishCloud)
                {
                    lastBarAlert = bar;
                    bullishCloud_Triggered = true;
                    bearishCloud_Triggered = false;
                    AddAlert("Alert1", TimeFrame, "Bullish Cloud", Colors.Black, Colors.Green);
                    CalcBullishConf();
                    CalcBearishConf();

                }
                else if (bearishCloud)
                {
                    lastBarAlert = bar;
                    bullishCloud_Triggered = false;
                    bearishCloud_Triggered = true;
                    AddAlert("Alert1", TimeFrame, "Bearish Cloud", Colors.Black, Colors.Green);
                    CalcBullishConf();
                    CalcBearishConf();


                }
            }
        }
        ////////////////////////////
        ///display logic
        /////////////////////////////
        public enum Location
        {
            [Display(Name = "Center")]
            Center,

            [Display(Name = "TopLeft")]
            TopLeft,

            [Display(Name = "TopRight")]
            TopRight,

            [Display(Name = "BottomLeft")]
            BottomLeft,

            [Display(Name = "BottomRight")]
            BottomRight
        }



        #region Properties

        [Display(Name = "Color", GroupName = "Display Settings", Order = 10)]
        public Color TextColor { get; set; } = Color.FromArgb(255, 225, 225, 225);

        [Display(Name = "TextLocation", GroupName = "Display Settings", Order = 20)]
        public Location TextLocation { get; set; } = Location.TopRight;

        [Display(Name = "HorizontalOffset", GroupName = "Display Settings", Order = 30)]
        public int HorizontalOffset { get; set; } = -302;

        [Display(Name = "VerticalOffset", GroupName = "Display Settings", Order = 40)]
        public int VerticalOffset { get; set; } = 11;

        
        public FontSetting Font { get; set; } = new FontSetting { Size = 60, Bold = true };

  
        [Display(Name = "Font", GroupName = "Display Settings", Order = 90)]

        public FontSetting AdditionalFont { get; set; } = new FontSetting { Size = 8 };

        [Display(Name = "VerticalOffset", GroupName = "Display Settings", Order = 90)]
        public int confTextYOffset { get; set; } = -39;

        [Display(Name = "Show Active Signals", GroupName = "Display Settings", Order = 100)]
        public bool ShowCondText { get; set; } = true;

        [Display(Name = "Show Signal Counter", GroupName = "Display Settings", Order = 110)]
        public bool ShowConfText { get; set; } = true;

        #endregion

        protected override void OnRender(RenderContext context, DrawingLayouts layout)
        {
            string confText = "Bullish Signals: " + totalBullishConf + "\nBearish Signals: " + totalBearishConf + "\n";
            string condText = "";
            
            if (ShowCondText) 
            {

                if (above_Kumo_Triggered)
                    condText += "Bullish Close\n";

                if (below_Kumo_Triggered)
                    condText += "Bearish Close\n";

                if (tk_TriggeredBullish)
                    condText += "Bullish Tenkan\n";

                if (tk_TriggeredBearish)
                    condText += "Bearish Tenkan\n";

                if (lagging_Bullish_Triggered)
                    condText += "Bullish Lagging Span\n";

                if (lagging_Bearish_Triggered)
                    condText += "Bearish Lagging Span\n";

                if (bullishCloud_Triggered)
                    condText += "Bullish Cloud\n";

                if (bearishCloud_Triggered)
                    condText += "Bearish Cloud\n";

                if (condText == "")
                    condText = "No Active Signals";


            }


            var showSecondLine = !string.IsNullOrWhiteSpace(confText);
            if (!showSecondLine)
                return;

      



            var textColor = TextColor.Convert();
            var mainTextRectangle = new Rectangle();
            var confTextRectangle = new Rectangle();

            if (showSecondLine && ShowConfText && !string.IsNullOrEmpty(confText))
            {
                var size = context.MeasureString(confText, AdditionalFont.RenderObject);
                confTextRectangle = new Rectangle(0, 0, (int)size.Width, (int)size.Height);
            }

            if (showSecondLine && ShowCondText && !string.IsNullOrEmpty(condText))
            {
                var size = context.MeasureString(condText, AdditionalFont.RenderObject);
                confTextRectangle = new Rectangle(0, 0, (int)size.Width, (int)size.Height);
            }



            if (mainTextRectangle.Height > 0 && confTextRectangle.Height > 0 )
            {
                int secondLineX;
                var y = 0;

                var totalHeight = mainTextRectangle.Height + confTextRectangle.Height + confTextYOffset;

                switch (TextLocation)
                {
                    case Location.Center:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width / 2 - confTextRectangle.Width / 2 + HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height / 2 - totalHeight / 2 + VerticalOffset;

                            break;
                        }
                    case Location.TopLeft:
                        {
                            secondLineX = HorizontalOffset;
                            break;
                        }
                    case Location.TopRight:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width - confTextRectangle.Width + HorizontalOffset;

                            break;
                        }
                    case Location.BottomLeft:
                        {
                            secondLineX = HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height - totalHeight + VerticalOffset;

                            break;
                        }
                    case Location.BottomRight:
                        {
                            secondLineX = ChartInfo.PriceChartContainer.Region.Width - confTextRectangle.Width + HorizontalOffset;
                            y = ChartInfo.PriceChartContainer.Region.Height - totalHeight + VerticalOffset;

                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                context.DrawString(confText + "\n" + condText, AdditionalFont.RenderObject, textColor, secondLineX, y + mainTextRectangle.Height + confTextYOffset);
            }
            else if (confTextRectangle.Height > 0)
            {
                DrawString(context, confText +"\n"+ condText, AdditionalFont.RenderObject, textColor, confTextRectangle);
            }

            
        }
        

        private void DrawString(RenderContext context, string text, RenderFont font, System.Drawing.Color color, Rectangle rectangle)
        {
            switch (TextLocation)
            {
                case Location.Center:
                    {
                        context.DrawString(text, font, color, ChartInfo.PriceChartContainer.Region.Width / 2 - rectangle.Width / 2 + HorizontalOffset,
                                           ChartInfo.PriceChartContainer.Region.Height / 2 - rectangle.Height / 2 + VerticalOffset);
                        break;
                    }
                case Location.TopLeft:
                    {
                        context.DrawString(text, font, color, HorizontalOffset, VerticalOffset);
                        break;
                    }
                case Location.TopRight:
                    {
                        context.DrawString(text, font, color, ChartInfo.PriceChartContainer.Region.Width - rectangle.Width + HorizontalOffset, VerticalOffset);
                        break;
                    }
                case Location.BottomLeft:
                    {
                        context.DrawString(text, font, color, HorizontalOffset, ChartInfo.PriceChartContainer.Region.Height - rectangle.Height + VerticalOffset);
                        break;
                    }
                case Location.BottomRight:
                    {
                        context.DrawString(text, font, color, ChartInfo.PriceChartContainer.Region.Width - rectangle.Width + HorizontalOffset,
                                           ChartInfo.PriceChartContainer.Region.Height - rectangle.Height + VerticalOffset);
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

            


    }
}

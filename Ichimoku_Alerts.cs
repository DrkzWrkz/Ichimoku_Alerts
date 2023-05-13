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
using Utils.Common.Localization;
using ATAS.Indicators;



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
            Color = Color.FromRgb((byte)153, (byte)21, (byte)21)
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
            Color = Color.FromRgb((byte)4, (byte)150, byte.MaxValue)
        };

        private readonly Lowest _conversionLow = new Lowest
        {
            Period = 9
        };

        private readonly RangeDataSeries _downSeries = new RangeDataSeries("Down")
        {
            RangeColor = Color.FromArgb((byte)100, byte.MaxValue, (byte)0, (byte)0)
        };

        private readonly ValueDataSeries _laggingSpan = new ValueDataSeries("Lagging Span")
        {
            Color = Color.FromRgb((byte)69, (byte)153, (byte)21)
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
            RangeColor = Color.FromArgb((byte)100, (byte)0, byte.MaxValue, (byte)0)
        };
        private int _days;
        private int _displacement = 26;
        private int _targetBar;
        private bool _enable_Alerts = false;
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
            base.DenyToChangePanel = true;
            base.DataSeries[0] = _conversionLine;
            base.DataSeries.Add(_laggingSpan);
            base.DataSeries.Add(_baseLine);
            base.DataSeries.Add(_leadLine1);
            base.DataSeries.Add(_leadLine2);
            base.DataSeries.Add(_upSeries);
            base.DataSeries.Add(_downSeries);
        }
        private int lastBarAlert = 0;

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
            bool lagging_Bullish = _laggingSpan[bar - Displacement] > _leadLine1[index] && _laggingSpan[bar - Displacement] > _leadLine2[index] && !lagging_Bullish_Triggered;
            bool lagging_Bearish = _laggingSpan[bar - Displacement] < _leadLine1[index] && _laggingSpan[bar - Displacement] < _leadLine2[index] && !lagging_Bearish_Triggered;
            bool bullishCloud = _leadLine1[bar] > _leadLine2[bar] && !bullishCloud_Triggered;
            bool bearishCloud = _leadLine1[bar] < _leadLine2[bar] && !bearishCloud_Triggered;
            bool close_Above_Kumo = candle.Close > _leadLine1[index] && candle.Close > _leadLine2[index] && !above_Kumo_Triggered;
            bool close_Below_Kumo = candle.Close < _leadLine1[index] && candle.Close < _leadLine2[index] && !below_Kumo_Triggered;

            
            if (enable_Alerts && CurrentBar - 1 == bar && lastBarAlert != bar)
            {
                if (close_Above_Kumo)
                {
                    lastBarAlert = bar;
                    above_Kumo_Triggered = true;
                    below_Kumo_Triggered = false;
                    AddAlert("Alert1", "Close Above CLoud");

                }
                else if (close_Below_Kumo)
                {
                    lastBarAlert = bar;
                    above_Kumo_Triggered = false;
                    below_Kumo_Triggered = true;
                    AddAlert("Alert1", "Close Below CLoud");

                }

                if (tk_Bullish)
                {
                    lastBarAlert = bar;
                    tk_TriggeredBullish = true;
                    tk_TriggeredBearish = false;

                    AddAlert("Alert1", "Bullish Crossover");
                }
                else if (tk_Bearish)
                {
                    lastBarAlert = bar;
                    tk_TriggeredBullish = false;
                    tk_TriggeredBearish = true;

                    AddAlert("Alert1", "Bearish Crossover");
                }


                if (lagging_Bullish)
                {
                    lastBarAlert = bar;
                    lagging_Bullish_Triggered = true;
                    lagging_Bearish_Triggered = false;

                    AddAlert("Alert1", "Lagging Span is Bullish");
                }
                else if (lagging_Bearish)
                {
                    lastBarAlert = bar;
                    lagging_Bullish_Triggered = false;
                    lagging_Bearish_Triggered = true;

                    AddAlert("Alert1", "Lagging Span is Bearish");
                }

                if (bullishCloud)
                {
                    lastBarAlert = bar;
                    bullishCloud_Triggered = true;
                    bearishCloud_Triggered = false;

                    AddAlert("Alert1", "Bullish Cloud");

                }
                else if (bearishCloud)
                {
                    lastBarAlert = bar;
                    bullishCloud_Triggered = false;
                    bearishCloud_Triggered = true;

                    AddAlert("Alert1", "Bearish Cloud");

                }
            }
        }
    }
}

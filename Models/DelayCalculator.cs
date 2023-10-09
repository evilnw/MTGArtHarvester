using System;

namespace MTGArtHarvester.Models;

public class DelayCalculator
{
    private DateTime _dateTimeUtc;
    
    private readonly object _sync = new object();
    
    public int StepBetweenDelay { get; }

    public TimeSpan Delay => GetDelay();

    public DelayCalculator(int stepBetweenDelay)
        => StepBetweenDelay = stepBetweenDelay;

    private TimeSpan GetDelay()
    {
        lock(_sync)
        {
            var currentDateTimeUtc = DateTime.UtcNow;

            if (_dateTimeUtc.AddMilliseconds(StepBetweenDelay) <= currentDateTimeUtc)
            {
                _dateTimeUtc = currentDateTimeUtc;
                return TimeSpan.Zero;
            }

            _dateTimeUtc = _dateTimeUtc.AddMilliseconds(StepBetweenDelay);
            return _dateTimeUtc - currentDateTimeUtc;
        }
    }  
}
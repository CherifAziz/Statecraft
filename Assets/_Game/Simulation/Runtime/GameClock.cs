using System;

namespace Statecraft.Simulation
{
    public sealed class GameClock
    {
        public static readonly DateTime V1InitialDate = new(2026, 9, 1);

        public GameClock(DateTime initialDate)
        {
            CurrentDate = initialDate.Date;
        }

        public DateTime CurrentDate { get; private set; }

        public event Action<GameClock> DateChanged;

        public DateTime AdvanceOneDay()
        {
            return AdvanceDays(1);
        }

        public DateTime AdvanceDays(int days)
        {
            if (days < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(days), "The game clock cannot move backwards.");
            }

            if (days == 0)
            {
                return CurrentDate;
            }

            CurrentDate = CurrentDate.AddDays(days);
            DateChanged?.Invoke(this);
            return CurrentDate;
        }
    }
}

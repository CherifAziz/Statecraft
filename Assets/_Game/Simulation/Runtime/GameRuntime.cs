using System;
using System.Collections.Generic;
using System.Linq;

namespace Statecraft.Simulation
{
    public sealed class GameRuntime
    {
        private readonly IReadOnlyList<ISimulationCountryDefinition> countryDefinitions;

        public GameRuntime(IEnumerable<ISimulationCountryDefinition> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            countryDefinitions = definitions.ToArray();
        }

        public bool HasActiveSession => CurrentSession != null && CurrentSession.IsActive;
        public GameSession CurrentSession { get; private set; }
        public IReadOnlyList<ISimulationCountryDefinition> CountryDefinitions => countryDefinitions;

        public event Action<GameRuntime> SessionChanged;

        public GameSession StartNewGame(ISimulationCountryDefinition playerCountry)
        {
            if (CurrentSession != null)
            {
                CurrentSession.End();
            }

            CurrentSession = GameSessionFactory.CreateNewGame(playerCountry, countryDefinitions);
            SessionChanged?.Invoke(this);
            return CurrentSession;
        }

        public void EndSession()
        {
            if (CurrentSession == null)
            {
                return;
            }

            CurrentSession.End();
            CurrentSession = null;
            SessionChanged?.Invoke(this);
        }
    }
}

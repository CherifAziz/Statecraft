using System;
using System.Collections.Generic;

namespace Statecraft.Simulation
{
    public static class GameSessionFactory
    {
        public static GameSession CreateNewGame(ISimulationCountryDefinition playerCountry)
        {
            if (playerCountry == null)
            {
                throw new ArgumentNullException(nameof(playerCountry));
            }

            return CreateNewGame(playerCountry, new[] { playerCountry });
        }

        public static GameSession CreateNewGame(
            ISimulationCountryDefinition playerCountry,
            IEnumerable<ISimulationCountryDefinition> countryDefinitions)
        {
            if (playerCountry == null)
            {
                throw new ArgumentNullException(nameof(playerCountry));
            }

            if (countryDefinitions == null)
            {
                throw new ArgumentNullException(nameof(countryDefinitions));
            }

            var states = new Dictionary<string, CountryState>(StringComparer.Ordinal);
            foreach (var definition in countryDefinitions)
            {
                if (definition == null)
                {
                    throw new ArgumentException("The country catalog cannot contain null definitions.", nameof(countryDefinitions));
                }

                if (string.IsNullOrWhiteSpace(definition.Id))
                {
                    throw new ArgumentException("Every country definition requires a stable ID.", nameof(countryDefinitions));
                }

                if (!states.TryAdd(definition.Id, new CountryState(definition)))
                {
                    throw new ArgumentException(
                        $"The country catalog contains the duplicate ID '{definition.Id}'.",
                        nameof(countryDefinitions));
                }
            }

            if (!states.TryGetValue(playerCountry.Id, out var playerState))
            {
                throw new ArgumentException(
                    $"Player country '{playerCountry.Id}' is not present in the supplied country catalog.",
                    nameof(playerCountry));
            }

            return new GameSession(playerState, states, new GameClock(GameClock.V1InitialDate));
        }
    }
}

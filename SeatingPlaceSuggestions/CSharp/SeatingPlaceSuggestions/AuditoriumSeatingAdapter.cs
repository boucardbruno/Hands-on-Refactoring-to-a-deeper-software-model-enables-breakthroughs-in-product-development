﻿using System.Collections.Generic;
using ExternalDependencies;

namespace SeatsSuggestions;

/// <summary>
///     Adapt Dtos coming from the external dependencies (ReservationsProvider, AuditoriumLayoutRepository) to
///     AuditoriumSeating instances.
/// </summary>
public class AuditoriumSeatingAdapter(
    IProvideAuditoriumLayouts auditoriumLayoutRepository,
    IProvideCurrentReservations reservationsProvider)
{
    public AuditoriumSeating FindByShowId(string showId)
    {
        return Adapt(auditoriumLayoutRepository.FindByShowId(showId),
            reservationsProvider.GetReservedSeats(showId));
    }

    private static AuditoriumSeating Adapt(AuditoriumDto auditoriumDto, ReservedSeatsDto reservedSeatsDto)
    {
        var rows = new Dictionary<string, Row>();

        foreach (var rowDto in auditoriumDto.Rows)
        {
            var seats = new List<SeatingPlace>();

            foreach (var seatDto in rowDto.Value)
            {
                var rowName = rowDto.Key;
                var number = ExtractNumber(seatDto.Name);
                var pricingCategory = ConvertCategory(seatDto.Category);

                var isReserved = reservedSeatsDto.ReservedSeats.Contains(seatDto.Name);

                seats.Add(new SeatingPlace(rowName, number, pricingCategory,
                    isReserved ? SeatingPlaceAvailability.Reserved : SeatingPlaceAvailability.Available));
            }

            rows[rowDto.Key] = new Row(rowDto.Key, seats);
        }

        return new(rows);
    }

    private static PricingCategory ConvertCategory(int seatDtoCategory)
    {
        return (PricingCategory)seatDtoCategory;
    }

    private static int ExtractNumber(string name)
    {
        return int.Parse(name.Substring(1));
    }
}
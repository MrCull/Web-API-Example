﻿using Domain.Aggregates.TheaterAggregate;
using Domain.Aggregates.TheaterChainAggregate;
using Domain.Exceptions;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.ShowtimeAggregate;

internal class Showtime : IShowtime
{
    public Showtime(int id, Movie movie, Screen screen, DateTime showDateTimeUtc, decimal price)
    {
        Id = id;
        ArgumentNullException.ThrowIfNull(movie);
        Movie = movie;
        MovieId = movie.Id;
        ArgumentNullException.ThrowIfNull(screen);
        Screen = screen;
        ScreenId = screen.Id;
        Price = price;

        ShowDateTimeUtc = showDateTimeUtc;
        SeatReservations = [];
        Bookings = [];
    }

    [Newtonsoft.Json.JsonConstructor]
    public Showtime(int id, int movieId, Guid screenId, DateTime showDateTimeUtc, decimal price)
    {
        Id = id;
        MovieId = movieId;
        ScreenId = screenId;
        ShowDateTimeUtc = showDateTimeUtc;
        Price = price;
        SeatReservations = [];
        Bookings = [];
    }

    public int Id { get; private set; }
    public int MovieId { get; private set; }
    public Guid ScreenId { get; private set; }


    [Required]
    public DateTime ShowDateTimeUtc { get; private set; }

    public int AvailableSeats()
        => Screen.Seats.Count - SeatReservations.Where(_ => _.IsReservationCnfirmedOrAcitivePending()).Select(_ => _.Seats.Count()).Sum();

    [Required]
    [Range(0.01, 1000, ErrorMessage = "Price must be greater than zero.")]
    public decimal Price { get; private set; }

    // Navigation properties
    internal Movie Movie { get; private set; }
    internal Screen Screen { get; private set; }

    [JsonProperty("seatReservations")]
    public List<SeatReservation> SeatReservations { get; private set; }

    public List<Booking> Bookings { get; private set; }

    public IBooking CompleteBookingForSeatReservationAndReturnBooking(Guid reservationId)
    {
        SeatReservation? seatReservation = SeatReservations
            .Find(SeatReservations => SeatReservations.Id == reservationId);

        if (seatReservation == null)
        {
            throw new ShowtimeException($"Seat reservation timed out, or invalid");
        }
        else if (!seatReservation.IsReserved())
        {
            throw new ShowtimeException($"Seat reservation is already booked");
        }
        else if (seatReservation.IsTimedOut())
        {
            throw new ShowtimeException($"Seat reservation timed out");
        }

        seatReservation.Confirm();

        Booking booking = new(Guid.NewGuid(), DateTime.UtcNow, seatReservation);
        Bookings.Add(booking);

        return booking;
    }

    public List<ISeatReservation> GetSeatReservations()
        => SeatReservations.Select(sr => (ISeatReservation)sr).ToList();

    public ISeatReservation ProvisionallyReserveSeatsAndReturnReservation(List<string> seatNames)
    {
        List<string> alreadyReservedMatchingSeatNumbers = SeatReservations
            .Where(s => s.IsReservationCnfirmedOrAcitivePending())
            .SelectMany(s => s.Seats)
            .Where(s => seatNames.Contains(s.SeatNumber))
            .Select(s => s.SeatNumber)
            .ToList();

        if (alreadyReservedMatchingSeatNumbers.Count != 0)
        {
            throw new ShowtimeException($"Seats no longer available [{string.Join(',', alreadyReservedMatchingSeatNumbers)}]");
        }

        List<Seat> seats = Screen!.GetSeatsByNames(seatNames);
        SeatReservation seatReservation = new(Guid.NewGuid(), DateTime.UtcNow, DateTime.UtcNow.AddMinutes(20), this, seats);

        SeatReservations.Add(seatReservation);

        return seatReservation;
    }

    internal void ClearSeatReservationsWithExpiredTimeouts()
    {
        SeatReservations.RemoveAll(s => s.IsTimedOut());
    }

    internal bool HasActiveSeatReservations()
        => SeatReservations.Any(s => s.IsReservationCnfirmedOrAcitivePending());

    public void UpdateInformation(DateTime newDateTime, decimal newPrice, Screen screen)
    {
        ShowDateTimeUtc = newDateTime;
        Price = newPrice;
        Screen = screen;
        ScreenId = screen.Id;
    }

    internal void SetMovie(Movie movie)
    {
        Movie = movie;
    }

    internal void SetScreen(Screen screen)
    {
        Screen = screen;
    }
}

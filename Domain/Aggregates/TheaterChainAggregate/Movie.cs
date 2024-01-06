﻿using Domain.Aggregates.Exceptions;
using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.TheaterChainAggregate;

public class Movie(int id, string title, string description, TimeSpan duration, string genre, DateTime releaseDateUtc)
{
    public int Id { get; private set; } = id;

    [Required]
    [StringLength(100, ErrorMessage = "Title length can't be more than 100 characters.")]
    public string Title { get; private set; } = title;

    [Required]
    [StringLength(500, ErrorMessage = "Description length can't be more than 500 characters.")]
    public string Description { get; private set; } = description;

    [Required]
    public TimeSpan Duration { get; private set; } = duration;

    [Required]
    [StringLength(50, ErrorMessage = "Genre length can't be more than 50 characters.")]
    public string Genre { get; private set; } = genre;

    [DataType(DataType.Date)]
    public DateTime ReleaseDateUtc { get; private set; } = releaseDateUtc;

    [Required]
    public TheaterChainMovieStatus TheaterChainMovieStatus { get; private set; } = TheaterChainMovieStatus.Available;

    public void MarkAsAvailable()
    {
        if (TheaterChainMovieStatus == TheaterChainMovieStatus.Available)
        {
            throw new MovieException("Movie is already available");
        }

        TheaterChainMovieStatus = TheaterChainMovieStatus.Available;
    }

    public void MarkAsNoLongerAvailable()
    {
        if (TheaterChainMovieStatus == TheaterChainMovieStatus.NoLongerAvailable)
        {
            throw new MovieException("Movie is already no longer available");
        }

        TheaterChainMovieStatus = TheaterChainMovieStatus.NoLongerAvailable;
    }

    public void UpdateInformation(string title, string description, string genre, TimeSpan duration, DateTime releaseDate)
    {
        Title = title;
        Description = description;
        Genre = genre;
        Duration = duration;
        ReleaseDateUtc = releaseDate;
    }
}

public enum TheaterChainMovieStatus
{
    Available,
    NoLongerAvailable
}
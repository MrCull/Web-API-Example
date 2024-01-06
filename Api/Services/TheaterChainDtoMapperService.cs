﻿using Api.Dtos;
using Domain.Aggregates.TheaterChainAggregate;

namespace Api.Services
{
    public class TheaterChainDtoMapperService : ITheaterChainDtoMapperService
    {
        public MovieDto MapMovieToMovideDto(Movie movie)
        {
            MovieStatus movieStatus = movie.TheaterChainMovieStatus switch
            {
                TheaterChainMovieStatus.Available => MovieStatus.Available,
                TheaterChainMovieStatus.NoLongerAvailable => MovieStatus.NoLongerAvailable,
                _ => throw new ArgumentOutOfRangeException()
            };

            return new MovieDto(movie.Id, movie.Title, movie.Description, movie.Duration, movie.Genre, movie.ReleaseDateUtc, movieStatus);
        }

        public IEnumerable<MovieDto> MapMovieToMovideDto(List<Movie> movies)
            => movies.Select(MapMovieToMovideDto);
    }
}
using Personenvervoer.Models;

namespace Personenvervoer.Services;

public class GoogleApiResult
{
    public double DistanceKm { get; set; }
    public int DurationMinutes { get; set; }
    public string Status { get; set; } = "OK";
}

public class GoogleApiService
{
    private readonly ILogger<GoogleApiService> _logger;

    public GoogleApiService(ILogger<GoogleApiService> logger)
    {
        _logger = logger;
    }

    // Fake Google Maps Distance Matrix API endpoint
    public Task<GoogleApiResult> GetDistanceAndDuration(
        decimal? originLat, decimal? originLng,
        decimal? destLat, decimal? destLng)
    {
        if (originLat == null || originLng == null || destLat == null || destLng == null)
        {
            return Task.FromResult(new GoogleApiResult
            {
                DistanceKm = 0,
                DurationMinutes = 0,
                Status = "INVALID_REQUEST"
            });
        }

        // Haversine formula to calculate distance between two coordinates
        double distanceKm = CalculateHaversineDistance(
            (double)originLat, (double)originLng,
            (double)destLat, (double)destLng);

        // Estimate driving time: average speed 50 km/h in urban areas
        int durationMinutes = (int)Math.Ceiling(distanceKm / 50.0 * 60.0);

        _logger.LogInformation(
            "Fake Google API: distance={Distance}km, duration={Duration}min from ({OriginLat},{OriginLng}) to ({DestLat},{DestLng})",
            distanceKm, durationMinutes, originLat, originLng, destLat, destLng);

        return Task.FromResult(new GoogleApiResult
        {
            DistanceKm = Math.Round(distanceKm, 2),
            DurationMinutes = durationMinutes,
            Status = "OK"
        });
    }

    public async Task<GoogleApiResult> GetRideDistance(RideModel ride)
    {
        if (ride.Ridepattern?.Member == null || ride.EndLocation == null)
        {
            return new GoogleApiResult { Status = "INCOMPLETE_DATA", DistanceKm = 0, DurationMinutes = 0 };
        }

        return await GetDistanceAndDuration(
            ride.Ridepattern.Member.Latitude,
            ride.Ridepattern.Member.Longitude,
            ride.EndLocation.Latitude,
            ride.EndLocation.Longitude);
    }

    private static double CalculateHaversineDistance(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        const double earthRadiusKm = 6371.0;
        double dLat = ToRadians(lat2 - lat1);
        double dLon = ToRadians(lon2 - lon1);

        double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                 + Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2))
                 * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return earthRadiusKm * c;
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180.0;
}

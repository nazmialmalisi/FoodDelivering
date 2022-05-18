namespace OrderService.GraphQL
{
    public record TrackingInput
    (
        int OrderId,
        string Longitude,
        string Latitude
    );
}

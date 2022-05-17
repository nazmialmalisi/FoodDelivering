namespace FoodService.GraphQL
{
    public record FoodInput
    (
        int? Id,
        string Name,
        double Price
    );
}

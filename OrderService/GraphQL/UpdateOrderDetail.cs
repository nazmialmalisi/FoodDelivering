namespace OrderService.GraphQL
{
    public record UpdateOrderDetail
    (
        int id,
        int FoodId,
        int Quantity
    );
}

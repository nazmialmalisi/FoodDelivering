namespace OrderService.GraphQL
{
    public record OrderInput
    (
        int CourierId,
        List<ListOrderDetails> ListOrderDetails
    );
}

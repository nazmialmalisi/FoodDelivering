namespace UserService.GraphQL
{
    public record ChangePassword
    (
        string Username,
        string NewPassword
    );
}

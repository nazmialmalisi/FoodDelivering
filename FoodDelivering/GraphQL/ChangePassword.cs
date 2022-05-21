namespace UserService.GraphQL
{
    public record ChangePassword
    (
        string OldPassword,
        string NewPassword
    );
}

namespace UserService.GraphQL
{
    public record RegisterUser
    (
       string FullName,
       string Email,
       string Role,
       string UserName,
       string Password
    );
}

namespace ProductServiceApp.Api.Auth;

public static class AuthPolicies
{
    public const string ProductsRead = "products.read";
    public const string ProductsWrite = "products.write";
    public const string OrdersRead = "orders.read";
    public const string OrdersWrite = "orders.write";
    public const string OrdersViewAll = "orders.view_all";
    public const string OrdersViewOwn = "orders.view_own";
}

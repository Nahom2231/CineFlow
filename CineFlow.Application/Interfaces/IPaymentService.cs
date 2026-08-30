namespace Application.Interfaces;

public interface IPaymentService
{
    Task<string> InitializePaymentAsync(
        decimal amount,
        string email,
        string reference);
}
namespace ProductServiceApp.Domain.Commom;

public interface IToMapper<out TInput> where TInput : class
{
    TInput MapTo();
}

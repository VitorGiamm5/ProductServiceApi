namespace ProductServiceApp.Domain.Commom;

public interface IFromMapper<out TOutput, in TInput> where TOutput : class where TInput : class
{
    TOutput MapFrom(TInput? input);
}

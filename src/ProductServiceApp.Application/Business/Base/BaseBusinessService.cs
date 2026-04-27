
using ProductServiceApp.Domain.Business.Base;

namespace ProductServiceApp.Application.Business.Base;

/// <summary>
/// Classe base que implementa o pipeline:
/// PreProcessAsync → ProcessAsync → PostProcessAsync
///
/// Inbox  = TInObject  (dados que entram)
/// Outbox = TOutObject (dados que saem)
/// </summary>
public abstract class BaseBusinessService<TInObject, TOutObject> : IBaseBusinessService<TInObject, TOutObject>
    where TInObject : class
    where TOutObject : class
{
    // ------------------------------------------------------------------ //
    //  Entry-point público — orquestra o pipeline completo
    // ------------------------------------------------------------------ //
    public async Task<TOutObject> ExecuteAsync(TInObject input, CancellationToken ct = default)
    {
        // 1 Inbox — pré-processamento (validação, enriquecimento, sanitização…)
        var processedInput = await PreProcessAsync(input, ct);

        // 2️ Processamento principal
        var result = await ProcessAsync(processedInput, ct);

        // 3️ Outbox — pós-processamento (mapeamento, auditoria, notificações…)
        var processedResult = await PostProcessAsync(result, ct);

        return processedResult;
    }

    // ------------------------------------------------------------------ //
    //  Etapas do pipeline — obrigatória apenas ProcessAsync
    // ------------------------------------------------------------------ //

    /// <summary>
    /// INBOX — recebe o input cru e devolve o input pronto para processar.
    /// Use para: validação, enriquecimento, normalização de dados.
    /// Implementação padrão: passa o input sem alteração.
    /// </summary>
    protected virtual Task<TInObject> PreProcessAsync(TInObject input, CancellationToken ct)
        => Task.FromResult(input);

    /// <summary>
    /// PROCESSAMENTO — lógica de negócio principal.
    /// Obrigatório implementar nas classes filhas.
    /// </summary>
    protected abstract Task<TOutObject> ProcessAsync(TInObject input, CancellationToken ct);

    /// <summary>
    /// OUTBOX — recebe o resultado e o transforma antes de entregar ao caller.
    /// Use para: mapeamento de resposta, auditoria, eventos de domínio, notificações.
    /// Implementação padrão: passa o resultado sem alteração.
    /// </summary>
    protected virtual Task<TOutObject> PostProcessAsync(TOutObject result, CancellationToken ct)
        => Task.FromResult(result);
}

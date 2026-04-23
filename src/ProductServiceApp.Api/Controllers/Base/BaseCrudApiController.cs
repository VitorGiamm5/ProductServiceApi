namespace ProductServiceApp.Api.Controllers.Base.BaseCrudApiController;

public abstract partial class BaseCrudApiController<
    TGetResponse,
    TCreateRequest,
    TCreateResponse,
    TUpdateRequest,
    TUpdateResponse
    >
    : BaseApiController
{
}

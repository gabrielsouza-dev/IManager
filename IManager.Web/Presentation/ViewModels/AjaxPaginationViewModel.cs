namespace IManager.Web.Presentation.ViewModels;

/// <summary>
/// Dados do rodapé de paginação para tabelas carregadas via fetch (partials).
/// Os links não navegam: emitem data-page / data-page-size para o JS da página recarregar o partial.
/// </summary>
public record AjaxPaginationViewModel(int TotalCount, int Page, int PageSize);

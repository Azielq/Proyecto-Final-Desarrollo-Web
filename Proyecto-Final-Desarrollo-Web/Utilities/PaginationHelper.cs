using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Proyecto_Final_Desarrollo_Web.Helpers
{
    public static class PaginationHelper
    {
        public static void ConfigurePagination(ViewDataDictionary viewData, int totalRecords, int page, int pageSize)
        {
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            viewData["CurrentPage"] = page;
            viewData["TotalPages"] = totalPages;
            viewData["PageSize"] = pageSize;
            viewData["TotalRecords"] = totalRecords;
            viewData["StartRecord"] = totalRecords == 0 ? 0 : (page - 1) * pageSize + 1;
            viewData["EndRecord"] = Math.Min(page * pageSize, totalRecords);
        }

        public static MvcHtmlString RenderPagination(this HtmlHelper htmlHelper, int currentPage, int totalPages, int pageSize, string actionName, string controllerName, object routeValues = null)
        {
            if (totalPages <= 1)
                return MvcHtmlString.Empty;

            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var div = new TagBuilder("div");
            div.AddCssClass("d-flex justify-content-between align-items-center mt-3");

            // Info de paginación
            var infoSpan = new TagBuilder("div");
            infoSpan.InnerHtml = $"<span class='text-muted'>Mostrando registros {htmlHelper.ViewData["StartRecord"]} a {htmlHelper.ViewData["EndRecord"]} de {htmlHelper.ViewData["TotalRecords"]} | Página {currentPage} de {totalPages}</span>";
            div.InnerHtml += infoSpan.ToString();

            // Controles de paginación
            var nav = new TagBuilder("nav");
            nav.Attributes.Add("aria-label", "Navegación de páginas");

            var ul = new TagBuilder("ul");
            ul.AddCssClass("pagination");

            // Botón Anterior
            var liPrev = new TagBuilder("li");
            liPrev.AddCssClass("page-item");
            if (currentPage <= 1)
                liPrev.AddCssClass("disabled");

            var aPrev = new TagBuilder("a");
            aPrev.AddCssClass("page-link");
            if (currentPage > 1)
            {
                var routeValuesPrev = new RouteValueDictionary(routeValues ?? new { });
                routeValuesPrev["page"] = currentPage - 1;
                routeValuesPrev["pageSize"] = pageSize;
                aPrev.Attributes.Add("href", urlHelper.Action(actionName, controllerName, routeValuesPrev));
            }
            else
            {
                aPrev.Attributes.Add("href", "#");
            }
            aPrev.Attributes.Add("aria-label", "Anterior");
            aPrev.InnerHtml = "<span aria-hidden='true'>&laquo;</span>";
            liPrev.InnerHtml += aPrev.ToString();
            ul.InnerHtml += liPrev.ToString();

            // Números de página
            for (int i = Math.Max(1, currentPage - 2); i <= Math.Min(totalPages, currentPage + 2); i++)
            {
                var li = new TagBuilder("li");
                li.AddCssClass("page-item");
                if (i == currentPage)
                    li.AddCssClass("active");

                var a = new TagBuilder("a");
                a.AddCssClass("page-link");

                var routeValuesPage = new RouteValueDictionary(routeValues ?? new { });
                routeValuesPage["page"] = i;
                routeValuesPage["pageSize"] = pageSize;
                a.Attributes.Add("href", urlHelper.Action(actionName, controllerName, routeValuesPage));

                a.InnerHtml = i.ToString();
                li.InnerHtml += a.ToString();
                ul.InnerHtml += li.ToString();
            }

            // Botón Siguiente
            var liNext = new TagBuilder("li");
            liNext.AddCssClass("page-item");
            if (currentPage >= totalPages)
                liNext.AddCssClass("disabled");

            var aNext = new TagBuilder("a");
            aNext.AddCssClass("page-link");
            if (currentPage < totalPages)
            {
                var routeValuesNext = new RouteValueDictionary(routeValues ?? new { });
                routeValuesNext["page"] = currentPage + 1;
                routeValuesNext["pageSize"] = pageSize;
                aNext.Attributes.Add("href", urlHelper.Action(actionName, controllerName, routeValuesNext));
            }
            else
            {
                aNext.Attributes.Add("href", "#");
            }
            aNext.Attributes.Add("aria-label", "Siguiente");
            aNext.InnerHtml = "<span aria-hidden='true'>&raquo;</span>";
            liNext.InnerHtml += aNext.ToString();
            ul.InnerHtml += liNext.ToString();

            nav.InnerHtml += ul.ToString();
            div.InnerHtml += nav.ToString();

            // Selector de tamaño de página
            var pageSizeDiv = new TagBuilder("div");
            var select = new TagBuilder("select");
            select.AddCssClass("form-select form-select-sm");
            select.Attributes.Add("id", "pageSizeSelector");
            select.Attributes.Add("onchange", $"changePageSize(this.value)");

            var options = new[] { 10, 15, 25, 50, 100 };
            foreach (var size in options)
            {
                var option = new TagBuilder("option");
                option.Attributes.Add("value", size.ToString());
                if (size == pageSize)
                    option.Attributes.Add("selected", "selected");
                option.InnerHtml = $"{size} por página";
                select.InnerHtml += option.ToString();
            }

            pageSizeDiv.InnerHtml += select.ToString();
            div.InnerHtml += pageSizeDiv.ToString();

            return MvcHtmlString.Create(div.ToString());
        }
    }
}
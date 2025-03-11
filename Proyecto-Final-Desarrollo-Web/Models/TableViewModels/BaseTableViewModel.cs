using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proyecto_Final_Desarrollo_Web.TableViewModels
{
    /// <summary>
    /// Esta de acá es una clase base para solicitudes de tabla que contiene propiedades comunes para paginación,
    /// búsqueda y ordenamiento (espero que entiendan)
    /// </summary>
    public abstract class BaseTableRequest
    {
        // Esto tiene los parámetros de paginación
        public int Start { get; set; }
        public int Length { get; set; }

        // Esto tiene los parámetros de búsqueda
        public string SearchValue { get; set; }

        // Esto tiene los parámetros de ordenamiento
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        // Esto tiene el constructor con valores predeterminados
        public BaseTableRequest()
        {
            Start = 0;
            Length = 10;
            SortDirection = "asc";
        }
    }

    /// <summary>
    /// Esta de acá es una clase base para respuestas de tabla que contiene propiedades comunes requeridas
    /// por DataTables
    /// </summary>
    /// <typeparam name="T">Tipo de datos que contiene la tabla</typeparam>
    public abstract class BaseTableResponse<T>
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public List<T> data { get; set; }

        public BaseTableResponse()
        {
            data = new List<T>();
        }
    }

    /// <summary>
    /// Esta de acá es una clase que se usa con métodos para trabajar con TableViewModels
    /// </summary>
    public static class TableViewModelHelper
    {
        /// <summary>
        /// Esto de acá aplica ordenamiento a una consulta IQueryable según los parámetros de la solicitud
        /// </summary>
        public static IQueryable<T> ApplySort<T>(this IQueryable<T> query, string sortColumn, string sortDirection)
        {
            if (string.IsNullOrEmpty(sortColumn))
                return query;

            // Esto obtiene la propiedad por nombre
            var propertyInfo = typeof(T).GetProperty(sortColumn);
            if (propertyInfo == null)
                return query;

            // Esto le aplica el orden ascendente o descendente
            if (sortDirection.ToLower() == "asc")
                return query.OrderBy(x => propertyInfo.GetValue(x, null));
            else
                return query.OrderByDescending(x => propertyInfo.GetValue(x, null));
        }

        /// <summary>
        /// Esto de acá le aplica paginación a una consulta IQueryable según los parámetros de la solicitud
        /// </summary>
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int start, int length)
        {
            if (length <= 0)
                return query;

            return query.Skip(start).Take(length);
        }
    }
}
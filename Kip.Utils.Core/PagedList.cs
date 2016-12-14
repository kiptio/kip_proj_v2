using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kip.Utils.Core
{
    #region [IPagedList]
    /// <summary>
    /// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
    /// </summary>
    /// <remarks>
    /// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
    /// </remarks>
    /// <typeparam name="T">The type of object the collection should contain.</typeparam>
    /// <seealso cref="IEnumerable{T}"/>
    public interface IPagedList<out T> : IPagedList, IEnumerable<T>
    {
        ///<summary>
        /// Gets the element at the specified index.
        ///</summary>
        ///<param name="index">The zero-based index of the element to get.</param>
        T this[int index] { get; }

        ///<summary>
        /// Gets the number of elements contained on this page.
        ///</summary>
        int Count { get; }

        ///<summary>
        /// Gets a non-enumerable copy of this paged list.
        ///</summary>
        ///<returns>A non-enumerable copy of this paged list.</returns>
        IPagedList GetMetaData();
    }

    /// <summary>
    /// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
    /// </summary>
    /// <remarks>
    /// Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
    /// </remarks>
    public interface IPagedList
    {
        /// <summary>
        /// Total number of subsets within the superset.
        /// </summary>
        /// <value>
        /// Total number of subsets within the superset.
        /// </value>
        int PageCount { get; }

        /// <summary>
        /// Total number of objects contained within the superset.
        /// </summary>
        /// <value>
        /// Total number of objects contained within the superset.
        /// </value>
        int TotalItemCount { get; }

        /// <summary>
        /// One-based index of this subset within the superset.
        /// </summary>
        /// <value>
        /// One-based index of this subset within the superset.
        /// </value>
        int PageNumber { get; }

        /// <summary>
        /// Maximum size any individual subset.
        /// </summary>
        /// <value>
        /// Maximum size any individual subset.
        /// </value>
        int PageSize { get; }

        /// <summary>
        /// Returns true if this is NOT the first subset within the superset.
        /// </summary>
        /// <value>
        /// Returns true if this is NOT the first subset within the superset.
        /// </value>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Returns true if this is NOT the last subset within the superset.
        /// </summary>
        /// <value>
        /// Returns true if this is NOT the last subset within the superset.
        /// </value>
        bool HasNextPage { get; }

        /// <summary>
        /// Returns true if this is the first subset within the superset.
        /// </summary>
        /// <value>
        /// Returns true if this is the first subset within the superset.
        /// </value>
        bool IsFirstPage { get; }

        /// <summary>
        /// Returns true if this is the last subset within the superset.
        /// </summary>
        /// <value>
        /// Returns true if this is the last subset within the superset.
        /// </value>
        bool IsLastPage { get; }

        /// <summary>
        /// One-based index of the first item in the paged subset.
        /// </summary>
        /// <value>
        /// One-based index of the first item in the paged subset.
        /// </value>
        int FirstItemOnPage { get; }

        /// <summary>
        /// One-based index of the last item in the paged subset.
        /// </summary>
        /// <value>
        /// One-based index of the last item in the paged subset.
        /// </value>
        int LastItemOnPage { get; }
    }
    #endregion

    #region [PagedListMetaData]
    ///<summary>
    /// Non-enumerable version of the PagedList class.
    ///</summary>
    [Serializable]
    public class PagedListMetaData : IPagedList
    {
        /// <summary>
        /// Protected constructor that allows for instantiation without passing in a separate list.
        /// </summary>
        protected PagedListMetaData()
        {
        }

        ///<summary>
        /// Non-enumerable version of the PagedList class.
        ///</summary>
        ///<param name="pagedList">A PagedList (likely enumerable) to copy metadata from.</param>
        public PagedListMetaData(IPagedList pagedList)
        {
            PageCount = pagedList.PageCount;
            TotalItemCount = pagedList.TotalItemCount;
            PageNumber = pagedList.PageNumber;
            PageSize = pagedList.PageSize;
            HasPreviousPage = pagedList.HasPreviousPage;
            HasNextPage = pagedList.HasNextPage;
            IsFirstPage = pagedList.IsFirstPage;
            IsLastPage = pagedList.IsLastPage;
            FirstItemOnPage = pagedList.FirstItemOnPage;
            LastItemOnPage = pagedList.LastItemOnPage;
        }

        #region IPagedList Members

        /// <summary>
        /// 	Total number of subsets within the superset.
        /// </summary>
        /// <value>
        /// 	Total number of subsets within the superset.
        /// </value>
        public int PageCount { get; protected set; }

        /// <summary>
        /// 	Total number of objects contained within the superset.
        /// </summary>
        /// <value>
        /// 	Total number of objects contained within the superset.
        /// </value>
        public int TotalItemCount { get; protected set; }

        /// <summary>
        /// 	One-based index of this subset within the superset.
        /// </summary>
        /// <value>
        /// 	One-based index of this subset within the superset.
        /// </value>
        public int PageNumber { get; protected set; }

        /// <summary>
        /// 	Maximum size any individual subset.
        /// </summary>
        /// <value>
        /// 	Maximum size any individual subset.
        /// </value>
        public int PageSize { get; protected set; }

        /// <summary>
        /// 	Returns true if this is NOT the first subset within the superset.
        /// </summary>
        /// <value>
        /// 	Returns true if this is NOT the first subset within the superset.
        /// </value>
        public bool HasPreviousPage { get; protected set; }

        /// <summary>
        /// 	Returns true if this is NOT the last subset within the superset.
        /// </summary>
        /// <value>
        /// 	Returns true if this is NOT the last subset within the superset.
        /// </value>
        public bool HasNextPage { get; protected set; }

        /// <summary>
        /// 	Returns true if this is the first subset within the superset.
        /// </summary>
        /// <value>
        /// 	Returns true if this is the first subset within the superset.
        /// </value>
        public bool IsFirstPage { get; protected set; }

        /// <summary>
        /// 	Returns true if this is the last subset within the superset.
        /// </summary>
        /// <value>
        /// 	Returns true if this is the last subset within the superset.
        /// </value>
        public bool IsLastPage { get; protected set; }

        /// <summary>
        /// 	One-based index of the first item in the paged subset.
        /// </summary>
        /// <value>
        /// 	One-based index of the first item in the paged subset.
        /// </value>
        public int FirstItemOnPage { get; protected set; }

        /// <summary>
        /// 	One-based index of the last item in the paged subset.
        /// </summary>
        /// <value>
        /// 	One-based index of the last item in the paged subset.
        /// </value>
        public int LastItemOnPage { get; protected set; }

        #endregion
    }
    #endregion

    #region [BasePagedList]
    /// <summary>
    /// 	Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
    /// </summary>
    /// <remarks>
    /// 	Represents a subset of a collection of objects that can be individually accessed by index and containing metadata about the superset collection of objects this subset was created from.
    /// </remarks>
    /// <typeparam name = "T">The type of object the collection should contain.</typeparam>
    /// <seealso cref = "IPagedList{T}" />
    /// <seealso cref = "List{T}" />
    [Serializable]
    public abstract class BasePagedList<T> : PagedListMetaData, IPagedList<T>
    {
        /// <summary>
        /// 	The subset of items contained only within this one page of the superset.
        /// </summary>
        protected readonly List<T> Subset = new List<T>();

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        protected internal BasePagedList()
        {
        }

        /// <summary>
        /// 	Initializes a new instance of a type deriving from <see cref = "BasePagedList{T}" /> and sets properties needed to calculate position and size data on the subset and superset.
        /// </summary>
        /// <param name = "pageNumber">The one-based index of the subset of objects contained by this instance.</param>
        /// <param name = "pageSize">The maximum size of any individual subset.</param>
        /// <param name = "totalItemCount">The size of the superset.</param>
        protected internal BasePagedList(int pageNumber, int pageSize, int totalItemCount)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "PageNumber cannot be below 1.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, "PageSize cannot be less than 1.");

            // set source to blank list if superset is null to prevent exceptions
            TotalItemCount = totalItemCount;
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageCount = TotalItemCount > 0
                            ? (int)Math.Ceiling(TotalItemCount / (double)PageSize)
                            : 0;
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            var numberOfLastItemOnPage = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = numberOfLastItemOnPage > TotalItemCount
                                ? TotalItemCount
                                : numberOfLastItemOnPage;
        }

        #region IPagedList<T> Members

        /// <summary>
        /// 	Returns an enumerator that iterates through the BasePagedList&lt;T&gt;.
        /// </summary>
        /// <returns>A BasePagedList&lt;T&gt;.Enumerator for the BasePagedList&lt;T&gt;.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return Subset.GetEnumerator();
        }

        /// <summary>
        /// 	Returns an enumerator that iterates through the BasePagedList&lt;T&gt;.
        /// </summary>
        /// <returns>A BasePagedList&lt;T&gt;.Enumerator for the BasePagedList&lt;T&gt;.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        ///<summary>
        ///	Gets the element at the specified index.
        ///</summary>
        ///<param name = "index">The zero-based index of the element to get.</param>
        public T this[int index]
        {
            get { return Subset[index]; }
        }

        /// <summary>
        /// 	Gets the number of elements contained on this page.
        /// </summary>
        public int Count
        {
            get { return Subset.Count; }
        }

        ///<summary>
        /// Gets a non-enumerable copy of this paged list.
        ///</summary>
        ///<returns>A non-enumerable copy of this paged list.</returns>
        public IPagedList GetMetaData()
        {
            return new PagedListMetaData(this);
        }

        #endregion
    }
    #endregion

    #region [PagedList]
    /// <summary>
    /// see https://github.com/troygoode/PagedList/blob/master/src/PagedList/PagedList.cs
    /// </summary>
    [Serializable]
    public class PagedList<T> : BasePagedList<T>
    {
        public PagedList(IEnumerable<T> superset, int pageNumber, int pageSize, int totalItemCount)
        {
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException("pageNumber", pageNumber, "PageNumber cannot be below 1.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException("pageSize", pageSize, "PageSize cannot be less than 1.");

            // set source to blank list if superset is null to prevent exceptions
            TotalItemCount = totalItemCount;
            PageSize = pageSize;
            PageNumber = pageNumber;
            PageCount = TotalItemCount > 0
                        ? (int)Math.Ceiling(TotalItemCount / (double)PageSize)
                        : 0;
            HasPreviousPage = PageNumber > 1;
            HasNextPage = PageNumber < PageCount;
            IsFirstPage = PageNumber == 1;
            IsLastPage = PageNumber >= PageCount;
            FirstItemOnPage = (PageNumber - 1) * PageSize + 1;
            var numberOfLastItemOnPage = FirstItemOnPage + PageSize - 1;
            LastItemOnPage = numberOfLastItemOnPage > TotalItemCount
                            ? TotalItemCount
                            : numberOfLastItemOnPage;

            // add items to internal list
            if (superset != null && TotalItemCount > 0)
                Subset.AddRange(superset);
        }
    }
    #endregion

    #region [PagedListExtensions]
    /// <summary>
    ///  see https://github.com/troygoode/PagedList/blob/master/src/PagedList/PagedListExtensions.cs
    /// </summary>
    public static class PagedListExtensions
    {
        public static PagedList<T> ToPagedList<T>(this IEnumerable<T> superset, int pageNumber, int pageSize, int totalItemCount)
        {
            return new PagedList<T>(superset, pageNumber, pageSize, totalItemCount);
        }
    }
    #endregion
}

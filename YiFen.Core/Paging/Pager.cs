using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using YiFen.Core;
using System.Web.Mvc;


public static class HtmlExtensions
{

    public static MvcHtmlString Pager<T>(this HtmlHelper helper, IPagingList<T> pagination, PagerStyleEnum style)
    {
        return MvcHtmlString.Create(new KeeyoPager<T>(pagination, helper.ViewContext.HttpContext.Request, style).ToString());
    }
    public static MvcHtmlString PagerFor<T>(this HtmlHelper helper, PagerStyleEnum style)
    {
        IPagingList<T> pagination = helper.ViewData.Model as IPagingList<T>;
        return MvcHtmlString.Create(new KeeyoPager<T>(pagination, helper.ViewContext.HttpContext.Request, style).ToString());
    }

}

public class KeeyoPager<T>
{
    private readonly IPagingList<T> _pagination;
    private readonly HttpRequestBase _request;
    private PagerStyleEnum _paginationStyle;//生成的分页类型
    private int _paginationMaxNum = 10;//显示页码的个数
    private string _paginationFormat = "页码：<b>{0}</b>/{1}，共<b>{2}</b>条记录";
    private string _currentFormat = "<span title=\"当前页\">{0}</span>";

    private string _paginationSingleFormat = "总共{0}条数据,页码{1}";
    private string _paginationFirst = "首页";
    private string _paginationPrev = "上页";
    private string _paginationNext = "下页";
    private string _paginationLast = "尾页";
    private string _pageQueryName = "page";
    private string _pageSizeQueryName = "pageSize";

    private string _paginationPrevX = "前10页";
    private string _paginationNextX = "后10页";

    /// <summary>
    /// Creates a new instance of the Pager class.
    /// </summary>
    /// <param name="pagination">The IPagination datasource</param>
    /// <param name="request">The current HTTP Request</param>
    public KeeyoPager(IPagingList<T> pagination, HttpRequestBase request, PagerStyleEnum style)
    {
        _pagination = pagination;
        _request = request;
        _paginationStyle = style;
    }

    /// <summary>
    /// Specifies the query string parameter to use when generating pager links. The default is 'page'
    /// </summary>
    public KeeyoPager<T> QueryParam(string queryStringParam)
    {
        _pageQueryName = queryStringParam;
        return this;
    }

    /// <summary>
    /// Specifies the format to use when rendering a pagination containing a single page. 
    /// The default is 'Showing {0} of {1}' (eg 'Showing 1 of 3')
    /// </summary>
    public KeeyoPager<T> SingleFormat(string format)
    {
        _paginationSingleFormat = format;
        return this;
    }

    /// <summary>
    /// Specifies the format to use when rendering a pagination containing multiple pages. 
    /// The default is 'Showing {0} - {1} of {2}' (eg 'Showing 1 to 3 of 6')
    /// </summary>
    public KeeyoPager<T> Format(string format)
    {
        _paginationFormat = format;
        return this;
    }

    /// <summary>
    /// Text for the 'first' link.
    /// </summary>
    public KeeyoPager<T> First(string first)
    {
        _paginationFirst = first;
        return this;
    }

    /// <summary>
    /// Text for the 'prev' link
    /// </summary>
    public KeeyoPager<T> Previous(string previous)
    {
        _paginationPrev = previous;
        return this;
    }

    /// <summary>
    /// Text for the 'next' link
    /// </summary>
    public KeeyoPager<T> Next(string next)
    {
        _paginationNext = next;
        return this;
    }

    /// <summary>
    /// Text for the 'last' link
    /// </summary>
    public KeeyoPager<T> Last(string last)
    {
        _paginationLast = last;
        return this;
    }

    /// <summary>
    /// 根据根据枚举生成不同的分页
    /// lusm 2009.07.20 add
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (_pagination.TotalItems == 0)
        {
            return null;
        }

        var builder = new StringBuilder();

        //if (_paginationStyle == DBWinPagerStyleEnum.Normal)
        //{
        builder.Append("<div class='Pager'>");
        builder.Append("<span class='Pager_Info'>");
        if (_pagination.PageSize == 1)
        {
            builder.AppendFormat(_paginationSingleFormat, _pagination.TotalItems, _pagination.PageNumber);
        }
        else
        {
            builder.AppendFormat(_paginationFormat, _pagination.PageNumber, _pagination.TotalPages, _pagination.TotalItems);
        }
        builder.Append("</span>");
        builder.Append("<span class='Pager_Link'>");

        if (_pagination.PageNumber == 1)
        {
            builder.Append("<span>" + _paginationFirst + "</span>");
        }
        else
        {
            builder.Append(CreatePageLink(1, _paginationFirst, _paginationFirst));
        }

        //builder.Append(" | ");

        if (_pagination.HasPreviousPage)
        {
            builder.Append(CreatePageLink(_pagination.PageNumber - 1, _paginationPrev, _paginationPrev));
        }
        else
        {
            builder.Append("<span>" + _paginationPrev + "</span>");

        }

        builder.Append(CreateNum());

        if (_pagination.HasNextPage)
        {
            builder.Append(CreatePageLink(_pagination.PageNumber + 1, _paginationNext, _paginationNext));
        }
        else
        {

            builder.Append("<span>" + _paginationNext + "</span>");
        }

        //builder.Append(" | ");

        int lastPage = _pagination.TotalPages;

        if (_pagination.PageNumber < lastPage)
        {
            builder.Append(CreatePageLink(lastPage, _paginationLast, _paginationLast));
        }
        else
        {
            builder.Append("<span>" + _paginationLast + "</span>");

        }

        if (!_paginationStyle.Equals(PagerStyleEnum.OnlyLink))
        {
            builder.Append(GoTo());
        }

        builder.Append(@"</span></div>");

        //}
        return builder.ToString();
    }

    private string CreatePageLink(int pageNumber, string text)
    {
        string queryString = CreateQueryString(_request.QueryString);
        string filePath = _request.FilePath;
        return string.Format("<a href=\"{0}?{1}={2}{3}\">{4}</a>", filePath, _pageQueryName, pageNumber, queryString, text);
    }
    private string CreatePageLink(int pageNumber, string text, string title)
    {
        string queryString = CreateQueryString(_request.QueryString);
        string filePath = _request.FilePath;
        return string.Format("<a href=\"{0}?{1}={2}{3}\" title=\"{4}\">{5}</a>", filePath, _pageQueryName, pageNumber, queryString, title, text);
    }
    private string GoTo()
    {
        string textFormat = "<input type='text' name='{0}' value='{1}' {2} />";
        string queryString = CreateQueryString(_request.QueryString, new string[] { _pageQueryName, _pageSizeQueryName });
        string hiddenString = CreateHidden(_request.QueryString);
        string filePath = _request.FilePath;

        StringBuilder sb = new StringBuilder("<form method='get'>");
        sb.Append(hiddenString);
        sb.Append("<div class='pageSize'>每页");
        sb.Append("<select title=\"每页显示的条数\" onchange=\"javascript:window.location=href='");
        sb.Append(filePath);
        sb.Append("?pageSize='+this.options[this.selectedIndex].value+'");
        sb.Append(queryString);
        sb.Append("';\">");
        //sb.Append("<option value=\"\">选择页数</option>");
        sb.Append("<option selected=\"selected\">选择</option><option value=\"10\">10</option><option value=\"20\">20</option><option value=\"50\">50</option><option value=\"100\">100</option> </select>");
        //sb.Append("<input id=\"");
        //sb.Append(_pageSizeQueryName);
        //sb.Append("\" style=\"position: absolute; width: 30px;\" value=\"10\" />");
        sb.Append(string.Format(textFormat, _pageSizeQueryName, _pagination.PageSize, ""));
        sb.Append("</div>条，第");
        sb.Append(string.Format(textFormat, _pageQueryName, _pagination.PageNumber, "class=\"page\""));
        sb.Append("页<button type='submit'title='跳转到指定页码' >转到</button>");
        sb.Append("</form>");
        return sb.ToString();
    }

    private string CreateNum()
    {
        int other = 2;

        StringBuilder builder = new StringBuilder();

        if (_pagination.PageNumber > 10)
        {
            builder.Append(CreatePageLink(_pagination.PageNumber - 10, "...", _paginationPrevX));
        }
        else if (_pagination.PageNumber > other + 1)
        {
            builder.Append("...");
        }
        for (int i = _pagination.PageNumber - other; i <= _pagination.PageNumber + other && i <= _pagination.TotalPages; i++)
        {
            if (i == _pagination.PageNumber)
            {
                builder.Append(string.Format(_currentFormat, i));
            }
            else if (i > 0)
            {
                builder.Append(CreatePageLink(i, i.ToString()));
            }
        }
        if (_pagination.PageNumber + 10 <= _pagination.TotalPages)
        {
            builder.Append(CreatePageLink(_pagination.PageNumber + 10, "...", _paginationNextX));
        }
        else if (_pagination.PageNumber + other < _pagination.TotalPages)
        {
            builder.Append("...");
        }
        return builder.ToString();
    }
    /// <summary>
    /// 修改过原代码，URL格式化。
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    private string CreateQueryString(NameValueCollection values)
    {
        var builder = new StringBuilder();

        foreach (string key in values.Keys)
        {
            if (key == _pageQueryName)
            //Don't re-add any existing 'page' variable to the querystring - this will be handled in CreatePageLink.
            {
                continue;
            }

            foreach (var value in values.GetValues(key))
            {
                builder.AppendFormat("&{0}={1}", key, HttpUtility.UrlEncode(value));
            }
        }

        return builder.ToString();

    }

    /// <summary>
    /// 修改过原代码，URL格式化。
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    private string CreateQueryString(NameValueCollection values, string[] withoutKeys)
    {
        var builder = new StringBuilder();
        foreach (string key in values.Keys)
        {
            bool without = false;
            foreach (string withoutKey in withoutKeys)
            {
                if (key == withoutKey)
                //Don't re-add any existing 'page' variable to the querystring - this will be handled in CreatePageLink.
                {
                    without = true;
                }
            }
            if (without)
            {
                continue;
            }
            foreach (var value in values.GetValues(key))
            {
                builder.AppendFormat("&{0}={1}", key, HttpUtility.UrlEncode(value));
            }
        }

        return builder.ToString();

    }
    /// <summary>
    /// 用于生成把QueryString存放在Hidden表单
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    private string CreateHidden(NameValueCollection values)
    {
        var builder = new StringBuilder();

        foreach (string key in values.Keys)
        {
            if (key == _pageQueryName || key == _pageSizeQueryName)
            //因为FORM里可以输入pageSize，所以也需要跳过pageSize。
            //Don't re-add any existing 'page' variable to the querystring - this will be handled in CreatePageLink.
            {
                continue;
            }

            foreach (var value in values.GetValues(key))
            {
                builder.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", key, value);
            }
        }

        return builder.ToString();

    }
}
/// <summary>
/// 分页类：{Page=0,PageSize=10,OrderBy="SortID,True"}
/// </summary>
public class PagerInfo
{
    public PagerInfo()
    {
        Page = 1;
        PageSize = 10;
        OrderBy = "";
    }
    public PagerInfo(int _Page, int _PageSize, string _OrderBy)
    {
        this.Page = _Page;
        this.PageSize = _PageSize;
        this.OrderBy = _OrderBy;
    }


    public int Page { get; set; }
    public int PageSize { get; set; }
    public string OrderBy { get; set; }
    public string OrderByToSQL
    {

        get
        {
            if (string.IsNullOrEmpty(OrderBy))
            {
                return string.Empty;
            }
            string[] temp = OrderBy.Split(',');
            if (temp.Length < 1)
            {
                return string.Empty;
            }
            if (temp[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
            {
                return temp[0] + " desc";
            }
            else
            {
                return temp[0] + "";
            }
        }
    }

    //public string OrderField { get; set; }
    //public bool OrderDesc { get; set; }
}
public enum PagerStyleEnum
{
    Normal = 0,
    OnlyLink = 1,
}
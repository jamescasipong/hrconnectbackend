using hrconnectbackend.Constants;
using hrconnectbackend.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;

public class PaginationParams
{
    private const int MaxPageSize = 100;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}

public class PagedResponse<T>
{
    public bool Success { get; } = true;
    public PaginationDetails Pagination { get; set; }
    public T Data { get; set; }
    public string? Message { get; } = null;

    public PagedResponse(T data, PaginationDetails paginationDetails, string? message = null)
    {
        if (paginationDetails.PageNumber < 1)
        {
            throw new BadRequestException(ErrorCodes.InvalidPageNumber, "Page number must be greater than zero.");
        }
        if (paginationDetails.PageSize < 1)
        {
            throw new BadRequestException(ErrorCodes.InvalidPageSize, "Page size must be greater than zero.");
        }

        Message = message != null ? message : $"Successfully retrieved records";

        Pagination = new PaginationDetails(paginationDetails.PageNumber, paginationDetails.PageSize, paginationDetails.TotalRecords);
        Data = data;
    }
}

public class PaginationDetails
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }

    public PaginationDetails(int pageNumber, int pageSize, int totalRecords)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
        TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
    }
}
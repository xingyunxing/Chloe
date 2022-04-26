namespace Chloe.Query.QueryExpressions
{
    class PagingExpression : QueryExpression
    {
        public PagingExpression(Type elementType, QueryExpression prevExpression, int pageNumber, int pageSize) : base(QueryExpressionType.Paging, elementType, prevExpression)
        {
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
        }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }

        public int Skip
        {
            get
            {
                return (this.PageNumber - 1) * this.PageSize;
            }
        }

        public int Take { get { return this.PageSize; } }

        public override T Accept<T>(QueryExpressionVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}

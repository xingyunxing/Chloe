using Chloe.Extensions;
using Chloe.Reflection;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Visitors
{
    /// <summary>
    /// 解析排除字段表达式
    /// </summary>
    public class ExcludeFieldExtractor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldsLambdaExpression">a => a.Name || a => new { a.Name, a.Age } || a => new object[] { a.Name, a.Age }</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static List<LinkedNode<MemberInfo>> Extract(LambdaExpression fieldsLambdaExpression)
        {
            var body = ExpressionExtension.StripConvert(fieldsLambdaExpression.Body);

            if (body.NodeType == ExpressionType.MemberAccess)
            {
                //a => a.Name
                return new List<LinkedNode<MemberInfo>>(1) { ExcludeFieldExtractorCore.Extract(body as MemberExpression) };
            }

            ReadOnlyCollection<Expression> fieldExps = null;
            if (body.NodeType == ExpressionType.New)
            {
                NewExpression newExpression = (NewExpression)body;
                if (newExpression.Type.IsAnonymousType())
                {
                    //a => new { a.Name, a.Age }
                    fieldExps = newExpression.Arguments;
                }
                else
                {
                    throw new NotSupportedException($"Not support exclude expression '{fieldsLambdaExpression.ToString()}'.");
                }
            }
            else if (body.NodeType == ExpressionType.NewArrayInit)
            {
                //a => new object[] { a.Name, a.Age }
                NewArrayExpression newArrayExpression = body as NewArrayExpression;
                if (newArrayExpression == null)
                    throw new NotSupportedException($"Not support exclude expression '{fieldsLambdaExpression.ToString()}'.");

                fieldExps = newArrayExpression.Expressions;
            }
            else
            {
                throw new NotSupportedException($"Not support exclude expression '{fieldsLambdaExpression.ToString()}'.");
            }

            List<LinkedNode<MemberInfo>> fields = new List<LinkedNode<MemberInfo>>(fieldExps.Count);

            foreach (var fieldExp in fieldExps)
            {
                fields.Add(ExcludeFieldExtractorCore.Extract(fieldExp));
            }

            return fields;
        }

        class ExcludeFieldExtractorCore : ExpressionVisitor<LinkedNode<MemberInfo>>
        {
            static readonly ExcludeFieldExtractorCore _extractor = new ExcludeFieldExtractorCore();

            public ExcludeFieldExtractorCore()
            {
            }

            public static LinkedNode<MemberInfo> Extract(Expression exp)
            {
                LinkedNode<MemberInfo> node = _extractor.Visit(exp);

                LinkedNode<MemberInfo> headNode = node;
                while (headNode.Previous != null)
                {
                    headNode = headNode.Previous;
                }

                return headNode;
            }

            public override LinkedNode<MemberInfo> Visit(Expression exp)
            {
                if (exp == null)
                    return null;

                switch (exp.NodeType)
                {
                    case ExpressionType.Lambda:
                        return this.VisitLambda((LambdaExpression)exp);
                    case ExpressionType.Parameter:
                        return this.VisitParameter((ParameterExpression)exp);
                    case ExpressionType.MemberAccess:
                        return this.VisitMemberAccess((MemberExpression)exp);
                    case ExpressionType.Convert:
                        return this.VisitUnary_Convert((UnaryExpression)exp);
                    default:
                        throw new NotSupportedException($"Not support exclude expression '{exp.ToString()}'.");
                }
            }

            protected override LinkedNode<MemberInfo> VisitParameter(ParameterExpression exp)
            {
                return null;
            }

            protected override LinkedNode<MemberInfo> VisitLambda(LambdaExpression exp)
            {
                return this.Visit(exp.Body);
            }

            protected override LinkedNode<MemberInfo> VisitMemberAccess(MemberExpression exp)
            {
                var parentNode = this.Visit(exp.Expression);
                LinkedNode<MemberInfo> node = new LinkedNode<MemberInfo>(exp.Member);

                if (parentNode != null)
                {
                    parentNode.Next = node;
                    node.Previous = parentNode;
                }

                return node;
            }

            protected override LinkedNode<MemberInfo> VisitUnary_Convert(UnaryExpression exp)
            {
                return this.Visit(exp.Operand);
            }
        }
    }
}
